using Microsoft.EntityFrameworkCore;
using Soforthilfe.Datenzugriff;
using Soforthilfe.Datenzugriff.Modell;
using Soforthilfe.Dienste.Protokoll;

namespace Soforthilfe.Dienste.Matching;

/// <summary>
/// Smart-Matching nach Distanz, Fähigkeit, Bewertung, Verfügbarkeit und Erfahrung mit einstellbaren Gewichten.
/// Gespeichert werden Rang, Punktzahl, Teilpunkte und die verwendeten Gewichte.
/// </summary>
public sealed class MatchingDienst(
    SoforthilfeDbContext datenbank,
    IKandidatenSuche kandidatenSuche,
    MatchingGewichte gewichte,
    IProtokollDienst protokoll) : IMatchingDienst
{
    public async Task<int> VorschlaegeErzeugenAsync(Guid anfrageId, CancellationToken abbruch)
    {
        var anfrage = await datenbank.Anfragen.FirstOrDefaultAsync(a => a.Id == anfrageId, abbruch);

        // Nur offene Anfragen mit Kategorie erhalten Vorschläge.
        if (anfrage?.KategorieId is null || anfrage.Status is not (AnfrageStatus.Offen or AnfrageStatus.Vorgeschlagen))
        {
            return 0;
        }

        var ergebnis = await kandidatenSuche.FuerAnfrageAsync(anfrageId, abbruch);
        if (ergebnis is null || ergebnis.Kandidaten.Count == 0)
        {
            return 0;
        }

        var vorhanden = await datenbank.Vorschlaege
            .Where(v => v.AnfrageId == anfrageId)
            .Select(v => new { v.HelferprofilId, v.Rang })
            .ToListAsync(abbruch);
        var bekannt = vorhanden.Select(v => v.HelferprofilId).ToHashSet();
        var rang = vorhanden.Count == 0 ? 1 : vorhanden.Max(v => v.Rang) + 1;

        var beste = Punktzahl.Ordnen(ergebnis.Kandidaten
                .Where(k => !bekannt.Contains(k.Profil.Id))
                .Select(k => Punktzahl.Bewerten(k, ergebnis.SuchradiusKm, gewichte)))
            .Take(Punktzahl.AnzahlVorschlaege)
            .ToList();
        if (beste.Count == 0)
        {
            return 0;
        }

        var jetzt = DateTimeOffset.UtcNow;
        foreach (var eintrag in beste)
        {
            datenbank.Vorschlaege.Add(Neu(anfrage, eintrag, rang++, jetzt));
        }

        anfrage.Status = AnfrageStatus.Vorgeschlagen;
        await datenbank.SaveChangesAsync(abbruch);

        // Im Protokoll steht nur die Anzahl, keine Personen.
        await protokoll.SchreibenAsync(ProtokollAktionen.AnfrageVorgeschlagen, ProtokollAktionen.ObjektAnfrage,
            anfrage.Id.ToString(), beste.Count == 1 ? "1 Vorschlag" : $"{beste.Count} Vorschläge", abbruch);

        return beste.Count;
    }

    public async Task<IReadOnlyList<VorschlagInfo>?> VorschlaegeAsync(Guid anfrageId, Guid? nurVon, CancellationToken abbruch)
    {
        // Der Regionsfilter blendet fremde Regionen aus; nurVon beschränkt auf die eigenen Anfragen.
        var vorhanden = await datenbank.Anfragen
            .AsNoTracking()
            .AnyAsync(a => a.Id == anfrageId && (nurVon == null || a.KontoId == nurVon), abbruch);
        if (!vorhanden)
        {
            return null;
        }

        var vorschlaege = await datenbank.Vorschlaege
            .AsNoTracking()
            .Include(v => v.Helferprofil!)
            .ThenInclude(p => p.Konto)
            .Where(v => v.AnfrageId == anfrageId)
            .OrderBy(v => v.Rang)
            .ToListAsync(abbruch);

        return vorschlaege.Select(Info).ToList();
    }

    private Vorschlag Neu(Anfrage anfrage, BewerteterKandidat eintrag, int rang, DateTimeOffset jetzt) => new()
    {
        Id = Guid.NewGuid(),
        RegionId = anfrage.RegionId,
        AnfrageId = anfrage.Id,
        HelferprofilId = eintrag.Kandidat.Profil.Id,
        Rang = rang,
        Punktzahl = Math.Round(eintrag.Punktzahl, 4),
        DistanzMeter = (int)Math.Round(eintrag.Kandidat.DistanzMeter),
        TeilpunkteDistanz = Math.Round(eintrag.Teilpunkte.Distanz, 4),
        TeilpunkteFaehigkeit = Math.Round(eintrag.Teilpunkte.Faehigkeit, 4),
        TeilpunkteBewertung = Math.Round(eintrag.Teilpunkte.Bewertung, 4),
        TeilpunkteVerfuegbarkeit = Math.Round(eintrag.Teilpunkte.Verfuegbarkeit, 4),
        TeilpunkteErfahrung = Math.Round(eintrag.Teilpunkte.Erfahrung, 4),
        GewichtDistanz = gewichte.Distanz,
        GewichtFaehigkeit = gewichte.Faehigkeit,
        GewichtBewertung = gewichte.Bewertung,
        GewichtVerfuegbarkeit = gewichte.Verfuegbarkeit,
        GewichtErfahrung = gewichte.Erfahrung,
        PartnerBevorzugt = eintrag.PartnerBevorzugt,
        ErstelltAm = jetzt,
    };

    private static VorschlagInfo Info(Vorschlag v)
    {
        var profil = v.Helferprofil!;
        return new VorschlagInfo(
            v.Id,
            v.HelferprofilId,
            v.Rang,
            v.Punktzahl,
            v.DistanzMeter,
            profil.Konto?.Anzeigename ?? string.Empty,
            profil.Typ,
            profil.Betriebsname,
            profil.Postleitzahl,
            profil.Ort,
            profil.Einsatzbereit,
            profil.Bewertungsschnitt,
            profil.AnzahlBewertungen,
            profil.AnzahlEinsaetze,
            v.PartnerBevorzugt,
            new Teilpunkte(v.TeilpunkteDistanz, v.TeilpunkteFaehigkeit, v.TeilpunkteBewertung, v.TeilpunkteVerfuegbarkeit,
                v.TeilpunkteErfahrung),
            new MatchingGewichte(v.GewichtDistanz, v.GewichtFaehigkeit, v.GewichtBewertung, v.GewichtVerfuegbarkeit,
                v.GewichtErfahrung),
            v.ErstelltAm);
    }
}
