using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Soforthilfe.Api.Fehlerbehandlung;
using Soforthilfe.Api.Modelle;
using Soforthilfe.Dienste.Konten;
using Soforthilfe.Dienste.Sprachen;

namespace Soforthilfe.Api.Controllers;

/// <summary>
/// Sprachen der Oberfläche. Der Plattformbetrieb importiert neue Sprachen und Texte im laufenden Betrieb; sie gelten
/// sofort, ohne Neustart und ohne Deployment. Lesen ist ohne Anmeldung möglich, weil die Oberfläche die Texte
/// vor der Anmeldung braucht.
/// </summary>
[ApiController]
[Route("api/sprachen")]
public sealed class SprachenController(ISprachDienst sprachen) : ControllerBase
{
    // Aktive importierte Sprachen für den Umschalter.
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<SprachAntwort>>> Aktive(CancellationToken abbruch) =>
        Ok((await sprachen.ListeAsync(nurAktive: true, abbruch)).Select(SprachAntwort.Aus).ToList());

    // Alle importierten Sprachen, auch ausgeschaltete.
    [HttpGet("verwaltung")]
    [Authorize(Roles = Rollen.Plattformbetrieb)]
    public async Task<ActionResult<IReadOnlyList<SprachAntwort>>> Alle(CancellationToken abbruch) =>
        Ok((await sprachen.ListeAsync(nurAktive: false, abbruch)).Select(SprachAntwort.Aus).ToList());

    // Texte einer aktiven Sprache; die Oberfläche legt sie über ihre mitgelieferten Texte.
    [HttpGet("{code}/texte")]
    public async Task<IActionResult> Texte(string code, CancellationToken abbruch)
    {
        var texte = await sprachen.TexteAsync(code.ToLowerInvariant(), abbruch);
        if (texte is null)
        {
            return this.Fehler(StatusCodes.Status404NotFound, "Sprache nicht gefunden",
                "Für diese Sprache sind keine aktiven Texte vorhanden.");
        }

        Response.Headers.CacheControl = "no-cache";
        return Content(texte, "application/json; charset=utf-8");
    }

    // Import oder Ersatz einer Sprache; Grösse begrenzt, damit grosse Uploads die Schnittstelle nicht belasten.
    [HttpPut("{code}")]
    [Authorize(Roles = Rollen.Plattformbetrieb)]
    [Consumes("application/json")]
    [RequestSizeLimit(1_000_000)]
    public async Task<IActionResult> Importieren(string code, SprachImport eingabe, CancellationToken abbruch)
    {
        var ergebnis = await sprachen.ImportierenAsync(code.ToLowerInvariant(), eingabe.Name, eingabe.Aktiv, eingabe.Texte,
            KontoId(), abbruch);

        return ergebnis.Fehler switch
        {
            null => Ok(SprachAntwort.Aus(ergebnis.Sprache!)),
            SprachdateiFehler.CodeUngueltig => Ungueltig("Der Code besteht aus zwei Kleinbuchstaben, zum Beispiel «it».", "code"),
            SprachdateiFehler.NameUngueltig => Ungueltig("Der Name hat 2 bis 40 Zeichen, zum Beispiel «Italiano».", "name"),
            _ => Ungueltig(Meldung(ergebnis.Fehler ?? SprachdateiFehler.Leer, ergebnis.Pfad), "texte"),
        };
    }

    [HttpPut("{code}/aktiv")]
    [Authorize(Roles = Rollen.Plattformbetrieb)]
    public async Task<IActionResult> Schalten(string code, SprachSchalter eingabe, CancellationToken abbruch)
    {
        var sprache = await sprachen.AktivSetzenAsync(code.ToLowerInvariant(), eingabe.Aktiv, abbruch);
        return sprache is null ? NichtGefunden() : Ok(SprachAntwort.Aus(sprache));
    }

    [HttpDelete("{code}")]
    [Authorize(Roles = Rollen.Plattformbetrieb)]
    public async Task<IActionResult> Loeschen(string code, CancellationToken abbruch) =>
        await sprachen.LoeschenAsync(code.ToLowerInvariant(), abbruch) ? NoContent() : NichtGefunden();

    private static string Meldung(SprachdateiFehler fehler, string? pfad)
    {
        var ort = pfad is null ? string.Empty : $" Betroffen: «{pfad}».";
        return fehler switch
        {
            SprachdateiFehler.KeinObjekt => "Die Texte müssen ein JSON-Objekt sein.",
            SprachdateiFehler.Leer => "Die Sprachdatei enthält keine Texte.",
            SprachdateiFehler.SchluesselUngueltig => "Schlüssel bestehen aus Buchstaben, Ziffern, - und _." + ort,
            SprachdateiFehler.ZuTief => $"Höchstens {Sprachdatei.MaxTiefe} Ebenen sind erlaubt." + ort,
            SprachdateiFehler.KeinText => "Jeder Eintrag ist entweder ein Text oder eine Gruppe von Texten." + ort,
            SprachdateiFehler.TextZuLang => $"Ein Text hat höchstens {Sprachdatei.MaxTextlaenge} Zeichen." + ort,
            SprachdateiFehler.ZeichenUnzulaessig => "Die Zeichen < > @ | sind in Texten nicht erlaubt." + ort,
            SprachdateiFehler.PlatzhalterUngueltig => "Geschweifte Klammern nur als Platzhalter wie {name}." + ort,
            _ => $"Höchstens {Sprachdatei.MaxTexte} Texte sind erlaubt.",
        };
    }

    private ObjectResult Ungueltig(string text, string feld) =>
        this.Fehler(StatusCodes.Status400BadRequest, "Sprachdatei ungültig", text, feld);

    private ObjectResult NichtGefunden() =>
        this.Fehler(StatusCodes.Status404NotFound, "Sprache nicht gefunden", "Diese Sprache ist nicht importiert.");

    private Guid KontoId() => Guid.TryParse(User.FindFirstValue("sub"), out var id) ? id : Guid.Empty;
}
