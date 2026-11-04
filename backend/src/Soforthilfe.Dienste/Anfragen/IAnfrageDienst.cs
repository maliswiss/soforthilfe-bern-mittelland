namespace Soforthilfe.Dienste.Anfragen;

/// <summary>
/// Anfragen erfassen und lesen. Die Verständniskette (Regeln, Klassifikator, Sprachmodell) ist hier noch nicht beteiligt.
/// </summary>
public interface IAnfrageDienst
{
    Task<AnfrageErgebnis> ErfassenAsync(AnfrageDaten daten, CancellationToken abbruch);

    // nurVon gesetzt: nur Anfragen dieses Kontos; sonst alle Anfragen der aktuellen Region.
    Task<IReadOnlyList<AnfrageInfo>> ListeAsync(Guid? nurVon, CancellationToken abbruch);

    Task<AnfrageInfo?> LiesAsync(Guid id, Guid? nurVon, CancellationToken abbruch);
}
