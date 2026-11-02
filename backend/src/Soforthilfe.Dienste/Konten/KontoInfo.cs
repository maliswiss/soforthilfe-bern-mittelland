namespace Soforthilfe.Dienste.Konten;

/// <summary>
/// Angaben zu einem Konto, wie sie die Schnittstelle zurückgibt. Enthält nie den Passwort-Hash.
/// </summary>
public sealed record KontoInfo(Guid Id, string EMail, string Anzeigename, string Rolle, string? Region);

/// <summary>
/// Daten für eine Registrierung.
/// </summary>
public sealed record RegistrierungsDaten(string EMail, string Passwort, string Anzeigename, string Rolle);

/// <summary>
/// Gründe, warum eine Registrierung abgelehnt wird.
/// </summary>
public enum KontoFehler
{
    RegionFehlt,
    RolleNichtErlaubt,
    EMailVergeben,
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
