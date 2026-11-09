namespace Soforthilfe.Dienste.Protokoll;

/// <summary>
/// Protokolleintrag, wie ihn die Schnittstelle zurückgibt. Region ist die Kennung, leer ohne Region.
/// </summary>
public sealed record ProtokollInfo(
    long Id,
    string Aktion,
    string? Objekt,
    string? ObjektId,
    string? Detail,
    DateTimeOffset Zeitpunkt,
    string? Region);
