namespace Soforthilfe.Dienste.Protokoll;

/// <summary>
/// Schreibt und liest das Protokoll sicherheitsrelevanter Vorgänge. Einträge enthalten keine Personendaten.
/// </summary>
public interface IProtokollDienst
{
    /// <summary>Schreibt einen Eintrag in der Region der laufenden Anfrage.</summary>
    Task SchreibenAsync(string aktion, string? objekt, string? objektId, string? detail, CancellationToken abbruch);

    /// <summary>Liest die neuesten Einträge der eigenen Region, für den Plattformbetrieb aller Regionen.</summary>
    Task<IReadOnlyList<ProtokollInfo>> ListeAsync(string? aktion, int anzahl, CancellationToken abbruch);
}
