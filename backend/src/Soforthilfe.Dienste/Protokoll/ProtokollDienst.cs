using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Soforthilfe.Datenzugriff;
using Soforthilfe.Datenzugriff.Mandanten;
using Soforthilfe.Datenzugriff.Modell;

namespace Soforthilfe.Dienste.Protokoll;

/// <summary>
/// Protokoll in der Datenbank. Die Region kommt aus der laufenden Anfrage; der Datenzugriff trennt beim Lesen.
/// Gespeichert werden nur Vorgang, Bereich, Zeitpunkt und ein fester Detailtext, nie Konto, IP-Adresse oder Eingaben.
/// </summary>
public sealed class ProtokollDienst(
    SoforthilfeDbContext datenbank,
    IMandantenKontext mandant,
    ILogger<ProtokollDienst> logger) : IProtokollDienst
{
    public const int StandardAnzahl = 100;
    public const int HoechstAnzahl = 500;

    public async Task SchreibenAsync(string aktion, string? objekt, string? objektId, string? detail, CancellationToken abbruch)
    {
        var eintrag = new Protokolleintrag
        {
            RegionId = mandant.Uebergreifend ? null : mandant.RegionId,
            Aktion = aktion,
            Objekt = objekt,
            ObjektId = objektId,
            Detail = detail,
            Zeitpunkt = DateTimeOffset.UtcNow,
        };
        datenbank.Protokoll.Add(eintrag);

        try
        {
            await datenbank.SaveChangesAsync(abbruch);
        }
        catch (Exception fehler) when (fehler is not OperationCanceledException)
        {
            // Der Hinweis auf 144 hat Vorrang: Scheitert das Protokoll (zum Beispiel ohne Datenbank), läuft der Vorgang weiter.
            datenbank.Entry(eintrag).State = EntityState.Detached;
            logger.LogError(fehler, "Protokolleintrag {Aktion} konnte nicht gespeichert werden", aktion);
        }
    }

    public async Task<IReadOnlyList<ProtokollInfo>> ListeAsync(string? aktion, int anzahl, CancellationToken abbruch)
    {
        var abfrage = datenbank.Protokoll.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(aktion))
        {
            var gesucht = aktion.Trim();
            abfrage = abfrage.Where(p => p.Aktion == gesucht);
        }

        return await abfrage
            .OrderByDescending(p => p.Zeitpunkt)
            .ThenByDescending(p => p.Id)
            .Take(Math.Clamp(anzahl, 1, HoechstAnzahl))
            .Select(p => new ProtokollInfo(
                p.Id, p.Aktion, p.Objekt, p.ObjektId, p.Detail, p.Zeitpunkt, p.Region == null ? null : p.Region.Kennung))
            .ToListAsync(abbruch);
    }
}
