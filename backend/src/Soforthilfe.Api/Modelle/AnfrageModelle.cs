using System.ComponentModel.DataAnnotations;

namespace Soforthilfe.Api.Modelle;

/// <summary>
/// Eingabe für eine neue Anfrage: Kategorie, Freitext oder beides, dazu die Postleitzahl des Einsatzorts.
/// </summary>
public sealed record NeueAnfrage(
    Guid? KategorieId,
    [StringLength(2000)] string? Freitext,
    [Required, StringLength(4, MinimumLength = 4)] string Postleitzahl);
