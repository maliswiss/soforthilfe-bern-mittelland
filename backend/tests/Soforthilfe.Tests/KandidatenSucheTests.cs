using Soforthilfe.Datenzugriff.Modell;
using Soforthilfe.Dienste.Matching;

namespace Soforthilfe.Tests;

/// <summary>
/// Kandidatensuche: Suchradius nach Dringlichkeit, Such- und Einsatzradius und die Reihenfolge der Kandidaten.
/// </summary>
public sealed class KandidatenSucheTests
{
    private static Kandidat NeuerKandidat(double distanzMeter, int einsatzradiusKm = 20, Guid? id = null) => new(
        new Helferprofil
        {
            Id = id ?? Guid.NewGuid(),
            Postleitzahl = "3011",
            Ort = "Bern",
            EinsatzradiusKm = einsatzradiusKm,
        },
        distanzMeter);

    [Theory]
    [InlineData(Prioritaet.Hoch, 15)]
    [InlineData(Prioritaet.Mittel, 10)]
    [InlineData(Prioritaet.Niedrig, 5)]
    public void Suchradius_folgt_der_Dringlichkeit(Prioritaet prioritaet, int km) =>
        Assert.Equal(km, Suchradius.FuerPrioritaet(prioritaet));

    [Theory]
    [InlineData(0, 10, 5, true)]
    [InlineData(5000, 10, 5, true)]
    [InlineData(5001, 10, 5, false)]
    [InlineData(10000, 10, 20, true)]
    [InlineData(10001, 10, 20, false)]
    [InlineData(-1, 10, 20, false)]
    public void Distanz_muss_im_Such_und_im_Einsatzradius_liegen(double meter, int suchradius, int einsatzradius, bool erwartet) =>
        Assert.Equal(erwartet, Suchradius.ImEinsatzgebiet(meter, suchradius, einsatzradius));

    [Fact]
    public void Auswahl_entfernt_Kandidaten_ausserhalb_und_ordnet_nach_Distanz()
    {
        var nah = NeuerKandidat(1460);
        var mittel = NeuerKandidat(7450);
        var ausserhalbSuchradius = NeuerKandidat(11800, einsatzradiusKm: 30);
        var ausserhalbEinsatzradius = NeuerKandidat(7450, einsatzradiusKm: 5);

        var auswahl = KandidatenSuche.Auswaehlen([mittel, ausserhalbSuchradius, nah, ausserhalbEinsatzradius], 10);

        Assert.Equal(new[] { nah, mittel }, auswahl);
    }

    [Fact]
    public void Bei_gleicher_Distanz_entscheidet_die_Kennung()
    {
        var erste = NeuerKandidat(0, id: Guid.Parse("00000000-0000-0000-0000-000000000001"));
        var zweite = NeuerKandidat(0, id: Guid.Parse("00000000-0000-0000-0000-000000000002"));

        Assert.Equal(new[] { erste, zweite }, KandidatenSuche.Auswaehlen([zweite, erste], 5));
    }

    [Fact]
    public void Ohne_Kandidaten_bleibt_die_Auswahl_leer() =>
        Assert.Empty(KandidatenSuche.Auswaehlen([], 15));
}
