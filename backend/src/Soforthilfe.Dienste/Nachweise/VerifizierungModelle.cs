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
