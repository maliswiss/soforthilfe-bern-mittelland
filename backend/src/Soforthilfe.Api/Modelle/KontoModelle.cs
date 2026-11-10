using System.ComponentModel.DataAnnotations;
using Soforthilfe.Dienste.Konten;

namespace Soforthilfe.Api.Modelle;

/// <summary>
/// Art des Kontos bei der Registrierung: 0 Hilfe suchen, 1 Hilfe anbieten (Privatperson oder Betrieb).
/// </summary>
public enum Kontoart
{
    Hilfesuchende = 0,
    Helfende = 1,
}

/// <summary>
/// Eingabe für die Registrierung. Mandant ist die Kennung der Region, die die Oberfläche aus der Postleitzahl bestimmt.
/// Die Prüfung der Felder erfolgt vor dem Aufruf des Controllers.
/// </summary>
public sealed record RegistrierungAnfrage(
    [Required, EmailAddress, StringLength(254)] string EMail,
    [Required, StringLength(128, MinimumLength = 10)] string Passwort,
    [Required, StringLength(100, MinimumLength = 2)] string Anzeigename,
    [StringLength(40)] string? Telefon = null,
    Kontoart Kontoart = Kontoart.Hilfesuchende,
    [StringLength(50)] string Mandant = "bern-mittelland");

/// <summary>
/// Eingabe für die Anmeldung. Ohne «mandant» gilt die Region des Kontos; mit «mandant» muss sie dazu passen.
/// </summary>
public sealed record AnmeldungAnfrage(
    [Required, StringLength(254)] string EMail,
    [Required, StringLength(128)] string Passwort,
    [StringLength(50)] string? Mandant = null);

/// <summary>
/// Antwort auf Registrierung und Anmeldung: Token, Ablauf und Angaben zum Konto.
/// </summary>
public sealed record AnmeldungAntwort(string Token, DateTimeOffset GueltigBis, BenutzerAntwort Benutzer);

/// <summary>
/// Angaben zum eigenen Konto. MandantSchluessel und MandantName fehlen nur beim Plattformbetrieb.
/// </summary>
public sealed record BenutzerAntwort(
    Guid Id,
    string EMail,
    string Anzeigename,
    string? Telefon,
    IReadOnlyList<string> Rollen,
    bool HatHelferProfil,
    string? MandantSchluessel,
    string? MandantName)
{
    public static BenutzerAntwort Aus(KontoInfo konto) => new(
        konto.Id, konto.EMail, konto.Anzeigename, konto.Telefon, [konto.Rolle], konto.HatHelferprofil, konto.Region, konto.RegionName);
}

/// <summary>
/// Eintrag der Kontenliste für Koordination und Verwaltung, ohne Telefonnummer.
/// </summary>
public sealed record KontoUebersicht(Guid Id, string EMail, string Anzeigename, string Rolle, string? Region)
{
    public static KontoUebersicht Aus(KontoInfo konto) => new(konto.Id, konto.EMail, konto.Anzeigename, konto.Rolle, konto.Region);
}

/// <summary>
/// Neue Rolle für ein Konto.
/// </summary>
public sealed record RolleAnfrage([Required] string Rolle);
