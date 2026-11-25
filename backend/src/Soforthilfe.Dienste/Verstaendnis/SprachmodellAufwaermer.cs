using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Soforthilfe.Datenzugriff;

namespace Soforthilfe.Dienste.Verstaendnis;

/// <summary>
/// Lädt das Sprachmodell nach dem Start in den Speicher und lässt es die Anweisung einmal verarbeiten. Ohne diesen
/// Schritt wartet die erste unsichere Anfrage, bis Ollama das Modell von der Platte geladen und den gleichbleibenden
/// Anfang der Anweisung mit allen Kategorien gelesen hat. Läuft im Hintergrund, blockiert den Start nicht und ist
/// harmlos, wenn das Sprachmodell nicht erreichbar ist.
/// </summary>
public sealed class SprachmodellAufwaermer(
    IHttpClientFactory fabrik,
    IServiceScopeFactory bereiche,
    SprachmodellEinstellungen einstellungen,
    ILogger<SprachmodellAufwaermer> protokoll) : BackgroundService
{
    private const int MaxVersuche = 6;
    private static readonly TimeSpan Wartezeit = TimeSpan.FromSeconds(20);

    // Neutraler Text für das Vorladen; das Ergebnis wird nicht verwendet.
    private const string Vorlauftext = "Ich brauche Hilfe.";

    protected override async Task ExecuteAsync(CancellationToken abbruch)
    {
        if (!einstellungen.Eingeschaltet)
        {
            return;
        }

        try
        {
            for (var versuch = 1; versuch <= MaxVersuche; versuch++)
            {
                if (await LadenAsync(abbruch))
                {
                    protokoll.LogInformation("Sprachmodell {Modell} ist geladen.", einstellungen.Modell);
                    return;
                }

                await Task.Delay(Wartezeit, abbruch);
            }

            protokoll.LogWarning("Sprachmodell {Modell} konnte nicht vorgeladen werden.", einstellungen.Modell);
        }
        catch (OperationCanceledException)
        {
            // Die Schnittstelle wird beendet.
        }
    }

    // Ein Auftrag mit derselben Anweisung wie bei einer Anfrage, aber nur einem Token als Antwort.
    private async Task<bool> LadenAsync(CancellationToken abbruch)
    {
        try
        {
            IReadOnlyList<(string Kennung, string Bezeichnung)> kategorien;
            using (var bereich = bereiche.CreateScope())
            {
                var datenbank = bereich.ServiceProvider.GetRequiredService<SoforthilfeDbContext>();
                kategorien = OllamaSprachmodell.KategorienFuerAnweisung(
                    await datenbank.Kategorien.AsNoTracking().ToListAsync(abbruch));
            }

            var klient = fabrik.CreateClient(OllamaSprachmodell.KlientName);
            // Das erste Laden eines Modells dauert länger als eine Anfrage.
            klient.Timeout = TimeSpan.FromMinutes(5);
            using var antwort = await klient.PostAsJsonAsync("api/generate", OllamaSprachmodell.Rumpf(einstellungen,
                OllamaSprachmodell.Anweisung(Vorlauftext, kategorien), maxToken: 1), abbruch);
            return antwort.IsSuccessStatusCode;
        }
        catch (Exception fehler) when (!abbruch.IsCancellationRequested)
        {
            // Ohne laufendes Sprachmodell oder Datenbank normal; eine Zeile genügt, ohne Stapelverfolgung.
            protokoll.LogDebug("Sprachmodell beim Vorladen nicht erreichbar ({Grund}).", fehler.GetType().Name);
            return false;
        }
    }
}
