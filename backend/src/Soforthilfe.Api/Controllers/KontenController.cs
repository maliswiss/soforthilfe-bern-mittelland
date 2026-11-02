using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Soforthilfe.Api.Modelle;
using Soforthilfe.Dienste.Konten;

namespace Soforthilfe.Api.Controllers;

/// <summary>
/// Verwaltung der Konten einer Region. Die Rolle wird hier geprüft, die Region im Datenzugriff.
/// </summary>
[ApiController]
[Route("api/konten")]
public sealed class KontenController(IKontoDienst konten) : ControllerBase
{
    [Authorize(Roles = Rollen.KontenEinsehen)]
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<KontoInfo>>> Liste(CancellationToken abbruch) =>
        Ok(await konten.ListeAsync(abbruch));

    [Authorize(Roles = Rollen.RollenVergeben)]
    [HttpPut("{id:guid}/rolle")]
    public async Task<IActionResult> RolleSetzen(Guid id, RolleAnfrage anfrage, CancellationToken abbruch)
    {
        var ergebnis = await konten.RolleSetzenAsync(id, anfrage.Rolle, abbruch);

        return ergebnis switch
        {
            RollenErgebnis.Gesetzt => NoContent(),
            RollenErgebnis.NichtGefunden => Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Konto nicht gefunden",
                detail: "In der eigenen Region gibt es kein Konto mit dieser Kennung."),
            _ => Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Rolle nicht erlaubt",
                detail: "Vergeben werden können Hilfesuchende, Helfende, Koordination und Mandantenadministration."),
        };
    }
}
