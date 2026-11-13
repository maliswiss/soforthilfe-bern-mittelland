using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Soforthilfe.Api.Fehlerbehandlung;
using Soforthilfe.Api.Modelle;
using Soforthilfe.Dienste.Anfragen;
using Soforthilfe.Dienste.Konten;
using Soforthilfe.Dienste.Verstaendnis;

namespace Soforthilfe.Api.Controllers;

/// <summary>
/// Anfragen erfassen und lesen. Die Region kommt aus dem Token; der Datenzugriff trennt die Regionen.
/// </summary>
[ApiController]
[Route("api/anfragen")]
public sealed class AnfragenController(IAnfrageDienst anfragen, IVerstaendnisDienst verstaendnis) : ControllerBase
{
    private const string Lesende = Rollen.Hilfesuchende + "," + Rollen.KontenEinsehen;

    // Verständnis-Schritt vor dem Erfassen, auch ohne Anmeldung: zuerst immer die Notfallprüfung.
    [HttpPost("verstehen")]
    public async Task<IActionResult> Verstehen(VerstaendnisAnfrage eingabe, CancellationToken abbruch)
    {
        if (eingabe.KategorieId is null && string.IsNullOrWhiteSpace(eingabe.Freitext))
        {
            return Ungueltig("Anfrage unvollständig", "Bitte eine Kategorie wählen oder das Anliegen beschreiben.", "freitext");
        }

        var ergebnis = await verstaendnis.VerstehenAsync(eingabe.KategorieId, eingabe.Freitext, abbruch);
        return Ok(VerstaendnisAntwort.Aus(ergebnis));
    }

    // Nur Hilfesuchende erfassen eine Anfrage.
    [Authorize(Roles = Rollen.Hilfesuchende)]
    [HttpPost]
    public async Task<IActionResult> Erstellen(AnfrageErstellen eingabe, CancellationToken abbruch)
    {
        if (KontoId() is not Guid kontoId)
        {
            return Unauthorized();
        }

        var ergebnis = await anfragen.ErfassenAsync(
            new AnfrageDaten(kontoId, eingabe.KategorieId, eingabe.Freitext, eingabe.Plz, eingabe.Ort,
                eingabe.Adresse, eingabe.Zeitfenster, eingabe.Prioritaet),
            abbruch);

        return ergebnis.Fehler switch
        {
            null => Created($"/api/anfragen/{ergebnis.Anfrage!.Id}", ergebnis.Anfrage),
            AnfrageFehler.RegionFehlt => Ungueltig("Keine Region", "Das Konto ist keiner Region zugeordnet.", null),
            AnfrageFehler.InhaltFehlt => Ungueltig("Anfrage unvollständig", "Bitte eine Kategorie wählen oder das Anliegen beschreiben.", "freitext"),
            AnfrageFehler.PostleitzahlUngueltig => Ungueltig("Postleitzahl ungültig", "Bitte eine vierstellige Postleitzahl angeben.", "plz"),
            AnfrageFehler.OrtFehlt => Ungueltig("Ort fehlt", "Bitte den Ort angeben.", "ort"),
            AnfrageFehler.PrioritaetUngueltig => Ungueltig("Dringlichkeit ungültig", "Zulässig sind tief, mittel und hoch.", "prioritaet"),
            AnfrageFehler.Notfall => NotfallStopp(ergebnis.NotfallGrund),
            AnfrageFehler.PostleitzahlUnbekannt => Ungueltig("Postleitzahl unbekannt",
                "Diese Postleitzahl steht nicht im amtlichen Ortschaftenverzeichnis.", "plz"),
            AnfrageFehler.PostleitzahlAusserhalbRegion => Ungueltig("Postleitzahl ausserhalb der Region",
                "Diese Postleitzahl liegt ausserhalb der Region Ihres Kontos.", "plz"),
            _ => Ungueltig("Kategorie unbekannt", "Unbekannte Kategorie.", "kategorieId"),
        };
    }

    // Eigene Anfragen, neueste zuerst.
    [Authorize(Roles = Rollen.Hilfesuchende)]
    [HttpGet("meine")]
    public async Task<ActionResult<IReadOnlyList<AnfrageInfo>>> Meine(CancellationToken abbruch) =>
        Ok(await anfragen.ListeAsync(KontoId() ?? Guid.Empty, abbruch));

    // Hilfesuchende sehen ihre eigenen Anfragen, Koordination und Verwaltung alle Anfragen ihrer Region.
    [Authorize(Roles = Lesende)]
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AnfrageInfo>>> Liste(CancellationToken abbruch) =>
        Ok(await anfragen.ListeAsync(NurEigene(), abbruch));

    // Eine fremde oder regionsfremde Anfrage gilt als nicht vorhanden.
    [Authorize(Roles = Lesende)]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Einzeln(Guid id, CancellationToken abbruch)
    {
        var anfrage = await anfragen.LiesAsync(id, NurEigene(), abbruch);
        return anfrage is null ? NotFound() : Ok(anfrage);
    }

    private Guid? KontoId() => Guid.TryParse(User.FindFirstValue("sub"), out var id) ? id : null;

    private Guid? NurEigene() => User.IsInRole(Rollen.Hilfesuchende) ? KontoId() ?? Guid.Empty : null;

    // Gesundheitlicher Notfall: Status 451, weil die Plattform keine medizinischen Leistungen vermitteln darf.
    // Die Oberfläche zeigt «notfall», «hinweis» und «grund» auf der Notfallseite an.
    private ObjectResult NotfallStopp(string? grund)
    {
        var antwort = this.Fehler(StatusCodes.Status451UnavailableForLegalReasons, "Notfall-Stopp", NotfallTexte.Hinweis);
        var problem = (ProblemDetails)antwort.Value!;
        problem.Extensions["notfall"] = true;
        problem.Extensions["hinweis"] = NotfallTexte.Hinweis;
        problem.Extensions["grund"] = grund;
        return antwort;
    }

    private ObjectResult Ungueltig(string titel, string text, string? feld) =>
        this.Fehler(StatusCodes.Status400BadRequest, titel, text, feld);
}
