namespace Soforthilfe.Dienste.Verstaendnis;

/// <summary>
/// Verständnis-Schritt vor dem Erfassen einer Anfrage. Zuerst immer die Notfallprüfung;
/// die Einordnung in eine Kategorie folgt mit der Verständniskette.
/// </summary>
public interface IVerstaendnisDienst
{
    Task<VerstaendnisInfo> VerstehenAsync(Guid? kategorieId, string? freitext, CancellationToken abbruch);
}
