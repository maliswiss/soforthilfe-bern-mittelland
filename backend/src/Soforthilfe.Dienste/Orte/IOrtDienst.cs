namespace Soforthilfe.Dienste.Orte;

/// <summary>
/// Zuordnung von Postleitzahl, Ortschaft, Gemeinde und Region über das amtliche Ortschaftenverzeichnis.
/// </summary>
public interface IOrtDienst
{
    /// <summary>Alle Ortschaften zu einer Postleitzahl, leer bei unbekannter Postleitzahl.</summary>
    Task<IReadOnlyList<OrtInfo>> NachPlzAsync(string plz, CancellationToken abbruch);

    /// <summary>Prüft, ob eine Postleitzahl zur angegebenen Region passt.</summary>
    Task<PlzPruefung> PruefeFuerRegionAsync(string plz, Guid regionId, CancellationToken abbruch);
}
