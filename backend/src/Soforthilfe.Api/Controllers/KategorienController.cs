using Microsoft.AspNetCore.Mvc;
using Soforthilfe.Dienste.Anfragen;
namespace Soforthilfe.Api.Controllers;

[ApiController]
[Route("api/kategorien")]
public sealed class KategorienController(KategorienDienst kategorien) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<KategorieInfo>>> Liste([FromQuery] bool? kostenpflichtig, CancellationToken abbruch) =>
        Ok(await kategorien.ListeAsync(kostenpflichtig, abbruch));
}
