using Microsoft.Extensions.DependencyInjection;
using Soforthilfe.Datenzugriff;
using Soforthilfe.Datenzugriff.Testdaten;

namespace Soforthilfe.Dienste;

/// <summary>
/// Lädt beim Start die Testdaten mit fiktiven Personen, wenn das ausdrücklich eingeschaltet ist.
/// </summary>
public static class TestdatenErweiterungen
{
    public static async Task LadeTestdatenAsync(this IServiceProvider dienste)
    {
        // Eigener Gültigkeitsbereich, weil der Datenbankkontext pro Anfrage lebt.
        await using var bereich = dienste.CreateAsyncScope();
        var datenbank = bereich.ServiceProvider.GetRequiredService<SoforthilfeDbContext>();
        await TestdatenLader.LadenAsync(datenbank);
    }
}
