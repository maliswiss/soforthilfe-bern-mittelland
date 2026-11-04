using Microsoft.EntityFrameworkCore;
using Soforthilfe.Datenzugriff;

namespace Soforthilfe.Dienste.Kategorien;

/// <summary>
/// Liest die Kategorien aus den Stammdaten; kostenlose stehen vor kostenpflichtigen.
/// </summary>
public sealed class KategorienDienst(SoforthilfeDbContext datenbank) : IKategorienDienst
{
    public async Task<IReadOnlyList<KategorieInfo>> ListeAsync(bool? kostenpflichtig, CancellationToken abbruch)
    {
        var abfrage = datenbank.Kategorien.Include(k => k.Skills).AsNoTracking();

        if (kostenpflichtig is bool wert)
        {
            abfrage = abfrage.Where(k => k.Kostenpflichtig == wert);
        }

        var kategorien = await abfrage
            .OrderBy(k => k.Kostenpflichtig)
            .ThenBy(k => k.Bezeichnung)
            .ToListAsync(abbruch);

        return kategorien
            .Select(k => new KategorieInfo(
                k.Id,
                k.Kennung,
                k.Bezeichnung,
                k.Kostenpflichtig,
                k.Skills.OrderBy(s => s.Bezeichnung).Select(s => s.Bezeichnung).ToList()))
            .ToList();
    }
}
