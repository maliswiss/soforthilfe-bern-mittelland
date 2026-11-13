using Soforthilfe.Dienste.Verstaendnis;

namespace Soforthilfe.Tests;

/// <summary>
/// Regelstufe der Verständniskette: eindeutige Schlüsselwörter, Wortgrenzen und Gleichstand.
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
}
