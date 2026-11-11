using Soforthilfe.Dienste.Preise;

namespace Soforthilfe.Tests;

/// <summary>
/// Richtpreis für die erste Stunde (Anforderung A9): Zuschläge nach Schweizer Ortszeit, höherer Zuschlag gilt,
/// MWST nur bei MWST-Pflicht, Rundung auf 5 Rappen.
/// </summary>
public sealed class PreisrechnerTests
{
    private readonly Preisrechner _rechner = new(new PreisEinstellungen());

    // Anfahrt 40, Stundenansatz 95, Nacht 50 %, Wochenende 25 %, MWST-pflichtig.
    private static readonly Preisangaben Betrieb = new(40m, 95m, 50, 25, true);

    // Zeitpunkte im November 2026, Schweizer Ortszeit UTC+1.
    private static DateTimeOffset Ortszeit(int tag, int stunde, int minute = 0) =>
        new(2026, 11, tag, stunde, minute, 0, TimeSpan.FromHours(1));

    [Theory]
    [InlineData(11, 10, 145.95, null, 0)]           // Mittwoch, werktags
    [InlineData(11, 22, 197.30, "nacht", 50)]       // Mittwoch, nachts
    [InlineData(14, 10, 171.60, "wochenende", 25)]  // Samstag, tagsüber
    [InlineData(14, 23, 197.30, "nacht", 50)]       // Samstag, nachts: der höhere Zuschlag gilt, nicht beide
    public void Richtpreis_mit_Zuschlag_und_MWST(int tag, int stunde, double erwartet, string? zuschlag, int prozent)
    {
        var preis = _rechner.Berechnen(Betrieb, true, Ortszeit(tag, stunde))!;

        Assert.Equal((decimal)erwartet, preis.RichtpreisErsteStunde);
        Assert.Equal(zuschlag, preis.AngewandterZuschlag);
        Assert.Equal(prozent, preis.AngewandterZuschlagProzent);
        Assert.Equal(8.1m, preis.MwstSatzProzent);
    }

    [Theory]
    [InlineData(19, 59, false)]
    [InlineData(20, 0, true)]
    [InlineData(6, 59, true)]
    [InlineData(7, 0, false)]
    public void Nachtzeit_von_20_bis_7_Uhr(int stunde, int minute, bool nacht)
    {
        var preis = _rechner.Berechnen(Betrieb, true, Ortszeit(11, stunde, minute))!;
        Assert.Equal(nacht ? "nacht" : null, preis.AngewandterZuschlag);
    }

    [Fact]
    public void Massgebend_ist_die_Schweizer_Ortszeit()
    {
        // 19:30 UTC ist in der Schweiz 20:30 Uhr und damit Nacht.
        var preis = _rechner.Berechnen(Betrieb, true, new DateTimeOffset(2026, 11, 11, 19, 30, 0, TimeSpan.Zero))!;
        Assert.Equal("nacht", preis.AngewandterZuschlag);
    }

    [Fact]
    public void Ohne_MWST_Pflicht_kommt_keine_MWST_hinzu()
    {
        var preis = _rechner.Berechnen(Betrieb with { MwstPflichtig = false }, true, Ortszeit(11, 22))!;
        Assert.Equal(182.50m, preis.RichtpreisErsteStunde);
    }

    [Fact]
    public void Ohne_Zuschlag_im_Profil_gilt_der_Grundpreis()
    {
        var preis = _rechner.Berechnen(new Preisangaben(20m, 70m, null, null, false), true, Ortszeit(14, 23))!;
        Assert.Equal(90m, preis.RichtpreisErsteStunde);
        Assert.Null(preis.AngewandterZuschlag);
    }

    [Fact]
    public void Kostenlose_Leistungen_haben_keinen_Preis()
    {
        Assert.Null(_rechner.Berechnen(Betrieb, false, Ortszeit(11, 10)));
    }

    [Fact]
    public void Unvollstaendige_Angaben_ergeben_keinen_Richtpreis()
    {
        var preis = _rechner.Berechnen(new Preisangaben(40m, null, 50, 25, true), true, Ortszeit(11, 10))!;
        Assert.Null(preis.RichtpreisErsteStunde);
        Assert.Equal(40m, preis.AnfahrtChf);
    }

    [Theory]
    [InlineData(1.02, 1.00)]
    [InlineData(1.025, 1.05)]
    [InlineData(1.07, 1.05)]
    [InlineData(1.075, 1.10)]
    [InlineData(145.935, 145.95)]
    public void Rundung_auf_5_Rappen(double betrag, double gerundet)
    {
        Assert.Equal((decimal)gerundet, Preisrechner.Runden((decimal)betrag));
    }

    [Fact]
    public void MWST_Satz_ist_einstellbar()
    {
        var rechner = new Preisrechner(new PreisEinstellungen(MwstSatzProzent: 7.7m));
        var preis = rechner.Berechnen(Betrieb, true, Ortszeit(11, 10))!;
        Assert.Equal(145.40m, preis.RichtpreisErsteStunde);
        Assert.Equal(7.7m, preis.MwstSatzProzent);
    }
}
