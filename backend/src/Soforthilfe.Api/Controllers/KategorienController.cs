using Microsoft.AspNetCore.Mvc;
using Soforthilfe.Dienste.Kategorien;

namespace Soforthilfe.Api.Controllers;

/// <summary>
/// Kategorien der Startseite und Fähigkeiten aus den Stammdaten. Ohne Anmeldung abrufbar.
/// </summary>
[ApiController]
public sealed class KategorienController(IKategorienDienst kategorien) : ControllerBase
{
    // Mit ?bezahlpflichtig=true oder =false nur kostenpflichtige beziehungsweise kostenlose Kategorien (Anforderung A12).
    [HttpGet("api/kategorien")]
    public async Task<ActionResult<IReadOnlyList<KategorieInfo>>> Liste([FromQuery] bool? bezahlpflichtig, CancellationToken abbruch) =>
        Ok(await kategorien.ListeAsync(bezahlpflichtig, abbruch));

    [HttpGet("api/skills")]
    public async Task<ActionResult<IReadOnlyList<SkillInfo>>> Skills(CancellationToken abbruch) =>
        Ok(await kategorien.SkillsAsync(abbruch));
}
