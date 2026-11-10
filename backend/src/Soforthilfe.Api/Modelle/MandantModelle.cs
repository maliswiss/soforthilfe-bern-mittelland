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
/// Ziffern und einzelnen Bindestrichen, zum Beispiel «biel-seeland». Gemeinden sind BFS-Gemeindenummern;
/// über sie bestimmt die Postleitzahl einer Person die Region.
/// </summary>
public sealed record NeuerMandant(
    [Required, StringLength(50, MinimumLength = 2), RegularExpression("^[a-z0-9]+(-[a-z0-9]+)*$")] string Schluessel,
    [Required, StringLength(100, MinimumLength = 2)] string Name,
    [MaxLength(500)] int[]? Gemeinden = null) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext kontext)
    {
        if (Gemeinden?.Any(nummer => nummer is < 1 or > 9999) == true)
        {
            yield return new ValidationResult("BFS-Gemeindenummern liegen zwischen 1 und 9999.", [nameof(Gemeinden)]);
        }
    }
}
