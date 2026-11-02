using System.ComponentModel.DataAnnotations;

namespace Soforthilfe.Api.Modelle;

/// <summary>
/// Eingabe für die Registrierung. Die Prüfung der Felder erfolgt vor dem Aufruf des Controllers.
/// </summary>
public sealed record RegistrierungAnfrage(
    [Required, EmailAddress, StringLength(254)] string EMail,
    [Required, StringLength(128, MinimumLength = 10)] string Passwort,
    [Required, StringLength(100, MinimumLength = 2)] string Anzeigename,
    [Required] string Rolle);

/// <summary>
/// Eingabe für die Anmeldung.
/// </summary>
public sealed record AnmeldungAnfrage(
    [Required, StringLength(254)] string EMail,
    [Required, StringLength(128)] string Passwort);
