namespace Soforthilfe.Dienste.Kategorien;

/// <summary>
/// Kategorie mit Fähigkeiten, wie sie die Oberfläche zur Auswahl anzeigt.
/// </summary>
public sealed record KategorieInfo(Guid Id, string Kennung, string Bezeichnung, bool Kostenpflichtig, IReadOnlyList<string> Skills);
