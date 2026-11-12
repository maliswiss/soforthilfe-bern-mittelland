using Soforthilfe.Datenzugriff.Modell;
using Soforthilfe.Dienste.Kategorien;
using Soforthilfe.Dienste.Preise;

namespace Soforthilfe.Dienste.Helfende;

/// <summary>
/// Angaben zum Speichern eines Helferprofils.
/// </summary>
public sealed record HelferprofilDaten(
    Helfertyp Typ,
    string? Betriebsname,
    string? Beschreibung,
    string Plz,
    string Ort,
    int RadiusKm,
    IReadOnlyList<Guid> SkillIds,
    string? Adresse,
    Preisangaben Preise);

/// <summary>
/// Helferprofil, wie es die helfende Person selbst sieht.
/// </summary>
public sealed record HelferprofilInfo(
    Guid Id,
    Helfertyp Typ,
    string? Betriebsname,
    string? Beschreibung,
    string Plz,
    string Ort,
    int RadiusKm,
    bool JetztVerfuegbar,
    Verifizierungsstatus Verifizierung,
    bool Partner,
    double Bewertungsschnitt,
    int AnzahlBewertungen,
    int AnzahlEinsaetze,
    IReadOnlyList<SkillInfo> Skills,
    string? Adresse,
    Preisangaben Preise,
    bool PreiseErforderlich);

/// <summary>
/// Gründe, warum ein Helferprofil nicht gespeichert wird.
/// </summary>
public enum HelferprofilFehler
{
    KontoFehlt,
    SkillFehlt,
    BetriebsnameFehlt,
    PostleitzahlUngueltig,
    OrtFehlt,
    PostleitzahlUnbekannt,
    PostleitzahlAusserhalbRegion,
    PreiseFehlen,
}

/// <summary>
/// Ergebnis beim Speichern: entweder das Profil oder ein Fehler.
/// </summary>
public sealed record HelferprofilErgebnis(HelferprofilInfo? Profil, HelferprofilFehler? Fehler);
