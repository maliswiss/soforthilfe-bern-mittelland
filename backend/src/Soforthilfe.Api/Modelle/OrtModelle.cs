using Soforthilfe.Dienste.Orte;

namespace Soforthilfe.Api.Modelle;

/// <summary>
/// Ortschaft zu einer Postleitzahl. <c>Region</c> ist die Kennung der zuständigen Region, leer ausserhalb der Regionen;
/// <c>PlzUeberMehrereGemeinden</c> ist wahr, wenn die Postleitzahl mehrere Gemeinden berührt.
/// </summary>
public sealed record OrtAntwort(
    string Plz,
    string Ort,
    string Gemeinde,
    string Kanton,
    bool PlzUeberMehrereGemeinden,
    string? Region)
{
    public static OrtAntwort Aus(OrtInfo info) =>
        new(info.Plz, info.Ort, info.Gemeinde, info.Kanton, info.PlzUeberMehrereGemeinden, info.Region);
}
