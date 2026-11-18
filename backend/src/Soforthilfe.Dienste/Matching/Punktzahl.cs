using Soforthilfe.Datenzugriff.Modell;

namespace Soforthilfe.Dienste.Matching;

/// <summary>
/// Teilpunkte je Kriterium, jeweils zwischen 0 und 1.
/// </summary>
public sealed record Teilpunkte(
    double Distanz,
    double Faehigkeit,
    double Bewertung,
    double Verfuegbarkeit,
    double Erfahrung)
{
    /// <summary>Summe der gewichteten Teilpunkte.</summary>
    public double Gewichtet(MatchingGewichte g) =>
        Distanz * g.Distanz
        + Faehigkeit * g.Faehigkeit
        + Bewertung * g.Bewertung
        + Verfuegbarkeit * g.Verfuegbarkeit
        + Erfahrung * g.Erfahrung;
}

/// <summary>
/// Kandidat mit Teilpunkten und Punktzahl. <c>PartnerBevorzugt</c>: Ein Partner wurde wegen gleichwertiger
/// Punktzahl vor jemanden ohne Partner-Abo gestellt.
/// </summary>
public sealed record BewerteterKandidat(Kandidat Kandidat, Teilpunkte Teilpunkte, double Punktzahl, bool PartnerBevorzugt = false);

/// <summary>
/// Punktzahl des Smart-Matchings aus Distanz, Fähigkeit, Bewertung, Verfügbarkeit und Erfahrung.
/// </summary>
public static class Punktzahl
{
    // Ohne Bewertungen ein mittlerer Wert, damit neue Helfende eine Chance haben.
    public const double OhneBewertungen = 0.6;

    // Wer gerade keine Anfragen annimmt, bleibt sichtbar, aber weiter hinten.
    public const double NichtVerfuegbar = 0.4;

    // Ab so vielen Einsätzen gibt es die vollen Teilpunkte für Erfahrung.
    public const int VolleErfahrungAbEinsaetzen = 10;

    // Punktzahlen mit höchstens diesem Abstand gelten als gleichwertig.
    public const double Gleichwertigkeitsspanne = 0.02;

    // Rundungsfehler bei Gleitkommazahlen sollen die Gleichwertigkeit nicht verhindern.
    private const double Toleranz = 1e-9;

    public const int AnzahlVorschlaege = 3;

    /// <summary>
    /// Teilpunkte eines Kandidaten. Die Fähigkeit ist Voraussetzung der Kandidatensuche und gibt darum volle Punkte.
    /// </summary>
    public static Teilpunkte Berechnen(Helferprofil profil, double distanzMeter, int suchradiusKm)
    {
        var radiusMeter = suchradiusKm * 1000.0;
        var distanz = radiusMeter <= 0 ? 1.0 : Math.Clamp(1.0 - distanzMeter / radiusMeter, 0, 1);
        var bewertung = profil.AnzahlBewertungen == 0 ? OhneBewertungen : Math.Clamp(profil.Bewertungsschnitt / 5.0, 0, 1);
        var verfuegbarkeit = profil.Einsatzbereit ? 1.0 : NichtVerfuegbar;
        var erfahrung = Math.Clamp(profil.AnzahlEinsaetze / (double)VolleErfahrungAbEinsaetzen, 0, 1);

        return new Teilpunkte(distanz, 1.0, bewertung, verfuegbarkeit, erfahrung);
    }

    public static BewerteterKandidat Bewerten(Kandidat kandidat, int suchradiusKm, MatchingGewichte gewichte)
    {
        var teilpunkte = Berechnen(kandidat.Profil, kandidat.DistanzMeter, suchradiusKm);
        return new BewerteterKandidat(kandidat, teilpunkte, teilpunkte.Gewichtet(gewichte));
    }

    /// <summary>
    /// Reihenfolge nach Punktzahl, bei gleicher Punktzahl nach Distanz und Kennung. Das Partner-Abo wirkt nur bei
    /// gleichwertiger Punktzahl: Ein Partner rückt dann vor Kandidaten ohne Partner-Abo und wird gekennzeichnet.
    /// </summary>
    public static IReadOnlyList<BewerteterKandidat> Ordnen(IEnumerable<BewerteterKandidat> kandidaten)
    {
        var liste = kandidaten
            .OrderByDescending(b => b.Punktzahl)
            .ThenBy(b => b.Kandidat.DistanzMeter)
            .ThenBy(b => b.Kandidat.Profil.Id)
            .ToList();

        for (var i = 1; i < liste.Count; i++)
        {
            for (var j = i; j > 0 && VorziehenVor(liste[j], liste[j - 1]); j--)
            {
                (liste[j - 1], liste[j]) = (liste[j] with { PartnerBevorzugt = true }, liste[j - 1]);
            }
        }

        return liste;
    }

    private static bool VorziehenVor(BewerteterKandidat partner, BewerteterKandidat vorher) =>
        partner.Kandidat.Profil.Partner
        && !vorher.Kandidat.Profil.Partner
        && vorher.Punktzahl - partner.Punktzahl <= Gleichwertigkeitsspanne + Toleranz;
}
