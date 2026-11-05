using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Soforthilfe.Api.Fehlerbehandlung;

/// <summary>
/// Einheitliche Fehlerantworten: ein Problem-Dokument mit «fehler» (Text für die Oberfläche)
/// und, wo sinnvoll, «feld» (das betroffene Eingabefeld).
/// </summary>
public static class FehlerAntwort
{
    public const string Pruefen = "Bitte prüfen Sie die Eingaben.";

    public static ObjectResult Fehler(this ControllerBase controller, int status, string titel, string text, string? feld = null)
    {
        var problem = controller.ProblemDetailsFactory.CreateProblemDetails(controller.HttpContext, status, titel, detail: text);
        if (feld is not null)
        {
            problem.Extensions["feld"] = feld;
        }

        return new ObjectResult(problem) { StatusCode = status, ContentTypes = { "application/problem+json" } };
    }

    // Ergänzt jedes Problem-Dokument um «fehler» und bei Eingabefehlern um «feld».
    public static void Ergaenzen(ProblemDetailsContext kontext)
    {
        var problem = kontext.ProblemDetails;

        if (problem is HttpValidationProblemDetails pruefung && pruefung.Errors.Count > 0)
        {
            problem.Extensions["feld"] = Feldname(pruefung.Errors.Keys.First());
            problem.Extensions["fehler"] = Pruefen;
            return;
        }

        if (!problem.Extensions.ContainsKey("fehler"))
        {
            problem.Extensions["fehler"] = problem.Detail ?? Standardtext(problem.Status);
        }
    }

    // «EMail» wird zu «eMail», «$.kontoart» zu «kontoart», wie die Felder im JSON heissen.
    private static string Feldname(string schluessel)
    {
        var name = schluessel.StartsWith("$.", StringComparison.Ordinal) ? schluessel[2..] : schluessel;
        return name.Length == 0 ? name : char.ToLowerInvariant(name[0]) + name[1..];
    }

    private static string Standardtext(int? status) => status switch
    {
        StatusCodes.Status400BadRequest => Pruefen,
        StatusCodes.Status401Unauthorized => "Bitte melden Sie sich an.",
        StatusCodes.Status403Forbidden => "Dafür fehlt die Berechtigung.",
        StatusCodes.Status404NotFound => "Nicht gefunden.",
        StatusCodes.Status405MethodNotAllowed => "Diese Methode ist hier nicht erlaubt.",
        _ => "Die Anfrage konnte nicht verarbeitet werden.",
    };
}
