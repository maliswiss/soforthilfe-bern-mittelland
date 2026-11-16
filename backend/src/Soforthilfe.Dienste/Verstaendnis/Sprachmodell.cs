using System.Collections.Concurrent;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Soforthilfe.Dienste.Verstaendnis;

/// <summary>
/// Einstellungen des lokalen Sprachmodells. Ohne Adresse ist das Sprachmodell ausgeschaltet; die Kette endet dann
/// nach dem maschinellen Lernen.
/// </summary>
public sealed record SprachmodellEinstellungen(string? Adresse, string Modell, TimeSpan Zeitlimit, string BleibtGeladen)
{
    public bool Eingeschaltet => !string.IsNullOrWhiteSpace(Adresse);
}

/// <summary>
/// Antwort des Sprachmodells: die Kennung einer erlaubten Kategorie oder null, wenn keine passt oder das Modell
/// nicht rechtzeitig antwortet.
/// </summary>
public sealed record SprachmodellErgebnis(string? Kennung, bool Erreicht);

/// <summary>
/// Letzte Stufe der Verständniskette: ein lokal betriebenes Sprachmodell (Ollama), nur wenn Regeln und
/// maschinelles Lernen nicht sicher entscheiden.
/// </summary>
public interface ISprachmodell
{
    Task<SprachmodellErgebnis> KlassifizierenAsync(string freitext,
        IReadOnlyList<(string Kennung, string Bezeichnung)> kategorien, CancellationToken abbruch);
}

/// <summary>
/// Anbindung an Ollama über die konfigurierte Adresse. Der Freitext geht nur an diesen eigenen Dienst, nie an
/// Dritte. Für die Geschwindigkeit gilt: kurze Antwort im JSON-Modus, festes Zeitlimit, gleiche Texte aus dem
/// Zwischenspeicher und nach einem Fehler eine Minute Pause, damit Anfragen nicht auf ein fehlendes Modell warten.
/// </summary>
public sealed class OllamaSprachmodell(
    IHttpClientFactory fabrik,
    SprachmodellEinstellungen einstellungen,
    ILogger<OllamaSprachmodell> protokoll,
    TimeProvider zeit) : ISprachmodell
{
    public const string KlientName = "sprachmodell";

    // Kurze JSON-Antwort; begrenzt die Rechenzeit auf einem Rechner ohne Grafikkarte.
    private const int MaxToken = 60;
    private const int MaxZwischenspeicher = 500;
    private static readonly TimeSpan PauseNachFehler = TimeSpan.FromMinutes(1);

    private readonly ConcurrentDictionary<string, string?> zwischenspeicher = new();
    // Zeitpunkt in Ticks (UTC), bis zu dem nach einem Fehler nicht gefragt wird; über mehrere Anfragen geteilt.
    private long pauseBisTicks;

    public async Task<SprachmodellErgebnis> KlassifizierenAsync(string freitext,
        IReadOnlyList<(string Kennung, string Bezeichnung)> kategorien, CancellationToken abbruch)
    {
        if (!einstellungen.Eingeschaltet || string.IsNullOrWhiteSpace(freitext) || zeit.GetUtcNow().UtcTicks < Interlocked.Read(ref pauseBisTicks))
        {
            return new SprachmodellErgebnis(null, false);
        }

        var schluessel = Textnormalisierung.Normalisieren(freitext);
        if (zwischenspeicher.TryGetValue(schluessel, out var gespeichert))
        {
            return new SprachmodellErgebnis(gespeichert, true);
        }

        using var frist = CancellationTokenSource.CreateLinkedTokenSource(abbruch);
        frist.CancelAfter(einstellungen.Zeitlimit);

        try
        {
            var klient = fabrik.CreateClient(KlientName);
            var rumpf = new
            {
                model = einstellungen.Modell,
                prompt = Anweisung(freitext, kategorien),
                stream = false,
                format = "json",
                keep_alive = einstellungen.BleibtGeladen,
                options = new { temperature = 0, num_predict = MaxToken },
            };

            using var antwort = await klient.PostAsJsonAsync("api/generate", rumpf, frist.Token);
            if (!antwort.IsSuccessStatusCode)
            {
                return Fehlgeschlagen($"Status {(int)antwort.StatusCode}");
            }

            using var dokument = await JsonDocument.ParseAsync(
                await antwort.Content.ReadAsStreamAsync(frist.Token), cancellationToken: frist.Token);
            var kennung = KennungLesen(dokument.RootElement, kategorien);

            if (zwischenspeicher.Count >= MaxZwischenspeicher)
            {
                zwischenspeicher.Clear();
            }

            zwischenspeicher[schluessel] = kennung;
            return new SprachmodellErgebnis(kennung, true);
        }
        catch (Exception fehler) when (!abbruch.IsCancellationRequested)
        {
            // Zeitlimit, nicht erreichbar oder unlesbare Antwort: die Kette bietet dann Alternativen an.
            return Fehlgeschlagen(fehler.GetType().Name);
        }
    }

    /// <summary>Anweisung an das Modell; die Antwort ist nur ein kleines JSON-Objekt mit einer erlaubten Kennung.</summary>
    public static string Anweisung(string freitext, IReadOnlyList<(string Kennung, string Bezeichnung)> kategorien)
    {
        var liste = string.Join("\n", kategorien.Select(k => $"- {k.Kennung}: {k.Bezeichnung}"));
        return $$"""
            Du ordnest Hilfeanfragen aus dem Alltag einer Kategorie zu. Die Anfrage kann in jeder Sprache sein.
            Erlaubte Kategorien:
            {{liste}}
            Antworte nur mit JSON in der Form {"kategorie":"<kennung>"}.
            Passt keine Kategorie, antworte {"kategorie":null}.

            Anfrage: {{freitext}}
            """;
    }

    /// <summary>Liest die Kennung aus der Antwort von Ollama; nur erlaubte Kennungen zählen.</summary>
    public static string? KennungLesen(JsonElement ollamaAntwort, IReadOnlyList<(string Kennung, string Bezeichnung)> kategorien)
    {
        if (!ollamaAntwort.TryGetProperty("response", out var inhalt) || inhalt.ValueKind != JsonValueKind.String)
        {
            return null;
        }

        try
        {
            using var json = JsonDocument.Parse(inhalt.GetString() ?? string.Empty);
            if (json.RootElement.ValueKind != JsonValueKind.Object
                || !json.RootElement.TryGetProperty("kategorie", out var wert)
                || wert.ValueKind != JsonValueKind.String)
            {
                return null;
            }

            var kennung = wert.GetString()?.Trim().ToLowerInvariant();
            return kategorien.Any(k => k.Kennung == kennung) ? kennung : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private SprachmodellErgebnis Fehlgeschlagen(string grund)
    {
        Interlocked.Exchange(ref pauseBisTicks, (zeit.GetUtcNow() + PauseNachFehler).UtcTicks);
        protokoll.LogWarning("Sprachmodell nicht verfügbar ({Grund}); Pause für {Minuten} Minute.", grund,
            PauseNachFehler.TotalMinutes);
        return new SprachmodellErgebnis(null, false);
    }
}
