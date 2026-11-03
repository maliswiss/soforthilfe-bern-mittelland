using System.ComponentModel.DataAnnotations;
namespace Soforthilfe.Api.Modelle;
public sealed record NeueAnfrage([StringLength(2000, MinimumLength = 3)] string? Freitext, Guid? KategorieId, [Required, RegularExpression("^[0-9]{4}$")] string Postleitzahl);
