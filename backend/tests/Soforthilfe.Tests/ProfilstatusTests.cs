using Soforthilfe.Datenzugriff.Modell;
using Soforthilfe.Dienste.Nachweise;

namespace Soforthilfe.Tests;

/// <summary>
/// Stand des Helferprofils nach einem Entscheid der Koordination.
/// </summary>
public sealed class ProfilstatusTests
{
    private static (Helferprofil Profil, Verifizierung Entschieden) Profil(
        Verifizierungsstatus profilstatus, Verifizierungsstatus entscheid, params Verifizierungsstatus[] andere)
    {
        var profil = new Helferprofil { Postleitzahl = "3011", Ort = "Bern", Verifizierung = profilstatus };
        var entschieden = new Verifizierung { Id = Guid.NewGuid(), Status = entscheid };
        profil.Verifizierungen.Add(entschieden);
        foreach (var status in andere)
        {
            profil.Verifizierungen.Add(new Verifizierung { Id = Guid.NewGuid(), Status = status });
        }

        return (profil, entschieden);
    }

    [Theory]
    [InlineData(Verifizierungsstatus.InPruefung)]
    [InlineData(Verifizierungsstatus.Abgelehnt)]
    public void Freigabe_macht_das_Profil_verifiziert(Verifizierungsstatus anderer)
    {
        var (profil, entschieden) = Profil(Verifizierungsstatus.InPruefung, Verifizierungsstatus.Verifiziert, anderer);
        Assert.Equal(Verifizierungsstatus.Verifiziert, VerifizierungDienst.Profilstatus(profil, entschieden));
    }

    [Fact]
    public void Ablehnung_ohne_weitere_Nachweise_lehnt_das_Profil_ab()
    {
        var (profil, entschieden) = Profil(Verifizierungsstatus.InPruefung, Verifizierungsstatus.Abgelehnt,
            Verifizierungsstatus.Abgelehnt);
        Assert.Equal(Verifizierungsstatus.Abgelehnt, VerifizierungDienst.Profilstatus(profil, entschieden));
    }

    [Fact]
    public void Ablehnung_mit_weiteren_offenen_Nachweisen_bleibt_in_Pruefung()
    {
        var (profil, entschieden) = Profil(Verifizierungsstatus.InPruefung, Verifizierungsstatus.Abgelehnt,
            Verifizierungsstatus.Verifiziert, Verifizierungsstatus.InPruefung);
        Assert.Equal(Verifizierungsstatus.InPruefung, VerifizierungDienst.Profilstatus(profil, entschieden));
    }

    [Fact]
    public void Ablehnung_eines_Zusatznachweises_laesst_das_Profil_verifiziert()
    {
        var (profil, entschieden) = Profil(Verifizierungsstatus.InPruefung, Verifizierungsstatus.Abgelehnt,
            Verifizierungsstatus.Verifiziert);
        Assert.Equal(Verifizierungsstatus.Verifiziert, VerifizierungDienst.Profilstatus(profil, entschieden));
    }

    [Theory]
    [InlineData(Verifizierungsstatus.Verifiziert)]
    [InlineData(Verifizierungsstatus.Abgelehnt)]
    public void Eine_Sperre_bleibt_bestehen(Verifizierungsstatus entscheid)
    {
        var (profil, entschieden) = Profil(Verifizierungsstatus.Gesperrt, entscheid);
        Assert.Equal(Verifizierungsstatus.Gesperrt, VerifizierungDienst.Profilstatus(profil, entschieden));
    }
}
