using Microsoft.AspNetCore.Mvc;
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

    // Region der laufenden Anfrage, festgelegt über den Kopf «X-Region».
    [HttpGet("aktuell")]
    public ActionResult<RegionInfo> Aktuell() =>
        regionen.Aktuelle is { } region
            ? Ok(region)
            : Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Keine Region gewählt",
                detail: "Bitte die Kennung der Region im Kopf X-Region mitsenden.");
}
