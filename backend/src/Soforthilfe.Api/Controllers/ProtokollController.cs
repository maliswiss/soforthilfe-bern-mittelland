using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Soforthilfe.Api.Modelle;
using Soforthilfe.Dienste.Konten;
using Soforthilfe.Dienste.Protokoll;

namespace Soforthilfe.Api.Controllers;

/// <summary>
/// Protokoll sicherheitsrelevanter Vorgänge. Koordination und Mandantenadministration sehen die eigene Region,
/// der Plattformbetrieb alle Regionen und die Einträge ohne Region.
/// </summary>
[ApiController]
[Route("api/protokoll")]
[Authorize(Roles = Rollen.ProtokollEinsehen)]
public sealed class ProtokollController(IProtokollDienst protokoll) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProtokollEintragAntwort>>> Liste(
        [FromQuery] string? aktion,
        [FromQuery] int anzahl = ProtokollDienst.StandardAnzahl,
        CancellationToken abbruch = default) =>
        Ok((await protokoll.ListeAsync(aktion, anzahl, abbruch)).Select(ProtokollEintragAntwort.Aus).ToList());
}
