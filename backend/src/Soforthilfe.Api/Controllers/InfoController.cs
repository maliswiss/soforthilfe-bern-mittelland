using Microsoft.AspNetCore.Mvc;
using Soforthilfe.Dienste.Info;

namespace Soforthilfe.Api.Controllers;

/// <summary>
/// Gibt Name und Version der Anwendung zurück.
/// </summary>
[ApiController]
[Route("api/info")]
public sealed class InfoController(IInfoDienst infoDienst) : ControllerBase
{
    // Der Controller enthält keine Fachlogik, er reicht nur das Ergebnis des Dienstes weiter.
    [HttpGet]
    public ActionResult<AnwendungsInfo> Lies() => Ok(infoDienst.LiesInfo());
}
