namespace Soforthilfe.Dienste.Anfragen;

/// <summary>
/// Anfragen erfassen und lesen. Beim Erfassen ordnet die Verständniskette die Anfrage ein, danach erzeugt das
/// Smart-Matching die ersten Vorschläge.
/// </summary>
public interface IAnfrageDienst
{
    Task<AnfrageErgebnis> ErfassenAsync(AnfrageDaten daten, CancellationToken abbruch);

    // nurVon gesetzt: nur Anfragen dieses Kontos; sonst alle Anfragen der aktuellen Region.
    Task<IReadOnlyList<AnfrageInfo>> ListeAsync(Guid? nurVon, CancellationToken abbruch);

    Task<AnfrageInfo?> LiesAsync(Guid id, Guid? nurVon, CancellationToken abbruch);
}
