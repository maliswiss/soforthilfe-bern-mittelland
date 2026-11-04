using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Soforthilfe.Api.Anmeldung;
using Soforthilfe.Api.Modelle;
using Soforthilfe.Dienste.Konten;

namespace Soforthilfe.Api.Controllers;

/// <summary>
/// Registrierung, Anmeldung und eigenes Konto.
/// </summary>
[ApiController]
[Route("api/konto")]
public sealed class KontoController(IKontoDienst konten, TokenDienst tokens) : ControllerBase
{
    // Registrierung in der Region aus dem Kopf «X-Region».
    [HttpPost("registrieren")]
    public async Task<IActionResult> Registrieren(RegistrierungAnfrage anfrage, CancellationToken abbruch)
    {
        var ergebnis = await konten.RegistrierenAsync(
            new RegistrierungsDaten(anfrage.EMail, anfrage.Passwort, anfrage.Anzeigename, anfrage.Rolle, anfrage.Telefon),
            abbruch);

        return ergebnis.Fehler switch
        {
            null => Created("/api/konto/ich", ergebnis.Konto),
            KontoFehler.RegionFehlt => Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Keine Region gewählt",
                detail: "Bitte die Kennung der Region im Kopf X-Region mitsenden."),
            KontoFehler.KostenpflichtigeNummer => Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Kostenpflichtige Telefonnummer nicht erlaubt",
                detail: "0900-, 0901- und 0906-Nummern können nicht verwendet werden."),
            KontoFehler.TelefonUngueltig => Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Telefonnummer ungültig",
                detail: "Bitte eine Schweizer Telefonnummer angeben, zum Beispiel 031 123 45 67 oder +41 31 123 45 67."),
            KontoFehler.RolleNichtErlaubt => Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Rolle nicht erlaubt",
                detail: "Selbst registrieren können sich nur Hilfesuchende und Helfende."),
            _ => Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "E-Mail-Adresse bereits registriert",
                detail: "Für diese E-Mail-Adresse besteht bereits ein Konto."),
        };
    }

    [HttpPost("anmelden")]
    public async Task<IActionResult> Anmelden(AnmeldungAnfrage anfrage, CancellationToken abbruch)
    {
        var konto = await konten.AnmeldenAsync(anfrage.EMail, anfrage.Passwort, abbruch);

        // Gleiche Meldung für unbekannte Adresse und falsches Passwort.
        return konto is null
            ? Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Anmeldung fehlgeschlagen",
                detail: "E-Mail-Adresse oder Passwort ist falsch.")
            : Ok(tokens.Erstellen(konto));
    }

    [Authorize]
    [HttpGet("ich")]
    public async Task<IActionResult> Ich(CancellationToken abbruch)
    {
        if (!Guid.TryParse(User.FindFirstValue("sub"), out var kontoId))
        {
            return Unauthorized();
        }

        var konto = await konten.LiesAsync(kontoId, abbruch);
        return konto is null ? NotFound() : Ok(konto);
    }
}
