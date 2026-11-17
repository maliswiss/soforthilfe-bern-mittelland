using System.Text.Json;
using Soforthilfe.Dienste.Sprachen;

namespace Soforthilfe.Tests;

/// <summary>
/// Prüfung importierter Sprachdateien: Aufbau, Schlüssel, Texte, Platzhalter und unzulässige Zeichen.
/// </summary>
public sealed class SprachdateiTests
{
    private static SprachdateiPruefung Pruefen(string json) => Sprachdatei.Pruefen(JsonDocument.Parse(json).RootElement);

    [Fact]
    public void Verschachtelte_Texte_mit_Platzhaltern_werden_gezaehlt()
    {
        var ergebnis = Pruefen("""
            { "start": { "titel1": "Non cercate più.", "einleitung": "Regione {region}" },
              "kategorien": { "sanitaer-elektro": "Sanitari", "kleine_reparatur": "Riparazione" },
              "profil": { "nachweis": { "0": "Numero IDI" } } }
            """);

        Assert.Equal(new SprachdateiPruefung(5, null, null), ergebnis);
    }

    [Theory]
    [InlineData("[]", SprachdateiFehler.KeinObjekt, null)]
    [InlineData("\"Text\"", SprachdateiFehler.KeinObjekt, null)]
    [InlineData("{}", SprachdateiFehler.Leer, null)]
    [InlineData("""{ "start": {} }""", SprachdateiFehler.Leer, null)]
    [InlineData("""{ "start": { "titel 1": "x" } }""", SprachdateiFehler.SchluesselUngueltig, "start.titel 1")]
    [InlineData("""{ "start": { "anzahl": 3 } }""", SprachdateiFehler.KeinText, "start.anzahl")]
    [InlineData("""{ "start": { "liste": ["a"] } }""", SprachdateiFehler.KeinText, "start.liste")]
    [InlineData("""{ "start": { "leer": null } }""", SprachdateiFehler.KeinText, "start.leer")]
    [InlineData("""{ "a": { "b": { "c": { "d": { "e": { "f": "x" } } } } } }""", SprachdateiFehler.ZuTief, "a.b.c.d.e")]
    public void Falscher_Aufbau_wird_mit_Pfad_abgewiesen(string json, SprachdateiFehler fehler, string? pfad)
    {
        var ergebnis = Pruefen(json);
        Assert.Equal(fehler, ergebnis.Fehler);
        Assert.Equal(pfad, ergebnis.Pfad);
    }

    [Theory]
    [InlineData("<script>alert(1)</script>")]
    [InlineData("Klick <b>hier</b>")]
    [InlineData("Mail an name@beispiel.ch")]
    [InlineData("eins | zwei")]
    public void HTML_und_Steuerzeichen_der_Oberflaeche_sind_unzulaessig(string text)
    {
        var ergebnis = Pruefen(JsonSerializer.Serialize(new { start = new { titel = text } }));
        Assert.Equal(SprachdateiFehler.ZeichenUnzulaessig, ergebnis.Fehler);
        Assert.Equal("start.titel", ergebnis.Pfad);
    }

    [Theory]
    [InlineData("Region {region")]
    [InlineData("Region region}")]
    [InlineData("Region { region }")]
    public void Geschweifte_Klammern_nur_als_Platzhalter(string text) =>
        Assert.Equal(SprachdateiFehler.PlatzhalterUngueltig,
            Pruefen(JsonSerializer.Serialize(new { start = new { titel = text } })).Fehler);

    [Fact]
    public void Texte_und_Anzahl_sind_begrenzt()
    {
        var lang = JsonSerializer.Serialize(new { start = new { titel = new string('a', Sprachdatei.MaxTextlaenge + 1) } });
        Assert.Equal(SprachdateiFehler.TextZuLang, Pruefen(lang).Fehler);

        var viele = JsonSerializer.Serialize(Enumerable.Range(0, Sprachdatei.MaxTexte + 1).ToDictionary(i => $"t{i}", _ => "x"));
        Assert.Equal(SprachdateiFehler.ZuViele, Pruefen(viele).Fehler);
    }

    [Theory]
    [InlineData("it", true)]
    [InlineData("rm", true)]
    [InlineData("IT", false)]
    [InlineData("ita", false)]
    [InlineData("i", false)]
    [InlineData(null, false)]
    public void Code_aus_zwei_Kleinbuchstaben(string? code, bool gueltig) =>
        Assert.Equal(gueltig, Sprachdatei.CodeGueltig(code));

    [Theory]
    [InlineData("Italiano", true)]
    [InlineData("I", false)]
    [InlineData("   ", false)]
    [InlineData("<i>Italiano</i>", false)]
    public void Name_mit_2_bis_40_Zeichen_ohne_HTML(string name, bool gueltig) =>
        Assert.Equal(gueltig, Sprachdatei.NameGueltig(name));
}
