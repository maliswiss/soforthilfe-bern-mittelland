namespace Soforthilfe.Dienste.Anfragen;

/// <summary>
/// Eine Anfrage, wie sie die Schnittstelle zurückgibt.
/// </summary>
public sealed record AnfrageInfo(
    Guid Id,
    string? KategorieKennung,
    string? Kategorie,
    bool? Kostenpflichtig,
    string? Freitext,
    string Postleitzahl,
    string Status,
    DateTimeOffset ErstelltAm);

/// <summary>
/// Daten einer neuen Anfrage: Kategorie, Freitext oder beides.
/// </summary>
public sealed record AnfrageDaten(Guid KontoId, Guid? KategorieId, string? Freitext, string Postleitzahl);

/// <summary>
/// Gründe, warum eine Anfrage abgelehnt wird.
/// </summary>
public enum AnfrageFehler
{
    RegionFehlt,
    InhaltFehlt,
    PostleitzahlUngueltig,
    KategorieUnbekannt,
}

/// <summary>
/// Ergebnis beim Erfassen: entweder eine Anfrage oder ein Fehler.
/// </summary>
public sealed record AnfrageErgebnis(AnfrageInfo? Anfrage, AnfrageFehler? Fehler);
