using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Soforthilfe.Api.Fehlerbehandlung;
using Soforthilfe.Api.Modelle;
using Soforthilfe.Dienste.Konten;
using Soforthilfe.Dienste.Regionen;

namespace Soforthilfe.Api.Controllers;

/// <summary>
/// Regionen (Mandanten). Eine neue Region entsteht als Datensatz im laufenden Betrieb und braucht
/// weder neuen Code noch ein Deployment.
/// </summary>
[ApiController]
[Route("api/mandanten")]
public sealed class MandantenController(IRegionDienst regionen) : ControllerBase
{
    // Öffentlich: aktive Regionen für die Auswahl bei Anmeldung und Registrierung.
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<MandantAntwort>>> Aktive(CancellationToken abbruch) =>
        Ok((await regionen.ListeAktiveAsync(abbruch)).Select(MandantAntwort.Aus).ToList());

    // Neue Region ohne Deployment: sofort nach dem Anlegen nutzbar. Nur für den Plattformbetrieb.
    [Authorize(Roles = Rollen.Plattformbetrieb)]
    [HttpPost]
    public async Task<IActionResult> Anlegen(NeuerMandant eingabe, CancellationToken abbruch)
    {
        var ergebnis = await regionen.AnlegenAsync(eingabe.Schluessel, eingabe.Name, eingabe.Gemeinden ?? [], abbruch);

        return ergebnis.Fehler switch
        {
            null => Created("/api/mandanten", MandantAntwort.Aus(ergebnis.Region!)),
            RegionFehler.GemeindenVergeben => this.Fehler(StatusCodes.Status409Conflict, "Gemeinden vergeben",
                "Mindestens eine Gemeinde gehört bereits zu einer anderen Region.", "gemeinden"),
            _ => this.Fehler(StatusCodes.Status409Conflict, "Region besteht", "Diese Region besteht bereits.", "schluessel"),
        };
    }
}
