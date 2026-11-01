using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Soforthilfe.Api.Fehlerbehandlung;

/// <summary>
/// Fängt unbehandelte Ausnahmen ab, protokolliert sie und antwortet einheitlich mit Status 500.
/// </summary>
public sealed class ZentraleFehlerbehandlung(
    IProblemDetailsService problemDetailsDienst,
    ILogger<ZentraleFehlerbehandlung> protokoll) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext kontext, Exception ausnahme, CancellationToken abbruch)
    {
        var verfolgung = Activity.Current?.Id ?? kontext.TraceIdentifier;

        // Protokolliert werden Methode, Pfad und Verfolgungsnummer, aber keine Eingaben der Nutzenden.
        protokoll.LogError(
            ausnahme,
            "Unbehandelter Fehler bei {Methode} {Pfad} (Verfolgung {Verfolgung})",
            kontext.Request.Method,
            kontext.Request.Path,
            verfolgung);

        kontext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        // Die Antwort enthält keine technischen Details, nur die Verfolgungsnummer für die Fehlersuche.
        return await problemDetailsDienst.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = kontext,
            Exception = ausnahme,
            ProblemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Interner Fehler",
                Detail = "Die Anfrage konnte nicht verarbeitet werden.",
                Extensions = { ["verfolgung"] = verfolgung },
            },
        });
    }
}
