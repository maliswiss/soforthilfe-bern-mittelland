using System.ComponentModel.DataAnnotations;
using Soforthilfe.Datenzugriff.Modell;
using Soforthilfe.Dienste.Helfende;
using Soforthilfe.Dienste.Kategorien;

namespace Soforthilfe.Api.Modelle;

/// <summary>
/// Eingabe für das eigene Helferprofil. Typ 0 ist Privatperson, 1 Betrieb; Betriebe geben ihren Namen an.
/// Einen genauen Standort nimmt das Profil nicht entgegen: für Entfernungen gilt der Punkt der Postleitzahl.
/// </summary>
public sealed record HelferprofilAnfrage(
    [EnumDataType(typeof(Helfertyp))] Helfertyp Typ,
    [StringLength(200)] string? Betriebsname,
    [StringLength(1000)] string? Beschreibung,
    [Required, StringLength(10)] string Plz,
    [Required, StringLength(120)] string Ort,
    [Range(1, 100)] int RadiusKm,
    [Required, MaxLength(30)] IReadOnlyList<Guid> SkillIds,
    [StringLength(250)] string? Adresse = null);

/// <summary>
/// Eigenes Helferprofil in der Antwort.
/// </summary>
public sealed record HelferprofilAntwort(
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
    IReadOnlyList<SkillAntwort> Skills,
    string? Adresse)
{
    public static HelferprofilAntwort Aus(HelferprofilInfo p) => new(
        p.Id, p.Typ, p.Betriebsname, p.Beschreibung, p.Plz, p.Ort, p.RadiusKm, p.JetztVerfuegbar, p.Partner,
        p.Bewertungsschnitt, p.AnzahlBewertungen, p.AnzahlEinsaetze, p.Skills.Select(SkillAntwort.Aus).ToList(),
        p.Adresse);
}

/// <summary>
/// Fähigkeit im Helferprofil, bezahlpflichtig wie die zugehörige Kategorie.
/// </summary>
public sealed record SkillAntwort(Guid Id, string Code, string Bezeichnung, bool Bezahlpflichtig)
{
    public static SkillAntwort Aus(SkillInfo s) => new(s.Id, s.Code, s.Bezeichnung, s.Bezahlpflichtig);
}

/// <summary>
/// Verfügbarkeit umschalten.
/// </summary>
public sealed record VerfuegbarkeitAnfrage(bool JetztVerfuegbar);
