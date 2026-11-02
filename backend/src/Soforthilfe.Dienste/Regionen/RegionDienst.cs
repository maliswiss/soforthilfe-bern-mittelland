using Microsoft.EntityFrameworkCore;
using Soforthilfe.Datenzugriff;
using Soforthilfe.Datenzugriff.Mandanten;
using Soforthilfe.Datenzugriff.Modell;

namespace Soforthilfe.Dienste.Regionen;

/// <summary>
/// Regionen sind Daten, keine Konfiguration: Eine neue Region braucht kein Deployment.
/// </summary>
public sealed class RegionDienst(SoforthilfeDbContext datenbank, IMandantenKontext mandant) : IRegionDienst
{
    public RegionInfo? Aktuelle { get; private set; }

    public async Task<IReadOnlyList<RegionInfo>> ListeAktiveAsync(CancellationToken abbruch)
    {
        return await datenbank.Regionen
            .AsNoTracking()
            .Where(r => r.Aktiv)
            .OrderBy(r => r.Name)
            .Select(r => new RegionInfo(r.Kennung, r.Name))
            .ToListAsync(abbruch);
    }

    public async Task<RegionInfo?> SetzeAktuelleAsync(string kennung, CancellationToken abbruch)
    {
        var normiert = Normiere(kennung);
        var region = await datenbank.Regionen
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Kennung == normiert && r.Aktiv, abbruch);

        if (region is null)
        {
            return null;
        }

        // Ab hier liefert der Datenzugriff nur noch Daten dieser Region.
        mandant.SetzeRegion(region.Id);
        Aktuelle = new RegionInfo(region.Kennung, region.Name);
        return Aktuelle;
    }

    public void SetzeUebergreifend() => mandant.SetzeUebergreifend();

    public async Task<RegionInfo?> AnlegenAsync(string kennung, string name, CancellationToken abbruch)
    {
        var normiert = Normiere(kennung);

        // Kennungen sind eindeutig; eine bestehende Region wird nie überschrieben.
        if (await datenbank.Regionen.AnyAsync(r => r.Kennung == normiert, abbruch))
        {
            return null;
        }

        var region = new Region { Id = Guid.NewGuid(), Kennung = normiert, Name = name.Trim(), Aktiv = true };
        datenbank.Regionen.Add(region);
        await datenbank.SaveChangesAsync(abbruch);

        return new RegionInfo(region.Kennung, region.Name);
    }

    private static string Normiere(string kennung) => kennung.Trim().ToLowerInvariant();
}
