using System.Net.Http.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Soforthilfe.Dienste.Verstaendnis;

/// <summary>
/// Lädt das Sprachmodell nach dem Start in den Speicher. Ohne diesen Schritt wartet die erste unsichere Anfrage,
/// bis Ollama das Modell von der Platte geladen hat. Läuft im Hintergrund, blockiert den Start nicht und ist
/// harmlos, wenn das Sprachmodell nicht erreichbar ist.
/// </summary>
public sealed class SprachmodellAufwaermer(
    IHttpClientFactory fabrik,
    SprachmodellEinstellungen einstellungen,
    ILogger<SprachmodellAufwaermer> protokoll) : BackgroundService
{
    private const int MaxVersuche = 6;
    private static readonly TimeSpan Wartezeit = TimeSpan.FromSeconds(20);

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

    // Ein Auftrag ohne Text lädt das Modell, ohne etwas zu erzeugen.
    private async Task<bool> LadenAsync(CancellationToken abbruch)
    {
        try
        {
            var klient = fabrik.CreateClient(OllamaSprachmodell.KlientName);
            // Das erste Laden eines Modells dauert länger als eine Anfrage.
            klient.Timeout = TimeSpan.FromMinutes(5);
            using var antwort = await klient.PostAsJsonAsync("api/generate",
                new { model = einstellungen.Modell, keep_alive = einstellungen.BleibtGeladen }, abbruch);
            return antwort.IsSuccessStatusCode;
        }
        catch (Exception fehler) when (!abbruch.IsCancellationRequested)
        {
            protokoll.LogDebug(fehler, "Sprachmodell beim Vorladen nicht erreichbar.");
            return false;
        }
    }
}
