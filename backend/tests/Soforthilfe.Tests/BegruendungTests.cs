using Soforthilfe.Datenzugriff.Modell;
using Soforthilfe.Dienste.Matching;

namespace Soforthilfe.Tests;

/// <summary>
/// Begründung der Vorschläge: Text aus Distanz, Fähigkeit, Bewertung, Verifizierung, Verfügbarkeit und Einsätzen
/// sowie die Beiträge aller Kriterien zur Punktzahl.
/// </summary>
public sealed class BegruendungTests
{
    private static Helferprofil Profil(int bewertungen = 0, double schnitt = 0, bool bereit = true, int einsaetze = 0) => new()
    {
        Postleitzahl = "3012",
        Ort = "Bern",
        AnzahlBewertungen = bewertungen,
        Bewertungsschnitt = schnitt,
        Einsatzbereit = bereit,
        AnzahlEinsaetze = einsaetze,
    };

    [Fact]
    public void Neue_Helfende_ohne_Bewertungen_und_Einsaetze()
    {
        var text = Begruendung.Text(Profil(), 1460, "Nachbarschaftshilfe", false);

        Assert.Equal(
            "1.5 km entfernt · Fähigkeit «Nachbarschaftshilfe» · noch keine Bewertungen · verifiziert · jetzt verfügbar · noch keine Einsätze",
            text);
    }

    [Fact]
    public void Mit_Bewertungen_Einsaetzen_und_Partner()
    {
        var text = Begruendung.Text(Profil(bewertungen: 12, schnitt: 4.83, bereit: false, einsaetze: 7), 0, "Schlüsseldienst", true);

        Assert.Equal(
            "gleiche Postleitzahl · Fähigkeit «Schlüsseldienst» · Bewertung 4.8 von 5 (12 Bewertungen) · verifiziert · "
            + "derzeit nicht verfügbar · 7 Einsätze · Partner, bei gleichwertiger Punktzahl vorgezogen",
            text);
    }

    [Fact]
    public void Einzahl_bei_einer_Bewertung_und_einem_Einsatz()
    {
        var text = Begruendung.Text(Profil(bewertungen: 1, schnitt: 5, einsaetze: 1), 800, "Begleitung", false);

        Assert.Contains("Bewertung 5.0 von 5 (1 Bewertung)", text);
        Assert.EndsWith("· 1 Einsatz", text);
    }

    [Theory]
    [InlineData(0, "gleiche Postleitzahl")]
    [InlineData(420.4, "420 m entfernt")]
    [InlineData(999.6, "1.0 km entfernt")]
    [InlineData(1000, "1.0 km entfernt")]
    [InlineData(2250, "2.3 km entfernt")]
    [InlineData(11796, "11.8 km entfernt")]
    public void Distanz_unter_einem_Kilometer_in_Metern(double meter, string erwartet) =>
        Assert.Equal(erwartet, Begruendung.Distanz(meter));

    [Fact]
    public void Kriterien_nennen_Teilpunkte_Gewicht_und_Beitrag_in_fester_Reihenfolge()
    {
        var teilpunkte = new Teilpunkte(0.75, 1, 0.6, 1, 0.4);

        var kriterien = Begruendung.Kriterien(teilpunkte, MatchingGewichte.Standard);

        Assert.Equal(new[] { "distanz", "faehigkeit", "bewertung", "verfuegbarkeit", "erfahrung" },
            kriterien.Select(k => k.Kriterium));
        Assert.Equal(new KriteriumInfo("distanz", 0.75, 0.35, 0.2625), kriterien[0]);
        Assert.Equal(teilpunkte.Gewichtet(MatchingGewichte.Standard), kriterien.Sum(k => k.Beitrag), 6);
    }
}
