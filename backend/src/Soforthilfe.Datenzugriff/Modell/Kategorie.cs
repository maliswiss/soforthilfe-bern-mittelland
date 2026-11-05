namespace Soforthilfe.Datenzugriff.Modell;

/// <summary>
/// Kategorie einer Anfrage, zum Beispiel Schlüsseldienst oder Nachbarschaftshilfe.
/// </summary>
public sealed class Kategorie
{
    public Guid Id { get; set; }

    public required string Kennung { get; set; }

    public required string Bezeichnung { get; set; }

    // Kostenpflichtige und kostenlose Angebote werden strikt getrennt.
    public bool Kostenpflichtig { get; set; }

    // Gesundheitliche Anliegen: nur Orientierung, Transport und Begleitung, keine medizinischen Leistungen.
    public bool NurOrientierung { get; set; }

    // Reihenfolge auf der Startseite.
    public int Reihenfolge { get; set; }

    public List<Skill> Skills { get; set; } = [];
}
