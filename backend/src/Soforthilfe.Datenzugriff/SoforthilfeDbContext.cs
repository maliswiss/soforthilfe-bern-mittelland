using Microsoft.EntityFrameworkCore;

namespace Soforthilfe.Datenzugriff;

/// <summary>
/// Datenbankkontext der Plattform (PostgreSQL mit PostGIS).
/// </summary>
public sealed class SoforthilfeDbContext(DbContextOptions<SoforthilfeDbContext> optionen) : DbContext(optionen)
{
    protected override void OnModelCreating(ModelBuilder modell)
    {
        // PostGIS wird über die Migrationen in der Datenbank aktiviert, nicht von Hand.
        modell.HasPostgresExtension("postgis");
    }
}
