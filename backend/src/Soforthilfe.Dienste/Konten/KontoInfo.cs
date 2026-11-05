namespace Soforthilfe.Dienste.Konten;

/// <summary>
/// Angaben zu einem Konto, wie sie die Schnittstelle zurückgibt. Enthält nie den Passwort-Hash.
/// Region ist die Kennung (zum Beispiel «bern-mittelland»), RegionName der angezeigte Name.
/// </summary>
public sealed record KontoInfo(
    Guid Id,
    string EMail,
    string Anzeigename,
    string? Telefon,
    string Rolle,
    string? Region,
    string? RegionName,
    bool HatHelferprofil);

/// <summary>
/// Daten für eine Registrierung in einer Region.
/// </summary>
public sealed record RegistrierungsDaten(string EMail, string Passwort, string Anzeigename, string Rolle, string? Telefon);

/// <summary>
/// Gründe, warum eine Registrierung abgelehnt wird.
/// </summary>
public enum KontoFehler
{
    RegionFehlt,
    RolleNichtErlaubt,
    EMailVergeben,
    KostenpflichtigeNummer,
    TelefonUngueltig,
}

/// <summary>
/// Ergebnis einer Registrierung: entweder ein Konto oder ein Fehler.
/// </summary>
public sealed record KontoErgebnis(KontoInfo? Konto, KontoFehler? Fehler);

/// <summary>
/// Ergebnis beim Setzen einer Rolle.
/// </summary>
public enum RollenErgebnis
{
    Gesetzt,
    NichtGefunden,
    NichtErlaubt,
}
