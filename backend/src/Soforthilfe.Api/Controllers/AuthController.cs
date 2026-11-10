using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Soforthilfe.Api.Anmeldung;
using Soforthilfe.Api.Fehlerbehandlung;
using Soforthilfe.Api.Modelle;
using Soforthilfe.Dienste.Konten;
using Soforthilfe.Dienste.Regionen;

namespace Soforthilfe.Api.Controllers;

/// <summary>
/// Registrierung, Anmeldung und eigenes Konto. Die Region der Registrierung (Feld «mandant») bestimmt die Oberfläche
/// aus der Postleitzahl; die Anmeldung kommt ohne Region aus.
/// </summary>
[ApiController]
[Route("api/auth")]
public sealed class AuthController(IKontoDienst konten, IRegionDienst regionen, TokenDienst tokens) : ControllerBase
{
    // Neues Konto in der angegebenen Region; die Antwort meldet die Person gleich an.
    [HttpPost("registrierung")]
    public async Task<IActionResult> Registrieren(RegistrierungAnfrage anfrage, CancellationToken abbruch)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return this.Fehler(StatusCodes.Status400BadRequest, "Bereits angemeldet", "Für ein neues Konto bitte zuerst abmelden.");
        }

        if (await RegionFestlegenAsync(anfrage.Mandant, abbruch) is { } abweisung)
        {
            return abweisung;
        }

        // Selbst registrieren können sich nur Hilfesuchende und Helfende.
        var rolle = anfrage.Kontoart switch
        {
            Kontoart.Hilfesuchende => Rollen.Hilfesuchende,
            Kontoart.Helfende => Rollen.Helfende,
            _ => null,
        };
        if (rolle is null)
        {
            return this.Fehler(StatusCodes.Status400BadRequest, "Kontoart ungültig",
                "Zulässig sind «Hilfe suchen» und «Hilfe anbieten».", "kontoart");
        }

        var ergebnis = await konten.RegistrierenAsync(
            new RegistrierungsDaten(anfrage.EMail, anfrage.Passwort, anfrage.Anzeigename, rolle, anfrage.Telefon),
            abbruch);

        return ergebnis.Fehler switch
        {
            null => Ok(Antwort(ergebnis.Konto!)),
            KontoFehler.RegionFehlt => this.Fehler(StatusCodes.Status400BadRequest, "Unbekannte Region",
                "Unbekannte oder inaktive Region.", "mandant"),
            KontoFehler.KostenpflichtigeNummer => this.Fehler(StatusCodes.Status400BadRequest, "Kostenpflichtige Telefonnummer",
                "Nummern mit 0900 (sowie 0901 und 0906) sind kostenpflichtig und werden nicht akzeptiert.", "telefon"),
            KontoFehler.TelefonUngueltig => this.Fehler(StatusCodes.Status400BadRequest, "Telefonnummer ungültig",
                "Bitte eine Schweizer Telefonnummer angeben, zum Beispiel 031 123 45 67 oder +41 31 123 45 67.", "telefon"),
            KontoFehler.RolleNichtErlaubt => this.Fehler(StatusCodes.Status400BadRequest, "Kontoart ungültig",
                "Zulässig sind «Hilfe suchen» und «Hilfe anbieten».", "kontoart"),
            _ => this.Fehler(StatusCodes.Status409Conflict, "E-Mail-Adresse vergeben",
                "Diese E-Mail-Adresse ist bereits registriert.", "eMail"),
        };
    }

    // Anmeldung in der Region des Kontos. Gleiche Antwort für unbekannte Adresse, falsches Passwort und fremde Region.
    [HttpPost("anmeldung")]
    public async Task<IActionResult> Anmelden(AnmeldungAnfrage anfrage, CancellationToken abbruch)
    {
        var konto = await konten.AnmeldenAsync(anfrage.EMail, anfrage.Passwort, anfrage.Mandant, abbruch);

        return konto is null
            ? this.Fehler(StatusCodes.Status401Unauthorized, "Anmeldung fehlgeschlagen", "E-Mail-Adresse oder Passwort ist falsch.")
            : Ok(Antwort(konto));
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
        return konto is null ? NotFound() : Ok(BenutzerAntwort.Aus(konto));
    }

    private AnmeldungAntwort Antwort(KontoInfo konto)
    {
        var token = tokens.Erstellen(konto);
        return new AnmeldungAntwort(token.Token, token.GueltigBis, BenutzerAntwort.Aus(konto));
    }

    // Legt die Region für die Registrierung fest. Ein Kopf X-Region muss zur angegebenen Region passen.
    private async Task<IActionResult?> RegionFestlegenAsync(string mandant, CancellationToken abbruch)
    {
        var kennung = (mandant ?? string.Empty).Trim().ToLowerInvariant();

        if (regionen.Aktuelle is { } aktuelle)
        {
            return aktuelle.Kennung == kennung
                ? null
                : this.Fehler(StatusCodes.Status400BadRequest, "Region passt nicht",
                    "Die angegebene Region passt nicht zum Kopf X-Region.", "mandant");
        }

        return await regionen.SetzeAktuelleAsync(kennung, abbruch) is null
            ? this.Fehler(StatusCodes.Status400BadRequest, "Unbekannte Region", "Unbekannte oder inaktive Region.", "mandant")
            : null;
    }
}
