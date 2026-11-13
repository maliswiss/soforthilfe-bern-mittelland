namespace Soforthilfe.Dienste.Verstaendnis;

/// <summary>
/// Verständnis-Schritt vor dem Erfassen einer Anfrage. Zuerst immer die Notfallprüfung,
/// danach die Einordnung in eine Kategorie.
/// </summary>
public interface IVerstaendnisDienst
{
    Task<VerstaendnisInfo> VerstehenAsync(Guid? kategorieId, string? freitext, CancellationToken abbruch);
}
