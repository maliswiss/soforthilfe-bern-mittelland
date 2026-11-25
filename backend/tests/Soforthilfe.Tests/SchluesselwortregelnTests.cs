using Soforthilfe.Dienste.Verstaendnis;

namespace Soforthilfe.Tests;

/// <summary>
/// Regelstufe der Verständniskette: eindeutige Schlüsselwörter, Wortgrenzen, Gleichstand und Tippfehler.
/// </summary>
public sealed class SchluesselwortregelnTests
{
    [Theory]
    [InlineData("Die Heizung ist ausgefallen.", "sanitaer-elektro")]
    [InlineData("Ich habe mich ausgesperrt.", "schluesseldienst")]
    [InlineData("Mein Auto springt nicht an.", "pannenhilfe")]
    [InlineData("Ma voiture ne démarre pas.", "pannenhilfe")]
    [InlineData("Kann jemand meine Katze füttern?", "nachbarschaftshilfe")]
    [InlineData("Begleitung zum Arzttermin", "begleitung")]
    [InlineData("Medikamente aus der Apotheke holen", "lieferdienst")]
    [InlineData("Le robinet fuit", "sanitaer-elektro")]
    [InlineData("Der Laptop startet nicht", "it-soforthilfe")]
    [InlineData("Nach dem Wasserschaden putzen", "reinigung")]
    [InlineData("Umzug am Samstag, ein paar Kisten", "transporthilfe")]
    [InlineData("Regal an der Wand montieren", "kleine-reparatur")]
    public void Eindeutige_Schluesselwoerter_ergeben_die_Kategorie(string freitext, string kennung) =>
        Assert.Equal(kennung, Schluesselwortregeln.Anwenden(freitext)?.Kennung);

    [Theory]
    [InlineData("Ich brauche Hilfe")]
    [InlineData("")]
    [InlineData(null)]
    // Kurze Begriffe nur als ganzes Wort: «auto» passt nicht auf «Autobahn»
    [InlineData("Ich stehe an der Autobahn")]
    public void Ohne_Schluesselwort_entscheidet_die_naechste_Stufe(string? freitext) =>
        Assert.Null(Schluesselwortregeln.Anwenden(freitext));

    [Fact]
    public void Gleich_viele_Treffer_in_zwei_Kategorien_entscheiden_nicht() =>
        Assert.Null(Schluesselwortregeln.Anwenden("Katze im Auto"));

    [Fact]
    public void Mehr_Treffer_entscheiden_bei_mehreren_Kategorien()
    {
        // Zwei Begriffe für den Schlüsseldienst, einer für die Nachbarschaftshilfe
        var ergebnis = Schluesselwortregeln.Anwenden("Nachbar hat den Schlüssel im Schloss abgebrochen");
        Assert.Equal("schluesseldienst", ergebnis?.Kennung);
        Assert.Equal(0.8, ergebnis?.Konfidenz);
    }

    [Theory]
    [InlineData("Die Heizung ist ausgefallen.", 0.8)]
    [InlineData("Wasserhahn tropft und der Abfluss ist verstopft", 0.95)]
    public void Sicherheit_steigt_mit_der_Zahl_der_Treffer(string freitext, double konfidenz) =>
        Assert.Equal(konfidenz, Schluesselwortregeln.Anwenden(freitext)?.Konfidenz);

    [Fact]
    public void Das_erste_Schluesselwort_wird_genannt() =>
        Assert.Equal("ausgesperrt", Schluesselwortregeln.Anwenden("Ich habe mich ausgesperrt")?.Begriff);

    [Theory]
    [InlineData("heitzung geht nich, seit gestern eiskalt", "sanitaer-elektro", "heizung", "heitzung")]
    [InlineData("Die Hiezung bleibt kalt", "sanitaer-elektro", "heizung", "hiezung")]
    [InlineData("Der Comupter startet nicht mehr", "it-soforthilfe", "computer", "comupter")]
    [InlineData("Kann jemand zur Apoteke gehen?", "lieferdienst", "apotheke", "apoteke")]
    public void Ein_Tippfehler_in_einem_laengeren_Schluesselwort_wird_erkannt(
        string freitext, string kennung, string begriff, string schreibweise)
    {
        var ergebnis = Schluesselwortregeln.Anwenden(freitext);

        Assert.Equal(kennung, ergebnis?.Kennung);
        Assert.Equal(begriff, ergebnis?.Begriff);
        Assert.Equal(schreibweise, ergebnis?.Schreibweise);
        Assert.Equal(0.7, ergebnis?.Konfidenz);
    }

    [Fact]
    public void Ein_genauer_Treffer_geht_dem_Tippfehler_vor()
    {
        var ergebnis = Schluesselwortregeln.Anwenden("Schlussel verloren, ich komme nicht rein");

        Assert.Equal("schluesseldienst", ergebnis?.Kennung);
        Assert.Equal("komme nicht rein", ergebnis?.Begriff);
        Assert.Null(ergebnis?.Schreibweise);
    }

    [Theory]
    // Kurze Schlüsselwörter («storen», «bohren») gelten nicht als ähnlich
    [InlineData("Ich möchte nicht stören")]
    [InlineData("Wir kochen heute Bohnen")]
    // Ähnliche Wörter in zwei Kategorien entscheiden nicht
    [InlineData("Heitzung kalt und Comupter kaputt")]
    public void Unsichere_Tippfehler_entscheiden_nicht(string freitext) =>
        Assert.Null(Schluesselwortregeln.Anwenden(freitext));

    [Theory]
    [InlineData("heitzung", "heizung", true)]
    [InlineData("heizng", "heizung", true)]
    [InlineData("heizunk", "heizung", true)]
    [InlineData("hiezung", "heizung", true)]
    [InlineData("heizung", "heizung", false)]
    [InlineData("heitzungg", "heizung", false)]
    [InlineData("zeihung", "heizung", false)]
    public void Genau_ein_Buchstabe_darf_abweichen(string wort, string begriff, bool erwartet) =>
        Assert.Equal(erwartet, Schluesselwortregeln.EinBuchstabeAnders(wort, begriff));
}
