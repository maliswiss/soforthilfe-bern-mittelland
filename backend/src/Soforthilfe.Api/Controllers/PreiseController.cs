using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Soforthilfe.Api.Modelle;
using Soforthilfe.Dienste.Preise;

namespace Soforthilfe.Api.Controllers;

/// <summary>
/// Richtpreis aus Preisangaben berechnen (Anforderung A9), ohne etwas zu speichern. Die Oberfläche zeigt damit im
/// Profil, wie Anfragende den Preis werktags, nachts und am Wochenende sehen. Es gilt dieselbe Berechnung wie in den
/// Vorschlägen.
/// </summary>
[ApiController]
[Route("api/preise")]
[AllowAnonymous]
public sealed class PreiseController(IPreisrechner preisrechner) : ControllerBase
{
    public const int HoechstZeitpunkte = 10;

    [HttpPost("richtpreis")]
    public ActionResult<IReadOnlyList<PreisAntwort>> Richtpreis(RichtpreisAnfrage eingabe)
    {
        var angaben = new Preisangaben(eingabe.AnfahrtChf, eingabe.StundenansatzChf, eingabe.NachtzuschlagProzent,
            eingabe.WochenendzuschlagProzent, eingabe.MwstPflichtig);
        var zeitpunkte = eingabe.Zeitpunkte is { Count: > 0 } ? eingabe.Zeitpunkte : [DateTimeOffset.UtcNow];

        return Ok(zeitpunkte
            .Take(HoechstZeitpunkte)
            .Select(zeitpunkt => PreisAntwort.Aus(preisrechner.Berechnen(angaben, true, zeitpunkt)!))
            .ToList());
    }
}
