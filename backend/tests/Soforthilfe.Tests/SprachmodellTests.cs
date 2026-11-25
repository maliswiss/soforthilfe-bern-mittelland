using System.Net;
using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using Soforthilfe.Datenzugriff.Modell;
using Soforthilfe.Dienste.Verstaendnis;

namespace Soforthilfe.Tests;

/// <summary>
/// Anbindung an das lokale Sprachmodell ohne laufendes Ollama: Antworten, Zeitlimit, Zwischenspeicher und Pause
/// nach einem Fehler werden mit einer Attrappe geprüft.
/// </summary>
public sealed class SprachmodellTests
{
    private static readonly IReadOnlyList<(string Kennung, string Bezeichnung)> Kategorien =
    [
        ("transporthilfe", "Transport- und Umzugshilfe"),
        ("begleitung", "Begleitung und Orientierung"),
        ("lieferdienst", "Lieferung Apotheke und Einkauf"),
    ];

    private const string Freitext = "Ich brauche jemanden mit einem Bus fuer einen Tisch";

    private sealed class Attrappe(Func<CancellationToken, Task<HttpResponseMessage>> antwort) : HttpMessageHandler
    {
        public List<string> Rumpfe { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage anfrage, CancellationToken abbruch)
        {
            Rumpfe.Add(anfrage.Content is null ? string.Empty : await anfrage.Content.ReadAsStringAsync(abbruch));
            return await antwort(abbruch);
        }
    }

