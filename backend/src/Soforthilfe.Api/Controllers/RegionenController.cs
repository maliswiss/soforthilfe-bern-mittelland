using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Soforthilfe.Api.Modelle;
using Soforthilfe.Dienste.Konten;
using Soforthilfe.Dienste.Regionen;

namespace Soforthilfe.Api.Controllers;

/// <summary>
/// Regionen (Mandanten) der Plattform.
/// </summary>
[ApiController]
[Route("api/regionen")]
public sealed class RegionenController(IRegionDienst regionen) : ControllerBase
{
    // Öffentlich: Liste der aktiven Regionen für die Regionswahl.
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<RegionInfo>>> Liste(CancellationToken abbruch) =>
        Ok(await regionen.ListeAktiveAsync(abbruch));

    // Region der laufenden Anfrage, festgelegt über das Token oder den Kopf «X-Region».
    [HttpGet("aktuell")]
    public ActionResult<RegionInfo> Aktuell() =>
        regionen.Aktuelle is { } region
            ? Ok(region)
            : Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Keine Region gewählt",
                detail: "Bitte die Kennung der Region im Kopf X-Region mitsenden.");

    // Neue Region ohne Deployment: sofort nach dem Anlegen nutzbar. Nur für den Plattformbetrieb.
    [Authorize(Roles = Rollen.Plattformbetrieb)]
    [HttpPost]
    public async Task<IActionResult> Anlegen(NeueRegionAnfrage anfrage, CancellationToken abbruch)
    {
        var region = await regionen.AnlegenAsync(anfrage.Kennung, anfrage.Name, abbruch);

        return region is null
            ? Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Region bereits vorhanden",
                detail: "Für diese Kennung besteht bereits eine Region.")
            : Created("/api/regionen", region);
    }
}
