namespace Soforthilfe.Dienste.Anfragen;

/// <summary>
/// Eine Anfrage, wie sie die Schnittstelle zurückgibt. Prioritaet (0 tief, 1 mittel, 2 hoch) und Status
/// (1 offen, 2 Vorschläge vorhanden, 3 angenommen, 4 unterwegs, 5 erledigt, 6 abgebrochen) sind Zahlen.
/// </summary>
public sealed record AnfrageInfo(
    Guid Id,
    Guid? KategorieId,
    string? KategorieBezeichnung,
    string? Freitext,
    string Plz,
    string Ort,
    string? Adresse,
    string? Zeitfenster,
    int Prioritaet,
    int Status,
    DateTimeOffset ErstelltAm,
    bool Bezahlpflichtig);

/// <summary>
/// Daten einer neuen Anfrage: Kategorie, Freitext oder beides, dazu der Einsatzort.
/// </summary>
public sealed record AnfrageDaten(
    Guid KontoId,
    Guid? KategorieId,
    string? Freitext,
    string Plz,
    string Ort,
    string? Adresse,
    string? Zeitfenster,
    int Prioritaet);

/// <summary>
/// Gründe, warum eine Anfrage abgelehnt wird.
/// </summary>
public enum AnfrageFehler
{
    RegionFehlt,
    InhaltFehlt,
    PostleitzahlUngueltig,
    OrtFehlt,
    KategorieUnbekannt,
    PrioritaetUngueltig,
    Notfall,
}

/// <summary>
/// Ergebnis beim Erfassen: entweder eine Anfrage oder ein Fehler. Beim Notfall-Stopp nennt
/// «NotfallGrund» das erkannte Wort; die Anfrage wird dann nicht gespeichert.
/// </summary>
public sealed record AnfrageErgebnis(AnfrageInfo? Anfrage, AnfrageFehler? Fehler, string? NotfallGrund = null);
