using Soforthilfe.Datenzugriff.Modell;
using Soforthilfe.Dienste.Preise;

namespace Soforthilfe.Dienste.Matching;

/// <summary>
/// Vorschlag zu einer Anfrage, wie ihn die anfragende Person und die Koordination sehen: ohne Telefonnummer und
/// Adresse der helfenden Person, die Distanz zwischen den Punkten der Postleitzahlen, mit Begründung, Beitrag
/// jedes Kriteriums und bei kostenpflichtigen Kategorien den Preisangaben.
/// </summary>
public sealed record VorschlagInfo(
    Guid Id,
    Guid HelferprofilId,
    int Rang,
    double Punktzahl,
    int DistanzMeter,
    string Anzeigename,
    Helfertyp Typ,
    string? Betriebsname,
    string Plz,
    string Ort,
    bool JetztVerfuegbar,
    double Bewertungsschnitt,
    int AnzahlBewertungen,
    int AnzahlEinsaetze,
    bool PartnerBevorzugt,
    Teilpunkte Teilpunkte,
    MatchingGewichte Gewichte,
    string Begruendung,
    IReadOnlyList<KriteriumInfo> Kriterien,
    PreisInfo? Preis,
    DateTimeOffset ErstelltAm);
