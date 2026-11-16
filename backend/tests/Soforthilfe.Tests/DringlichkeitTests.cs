using Soforthilfe.Datenzugriff.Modell;
using Soforthilfe.Dienste.Verstaendnis;

namespace Soforthilfe.Tests;

/// <summary>
/// Dringlichkeit aus Text und Kategorie: Signalwörter vor der Kategorie, «keine Eile» vor «sofort».
/// </summary>
public sealed class DringlichkeitTests
{
    [Theory]
    [InlineData("Bitte sofort kommen", null, Prioritaet.Hoch)]
    [InlineData("Die Wohnung ist kalt, die Heizung geht nicht", "sanitaer-elektro", Prioritaet.Hoch)]
    [InlineData("Die Kinder sind allein zu Hause", null, Prioritaet.Hoch)]
    [InlineData("Ma voiture est en panne, c'est urgent", "pannenhilfe", Prioritaet.Hoch)]
    [InlineData("Umzug heute noch", "transporthilfe", Prioritaet.Hoch)]
    [InlineData("Keine Eile, nächste Woche reicht", "transporthilfe", Prioritaet.Niedrig)]
    [InlineData("Es ist nicht dringend", null, Prioritaet.Niedrig)]
    [InlineData("Pas pressé, la semaine prochaine", null, Prioritaet.Niedrig)]
    [InlineData("Ausgesperrt, aber keine Eile", "schluesseldienst", Prioritaet.Niedrig)]
    public void Signalwoerter_im_Text_bestimmen_die_Dringlichkeit(string freitext, string? kennung, Prioritaet erwartet) =>
        Assert.Equal(erwartet, Dringlichkeit.Bestimmen(freitext, kennung).Prioritaet);

    [Theory]
    [InlineData("Ich habe mich ausgesperrt", "schluesseldienst", Prioritaet.Hoch)]
    [InlineData(null, "pannenhilfe", Prioritaet.Hoch)]
    [InlineData("Umzug am Samstag", "transporthilfe", Prioritaet.Niedrig)]
    [InlineData("Der Wasserhahn tropft", "sanitaer-elektro", Prioritaet.Mittel)]
    [InlineData(null, "begleitung", Prioritaet.Mittel)]
    [InlineData(null, null, Prioritaet.Mittel)]
    public void Ohne_Signalwort_gilt_die_uebliche_Dringlichkeit_der_Kategorie(string? freitext, string? kennung, Prioritaet erwartet) =>
        Assert.Equal(erwartet, Dringlichkeit.Bestimmen(freitext, kennung).Prioritaet);

    [Theory]
    // Kurze Begriffe nur als ganzes Wort
    [InlineData("Das Kaltwasser tropft")]
    [InlineData("Flyer werden verteilt")]
    public void Signalwoerter_zaehlen_nur_mit_Wortgrenze(string freitext) =>
        Assert.Equal(Prioritaet.Mittel, Dringlichkeit.Bestimmen(freitext, null).Prioritaet);

    [Fact]
    public void Die_Begruendung_nennt_das_Wort_wie_geschrieben() =>
        Assert.Equal("Dringlichkeit hoch wegen «SOFORT».", Dringlichkeit.Bestimmen("Bitte SOFORT", null).Grund);
}
