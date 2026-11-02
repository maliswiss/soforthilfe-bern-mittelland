using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Soforthilfe.Datenzugriff;
using Soforthilfe.Datenzugriff.Mandanten;
using Soforthilfe.Datenzugriff.Testdaten;
using Soforthilfe.Dienste.Konten;

namespace Soforthilfe.Dienste;

/// <summary>
/// Lädt beim Start die Testdaten mit fiktiven Personen, wenn das ausdrücklich eingeschaltet ist.
/// </summary>
public static class TestdatenErweiterungen
{
    public static async Task LadeTestdatenAsync(this IServiceProvider dienste, string? testpasswort)
    {
        // Eigener Gültigkeitsbereich, weil der Datenbankkontext pro Anfrage lebt.
        await using var bereich = dienste.CreateAsyncScope();

        // Testdaten betreffen alle Regionen, darum regionsübergreifend schreiben.
        bereich.ServiceProvider.GetRequiredService<IMandantenKontext>().SetzeUebergreifend();

        var datenbank = bereich.ServiceProvider.GetRequiredService<SoforthilfeDbContext>();
        await TestdatenLader.LadenAsync(datenbank);

        // Ohne Passwort können sich die Testkonten nicht anmelden. Das Passwort kommt nur aus der
        // Umgebung (TESTDATEN_PASSWORT) und gilt nur für die fiktiven Konten auf «beispiel.invalid».
        if (string.IsNullOrWhiteSpace(testpasswort) || testpasswort.StartsWith('<') || testpasswort.Length < 10)
        {
            return;
        }

        var passwortDienst = bereich.ServiceProvider.GetRequiredService<PasswortDienst>();
        var ohnePasswort = await datenbank.Konten
            .Where(k => k.PasswortHash == null && k.EMail.EndsWith("@beispiel.invalid"))
            .ToListAsync();

        foreach (var konto in ohnePasswort)
        {
            konto.PasswortHash = passwortDienst.Hash(testpasswort);
        }

        await datenbank.SaveChangesAsync();
    }
}
