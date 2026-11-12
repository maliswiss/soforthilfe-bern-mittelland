using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Soforthilfe.Api.Fehlerbehandlung;
using Soforthilfe.Api.Modelle;
using Soforthilfe.Dienste.Konten;
using Soforthilfe.Dienste.Nachweise;

namespace Soforthilfe.Api.Controllers;

/// <summary>
/// Nachweise zum Helferprofil. Helfende reichen sie ein, die Koordination ihrer Region prüft und entscheidet.
/// Angenommen wird nur JSON mit überprüfbaren Angaben; Uploads gibt es nicht.
/// </summary>
[ApiController]
[Route("api/verifizierungen")]
[Authorize]
public sealed class VerifizierungenController(IVerifizierungDienst nachweise) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = Rollen.Helfende)]
    [Consumes("application/json")]
    public async Task<IActionResult> Einreichen(VerifizierungAnfrage eingabe, CancellationToken abbruch)
    {
        var ergebnis = await nachweise.EinreichenAsync(KontoId(), eingabe.Art, eingabe.Nachweiswert, abbruch);

        return ergebnis.Fehler switch
        {
            null => Ok(VerifizierungAntwort.Aus(ergebnis.Nachweis!)),
            VerifizierungFehler.KeinHelferprofil => Ungueltig("Kein Helferprofil",
                "Es besteht noch kein Helferprofil.", null),
            VerifizierungFehler.Gesperrt => this.Fehler(StatusCodes.Status403Forbidden, "Profil gesperrt",
                "Das Helferprofil ist gesperrt. Bitte wenden Sie sich an die Koordination."),
            VerifizierungFehler.ArtNichtUnterstuetzt => Ungueltig("Nachweisart nicht unterstützt",
                "Diese Nachweisart wird nicht angenommen.", "art"),
            VerifizierungFehler.NachweiswertFehlt => Ungueltig("Nachweis fehlt",
                "Für diese Nachweisart ist ein Wert erforderlich.", "nachweiswert"),
            VerifizierungFehler.Mehrwertnummer => Ungueltig("Kostenpflichtige Telefonnummer",
                "Nummern mit 0900 (sowie 0901 und 0906) sind kostenpflichtig und werden nicht akzeptiert.", "nachweiswert"),
            VerifizierungFehler.TelefonUngueltig => Ungueltig("Telefonnummer ungültig",
                "Bitte eine Schweizer Telefonnummer angeben, zum Beispiel 031 123 45 67 oder +41 31 123 45 67.", "nachweiswert"),
            VerifizierungFehler.EMailNichtKonto => Ungueltig("Andere E-Mail-Adresse",
                "Bestätigt wird die E-Mail-Adresse Ihres Kontos.", "nachweiswert"),
            VerifizierungFehler.ReferenzUngueltig => Ungueltig("Referenz ungültig",
                "Bitte Organisation oder Gemeinde und eine Kontaktperson nennen (5 bis 200 Zeichen).", "nachweiswert"),
            _ => Ungueltig("UID ungültig",
                "Bitte eine gültige UID-Nummer im Format CHE-123.456.789 angeben.", "nachweiswert"),
        };
    }

    [HttpGet("meine")]
    [Authorize(Roles = Rollen.Helfende)]
    public async Task<ActionResult<IReadOnlyList<VerifizierungAntwort>>> Meine(CancellationToken abbruch) =>
        Ok((await nachweise.MeineAsync(KontoId(), abbruch)).Select(VerifizierungAntwort.Aus).ToList());

    // Offene Nachweise der eigenen Region, älteste zuerst.
    [HttpGet("offen")]
    [Authorize(Roles = Rollen.Koordination)]
    public async Task<ActionResult<IReadOnlyList<PruefungAntwort>>> Offen(CancellationToken abbruch) =>
        Ok((await nachweise.OffeneAsync(abbruch)).Select(PruefungAntwort.Aus).ToList());

    // Freigabe oder Ablehnung; ein Nachweis einer anderen Region gilt als nicht vorhanden.
    [HttpPost("{id:guid}/entscheid")]
    [Authorize(Roles = Rollen.Koordination)]
    [Consumes("application/json")]
    public async Task<IActionResult> Entscheiden(Guid id, VerifizierungEntscheidAnfrage eingabe, CancellationToken abbruch)
    {
        var ergebnis = await nachweise.EntscheidenAsync(id, KontoId(), eingabe.Freigeben, eingabe.Bemerkung, abbruch);

        return ergebnis.Fehler switch
        {
            null => Ok(VerifizierungAntwort.Aus(ergebnis.Nachweis!)),
            EntscheidFehler.NichtGefunden => this.Fehler(StatusCodes.Status404NotFound, "Nachweis nicht gefunden",
                "Dieser Nachweis ist nicht vorhanden."),
            _ => this.Fehler(StatusCodes.Status409Conflict, "Bereits entschieden",
                "Über diesen Nachweis ist bereits entschieden."),
        };
    }

    private Guid KontoId() => Guid.TryParse(User.FindFirstValue("sub"), out var id) ? id : Guid.Empty;

    private IActionResult Ungueltig(string titel, string meldung, string? feld) =>
        this.Fehler(StatusCodes.Status400BadRequest, titel, meldung, feld);
}
