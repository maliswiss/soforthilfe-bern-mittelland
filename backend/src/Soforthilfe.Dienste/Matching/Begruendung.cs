using System.Globalization;
using Soforthilfe.Datenzugriff.Modell;

namespace Soforthilfe.Dienste.Matching;

/// <summary>
/// Beitrag eines Kriteriums zur Punktzahl: Teilpunkte mal Gewicht. Die Summe der Beiträge ist die Punktzahl.
/// Kriterien: distanz, faehigkeit, bewertung, verfuegbarkeit, erfahrung.
/// </summary>
public sealed record KriteriumInfo(string Kriterium, double Teilpunkte, double Gewicht, double Beitrag);

/// <summary>
/// Nachvollziehbare Begründung eines Vorschlags aus messbaren Grössen: Distanz, Fähigkeit, Bewertung, Verifizierung,
/// Verfügbarkeit und Einsätze. Der Text ist deutsch und wird beim Vorschlag gespeichert; die Kriterien mit Teilpunkten
/// erlauben der Oberfläche eine Darstellung in jeder Sprache.
/// </summary>
public static class Begruendung
{
    private static readonly CultureInfo Zahlen = CultureInfo.InvariantCulture;

    public static string Text(Helferprofil profil, double distanzMeter, string kategorie, bool partnerBevorzugt)
    {
        var teile = new List<string>
        {
            Distanz(distanzMeter),
            $"Fähigkeit «{kategorie}»",
            profil.AnzahlBewertungen switch
            {
                0 => "noch keine Bewertungen",
                1 => $"Bewertung {Schnitt(profil)} von 5 (1 Bewertung)",
                var n => $"Bewertung {Schnitt(profil)} von 5 ({n} Bewertungen)",
            },
            "verifiziert",
            profil.Einsatzbereit ? "jetzt verfügbar" : "derzeit nicht verfügbar",
            profil.AnzahlEinsaetze switch
            {
                0 => "noch keine Einsätze",
                1 => "1 Einsatz",
                var n => $"{n} Einsätze",
            },
        };

        if (partnerBevorzugt)
        {
            teile.Add("Partner, bei gleichwertiger Punktzahl vorgezogen");
        }

        return string.Join(" · ", teile);
    }

    /// <summary>
    /// Distanz zwischen den Punkten der Postleitzahlen, unter 1 km in Metern. Gerundet wird wie bei der gespeicherten
    /// Distanz zuerst auf ganze Meter, damit Text und Zahl übereinstimmen.
    /// </summary>
    public static string Distanz(double meter)
    {
        var ganz = Math.Round(meter);
        return ganz switch
        {
            <= 0 => "gleiche Postleitzahl",
            < 1000 => $"{ganz.ToString("0", Zahlen)} m entfernt",
            // Über ganze Hundert Meter, damit halbe Werte wie 2250 m sicher aufgerundet werden.
            _ => $"{(Math.Round(ganz / 100, MidpointRounding.AwayFromZero) / 10).ToString("0.0", Zahlen)} km entfernt",
        };
    }

    /// <summary>Teilpunkte, Gewicht und Beitrag je Kriterium in fester Reihenfolge.</summary>
    public static IReadOnlyList<KriteriumInfo> Kriterien(Teilpunkte t, MatchingGewichte g) =>
    [
        Kriterium("distanz", t.Distanz, g.Distanz),
        Kriterium("faehigkeit", t.Faehigkeit, g.Faehigkeit),
        Kriterium("bewertung", t.Bewertung, g.Bewertung),
        Kriterium("verfuegbarkeit", t.Verfuegbarkeit, g.Verfuegbarkeit),
        Kriterium("erfahrung", t.Erfahrung, g.Erfahrung),
    ];

    private static KriteriumInfo Kriterium(string name, double teilpunkte, double gewicht) =>
        new(name, teilpunkte, Math.Round(gewicht, 4), Math.Round(teilpunkte * gewicht, 4));

    private static string Schnitt(Helferprofil profil) => Math.Round(profil.Bewertungsschnitt, 1).ToString("0.0", Zahlen);
}
