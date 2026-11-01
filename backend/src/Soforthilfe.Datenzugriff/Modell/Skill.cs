namespace Soforthilfe.Datenzugriff.Modell;

/// <summary>
/// Fähigkeit innerhalb einer Kategorie, zum Beispiel «Sanitär» in der Kategorie Handwerk.
/// </summary>
public sealed class Skill
{
    public Guid Id { get; set; }

    public Guid KategorieId { get; set; }

    public Kategorie? Kategorie { get; set; }

    public required string Kennung { get; set; }

    public required string Bezeichnung { get; set; }
}
