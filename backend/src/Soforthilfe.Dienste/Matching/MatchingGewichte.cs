namespace Soforthilfe.Dienste.Matching;

/// <summary>
/// Gewichte der Kriterien im Smart-Matching, einstellbar im Abschnitt «Matching:Gewichte» der Konfiguration.
/// Die Gewichte werden auf die Summe 1 gebracht, damit die Punktzahl immer zwischen 0 und 1 liegt.
/// </summary>
public sealed record MatchingGewichte(
    double Distanz,
    double Faehigkeit,
    double Bewertung,
    double Verfuegbarkeit,
    double Erfahrung)
{
    /// <summary>Nähe zählt am meisten, danach Fähigkeit und Bewertung; Verfügbarkeit und Erfahrung ergänzen.</summary>
    public static MatchingGewichte Standard { get; } = new(0.35, 0.25, 0.20, 0.10, 0.10);

    public double Summe => Distanz + Faehigkeit + Bewertung + Verfuegbarkeit + Erfahrung;

    /// <summary>
    /// Gewichte mit Summe 1. Negative Gewichte oder lauter Nullen sind ein Konfigurationsfehler; die Schnittstelle
    /// startet dann nicht, statt mit unsinnigen Punktzahlen zu arbeiten.
    /// </summary>
    public MatchingGewichte Normalisiert()
    {
        double[] werte = [Distanz, Faehigkeit, Bewertung, Verfuegbarkeit, Erfahrung];
        if (werte.Any(w => w < 0 || double.IsNaN(w) || double.IsInfinity(w)) || Summe <= 0)
        {
            throw new ArgumentException(
                "Die Gewichte im Abschnitt «Matching:Gewichte» müssen positiv oder null sein, mindestens eines grösser als null.");
        }

        var summe = Summe;
        return new MatchingGewichte(Distanz / summe, Faehigkeit / summe, Bewertung / summe, Verfuegbarkeit / summe,
            Erfahrung / summe);
    }
}
