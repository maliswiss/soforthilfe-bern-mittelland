using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Soforthilfe.Datenzugriff;
using Soforthilfe.Datenzugriff.Mandanten;

namespace Soforthilfe.Dienste;

/// <summary>
/// Bringt das Datenbankschema beim Start auf den Stand der Migrationen (Containerbetrieb).
/// </summary>
public static class DatenbankErweiterungen
{
    public static async Task MigriereDatenbankAsync(this IServiceProvider dienste, CancellationToken abbruch = default)
    {
        // Eigener Gültigkeitsbereich, weil der Datenbankkontext pro Anfrage lebt.
        await using var bereich = dienste.CreateAsyncScope();

        // Migrationen betreffen das ganze Schema, nicht eine einzelne Region.
        bereich.ServiceProvider.GetRequiredService<IMandantenKontext>().SetzeUebergreifend();

        var datenbank = bereich.ServiceProvider.GetRequiredService<SoforthilfeDbContext>();
        await datenbank.Database.MigrateAsync(abbruch);
    }
}
