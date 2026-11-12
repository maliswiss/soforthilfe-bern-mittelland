using Soforthilfe.Datenzugriff.Modell;

namespace Soforthilfe.Dienste.Nachweise;

/// <summary>
/// Eingereichter Nachweis, wie ihn die helfende Person und die Koordination sehen.
/// </summary>
public sealed record VerifizierungInfo(
    Guid Id,
    Guid HelferProfilId,
    string Anzeigename,
    Nachweisart Art,
    string? Nachweiswert,
    Verifizierungsstatus Status,
    string? Bemerkung,
    DateTimeOffset EingereichtAm,
    DateTimeOffset? GeprueftAm);

/// <summary>
/// Gründe, warum ein Nachweis nicht angenommen wird.
/// </summary>
public enum VerifizierungFehler
{
    KeinHelferprofil,
    Gesperrt,
    ArtNichtUnterstuetzt,
    NachweiswertFehlt,
    UidUngueltig,
    Mehrwertnummer,
    TelefonUngueltig,
    EMailNichtKonto,
    ReferenzUngueltig,
}

/// <summary>
/// Ergebnis beim Einreichen: entweder der Nachweis oder ein Fehler.
/// </summary>
public sealed record VerifizierungErgebnis(VerifizierungInfo? Nachweis, VerifizierungFehler? Fehler);

/// <summary>
/// Offener Nachweis, wie ihn die Koordination prüft: mit den Angaben zum Profil, die sie zum Abgleich braucht,
/// zum Beispiel den Betriebsnamen zur UID-Nummer.
/// </summary>
public sealed record PruefungInfo(
    VerifizierungInfo Nachweis,
    Helfertyp Typ,
    string? Betriebsname,
    string Postleitzahl,
    string Ort,
    Verifizierungsstatus Profilstatus);

/// <summary>
/// Gründe, warum ein Entscheid nicht möglich ist.
/// </summary>
public enum EntscheidFehler
{
    // Unbekannt oder aus einer anderen Region; beides gilt als nicht vorhanden.
    NichtGefunden,
    BereitsEntschieden,
}

/// <summary>
/// Ergebnis eines Entscheids der Koordination: entweder der geprüfte Nachweis oder ein Fehler.
/// </summary>
public sealed record EntscheidErgebnis(VerifizierungInfo? Nachweis, EntscheidFehler? Fehler);
