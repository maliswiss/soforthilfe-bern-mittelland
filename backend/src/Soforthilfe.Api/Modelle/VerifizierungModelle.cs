using System.ComponentModel.DataAnnotations;
using Soforthilfe.Datenzugriff.Modell;
using Soforthilfe.Dienste.Nachweise;

namespace Soforthilfe.Api.Modelle;

/// <summary>
/// Nachweis zur Prüfung. Es werden keine Dokumente hochgeladen, nur überprüfbare Angaben: Art 0 UID-Nummer,
/// Art 1 Handelsregister (über die UID des eingetragenen Unternehmens).
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
