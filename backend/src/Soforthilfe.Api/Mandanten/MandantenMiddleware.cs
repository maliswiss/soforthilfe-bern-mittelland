using Soforthilfe.Dienste.Regionen;

namespace Soforthilfe.Api.Mandanten;

/// <summary>
/// Legt zu Beginn jeder Anfrage die Region (Mandant) fest, aus dem Kopf «X-Region».
/// Ohne Region liefert der Datenzugriff keine regionsbezogenen Daten.
/// </summary>
public sealed class MandantenMiddleware(RequestDelegate weiter)
{
    public const string RegionKopf = "X-Region";

    public async Task InvokeAsync(HttpContext kontext, IRegionDienst regionen)
    {
        var kennung = kontext.Request.Headers[RegionKopf].ToString();

        if (!string.IsNullOrWhiteSpace(kennung)
            && await regionen.SetzeAktuelleAsync(kennung, kontext.RequestAborted) is null)
        {
            // Unbekannte oder inaktive Regionen werden abgewiesen, bevor irgendwelche Daten gelesen werden.
            await Abweisen(kontext, StatusCodes.Status404NotFound, "Unbekannte Region", "Die angegebene Region gibt es nicht oder sie ist nicht aktiv.");
            return;
        }

        await weiter(kontext);
    }

    private static Task Abweisen(HttpContext kontext, int status, string titel, string text) =>
        Results.Problem(statusCode: status, title: titel, detail: text).ExecuteAsync(kontext);
}
