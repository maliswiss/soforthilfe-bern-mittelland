using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Soforthilfe.Api.Modelle;
using Soforthilfe.Dienste.Anfragen;
using Soforthilfe.Dienste.Konten;

namespace Soforthilfe.Api.Controllers;

/// <summary>
/// Anfragen erfassen und lesen. Die Region kommt aus dem Token; der Datenzugriff trennt die Regionen.
/// </summary>
[ApiController]
[Route("api/anfragen")]
public sealed class AnfragenController(IAnfrageDienst anfragen) : ControllerBase
{
    private const string Lesende = Rollen.Hilfesuchende + "," + Rollen.KontenEinsehen;

    // Nur Hilfesuchende erfassen eine Anfrage.
    [Authorize(Roles = Rollen.Hilfesuchende)]
    [HttpPost]
    public async Task<IActionResult> Erfassen(NeueAnfrage eingabe, CancellationToken abbruch)
    {
        if (KontoId() is not Guid kontoId)
        {
            return Unauthorized();
        }

        var ergebnis = await anfragen.ErfassenAsync(
            new AnfrageDaten(kontoId, eingabe.KategorieId, eingabe.Freitext, eingabe.Postleitzahl),
            abbruch);

        return ergebnis.Fehler switch
        {
            null => Created($"/api/anfragen/{ergebnis.Anfrage!.Id}", ergebnis.Anfrage),
            AnfrageFehler.RegionFehlt => Fehler("Keine Region", "Das Konto ist keiner Region zugeordnet."),
            AnfrageFehler.InhaltFehlt => Fehler("Anfrage unvollständig", "Bitte eine Kategorie wählen oder die Lage in eigenen Worten beschreiben."),
            AnfrageFehler.PostleitzahlUngueltig => Fehler("Postleitzahl ungültig", "Bitte eine vierstellige Schweizer Postleitzahl angeben, zum Beispiel 3011."),
            _ => Fehler("Kategorie unbekannt", "Die gewählte Kategorie gibt es nicht."),
        };
    }

    // Hilfesuchende sehen ihre eigenen Anfragen, Koordination und Verwaltung alle Anfragen ihrer Region.
    [Authorize(Roles = Lesende)]
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AnfrageInfo>>> Liste(CancellationToken abbruch) =>
        Ok(await anfragen.ListeAsync(NurEigene(), abbruch));

    // Eine fremde oder regionsfremde Anfrage gilt als nicht vorhanden.
    [Authorize(Roles = Lesende)]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Lies(Guid id, CancellationToken abbruch)
    {
        var anfrage = await anfragen.LiesAsync(id, NurEigene(), abbruch);
        return anfrage is null ? NotFound() : Ok(anfrage);
    }

    private Guid? KontoId() => Guid.TryParse(User.FindFirstValue("sub"), out var id) ? id : null;

    private Guid? NurEigene() => User.IsInRole(Rollen.Hilfesuchende) ? KontoId() ?? Guid.Empty : null;

    private ObjectResult Fehler(string titel, string text) =>
        Problem(statusCode: StatusCodes.Status400BadRequest, title: titel, detail: text);
}
