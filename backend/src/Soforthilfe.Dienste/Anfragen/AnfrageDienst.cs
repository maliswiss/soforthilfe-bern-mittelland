using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Soforthilfe.Datenzugriff;
using Soforthilfe.Datenzugriff.Mandanten;
using Soforthilfe.Datenzugriff.Modell;

namespace Soforthilfe.Dienste.Anfragen;

/// <summary>
/// Erfasst Anfragen in der Region der laufenden Anfrage und liest sie wieder.
/// </summary>
public sealed partial class AnfrageDienst(SoforthilfeDbContext datenbank, IMandantenKontext mandant) : IAnfrageDienst
{
    // Schweizer Postleitzahlen sind vierstellig und beginnen nicht mit 0.
    [GeneratedRegex("^[1-9][0-9]{3}$")]
    private static partial Regex PostleitzahlMuster();

    public async Task<AnfrageErgebnis> ErfassenAsync(AnfrageDaten daten, CancellationToken abbruch)
    {
        if (mandant.RegionId is not Guid region)
        {
            return new AnfrageErgebnis(null, AnfrageFehler.RegionFehlt);
        }

        // Mindestens eine Kategorie oder ein Freitext; beides zusammen ist erlaubt.
        var freitext = string.IsNullOrWhiteSpace(daten.Freitext) ? null : daten.Freitext.Trim();
        if (daten.KategorieId is null && freitext is null)
        {
            return new AnfrageErgebnis(null, AnfrageFehler.InhaltFehlt);
        }

        var postleitzahl = daten.Postleitzahl.Trim();
        if (!PostleitzahlMuster().IsMatch(postleitzahl))
        {
            return new AnfrageErgebnis(null, AnfrageFehler.PostleitzahlUngueltig);
        }

        Kategorie? kategorie = null;
        if (daten.KategorieId is Guid kategorieId)
        {
            kategorie = await datenbank.Kategorien.AsNoTracking().FirstOrDefaultAsync(k => k.Id == kategorieId, abbruch);
            if (kategorie is null)
            {
                return new AnfrageErgebnis(null, AnfrageFehler.KategorieUnbekannt);
            }
        }

        var anfrage = new Anfrage
        {
            Id = Guid.NewGuid(),
            RegionId = region,
            KontoId = daten.KontoId,
            KategorieId = kategorie?.Id,
            Freitext = freitext,
            Postleitzahl = postleitzahl,
            Status = AnfrageStatus.Offen,
            ErstelltAm = DateTimeOffset.UtcNow,
        };

        datenbank.Anfragen.Add(anfrage);
        await datenbank.SaveChangesAsync(abbruch);

        anfrage.Kategorie = kategorie;
        return new AnfrageErgebnis(Info(anfrage), null);
    }

    public async Task<IReadOnlyList<AnfrageInfo>> ListeAsync(Guid? nurVon, CancellationToken abbruch)
    {
        // Der Regionsfilter des Datenzugriffs gilt immer; nurVon schränkt zusätzlich auf ein Konto ein.
        var liste = await Abfrage(nurVon)
            .OrderByDescending(a => a.ErstelltAm)
            .ToListAsync(abbruch);

        return liste.Select(Info).ToList();
    }

    public async Task<AnfrageInfo?> LiesAsync(Guid id, Guid? nurVon, CancellationToken abbruch)
    {
        var anfrage = await Abfrage(nurVon).FirstOrDefaultAsync(a => a.Id == id, abbruch);
        return anfrage is null ? null : Info(anfrage);
    }

    private IQueryable<Anfrage> Abfrage(Guid? nurVon)
    {
        var abfrage = datenbank.Anfragen.Include(a => a.Kategorie).AsNoTracking();
        return nurVon is Guid konto ? abfrage.Where(a => a.KontoId == konto) : abfrage;
    }

    private static AnfrageInfo Info(Anfrage anfrage) => new(
        anfrage.Id,
        anfrage.Kategorie?.Kennung,
        anfrage.Kategorie?.Bezeichnung,
        anfrage.Kategorie?.Kostenpflichtig,
        anfrage.Freitext,
        anfrage.Postleitzahl,
        anfrage.Status.ToString(),
        anfrage.ErstelltAm);
}
