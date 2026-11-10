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

    // Nur für den Plattformbetrieb: Zugriff auf alle Regionen.
    void SetzeUebergreifend();

    // Legt eine Region mit ihren Gemeinden (BFS-Nummern) an. Kennung und Gemeinden dürfen noch nicht vergeben sein.
    Task<RegionErgebnis> AnlegenAsync(string kennung, string name, IReadOnlyCollection<int> gemeinden, CancellationToken abbruch);
}
