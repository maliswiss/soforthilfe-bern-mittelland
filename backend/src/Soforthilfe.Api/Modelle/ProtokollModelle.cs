using Soforthilfe.Dienste.Protokoll;

namespace Soforthilfe.Api.Modelle;

/// <summary>
/// Protokolleintrag in der Antwort. Enthält keine Personendaten: kein Konto, keine IP-Adresse, keine Eingaben.
/// </summary>
public sealed record ProtokollEintragAntwort(
    long Id,
    string Aktion,
    string? Objekt,
    string? ObjektId,
    string? Detail,
    DateTimeOffset Zeitpunkt,
    string? Region)
{
    public static ProtokollEintragAntwort Aus(ProtokollInfo info) =>
        new(info.Id, info.Aktion, info.Objekt, info.ObjektId, info.Detail, info.Zeitpunkt, info.Region);
}
