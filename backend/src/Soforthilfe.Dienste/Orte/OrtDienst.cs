using Microsoft.EntityFrameworkCore;
using Soforthilfe.Datenzugriff;
using Soforthilfe.Datenzugriff.Modell;

namespace Soforthilfe.Dienste.Orte;

/// <summary>
/// Postleitzahl-Zuordnung aus der Tabelle <c>Ortschaften</c>. Eine Ortschaft kann mehrere Gemeinden berühren;
/// massgebend ist die Gemeinde mit dem grössten Anteil an den Adressen.
/// </summary>
public sealed class OrtDienst(SoforthilfeDbContext datenbank) : IOrtDienst
{
    /// <summary>
    /// Anteil der Adressen in Prozent, ab dem eine Gemeinde für den Hinweis «mehrere Gemeinden» zählt.
    /// Kleinere Anteile sind Splitter am Rand und würden den Hinweis fast überall anzeigen.
    /// </summary>
    public const double MindestanteilProzent = 3;

    public async Task<IReadOnlyList<OrtInfo>> NachPlzAsync(string plz, CancellationToken abbruch)
    {
        var zeilen = await ZeilenAsync(plz, abbruch);
        if (zeilen.Count == 0)
        {
            return [];
        }

        var regionen = await RegionenAsync(abbruch);
        var mehrereGemeinden = zeilen
            .Where(z => z.Adressenanteil >= MindestanteilProzent)
            .Select(z => z.BfsNummer)
            .Distinct()
            .Count() > 1;

        return zeilen
            .GroupBy(z => new { z.Zusatzziffer, z.Name })
            .OrderBy(gruppe => gruppe.Key.Zusatzziffer, StringComparer.Ordinal)
            .Select(gruppe =>
            {
                var haupt = gruppe.OrderByDescending(z => z.Adressenanteil).First();
                return new OrtInfo(haupt.Postleitzahl, haupt.Name, haupt.Gemeinde, haupt.Kanton, mehrereGemeinden,
                    RegionVon(haupt.BfsNummer, regionen));
            })
            .ToList();
    }

    public async Task<PlzPruefung> PruefeFuerRegionAsync(string plz, Guid regionId, CancellationToken abbruch)
    {
        if (!await datenbank.Ortschaften.AnyAsync(abbruch))
        {
            return PlzPruefung.OhneVerzeichnis;
        }

        var zeilen = await ZeilenAsync(plz, abbruch);
        if (zeilen.Count == 0)
        {
            return PlzPruefung.Unbekannt;
        }

        var gemeinden = await datenbank.Regionen
            .AsNoTracking()
            .Where(r => r.Id == regionId)
            .Select(r => r.Gemeinden)
            .FirstOrDefaultAsync(abbruch);

        // Eine Region ohne festgelegte Gemeinden schränkt die Postleitzahl nicht ein.
        if (gemeinden is null || gemeinden.Length == 0)
        {
            return PlzPruefung.Passt;
        }

        // Grenzfälle: Eine Postleitzahl, die auch nur eine Gemeinde der Region berührt, passt.
        return zeilen.Any(z => gemeinden.Contains(z.BfsNummer)) ? PlzPruefung.Passt : PlzPruefung.AndereRegion;
    }

    private Task<List<Ortschaft>> ZeilenAsync(string plz, CancellationToken abbruch)
    {
        var gesucht = plz.Trim();
        return datenbank.Ortschaften
            .AsNoTracking()
            .Where(o => o.Postleitzahl == gesucht)
            .ToListAsync(abbruch);
    }

    private async Task<List<(string Kennung, int[] Gemeinden)>> RegionenAsync(CancellationToken abbruch)
    {
        var regionen = await datenbank.Regionen
            .AsNoTracking()
            .Where(r => r.Aktiv)
            .Select(r => new { r.Kennung, r.Gemeinden })
            .ToListAsync(abbruch);

        return regionen.Select(r => (r.Kennung, r.Gemeinden)).ToList();
    }

    private static string? RegionVon(int bfsNummer, List<(string Kennung, int[] Gemeinden)> regionen) =>
        regionen.FirstOrDefault(r => r.Gemeinden.Contains(bfsNummer)).Kennung;
}
