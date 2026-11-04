namespace Soforthilfe.Dienste.Kategorien;

/// <summary>
/// Kategorien lesen. Kostenlose und kostenpflichtige Angebote sind getrennt abrufbar (Anforderung A12).
/// </summary>
public interface IKategorienDienst
{
    // Ohne Angabe alle Kategorien, sonst nur kostenlose oder nur kostenpflichtige.
    Task<IReadOnlyList<KategorieInfo>> ListeAsync(bool? kostenpflichtig, CancellationToken abbruch);
}
