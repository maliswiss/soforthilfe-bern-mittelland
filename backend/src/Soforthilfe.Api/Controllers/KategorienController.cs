using Microsoft.AspNetCore.Mvc;
using Soforthilfe.Dienste.Kategorien;

namespace Soforthilfe.Api.Controllers;

/// <summary>
/// Kategorien und Fähigkeiten aus den Stammdaten.
/// </summary>
[ApiController]
[Route("api/kategorien")]
public sealed class KategorienController(IKategorienDienst kategorien) : ControllerBase
{
    // Öffentlich. Mit ?kostenpflichtig=true oder =false nur kostenpflichtige beziehungsweise kostenlose Kategorien.
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<KategorieInfo>>> Liste([FromQuery] bool? kostenpflichtig, CancellationToken abbruch) =>
        Ok(await kategorien.ListeAsync(kostenpflichtig, abbruch));
}
