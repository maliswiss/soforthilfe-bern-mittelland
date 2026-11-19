using Soforthilfe.Datenzugriff.Modell;
using Soforthilfe.Dienste.Preise;

namespace Soforthilfe.Dienste.Matching;

/// <summary>
/// Kriterien der unverbindlichen Vorschau: Kategorie, Postleitzahl und Ort; ohne Dringlichkeit gilt die übliche der
/// Kategorie. Ohne <c>RegionKennung</c> gilt die Region der laufenden Anfrage.
/// </summary>
public sealed record VorschauKriterien(Guid KategorieId, string Plz, string? Ort, Prioritaet? Prioritaet, string? RegionKennung = null);

/// <summary>
/// Ein Eintrag der Vorschau wie ein Vorschlag, aber nicht gespeichert. Welche Angaben zum Profil hinausgehen,
/// entscheidet die Schnittstelle je nach Anmeldung.
/// </summary>
public sealed record VorschauEintrag(
    int Rang,
    Helferprofil Profil,
    int DistanzMeter,
    double Punktzahl,
    bool PartnerBevorzugt,
    string Begruendung,
    IReadOnlyList<KriteriumInfo> Kriterien,
    PreisInfo? Preis);

/// <summary>
/// Ergebnis der Vorschau: Kategorie, angenommene Dringlichkeit, Suchradius und die drei besten Einträge.
/// </summary>
public sealed record VorschauErgebnis(
    Kategorie Kategorie,
    Prioritaet Prioritaet,
    int SuchradiusKm,
    IReadOnlyList<VorschauEintrag> Eintraege);
