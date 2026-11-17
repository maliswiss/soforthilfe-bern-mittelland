namespace Soforthilfe.Datenzugriff.Modell;

/// <summary>
/// Über die Schnittstelle importierte Sprache der Oberfläche. Deutsch und Französisch liefert die Oberfläche selbst
/// mit; ein Eintrag mit ihrem Code ergänzt oder ersetzt einzelne Texte. Gilt für alle Regionen.
/// </summary>
public sealed class Sprache
{
    // Zwei Buchstaben nach ISO 639-1, zum Beispiel «it».
    public required string Code { get; set; }

    // Name in der Sprache selbst, zum Beispiel «Italiano».
    public required string Name { get; set; }

    // Nur aktive Sprachen erscheinen im Umschalter.
    public bool Aktiv { get; set; }

    // Texte als JSON-Objekt mit derselben Gliederung wie die Sprachdateien der Oberfläche.
    public required string Texte { get; set; }

    public int AnzahlTexte { get; set; }

    public DateTimeOffset GeaendertAm { get; set; }

    // Konto des Plattformbetriebs, das zuletzt importiert hat.
    public Guid? GeaendertVonId { get; set; }
}
