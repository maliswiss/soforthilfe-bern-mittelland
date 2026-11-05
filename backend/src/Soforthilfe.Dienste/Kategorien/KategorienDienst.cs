using Microsoft.EntityFrameworkCore;
using Soforthilfe.Datenzugriff;

namespace Soforthilfe.Dienste.Kategorien;

/// <summary>
/// Liest Kategorien und Fähigkeiten aus den Stammdaten.
/// </summary>
public sealed class KategorienDienst(SoforthilfeDbContext datenbank) : IKategorienDienst
{
    public async Task<IReadOnlyList<KategorieInfo>> ListeAsync(bool? bezahlpflichtig, CancellationToken abbruch)
    {
        var abfrage = datenbank.Kategorien.AsNoTracking();

        if (bezahlpflichtig is bool wert)
        {
            abfrage = abfrage.Where(k => k.Kostenpflichtig == wert);
        }

        return await abfrage
            .OrderBy(k => k.Reihenfolge)
            .Select(k => new KategorieInfo(k.Id, k.Kennung, k.Bezeichnung, null, k.Kostenpflichtig, k.NurOrientierung, k.Reihenfolge))
            .ToListAsync(abbruch);
    }

    public async Task<IReadOnlyList<SkillInfo>> SkillsAsync(CancellationToken abbruch)
    {
        return await datenbank.Skills
            .AsNoTracking()
            .OrderBy(s => s.Kategorie!.Reihenfolge)
            .Select(s => new SkillInfo(s.Id, s.Kennung, s.Bezeichnung, s.Kategorie!.Kostenpflichtig))
            .ToListAsync(abbruch);
    }
}
