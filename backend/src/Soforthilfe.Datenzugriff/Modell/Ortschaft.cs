using NetTopologySuite.Geometries;

namespace Soforthilfe.Datenzugriff.Modell;

/// <summary>
/// Zeile des amtlichen Ortschaftenverzeichnisses der Schweiz (Quelle: swisstopo).
/// Eine Ortschaft (Postleitzahl und Zusatzziffer) kann mehrere Gemeinden berühren; jede Gemeinde ist eine eigene Zeile
/// mit ihrem Anteil an den Adressen. Referenzdaten für alle Regionen, darum ohne Region.
/// </summary>
public sealed class Ortschaft
{
    public int Id { get; set; }

    // Vierstellige Postleitzahl, zum Beispiel «3011».
    public required string Postleitzahl { get; set; }

    // Zusatzziffer, die mehrere Ortschaften mit gleicher Postleitzahl unterscheidet, zum Beispiel «00».
    public required string Zusatzziffer { get; set; }

    // Ortschaftsname, zum Beispiel «Schliern b. Köniz».
    public required string Name { get; set; }

    public required string Gemeinde { get; set; }

    // Gemeindenummer des Bundesamts für Statistik.
    public int BfsNummer { get; set; }

    public required string Kanton { get; set; }

    // Anteil der Adressen dieser Ortschaft, die in der Gemeinde liegen, in Prozent.
    public double Adressenanteil { get; set; }

    // Amtssprache der Ortschaft: de, fr, it, rm oder multiple.
    public required string Sprache { get; set; }

    // Repräsentativer Punkt in WGS84 (SRID 4326).
    public required Point Standort { get; set; }
}
