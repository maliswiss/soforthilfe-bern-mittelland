using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Soforthilfe.Datenzugriff;
using Soforthilfe.Datenzugriff.Mandanten;
using Soforthilfe.Datenzugriff.Modell;
using Soforthilfe.Datenzugriff.Ortschaften;

namespace Soforthilfe.Dienste.Orte;

/// <summary>
/// Liest das amtliche Ortschaftenverzeichnis beim Start ein, wenn die Tabelle noch leer ist.
/// Es sind Referenzdaten und keine Testdaten; sie werden darum in jeder Umgebung geladen.
/// </summary>
public static class OrtschaftenErweiterungen
{
    public const string AktionEingelesen = "Ortschaften.Eingelesen";

    // Sperre in der Datenbank, damit bei mehreren Instanzen nur eine einliest.
    private const string Sperre = "SELECT pg_advisory_xact_lock(262026)";

    public static async Task LeseOrtschaftenEinAsync(this IServiceProvider dienste, IConfiguration konfiguration,
        CancellationToken abbruch = default)
    {
        await using var bereich = dienste.CreateAsyncScope();
        var logger = bereich.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Ortschaftenverzeichnis");

        // Das Verzeichnis gilt für alle Regionen.
        bereich.ServiceProvider.GetRequiredService<IMandantenKontext>().SetzeUebergreifend();
        var datenbank = bereich.ServiceProvider.GetRequiredService<SoforthilfeDbContext>();

        try
        {
            await using var transaktion = await datenbank.Database.BeginTransactionAsync(abbruch);
            await datenbank.Database.ExecuteSqlRawAsync(Sperre, abbruch);

            if (await datenbank.Ortschaften.AnyAsync(abbruch))
            {
                return;
            }

            var datei = Ortschaftenverzeichnis.DateiFinden(konfiguration);
            if (datei is null)
            {
                logger.LogWarning("Das Ortschaftenverzeichnis wurde nicht gefunden; Postleitzahlen werden nicht geprüft.");
                return;
            }

            var ortschaften = Ortschaftenverzeichnis.Lesen(datei);
            datenbank.Ortschaften.AddRange(ortschaften);
            datenbank.Protokoll.Add(new Protokolleintrag
            {
                Aktion = AktionEingelesen,
                Objekt = "Ortschaften",
                Detail = $"{ortschaften.Count} Zeilen aus {Path.GetFileName(datei)}",
                Zeitpunkt = DateTimeOffset.UtcNow,
            });
            await datenbank.SaveChangesAsync(abbruch);
            await transaktion.CommitAsync(abbruch);

            logger.LogInformation("{Anzahl} Zeilen des Ortschaftenverzeichnisses aus {Datei} eingelesen.",
                ortschaften.Count, Path.GetFileName(datei));
        }
        catch (Exception fehler) when (fehler is not OperationCanceledException)
        {
            // Ohne Verzeichnis startet die Schnittstelle trotzdem; die Zustandsprüfung meldet eine fehlende Datenbank.
            logger.LogError(fehler, "Das Ortschaftenverzeichnis konnte nicht eingelesen werden.");
        }
    }
}
