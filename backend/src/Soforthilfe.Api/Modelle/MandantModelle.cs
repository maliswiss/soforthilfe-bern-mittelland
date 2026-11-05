using System.ComponentModel.DataAnnotations;
using Soforthilfe.Dienste.Regionen;

namespace Soforthilfe.Api.Modelle;

/// <summary>
/// Öffentliche Angaben einer aktiven Region (Auswahl bei Anmeldung und Registrierung).
/// </summary>
public sealed record MandantAntwort(string Schluessel, string Name, string Standardsprache)
{
    public static MandantAntwort Aus(RegionInfo region) => new(region.Kennung, region.Name, "de");
}

/// <summary>
/// Neue Region im laufenden Betrieb, ohne Deployment. Der Schlüssel besteht aus Kleinbuchstaben,
/// Ziffern und einzelnen Bindestrichen, zum Beispiel «biel-seeland».
/// </summary>
public sealed record NeuerMandant(
    [Required, StringLength(50, MinimumLength = 2), RegularExpression("^[a-z0-9]+(-[a-z0-9]+)*$")] string Schluessel,
    [Required, StringLength(100, MinimumLength = 2)] string Name);
