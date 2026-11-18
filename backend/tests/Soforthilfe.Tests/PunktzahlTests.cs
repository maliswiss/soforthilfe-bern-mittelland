using Soforthilfe.Datenzugriff.Modell;
using Soforthilfe.Dienste.Matching;

namespace Soforthilfe.Tests;

/// <summary>
/// Smart-Matching: Teilpunkte je Kriterium, gewichtete Punktzahl, Gewichte und Reihenfolge mit Partner-Abo.
/// </summary>
public sealed class PunktzahlTests
{
    private static readonly MatchingGewichte Standard = MatchingGewichte.Standard;

    private static Helferprofil Profil(bool bereit = true, int bewertungen = 0, double schnitt = 0, int einsaetze = 0,
        bool partner = false, Guid? id = null) => new()
    {
        Id = id ?? Guid.NewGuid(),
        Postleitzahl = "3011",
        Ort = "Bern",
        Einsatzbereit = bereit,
        AnzahlBewertungen = bewertungen,
        Bewertungsschnitt = schnitt,
        AnzahlEinsaetze = einsaetze,
        Partner = partner,
    };

    private static BewerteterKandidat Bewertet(double punktzahl, bool partner = false, double distanz = 0, Guid? id = null) =>
        new(new Kandidat(Profil(partner: partner, id: id), distanz), new Teilpunkte(0, 1, 0, 0, 0), punktzahl);

    [Theory]
    [InlineData(0, 1.0)]
    [InlineData(2500, 0.75)]
    [InlineData(10000, 0.0)]
    [InlineData(12000, 0.0)]
    public void Distanz_gibt_mehr_Punkte_je_naeher(double meter, double erwartet) =>
        Assert.Equal(erwartet, Punktzahl.Berechnen(Profil(), meter, 10).Distanz, 6);

    [Fact]
    public void Ohne_Bewertungen_gilt_ein_mittlerer_Wert()
    {
        Assert.Equal(Punktzahl.OhneBewertungen, Punktzahl.Berechnen(Profil(), 0, 10).Bewertung);
        Assert.Equal(0.96, Punktzahl.Berechnen(Profil(bewertungen: 12, schnitt: 4.8), 0, 10).Bewertung, 6);
    }

    [Fact]
    public void Verfuegbarkeit_Erfahrung_und_Faehigkeit()
    {
        var bereit = Punktzahl.Berechnen(Profil(einsaetze: 4), 0, 10);
        Assert.Equal(1.0, bereit.Verfuegbarkeit);
        Assert.Equal(0.4, bereit.Erfahrung, 6);
        Assert.Equal(1.0, bereit.Faehigkeit);

        var pause = Punktzahl.Berechnen(Profil(bereit: false, einsaetze: 25), 0, 10);
        Assert.Equal(Punktzahl.NichtVerfuegbar, pause.Verfuegbarkeit);
        Assert.Equal(1.0, pause.Erfahrung);
    }

    [Fact]
    public void Punktzahl_ist_die_Summe_der_gewichteten_Teilpunkte()
    {
        // 2.5 km bei 10 km Suchradius, keine Bewertungen, verfügbar, 4 Einsätze
        var teilpunkte = Punktzahl.Berechnen(Profil(einsaetze: 4), 2500, 10);
        var erwartet = 0.75 * 0.35 + 1.0 * 0.25 + 0.6 * 0.20 + 1.0 * 0.10 + 0.4 * 0.10;

        Assert.Equal(erwartet, teilpunkte.Gewichtet(Standard), 6);
        Assert.Equal(0.7725, teilpunkte.Gewichtet(Standard), 6);
    }

    [Fact]
    public void Gewichte_werden_auf_die_Summe_eins_gebracht()
    {
        var gewichte = new MatchingGewichte(7, 5, 4, 2, 2).Normalisiert();

        Assert.Equal(1.0, gewichte.Summe, 9);
        Assert.Equal(0.35, gewichte.Distanz, 9);
        Assert.Equal(0.10, gewichte.Erfahrung, 9);
        Assert.Equal(1.0, MatchingGewichte.Standard.Summe, 9);
    }

    [Theory]
    [InlineData(-0.1, 0.25)]
    [InlineData(0, 0)]
    [InlineData(double.NaN, 0.25)]
    public void Ungueltige_Gewichte_verhindern_den_Start(double distanz, double faehigkeit)
    {
        var gewichte = faehigkeit == 0 ? new MatchingGewichte(0, 0, 0, 0, 0) : Standard with { Distanz = distanz, Faehigkeit = faehigkeit };
        Assert.Throws<ArgumentException>(() => gewichte.Normalisiert());
    }

    [Fact]
    public void Hoehere_Punktzahl_zuerst_bei_gleicher_nach_Distanz()
    {
        var a = Bewertet(0.70);
        var b = Bewertet(0.90);
        var c = Bewertet(0.70, distanz: 500);
        var d = Bewertet(0.70, distanz: 100);

        Assert.Equal(new[] { b, a, d, c }, Punktzahl.Ordnen([c, a, b, d]));
    }

    [Fact]
    public void Partner_rueckt_nur_bei_gleichwertiger_Punktzahl_vor_und_wird_gekennzeichnet()
    {
        var ohne = Bewertet(0.80);
        var partner = Bewertet(0.79, partner: true);

        var reihe = Punktzahl.Ordnen([ohne, partner]);

        Assert.Equal(partner.Kandidat, reihe[0].Kandidat);
        Assert.True(reihe[0].PartnerBevorzugt);
        Assert.Equal(ohne, reihe[1]);
    }

    [Fact]
    public void Partner_bleibt_bei_deutlich_tieferer_Punktzahl_hinten()
    {
        var ohne = Bewertet(0.80);
        var partner = Bewertet(0.77, partner: true);

        var reihe = Punktzahl.Ordnen([partner, ohne]);

        Assert.Equal(new[] { ohne, partner }, reihe);
        Assert.False(reihe[1].PartnerBevorzugt);
    }

    [Fact]
    public void Abstand_von_genau_zwei_Hundertsteln_gilt_als_gleichwertig()
    {
        var ohne = Bewertet(0.75);
        var partner = Bewertet(0.73, partner: true);

        Assert.Equal(partner.Kandidat, Punktzahl.Ordnen([ohne, partner])[0].Kandidat);
    }

    [Fact]
    public void Partner_vor_Partner_ohne_Kennzeichnung()
    {
        var erster = Bewertet(0.80, partner: true);
        var zweiter = Bewertet(0.79, partner: true);

        var reihe = Punktzahl.Ordnen([zweiter, erster]);

        Assert.Equal(new[] { erster, zweiter }, reihe);
        Assert.All(reihe, b => Assert.False(b.PartnerBevorzugt));
    }
}
