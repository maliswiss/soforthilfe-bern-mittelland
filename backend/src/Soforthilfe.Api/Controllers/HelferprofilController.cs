using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Soforthilfe.Api.Fehlerbehandlung;
using Soforthilfe.Api.Modelle;
using Soforthilfe.Dienste.Helfende;
using Soforthilfe.Dienste.Konten;

namespace Soforthilfe.Api.Controllers;

/// <summary>
/// Eigenes Helferprofil: anlegen, ändern, Verfügbarkeit umschalten. Nur für Konten mit der Rolle «Helfende»,
/// in der Region des Kontos.
/// </summary>
[ApiController]
[Route("api/helferprofil")]
[Authorize(Roles = Rollen.Helfende)]
public sealed class HelferprofilController(IHelferprofilDienst profile) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Eigenes(CancellationToken abbruch)
    {
        var profil = await profile.LadenAsync(KontoId(), abbruch);
        return profil is null
            ? this.Fehler(StatusCodes.Status404NotFound, "Kein Helferprofil", "Es besteht noch kein Helferprofil.")
            : Ok(HelferprofilAntwort.Aus(profil));
    }

    // Anlegen oder vollständig ersetzen; Fähigkeiten werden ersetzt.
    [HttpPut]
    public async Task<IActionResult> Speichern(HelferprofilAnfrage eingabe, CancellationToken abbruch)
    {
        var ergebnis = await profile.SpeichernAsync(KontoId(), new HelferprofilDaten(
            eingabe.Typ, eingabe.Betriebsname, eingabe.Beschreibung, eingabe.Plz, eingabe.Ort, eingabe.RadiusKm,
            eingabe.SkillIds, eingabe.Adresse), abbruch);

        return ergebnis.Fehler switch
        {
            null => Ok(HelferprofilAntwort.Aus(ergebnis.Profil!)),
            HelferprofilFehler.KontoFehlt => Unauthorized(),
            HelferprofilFehler.SkillFehlt => Ungueltig("Fähigkeit fehlt",
                "Mindestens eine gültige Fähigkeit ist erforderlich.", "skillIds"),
            HelferprofilFehler.BetriebsnameFehlt => Ungueltig("Betriebsname fehlt",
                "Bitte den Namen des Betriebs angeben.", "betriebsname"),
            HelferprofilFehler.PostleitzahlUngueltig => Ungueltig("Postleitzahl ungültig",
                "Bitte eine vierstellige Postleitzahl angeben.", "plz"),
            HelferprofilFehler.OrtFehlt => Ungueltig("Ort fehlt", "Bitte den Ort angeben.", "ort"),
            HelferprofilFehler.PostleitzahlUnbekannt => Ungueltig("Postleitzahl unbekannt",
                "Diese Postleitzahl steht nicht im amtlichen Ortschaftenverzeichnis.", "plz"),
            _ => Ungueltig("Postleitzahl ausserhalb der Region",
                "Diese Postleitzahl liegt ausserhalb der Region Ihres Kontos.", "plz"),
        };
    }

    [HttpPut("verfuegbarkeit")]
    public async Task<IActionResult> Verfuegbarkeit(VerfuegbarkeitAnfrage eingabe, CancellationToken abbruch)
    {
        var verfuegbar = await profile.VerfuegbarkeitSetzenAsync(KontoId(), eingabe.JetztVerfuegbar, abbruch);
        return verfuegbar is null
            ? this.Fehler(StatusCodes.Status404NotFound, "Kein Helferprofil", "Es besteht noch kein Helferprofil.")
            : Ok(new { jetztVerfuegbar = verfuegbar.Value });
    }

    private Guid KontoId() => Guid.TryParse(User.FindFirstValue("sub"), out var id) ? id : Guid.Empty;

    private IActionResult Ungueltig(string titel, string meldung, string feld) =>
        this.Fehler(StatusCodes.Status400BadRequest, titel, meldung, feld);
}
