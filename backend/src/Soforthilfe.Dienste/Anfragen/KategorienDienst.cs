using Microsoft.EntityFrameworkCore;
using Soforthilfe.Datenzugriff;

namespace Soforthilfe.Dienste.Anfragen;

public sealed record KategorieInfo(Guid Id, string Kennung, string Bezeichnung, bool Kostenpflichtig);

public sealed class KategorienDienst(SoforthilfeDbContext datenbank)
{
    public async Task<IReadOnlyList<KategorieInfo>> ListeAsync(bool? kostenpflichtig, CancellationToken abbruch)
    {
        var abfrage = datenbank.Kategorien.AsNoTracking();
        if (kostenpflichtig.HasValue) abfrage = abfrage.Where(k => k.Kostenpflichtig == kostenpflichtig.Value);
        return await abfrage.OrderBy(k => k.Kostenpflichtig).ThenBy(k => k.Bezeichnung)
            .Select(k => new KategorieInfo(k.Id, k.Kennung, k.Bezeichnung, k.Kostenpflichtig)).ToListAsync(abbruch);
    }
}