    private sealed class Fabrik(HttpMessageHandler handler) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) =>
            new(handler, disposeHandler: false) { BaseAddress = new Uri("http://sprachmodell.test/") };
    }

    private sealed class FesteZeit : TimeProvider
    {
        public DateTimeOffset Jetzt { get; set; } = new(2026, 11, 16, 9, 0, 0, TimeSpan.Zero);

        public override DateTimeOffset GetUtcNow() => Jetzt;
    }

    private static HttpResponseMessage Ollama(string inhalt) => new(HttpStatusCode.OK)
    {
        Content = new StringContent(
            $$"""{"model":"test","response":{{System.Text.Json.JsonSerializer.Serialize(inhalt)}},"done":true}""",
            Encoding.UTF8, "application/json"),
    };

    private static (OllamaSprachmodell Modell, Attrappe Handler, FesteZeit Zeit) Erstellen(
        Func<CancellationToken, Task<HttpResponseMessage>> antwort, string? adresse = "http://sprachmodell.test",
        double zeitlimitSekunden = 5)
    {
        var handler = new Attrappe(antwort);
        var zeit = new FesteZeit();
        var einstellungen = new SprachmodellEinstellungen(adresse, "llama3.2:3b", TimeSpan.FromSeconds(zeitlimitSekunden), "30m");
        return (new OllamaSprachmodell(new Fabrik(handler), einstellungen, NullLogger<OllamaSprachmodell>.Instance, zeit),
            handler, zeit);
    }

    [Fact]
    public async Task Ohne_Adresse_ist_das_Sprachmodell_ausgeschaltet()
    {
        var (modell, handler, _) = Erstellen(_ => Task.FromResult(Ollama("""{"kategorie":"transporthilfe"}""")), adresse: null);

        var ergebnis = await modell.KlassifizierenAsync(Freitext, Kategorien, TestContext.Current.CancellationToken);

        Assert.Equal(new SprachmodellErgebnis(null, false), ergebnis);
        Assert.Empty(handler.Rumpfe);
    }

    [Fact]
    public async Task Eine_erlaubte_Kennung_wird_uebernommen_und_die_Anfrage_ist_kurz_und_im_JSON_Modus()
    {
        var (modell, handler, _) = Erstellen(_ => Task.FromResult(Ollama("""{"kategorie":" Transporthilfe "}""")));

        var ergebnis = await modell.KlassifizierenAsync(Freitext, Kategorien, TestContext.Current.CancellationToken);

        Assert.Equal(new SprachmodellErgebnis("transporthilfe", true), ergebnis);
        var rumpf = Assert.Single(handler.Rumpfe);
        Assert.Contains("\"model\":\"llama3.2:3b\"", rumpf);
        Assert.Contains("\"format\":\"json\"", rumpf);
        Assert.Contains("\"stream\":false", rumpf);
        Assert.Contains("\"temperature\":0", rumpf);
        Assert.Contains("\"num_predict\":24", rumpf);
        Assert.Contains(Freitext, rumpf);
    }

    [Theory]
    [InlineData("""{"kategorie":"schluesseldienst"}""")]
    [InlineData("""{"kategorie":null}""")]
    [InlineData("""{"andere":"transporthilfe"}""")]
    [InlineData("kein JSON")]
    public async Task Unbekannte_fehlende_oder_unlesbare_Kennungen_ergeben_keine_Kategorie(string inhalt)
    {
        var (modell, _, _) = Erstellen(_ => Task.FromResult(Ollama(inhalt)));

        var ergebnis = await modell.KlassifizierenAsync(Freitext, Kategorien, TestContext.Current.CancellationToken);

        Assert.Equal(new SprachmodellErgebnis(null, true), ergebnis);
    }

    [Fact]
    public async Task Gleiche_Texte_kommen_aus_dem_Zwischenspeicher()
    {
        var (modell, handler, _) = Erstellen(_ => Task.FromResult(Ollama("""{"kategorie":"transporthilfe"}""")));

        await modell.KlassifizierenAsync(Freitext, Kategorien, TestContext.Current.CancellationToken);
        var zweites = await modell.KlassifizierenAsync("  ich BRAUCHE jemanden mit einem Bus fuer einen Tisch ", Kategorien,
            TestContext.Current.CancellationToken);

        Assert.Equal("transporthilfe", zweites.Kennung);
        Assert.Single(handler.Rumpfe);
    }

    [Fact]
    public async Task Nach_einem_Fehler_wird_eine_Minute_lang_nicht_gefragt()
    {
        var (modell, handler, zeit) = Erstellen(_ => Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError)));

        Assert.False((await modell.KlassifizierenAsync(Freitext, Kategorien, TestContext.Current.CancellationToken)).Erreicht);
        Assert.False((await modell.KlassifizierenAsync("Anderer Text", Kategorien, TestContext.Current.CancellationToken)).Erreicht);
        Assert.Single(handler.Rumpfe);

        zeit.Jetzt += TimeSpan.FromSeconds(61);
        await modell.KlassifizierenAsync("Dritter Text", Kategorien, TestContext.Current.CancellationToken);
        Assert.Equal(2, handler.Rumpfe.Count);
    }

    [Fact]
    public async Task Eine_zu_langsame_Antwort_wird_nach_dem_Zeitlimit_abgebrochen()
    {
        var (modell, _, _) = Erstellen(async abbruch =>
        {
            await Task.Delay(TimeSpan.FromSeconds(5), abbruch);
            return Ollama("""{"kategorie":"transporthilfe"}""");
        }, zeitlimitSekunden: 0.2);

        var dauer = System.Diagnostics.Stopwatch.StartNew();
        var ergebnis = await modell.KlassifizierenAsync(Freitext, Kategorien, TestContext.Current.CancellationToken);

        Assert.Equal(new SprachmodellErgebnis(null, false), ergebnis);
        Assert.True(dauer.Elapsed < TimeSpan.FromSeconds(3));
    }

    [Fact]
    public void Die_Anweisung_nennt_alle_erlaubten_Kategorien_und_die_Anfrage()
    {
        var anweisung = OllamaSprachmodell.Anweisung(Freitext, Kategorien);

        Assert.Contains("- transporthilfe: Transport- und Umzugshilfe", anweisung);
        Assert.Contains("- begleitung: Begleitung und Orientierung", anweisung);
        Assert.Contains("- lieferdienst: Lieferung Apotheke und Einkauf", anweisung);
        Assert.Contains("{\"kategorie\":null}", anweisung);
        Assert.EndsWith($"Anfrage: {Freitext}", anweisung);
    }

    [Fact]
    public void Der_Anfang_der_Anweisung_ist_fuer_jede_Anfrage_gleich()
    {
        var erste = OllamaSprachmodell.Anweisung(Freitext, Kategorien);
        var zweite = OllamaSprachmodell.Anweisung("Mein Sofa muss in den dritten Stock.", Kategorien);

        var anfang = erste[..erste.LastIndexOf("Anfrage:", StringComparison.Ordinal)];
        Assert.StartsWith(anfang, zweite);
    }

    [Fact]
    public void Die_Kategorien_stehen_in_der_Reihenfolge_der_Startseite()
    {
        Kategorie[] ausDerDatenbank =
        [
            new() { Kennung = "lieferdienst", Bezeichnung = "Lieferung Apotheke und Einkauf", Reihenfolge = 3 },
            new() { Kennung = "transporthilfe", Bezeichnung = "Transport- und Umzugshilfe", Reihenfolge = 1 },
            new() { Kennung = "begleitung", Bezeichnung = "Begleitung und Orientierung", Reihenfolge = 2 },
        ];

        var kategorien = OllamaSprachmodell.KategorienFuerAnweisung(ausDerDatenbank);

        Assert.Equal(new[] { "transporthilfe", "begleitung", "lieferdienst" }, kategorien.Select(k => k.Kennung).ToArray());
        Assert.Equal(OllamaSprachmodell.KategorienFuerAnweisung(ausDerDatenbank.OrderByDescending(k => k.Kennung)), kategorien);
    }
}
