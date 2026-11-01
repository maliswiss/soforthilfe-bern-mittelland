namespace Soforthilfe.Datenzugriff.Modell;

/// <summary>
/// Kategorie einer Anfrage, zum Beispiel Handwerk oder Begleitung.
/// </summary>
public sealed class Kategorie
{
    public Guid Id { get; set; }

    public required string Kennung { get; set; }

    public required string Bezeichnung { get; set; }

    // Kostenpflichtige und kostenlose Angebote werden strikt getrennt.
    public bool Kostenpflichtig { get; set; }

    public List<Skill> Skills { get; set; } = [];
}
