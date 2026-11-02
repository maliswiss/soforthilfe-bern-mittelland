using System.ComponentModel.DataAnnotations;

namespace Soforthilfe.Api.Modelle;

/// <summary>
/// Eingabe für eine neue Region. Die Kennung besteht aus Kleinbuchstaben, Ziffern und Bindestrichen.
/// </summary>
public sealed record NeueRegionAnfrage(
    [Required, RegularExpression("^[a-z0-9-]{2,50}$")] string Kennung,
    [Required, StringLength(100, MinimumLength = 2)] string Name);
