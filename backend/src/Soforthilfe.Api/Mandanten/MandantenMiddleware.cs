using Soforthilfe.Dienste.Regionen;

namespace Soforthilfe.Api.Mandanten;

/// <summary>
/// Legt zu Beginn jeder Anfrage die Region (Mandant) fest.
/// Angemeldet: aus dem Token. Ohne Anmeldung: aus dem Kopf «X-Region».
/// Ohne Region liefert der Datenzugriff keine regionsbezogenen Daten.
/// </summary>
public sealed class MandantenMiddleware(RequestDelegate weiter)
{
    public const string RegionKopf = "X-Region";

    public async Task InvokeAsync(HttpContext kontext, IRegionDienst regionen)
    {
        var ausKopf = kontext.Request.Headers[RegionKopf].ToString().Trim().ToLowerInvariant();
        var kennung = ausKopf.Length > 0 ? ausKopf : null;

        if (kontext.User.Identity?.IsAuthenticated == true)
        {
            var ausToken = kontext.User.FindFirst("region")?.Value;

            // Angemeldete Personen arbeiten nur in der Region ihres Kontos.
            if (kennung is not null && ausToken is not null && kennung != ausToken)
            {
                await Abweisen(kontext, StatusCodes.Status403Forbidden, "Falsche Region", "Die Region im Kopf X-Region passt nicht zum Konto.");
                return;
            }

            kennung = ausToken ?? kennung;
        }

        if (kennung is not null && await regionen.SetzeAktuelleAsync(kennung, kontext.RequestAborted) is null)
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
