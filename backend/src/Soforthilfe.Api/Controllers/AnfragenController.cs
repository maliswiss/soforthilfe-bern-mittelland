using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Soforthilfe.Api.Fehlerbehandlung;
using Soforthilfe.Api.Modelle;
using Soforthilfe.Datenzugriff.Modell;
using Soforthilfe.Dienste.Anfragen;
using Soforthilfe.Dienste.Konten;
using Soforthilfe.Dienste.Matching;
using Soforthilfe.Dienste.Orte;
using Soforthilfe.Dienste.Regionen;
using Soforthilfe.Dienste.Verstaendnis;

namespace Soforthilfe.Api.Controllers;

/// <summary>
/// Anfragen erfassen und lesen. Die Region kommt aus dem Token; der Datenzugriff trennt die Regionen.
/// </summary>
[ApiController]
[Route("api/anfragen")]
public sealed class AnfragenController(
    IAnfrageDienst anfragen,
    IVerstaendnisDienst verstaendnis,
    IKandidatenSuche kandidaten,
    IMatchingDienst matching,
    IOrtDienst orte,
    IRegionDienst regionen) : ControllerBase
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

    // Unverbindliche Vorschau, auch ohne Anmeldung, mit gestaffelter Preisgabe: ohne Anmeldung ohne Namen, angemeldet
    // mit Namen; Telefonnummern erst nach der Zusage, nie in der Vorschau. Ohne Anmeldung bestimmt die Postleitzahl die
    // Region, wie bei der Registrierung; angemeldet gilt die Region des Kontos.
    [HttpPost("vorschau")]
    public async Task<IActionResult> Vorschau(VorschauEingabe eingabe, CancellationToken abbruch)
    {
        var plz = eingabe.Plz.Trim();
        if (plz.Length != 4 || !plz.All(char.IsAsciiDigit))
        {
            return Ungueltig("Postleitzahl ungültig", "Bitte eine vierstellige Postleitzahl angeben.", "plz");
        }

        if (eingabe.Prioritaet is < 0 or > 2)
        {
            return Ungueltig("Dringlichkeit ungültig", "Zulässig sind tief, mittel und hoch.", "prioritaet");
        }

        var ortschaften = await orte.NachPlzAsync(plz, abbruch);
        var ort = string.IsNullOrWhiteSpace(eingabe.Ort) ? ortschaften.FirstOrDefault()?.Ort : eingabe.Ort.Trim();
        var angemeldet = User.Identity?.IsAuthenticated == true;

        string? regionFuerSuche = null;
        if (!angemeldet || User.IsInRole(Rollen.Plattformbetrieb))
        {
            if (ortschaften.Count == 0)
            {
                return Ungueltig("Postleitzahl unbekannt",
                    "Diese Postleitzahl steht nicht im amtlichen Ortschaftenverzeichnis.", "plz");
            }

            var kennung = ortschaften.Select(o => o.Region).FirstOrDefault(r => r is not null);
            if (kennung is null)
            {
                return Ungueltig("Keine Region", $"Für die Postleitzahl {plz} gibt es noch keine Region auf der Plattform.", "plz");
            }

            if (angemeldet)
            {
                // Der Plattformbetrieb arbeitet regionsübergreifend; gesucht wird in der Region der Postleitzahl.
                regionFuerSuche = kennung;
            }
            else if (regionen.Aktuelle is { } gewaehlt && gewaehlt.Kennung != kennung)
            {
                return Ungueltig("Postleitzahl ausserhalb der Region", "Diese Postleitzahl liegt ausserhalb der gewählten Region.", "plz");
            }
            else if (regionen.Aktuelle is null && await regionen.SetzeAktuelleAsync(kennung, abbruch) is null)
            {
                return Ungueltig("Keine Region", $"Für die Postleitzahl {plz} gibt es noch keine Region auf der Plattform.", "plz");
            }
        }

        var ergebnis = await matching.VorschauAsync(
            new VorschauKriterien(eingabe.KategorieId, plz, ort, (Prioritaet?)eingabe.Prioritaet, regionFuerSuche), abbruch);
        return ergebnis is null
            ? Ungueltig("Kategorie unbekannt", "Unbekannte Kategorie.", "kategorieId")
            : Ok(VorschauAntwort.Aus(ergebnis, plz, ort, angemeldet));
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

    // Kandidaten der Kandidatensuche, nur für Koordination und Verwaltung der Region der Anfrage.
    [Authorize(Roles = Rollen.KontenEinsehen)]
    [HttpGet("{id:guid}/kandidaten")]
    public async Task<IActionResult> Kandidaten(Guid id, CancellationToken abbruch)
    {
        var ergebnis = await kandidaten.FuerAnfrageAsync(id, abbruch);
        return ergebnis is null ? NotFound() : Ok(KandidatenAntwort.Aus(ergebnis));
    }

    // Vorschläge des Smart-Matchings: Hilfesuchende zu eigenen Anfragen, Koordination und Verwaltung in der Region.
    [Authorize(Roles = Lesende)]
    [HttpGet("{id:guid}/vorschlaege")]
    public async Task<IActionResult> Vorschlaege(Guid id, CancellationToken abbruch)
    {
        var vorschlaege = await matching.VorschlaegeAsync(id, NurEigene(), abbruch);
        return vorschlaege is null ? NotFound() : Ok(vorschlaege.Select(VorschlagAntwort.Aus).ToList());
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
