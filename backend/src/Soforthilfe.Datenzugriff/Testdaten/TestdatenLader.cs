using Microsoft.EntityFrameworkCore;
using Soforthilfe.Datenzugriff.Modell;

namespace Soforthilfe.Datenzugriff.Testdaten;

/// <summary>
/// Testdaten mit ausschliesslich fiktiven Personen. Die E-Mail-Adressen enden auf «.invalid»,
/// einer Domain, die es nicht geben kann. Geladen wird nur auf ausdrücklichen Wunsch.
/// </summary>
public static class TestdatenLader
{
    public static async Task LadenAsync(SoforthilfeDbContext datenbank, CancellationToken abbruch = default)
    {
        // Nur in eine Datenbank ohne Konten laden, damit ein Neustart keine Doppelungen erzeugt.
        if (await datenbank.Konten.AnyAsync(abbruch))
        {
            return;
        }

        var jetzt = DateTimeOffset.UtcNow;
        var skills = await datenbank.Skills.ToDictionaryAsync(s => s.Kennung, abbruch);

        Konto NeuesKonto(Guid? region, string name, string kennung, Rolle rolle) => new()
        {
            Id = Guid.NewGuid(),
            RegionId = region,
            Anzeigename = name,
            EMail = $"{kennung}@beispiel.invalid",
            Rolle = rolle,
            ErstelltAm = jetzt,
        };

        // Region Bern
        var anna = NeuesKonto(Stammdaten.RegionBern, "Anna Beispiel", "anna.beispiel", Rolle.Hilfesuchende);
        var bruno = NeuesKonto(Stammdaten.RegionBern, "Bruno Muster", "bruno.muster", Rolle.Helfende);
        var clara = NeuesKonto(Stammdaten.RegionBern, "Clara Fiktiv", "clara.fiktiv", Rolle.Helfende);
        var dora = NeuesKonto(Stammdaten.RegionBern, "Dora Probe", "dora.probe", Rolle.Koordination);

        // Region Mittelland
        var emil = NeuesKonto(Stammdaten.RegionMittelland, "Emil Exempel", "emil.exempel", Rolle.Hilfesuchende);
        var fritz = NeuesKonto(Stammdaten.RegionMittelland, "Fritz Platzhalter", "fritz.platzhalter", Rolle.Helfende);
        var greta = NeuesKonto(Stammdaten.RegionMittelland, "Greta Muster", "greta.muster", Rolle.Mandantenadministration);

        // Plattformbetrieb ohne Region
        var hans = NeuesKonto(null, "Hans Betrieb", "hans.betrieb", Rolle.Plattformbetrieb);

        datenbank.Konten.AddRange(anna, bruno, clara, dora, emil, fritz, greta, hans);

        datenbank.Helferprofile.AddRange(
            new Helferprofil
            {
                KontoId = bruno.Id,
                RegionId = Stammdaten.RegionBern,
                Postleitzahl = "3012",
                Ort = "Bern",
                EinsatzradiusKm = 15,
                Skills = [skills["sanitaer"], skills["elektro"]],
            },
            new Helferprofil
            {
                KontoId = clara.Id,
                RegionId = Stammdaten.RegionBern,
                Postleitzahl = "3014",
                Ort = "Bern",
                EinsatzradiusKm = 5,
                Skills = [skills["einkauf"], skills["termin"]],
            },
            new Helferprofil
            {
                KontoId = fritz.Id,
                RegionId = Stammdaten.RegionMittelland,
                Postleitzahl = "3110",
                Ort = "Münsingen",
                EinsatzradiusKm = 20,
                Skills = [skills["computer"]],
            });

        datenbank.Anfragen.AddRange(
            new Anfrage
            {
                RegionId = Stammdaten.RegionBern,
                KontoId = anna.Id,
                KategorieId = Stammdaten.KategorieHandwerk,
                Freitext = "Der Wasserhahn in der Küche tropft stark.",
                Postleitzahl = "3011",
                ErstelltAm = jetzt,
            },
            new Anfrage
            {
                RegionId = Stammdaten.RegionMittelland,
                KontoId = emil.Id,
                Freitext = "Ich brauche Hilfe beim Einrichten meines neuen Smartphones.",
                Postleitzahl = "3098",
                ErstelltAm = jetzt,
            });

        await datenbank.SaveChangesAsync(abbruch);
    }
}
