namespace Soforthilfe.Dienste.Regionen;

/// <summary>
/// Öffentliche Angaben zu einer Region.
/// </summary>
public sealed record RegionInfo(string Kennung, string Name);

/// <summary>
/// Gründe, warum eine neue Region nicht angelegt wird.
/// </summary>
public enum RegionFehler
{
    KennungVergeben,
    GemeindenVergeben,
}

/// <summary>
/// Ergebnis beim Anlegen: entweder die neue Region oder ein Fehler.
/// </summary>
public sealed record RegionErgebnis(RegionInfo? Region, RegionFehler? Fehler);
