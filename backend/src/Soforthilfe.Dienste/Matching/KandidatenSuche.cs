using Microsoft.EntityFrameworkCore;
using Soforthilfe.Datenzugriff;
using Soforthilfe.Datenzugriff.Modell;
using Soforthilfe.Dienste.Orte;

namespace Soforthilfe.Dienste.Matching;

/// <summary>
/// Kandidatensuche für das Matching. Geeignet sind verifizierte Helferprofile derselben Region mit einer Fähigkeit
/// der Kategorie, bei kostenpflichtigen Kategorien nur mit Preisangaben (Anforderung A9). Die Distanz wird zwischen
/// den Punkten der Postleitzahlen gemessen; sie muss im Suchradius der Anfrage und im Einsatzradius des Profils
/// liegen. Genaue Standorte gibt es dabei nie.
/// </summary>
public sealed class KandidatenSuche(SoforthilfeDbContext datenbank, IOrtDienst orte) : IKandidatenSuche
{
    public async Task<KandidatenErgebnis?> FuerAnfrageAsync(Guid anfrageId, CancellationToken abbruch)
    {
        // Der Regionsfilter des Datenzugriffs blendet Anfragen anderer Regionen aus.
        var anfrage = await datenbank.Anfragen
            .AsNoTracking()
            .Include(a => a.Kategorie)
            .FirstOrDefaultAsync(a => a.Id == anfrageId, abbruch);
        if (anfrage is null)
        {
            return null;
        }

        var suchradius = Suchradius.FuerPrioritaet(anfrage.Prioritaet);
        if (anfrage.Kategorie is null)
        {
            return new KandidatenErgebnis(suchradius, true, []);
        }

        var standort = await orte.PunktZuPlzAsync(anfrage.Postleitzahl, anfrage.Ort, abbruch);
        var kandidaten = await SuchenAsync(new KandidatenKriterien(anfrage.RegionId, anfrage.Kategorie.Id,
            anfrage.Kategorie.Kostenpflichtig, anfrage.Postleitzahl, standort, suchradius), abbruch);

        return new KandidatenErgebnis(suchradius, false, kandidaten);
    }

    public async Task<IReadOnlyList<Kandidat>> SuchenAsync(KandidatenKriterien kriterien, CancellationToken abbruch)
    {
        var grundmenge = datenbank.Helferprofile
            .AsNoTracking()
            .Where(p => p.RegionId == kriterien.RegionId)
            .Where(p => p.Verifizierung == Verifizierungsstatus.Verifiziert)
            .Where(p => p.Skills.Any(s => s.KategorieId == kriterien.KategorieId));

        // Kostenpflichtig: nur mit Preisangaben, damit der Preis vor der Zusage feststeht.
        if (kriterien.Kostenpflichtig)
        {
            grundmenge = grundmenge.Where(p => p.AnfahrtChf != null && p.StundenansatzChf != null);
        }

        // Zuerst Kennung und Distanz; geography misst in Metern, die Vorauswahl nutzt den räumlichen Index.
        Dictionary<Guid, double> distanzen;
        if (kriterien.Standort is { } punkt)
        {
            var radiusMeter = kriterien.SuchradiusKm * 1000.0;
            distanzen = await grundmenge
                .Where(p => p.Standort != null && p.Standort.IsWithinDistance(punkt, radiusMeter))
                .Select(p => new { p.Id, Distanz = p.Standort!.Distance(punkt) })
                .ToDictionaryAsync(t => t.Id, t => t.Distanz, abbruch);
        }
        else
        {
            // Ohne Ortschaftenverzeichnis: nur dieselbe Postleitzahl.
            distanzen = await grundmenge
                .Where(p => p.Postleitzahl == kriterien.Postleitzahl)
                .Select(p => p.Id)
                .ToDictionaryAsync(id => id, _ => 0.0, abbruch);
        }

        if (distanzen.Count == 0)
        {
            return [];
        }

        // Danach die Profile mit Konto für Anzeige und Bewertung.
        var ids = distanzen.Keys.ToList();
        var profile = await datenbank.Helferprofile
            .AsNoTracking()
            .Include(p => p.Konto)
            .Where(p => ids.Contains(p.Id))
            .ToListAsync(abbruch);

        return Auswaehlen(profile.Select(p => new Kandidat(p, distanzen[p.Id])), kriterien.SuchradiusKm);
    }

    /// <summary>
    /// Nur Kandidaten im Such- und Einsatzradius, nächste zuerst; bei gleicher Distanz nach Kennung, damit die
    /// Reihenfolge immer gleich ist.
    /// </summary>
    public static IReadOnlyList<Kandidat> Auswaehlen(IEnumerable<Kandidat> kandidaten, int suchradiusKm) =>
        kandidaten
            .Where(k => Suchradius.ImEinsatzgebiet(k.DistanzMeter, suchradiusKm, k.Profil.EinsatzradiusKm))
            .OrderBy(k => k.DistanzMeter)
            .ThenBy(k => k.Profil.Id)
            .ToList();
}
