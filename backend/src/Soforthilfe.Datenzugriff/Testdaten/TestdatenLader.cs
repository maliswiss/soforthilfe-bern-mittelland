using Microsoft.EntityFrameworkCore;
using Soforthilfe.Datenzugriff.Modell;

namespace Soforthilfe.Datenzugriff.Testdaten;

/// <summary>
/// Testdaten mit ausschliesslich fiktiven Personen. Die E-Mail-Adressen enden auf «.invalid»,
/// einer Domain, die es nicht geben kann. Geladen wird nur auf ausdrücklichen Wunsch.
/// </summary>
public static class TestdatenLader
{
    // Zweite Region für die Prüfung der Datentrennung. Sie entsteht als Datensatz, nicht als Migration,
    // genau wie jede weitere Region im laufenden Betrieb.
    public const string ZweiteRegion = "biel-seeland";

    // Gemeinden der Verwaltungskreise Biel/Bienne (19) und Seeland (42) als BFS-Nummern, abgeglichen mit dem
    // amtlichen Ortschaftenverzeichnis vom 01.09.2026.
    public static readonly int[] GemeindenBielSeeland =
    [
        301, 302, 303, 304, 305, 306, 309, 310, 311, 312, 371, 372, 381, 382, 383,
        385, 386, 387, 388, 389, 390, 391, 392, 393, 394, 491, 492, 493, 494, 495,
        496, 497, 498, 499, 500, 501, 502, 731, 732, 733, 734, 735, 736, 737, 738,
        739, 740, 741, 742, 743, 744, 745, 746, 747, 748, 749, 750, 751, 754, 755,
        756,
    ];

    public static async Task LadenAsync(SoforthilfeDbContext datenbank, CancellationToken abbruch = default)
    {
        // Die zweite Region mit ihren Gemeinden; eine früher ohne Gemeinden angelegte Region wird ergänzt.
        var biel = await datenbank.Regionen.FirstOrDefaultAsync(r => r.Kennung == ZweiteRegion, abbruch);
        if (biel is null)
        {
            biel = new Region { Id = Guid.NewGuid(), Kennung = ZweiteRegion, Name = "Soforthilfe Biel/Seeland", Aktiv = true };
            datenbank.Regionen.Add(biel);
        }

        if (biel.Gemeinden.Length == 0)
        {
            biel.Gemeinden = GemeindenBielSeeland;
            await datenbank.SaveChangesAsync(abbruch);
        }

        // Nur in eine Datenbank ohne Konten laden, damit ein Neustart keine Doppelungen erzeugt.
        if (await datenbank.Konten.AnyAsync(abbruch))
        {
            return;
        }

        var jetzt = DateTimeOffset.UtcNow;
        var skills = await datenbank.Skills.ToDictionaryAsync(s => s.Kennung, abbruch);
        var kategorien = await datenbank.Kategorien.ToDictionaryAsync(k => k.Kennung, abbruch);

        var bern = Stammdaten.RegionBernMittelland;

        Konto NeuesKonto(Guid? region, string name, string kennung, Rolle rolle) => new()
        {
            Id = Guid.NewGuid(),
            RegionId = region,
            Anzeigename = name,
            EMail = $"{kennung}@beispiel.invalid",
            Rolle = rolle,
            ErstelltAm = jetzt,
        };

        // Region Bern/Mittelland
        var anna = NeuesKonto(bern, "Anna Beispiel", "anna.beispiel", Rolle.Hilfesuchende);
        var bruno = NeuesKonto(bern, "Bruno Muster", "bruno.muster", Rolle.Helfende);
        var clara = NeuesKonto(bern, "Clara Fiktiv", "clara.fiktiv", Rolle.Helfende);
        var dora = NeuesKonto(bern, "Dora Probe", "dora.probe", Rolle.Koordination);

        // Region Biel/Seeland
        var emil = NeuesKonto(biel.Id, "Emil Exempel", "emil.exempel", Rolle.Hilfesuchende);
        var fritz = NeuesKonto(biel.Id, "Fritz Platzhalter", "fritz.platzhalter", Rolle.Helfende);
        var greta = NeuesKonto(biel.Id, "Greta Muster", "greta.muster", Rolle.Mandantenadministration);

        // Plattformbetrieb ohne Region
        var hans = NeuesKonto(null, "Hans Betrieb", "hans.betrieb", Rolle.Plattformbetrieb);

        datenbank.Konten.AddRange(anna, bruno, clara, dora, emil, fritz, greta, hans);

        datenbank.Helferprofile.AddRange(
            new Helferprofil
            {
                KontoId = bruno.Id,
                RegionId = bern,
                Postleitzahl = "3012",
                Ort = "Bern",
                EinsatzradiusKm = 15,
                Skills = [skills["sanitaer-elektro"], skills["kleine-reparatur"]],
            },
            new Helferprofil
            {
                KontoId = clara.Id,
                RegionId = bern,
                Postleitzahl = "3110",
                Ort = "Münsingen",
                EinsatzradiusKm = 5,
                Skills = [skills["nachbarschaftshilfe"], skills["begleitung"]],
            },
            new Helferprofil
            {
                KontoId = fritz.Id,
                RegionId = biel.Id,
                Postleitzahl = "2502",
                Ort = "Biel/Bienne",
                EinsatzradiusKm = 20,
                Skills = [skills["it-soforthilfe"]],
            });

        datenbank.Anfragen.AddRange(
            new Anfrage
            {
                RegionId = bern,
                KontoId = anna.Id,
                KategorieId = kategorien["sanitaer-elektro"].Id,
                Freitext = "Der Wasserhahn in der Küche tropft stark.",
                Postleitzahl = "3011",
                Ort = "Bern",
                Zeitfenster = "heute zwischen 17 und 19 Uhr",
                ErstelltAm = jetzt,
            },
            new Anfrage
            {
                RegionId = biel.Id,
                KontoId = emil.Id,
                Freitext = "Ich brauche Hilfe beim Einrichten meines neuen Smartphones.",
                Postleitzahl = "2502",
                Ort = "Biel/Bienne",
                Prioritaet = Prioritaet.Niedrig,
                ErstelltAm = jetzt,
            });

        await datenbank.SaveChangesAsync(abbruch);
    }
}
