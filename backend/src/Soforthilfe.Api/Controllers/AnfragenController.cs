using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Soforthilfe.Api.Modelle;
using Soforthilfe.Dienste.Anfragen;
using Soforthilfe.Dienste.Konten;
namespace Soforthilfe.Api.Controllers;

[ApiController]
[Route("api/anfragen")]
public sealed class AnfragenController(AnfrageDienst anfragen) : ControllerBase
{
    [Authorize(Roles = Rollen.Hilfesuchende)]
    [HttpPost]
    public async Task<IActionResult> Erfassen(NeueAnfrage eingabe, CancellationToken abbruch)
    {
        if (!Guid.TryParse(User.FindFirstValue("sub"), out var kontoId)) return Unauthorized();
        var ergebnis = await anfragen.ErfassenAsync(new AnfrageDaten(kontoId, eingabe.KategorieId, eingabe.Freitext, eingabe.Postleitzahl), abbruch);
        return ergebnis.Fehler switch
        {
            null => Created($"/api/anfragen/{ergebnis.Anfrage!.Id}", ergebnis.Anfrage),
            AnfrageFehler.RegionFehlt => Problem(statusCode: 400, title: "Keine Region gewählt", detail: "Die Anfrage benötigt eine zugeordnete Region."),
            AnfrageFehler.KategorieNichtGefunden => Problem(statusCode: 400, title: "Kategorie unbekannt", detail: "Die gewählte Kategorie existiert nicht."),
            _ => Problem(statusCode: 400, title: "Ungültige Auswahl", detail: "Genau eine Kategorie oder ein Freitext sowie eine vierstellige Postleitzahl sind erforderlich."),
        };
    }
}
