using NetTopologySuite.Geometries;

namespace Soforthilfe.Dienste.Orte;

/// <summary>
/// Zuordnung von Postleitzahl, Ortschaft, Gemeinde und Region über das amtliche Ortschaftenverzeichnis.
/// </summary>
public interface IOrtDienst
{
    /// <summary>Alle Ortschaften zu einer Postleitzahl, leer bei unbekannter Postleitzahl.</summary>
    Task<IReadOnlyList<OrtInfo>> NachPlzAsync(string plz, CancellationToken abbruch);

    /// <summary>
    /// Nächste Ortschaft zu einem freigegebenen Standort, oder null, wenn keine Ortschaft näher als
    /// <see cref="OrtDienst.HoechstabstandMeter"/> liegt (zum Beispiel im Ausland). Der Standort wird nicht gespeichert.
    /// </summary>
    Task<OrtInfo?> NaechsteAsync(double breitengrad, double laengengrad, CancellationToken abbruch);

    /// <summary>
    /// Punkt einer Ortschaft aus dem Verzeichnis: die Ortschaft mit passendem Namen, sonst die mit dem grössten
    /// Anteil der Adressen; null bei unbekannter Postleitzahl.
    /// </summary>
    Task<Point?> PunktZuPlzAsync(string plz, string? ort, CancellationToken abbruch);

    /// <summary>Prüft, ob eine Postleitzahl zur angegebenen Region passt.</summary>
    Task<PlzPruefung> PruefeFuerRegionAsync(string plz, Guid regionId, CancellationToken abbruch);
}
