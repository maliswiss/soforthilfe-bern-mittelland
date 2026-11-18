namespace Soforthilfe.Datenzugriff.Modell;

/// <summary>
/// Vorschlag des Smart-Matchings: eine helfende Person zu einer Anfrage mit Rang, Punktzahl, Teilpunkten und den
/// Gewichten, mit denen die Punktzahl berechnet wurde. So bleibt jeder Vorschlag nachvollziehbar, auch wenn sich
/// Gewichte oder Profil später ändern.
/// </summary>
public sealed class Vorschlag
{
    public Guid Id { get; set; }

    public Guid RegionId { get; set; }

    public Guid AnfrageId { get; set; }

    public Anfrage? Anfrage { get; set; }

    public Guid HelferprofilId { get; set; }

    public Helferprofil? Helferprofil { get; set; }

    // 1 ist der beste Vorschlag.
    public int Rang { get; set; }

    // Summe der gewichteten Teilpunkte, zwischen 0 und 1.
    public double Punktzahl { get; set; }

    // Distanz zwischen den Punkten der Postleitzahlen, nie zwischen genauen Standorten.
    public int DistanzMeter { get; set; }

    // Teilpunkte je Kriterium, jeweils zwischen 0 und 1.
    public double TeilpunkteDistanz { get; set; }

    public double TeilpunkteFaehigkeit { get; set; }

    public double TeilpunkteBewertung { get; set; }

    public double TeilpunkteVerfuegbarkeit { get; set; }

    public double TeilpunkteErfahrung { get; set; }

    // Gewichte zum Zeitpunkt des Vorschlags, Summe 1.
    public double GewichtDistanz { get; set; }

    public double GewichtFaehigkeit { get; set; }

    public double GewichtBewertung { get; set; }

    public double GewichtVerfuegbarkeit { get; set; }

    public double GewichtErfahrung { get; set; }

    // Partner-Abo: nur bei gleichwertiger Punktzahl vorgezogen und dann gekennzeichnet.
    public bool PartnerBevorzugt { get; set; }

    public DateTimeOffset ErstelltAm { get; set; }
}
