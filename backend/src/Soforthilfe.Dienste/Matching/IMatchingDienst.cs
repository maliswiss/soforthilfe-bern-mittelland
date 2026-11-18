namespace Soforthilfe.Dienste.Matching;

/// <summary>
/// Smart-Matching: ordnet die Kandidaten einer Anfrage nach gewichteter Punktzahl und speichert die besten Vorschläge.
/// Benachrichtigt wird dabei niemand; die anfragende Person entscheidet, wen sie anfragt.
/// </summary>
public interface IMatchingDienst
{
    /// <summary>
    /// Ergänzt die Vorschläge einer offenen Anfrage um bis zu drei neue Helfende. Bestehende Vorschläge bleiben
    /// erhalten. Liefert die Anzahl neuer Vorschläge.
    /// </summary>
    Task<int> VorschlaegeErzeugenAsync(Guid anfrageId, CancellationToken abbruch);

    /// <summary>
    /// Vorschläge einer Anfrage nach Rang; null, wenn es die Anfrage nicht gibt oder sie nicht dem Konto gehört
    /// (nurVon gesetzt).
    /// </summary>
    Task<IReadOnlyList<VorschlagInfo>?> VorschlaegeAsync(Guid anfrageId, Guid? nurVon, CancellationToken abbruch);
}
