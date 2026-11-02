namespace Soforthilfe.Dienste.Regionen;

/// <summary>
/// Regionen (Mandanten) auflisten, für die laufende Anfrage festlegen und neu anlegen.
/// </summary>
public interface IRegionDienst
{
    // Region der laufenden Anfrage, falls festgelegt.
    RegionInfo? Aktuelle { get; }

    Task<IReadOnlyList<RegionInfo>> ListeAktiveAsync(CancellationToken abbruch);

    // Legt die Region der laufenden Anfrage fest. Liefert null bei unbekannter oder inaktiver Region.
    Task<RegionInfo?> SetzeAktuelleAsync(string kennung, CancellationToken abbruch);

    // Liefert null, wenn die Kennung schon vergeben ist.
    Task<RegionInfo?> AnlegenAsync(string kennung, string name, CancellationToken abbruch);
}
