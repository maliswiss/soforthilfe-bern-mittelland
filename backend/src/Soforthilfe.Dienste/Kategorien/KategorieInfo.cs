namespace Soforthilfe.Dienste.Kategorien;

/// <summary>
/// Kategorie, wie sie die Startseite zur Auswahl anzeigt. Code ist die feste Kennung, zum Beispiel «schluesseldienst».
/// </summary>
public sealed record KategorieInfo(
    Guid Id,
    string Code,
    string Bezeichnung,
    string? Beschreibung,
    bool Bezahlpflichtig,
    bool NurOrientierung,
    int Reihenfolge);

/// <summary>
/// Fähigkeit, die Helfende anbieten. Bezahlpflichtig wie die zugehörige Kategorie.
/// </summary>
public sealed record SkillInfo(Guid Id, string Code, string Bezeichnung, bool Bezahlpflichtig);
