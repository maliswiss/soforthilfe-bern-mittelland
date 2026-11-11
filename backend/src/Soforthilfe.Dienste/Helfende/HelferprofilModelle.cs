using Soforthilfe.Datenzugriff.Modell;
using Soforthilfe.Dienste.Kategorien;

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
    string? Adresse);

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
    bool Partner,
    double Bewertungsschnitt,
    int AnzahlBewertungen,
    int AnzahlEinsaetze,
    IReadOnlyList<SkillInfo> Skills,
    string? Adresse);

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
}

/// <summary>
/// Ergebnis beim Speichern: entweder das Profil oder ein Fehler.
/// </summary>
public sealed record HelferprofilErgebnis(HelferprofilInfo? Profil, HelferprofilFehler? Fehler);
