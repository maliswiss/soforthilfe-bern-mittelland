using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Soforthilfe.Api.Fehlerbehandlung;
using Soforthilfe.Api.Modelle;
using Soforthilfe.Dienste.Konten;
using Soforthilfe.Dienste.Nachweise;

namespace Soforthilfe.Api.Controllers;

/// <summary>
/// Nachweise zum eigenen Helferprofil. Angenommen wird nur JSON mit überprüfbaren Angaben; Uploads gibt es nicht.
/// </summary>
[ApiController]
[Route("api/verifizierungen")]
[Authorize(Roles = Rollen.Helfende)]
public sealed class VerifizierungenController(IVerifizierungDienst nachweise) : ControllerBase
{
    [HttpPost]
    [Consumes("application/json")]
    public async Task<IActionResult> Einreichen(VerifizierungAnfrage eingabe, CancellationToken abbruch)
    {
        var ergebnis = await nachweise.EinreichenAsync(KontoId(), eingabe.Art, eingabe.Nachweiswert, abbruch);

        return ergebnis.Fehler switch
        {
            null => Ok(VerifizierungAntwort.Aus(ergebnis.Nachweis!)),
            VerifizierungFehler.KeinHelferprofil => Ungueltig("Kein Helferprofil",
                "Es besteht noch kein Helferprofil.", null),
            VerifizierungFehler.Gesperrt => this.Fehler(StatusCodes.Status403Forbidden, "Profil gesperrt",
                "Das Helferprofil ist gesperrt. Bitte wenden Sie sich an die Koordination."),
            VerifizierungFehler.ArtNichtUnterstuetzt => Ungueltig("Nachweisart nicht unterstützt",
                "Diese Nachweisart wird nicht angenommen.", "art"),
            VerifizierungFehler.NachweiswertFehlt => Ungueltig("Nachweis fehlt",
                "Für diese Nachweisart ist ein Wert erforderlich.", "nachweiswert"),
            _ => Ungueltig("UID ungültig",
                "Bitte eine gültige UID-Nummer im Format CHE-123.456.789 angeben.", "nachweiswert"),
        };
    }

    [HttpGet("meine")]
    public async Task<ActionResult<IReadOnlyList<VerifizierungAntwort>>> Meine(CancellationToken abbruch) =>
        Ok((await nachweise.MeineAsync(KontoId(), abbruch)).Select(VerifizierungAntwort.Aus).ToList());

    private Guid KontoId() => Guid.TryParse(User.FindFirstValue("sub"), out var id) ? id : Guid.Empty;

    private IActionResult Ungueltig(string titel, string meldung, string? feld) =>
        this.Fehler(StatusCodes.Status400BadRequest, titel, meldung, feld);
}
