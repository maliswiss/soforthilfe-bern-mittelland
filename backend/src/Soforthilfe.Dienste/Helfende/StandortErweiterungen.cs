using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Soforthilfe.Datenzugriff;
using Soforthilfe.Datenzugriff.Mandanten;
using Soforthilfe.Dienste.Orte;

namespace Soforthilfe.Dienste.Helfende;

/// <summary>
/// Ergänzt beim Start den Punkt der Postleitzahl bei Helferprofilen ohne Standort, zum Beispiel bei Testdaten oder
/// Profilen, die vor dem Einlesen des Ortschaftenverzeichnisses entstanden sind. Ohne Punkt findet die
/// Kandidatensuche ein Profil nur über dieselbe Postleitzahl.
/// </summary>
public static class StandortErweiterungen
{
    public static async Task ErgaenzeStandorteAsync(this IServiceProvider dienste, CancellationToken abbruch = default)
    {
        await using var bereich = dienste.CreateAsyncScope();
        var logger = bereich.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Standorte");

        // Betrifft alle Regionen.
        bereich.ServiceProvider.GetRequiredService<IMandantenKontext>().SetzeUebergreifend();
        var datenbank = bereich.ServiceProvider.GetRequiredService<SoforthilfeDbContext>();
        var orte = bereich.ServiceProvider.GetRequiredService<IOrtDienst>();

        try
        {
            var ohneStandort = await datenbank.Helferprofile.Where(p => p.Standort == null).ToListAsync(abbruch);
            var ergaenzt = 0;
            foreach (var profil in ohneStandort)
            {
                profil.Standort = await orte.PunktZuPlzAsync(profil.Postleitzahl, profil.Ort, abbruch);
                ergaenzt += profil.Standort is null ? 0 : 1;
            }

            if (ergaenzt > 0)
            {
                await datenbank.SaveChangesAsync(abbruch);
                logger.LogInformation("Standort bei {Anzahl} Helferprofilen aus der Postleitzahl ergänzt.", ergaenzt);
            }
        }
        catch (Exception fehler) when (fehler is not OperationCanceledException)
        {
            // Ohne Datenbank startet die Schnittstelle trotzdem; die Zustandsprüfung meldet den Fehler.
            logger.LogError(fehler, "Standorte der Helferprofile konnten nicht ergänzt werden.");
        }
    }
}
