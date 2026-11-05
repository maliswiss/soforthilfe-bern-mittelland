namespace Soforthilfe.Dienste.Kategorien;

/// <summary>
/// Kategorien und Fähigkeiten lesen. Kostenlose und kostenpflichtige Angebote sind getrennt abrufbar (Anforderung A12).
/// </summary>
public interface IKategorienDienst
{
    // Ohne Angabe alle Kategorien, sonst nur kostenlose oder nur kostenpflichtige; Reihenfolge wie auf der Startseite.
    Task<IReadOnlyList<KategorieInfo>> ListeAsync(bool? bezahlpflichtig, CancellationToken abbruch);

    Task<IReadOnlyList<SkillInfo>> SkillsAsync(CancellationToken abbruch);
}
