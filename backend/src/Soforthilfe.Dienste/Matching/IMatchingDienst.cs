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

    /// <summary>
    /// Unverbindliche Vorschau in der Region der laufenden Anfrage: dieselbe Suche und Punktzahl wie bei Vorschlägen,
    /// aber ohne Anfrage und ohne Speichern. Null bei unbekannter Kategorie oder ohne Region.
    /// </summary>
    Task<VorschauErgebnis?> VorschauAsync(VorschauKriterien kriterien, CancellationToken abbruch);
}
