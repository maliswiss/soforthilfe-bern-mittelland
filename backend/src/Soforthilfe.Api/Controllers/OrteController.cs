using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Soforthilfe.Api.Fehlerbehandlung;
using Soforthilfe.Api.Modelle;
using Soforthilfe.Dienste.Orte;

namespace Soforthilfe.Api.Controllers;

/// <summary>
/// Postleitzahl, Ort, Gemeinde und Region aus dem amtlichen Ortschaftenverzeichnis. Ohne Anmeldung nutzbar,
/// weil die Region einer Person schon bei der Registrierung aus der Postleitzahl folgt. Ein freigegebener Standort
/// wird nur hier, über die eigene Schnittstelle, in Postleitzahl und Ort übersetzt; weder gespeichert noch protokolliert.
/// </summary>
[ApiController]
[Route("api/orte")]
[AllowAnonymous]
public sealed partial class OrteController(IOrtDienst orte) : ControllerBase
{
    [GeneratedRegex("^[1-9][0-9]{3}$")]
    private static partial Regex PostleitzahlMuster();

    // Ortschaften zu einer Postleitzahl, zum Beispiel für die Ergänzung des Orts und die Region.
    [HttpGet]
    public async Task<IActionResult> NachPlz([FromQuery] string? plz, CancellationToken abbruch)
    {
        var gesucht = plz?.Trim() ?? string.Empty;
        if (!PostleitzahlMuster().IsMatch(gesucht))
        {
            return this.Fehler(StatusCodes.Status400BadRequest, "Postleitzahl ungültig",
                "Bitte eine vierstellige Postleitzahl angeben.", "plz");
        }

        var liste = await orte.NachPlzAsync(gesucht, abbruch);
        return Ok(liste.Select(OrtAntwort.Aus).ToList());
    }

    // Postleitzahl und Ort zum freigegebenen Standort. Nur POST, damit die Koordinaten nie in der Adresse stehen.
    [HttpPost("umgebung")]
    public async Task<IActionResult> Umgebung(StandortAnfrage standort, CancellationToken abbruch)
    {
        var treffer = await orte.NaechsteAsync(standort.Breitengrad!.Value, standort.Laengengrad!.Value, abbruch);

        return treffer is null
            ? this.Fehler(StatusCodes.Status404NotFound, "Kein Ort zum Standort",
                "Zu diesem Standort wurde keine Schweizer Postleitzahl gefunden. Bitte Postleitzahl und Ort eintragen.")
            : Ok(OrtAntwort.Aus(treffer));
    }
}
