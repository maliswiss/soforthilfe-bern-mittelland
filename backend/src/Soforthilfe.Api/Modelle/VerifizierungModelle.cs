using System.ComponentModel.DataAnnotations;
using Soforthilfe.Datenzugriff.Modell;
using Soforthilfe.Dienste.Nachweise;

namespace Soforthilfe.Api.Modelle;

/// <summary>
/// Nachweis zur Prüfung. Es werden keine Dokumente hochgeladen, nur überprüfbare Angaben: Art 0 UID-Nummer,
/// Art 1 Handelsregister (über die UID des eingetragenen Unternehmens), Art 2 Telefonnummer (ohne Angabe die des
/// Kontos), Art 3 E-Mail-Adresse des Kontos, Art 4 Referenz einer Organisation oder Gemeinde.
/// </summary>
public sealed record VerifizierungAnfrage(
    [EnumDataType(typeof(Nachweisart))] Nachweisart Art,
    [StringLength(200)] string? Nachweiswert);

/// <summary>
/// Nachweis mit Stand der Prüfung. Status: 0 unverifiziert, 1 in Prüfung, 2 verifiziert, 3 abgelehnt, 4 gesperrt.
/// </summary>
public sealed record VerifizierungAntwort(
    Guid Id,
    Guid HelferProfilId,
    string Anzeigename,
    Nachweisart Art,
    string? Nachweiswert,
    Verifizierungsstatus Status,
    string? Bemerkung,
    DateTimeOffset EingereichtAm,
    DateTimeOffset? GeprueftAm)
{
    public static VerifizierungAntwort Aus(VerifizierungInfo v) => new(v.Id, v.HelferProfilId, v.Anzeigename, v.Art,
        v.Nachweiswert, v.Status, v.Bemerkung, v.EingereichtAm, v.GeprueftAm);
}

/// <summary>
/// Entscheid der Koordination: freigeben oder ablehnen, mit einer Bemerkung für die helfende Person.
/// </summary>
public sealed record VerifizierungEntscheidAnfrage(bool Freigeben, [StringLength(500)] string? Bemerkung);

/// <summary>
/// Offener Nachweis für die Koordination, mit Typ, Betriebsname, Postleitzahl, Ort und Stand des Profils.
/// </summary>
public sealed record PruefungAntwort(
    Guid Id,
    Guid HelferProfilId,
    string Anzeigename,
    Nachweisart Art,
    string? Nachweiswert,
    Verifizierungsstatus Status,
    string? Bemerkung,
    DateTimeOffset EingereichtAm,
    DateTimeOffset? GeprueftAm,
    Helfertyp Typ,
    string? Betriebsname,
    string Plz,
    string Ort,
    Verifizierungsstatus Profilstatus)
{
    public static PruefungAntwort Aus(PruefungInfo p) => new(p.Nachweis.Id, p.Nachweis.HelferProfilId,
        p.Nachweis.Anzeigename, p.Nachweis.Art, p.Nachweis.Nachweiswert, p.Nachweis.Status, p.Nachweis.Bemerkung,
        p.Nachweis.EingereichtAm, p.Nachweis.GeprueftAm, p.Typ, p.Betriebsname, p.Postleitzahl, p.Ort, p.Profilstatus);
}
