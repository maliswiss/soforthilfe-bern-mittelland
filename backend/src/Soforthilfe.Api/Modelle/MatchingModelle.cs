using System.ComponentModel.DataAnnotations;
using Soforthilfe.Datenzugriff.Modell;
using Soforthilfe.Dienste.Matching;
using Soforthilfe.Dienste.Preise;

namespace Soforthilfe.Api.Modelle;

/// <summary>
/// Kandidaten zu einer Anfrage für Koordination und Verwaltung, nächste zuerst. Ohne Telefonnummer, Adresse und
/// genauen Standort; die Distanz gilt zwischen den Punkten der Postleitzahlen.
/// </summary>
public sealed record KandidatenAntwort(int SuchradiusKm, bool KategorieFehlt, IReadOnlyList<KandidatAntwort> Kandidaten)
{
    public static KandidatenAntwort Aus(KandidatenErgebnis e) =>
        new(e.SuchradiusKm, e.KategorieFehlt, e.Kandidaten.Select(KandidatAntwort.Aus).ToList());
}

public sealed record KandidatAntwort(
    Guid HelferprofilId,
    string Anzeigename,
    Helfertyp Typ,
    string? Betriebsname,
    string Plz,
    string Ort,
    int DistanzMeter,
    int EinsatzradiusKm,
    bool JetztVerfuegbar)
{
    public static KandidatAntwort Aus(Kandidat k) => new(
        k.Profil.Id,
        k.Profil.Konto?.Anzeigename ?? string.Empty,
        k.Profil.Typ,
        k.Profil.Betriebsname,
        k.Profil.Postleitzahl,
        k.Profil.Ort,
        (int)Math.Round(k.DistanzMeter),
        k.Profil.EinsatzradiusKm,
        k.Profil.Einsatzbereit);
}

/// <summary>
/// Vorschlag des Smart-Matchings nach Rang mit Begründung und den Teilpunkten aller Kriterien. Name erst für
/// angemeldete Personen; Telefonnummer und Adresse gibt es hier nie.
/// </summary>
public sealed record VorschlagAntwort(
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
    string Begruendung,
    IReadOnlyList<KriteriumAntwort> Kriterien,
    PreisInfo? Preis)
{
    public static VorschlagAntwort Aus(VorschlagInfo v) => new(
        v.Id, v.HelferprofilId, v.Rang, v.Punktzahl, v.DistanzMeter, v.Anzeigename, v.Typ, v.Betriebsname, v.Plz, v.Ort,
        v.JetztVerfuegbar, v.Bewertungsschnitt, v.AnzahlBewertungen, v.AnzahlEinsaetze, v.PartnerBevorzugt,
        v.Begruendung, v.Kriterien.Select(KriteriumAntwort.Aus).ToList(), v.Preis);
}

/// <summary>
/// Beitrag eines Kriteriums: Teilpunkte (0 bis 1) mal Gewicht. Die Summe der Beiträge ergibt die Punktzahl.
/// </summary>
public sealed record KriteriumAntwort(string Kriterium, double Teilpunkte, double Gewicht, double Beitrag)
{
    public static KriteriumAntwort Aus(KriteriumInfo k) => new(k.Kriterium, k.Teilpunkte, k.Gewicht, k.Beitrag);
}

/// <summary>
/// Eingabe der unverbindlichen Vorschau: Kategorie, Postleitzahl, optional Ort und Dringlichkeit (0 tief, 1 mittel,
/// 2 hoch).
/// </summary>
public sealed record VorschauEingabe(
    Guid KategorieId,
    [Required, StringLength(10)] string Plz,
    [StringLength(120)] string? Ort = null,
    int? Prioritaet = null);

/// <summary>
/// Vorschau mit gestaffelter Preisgabe: ohne Anmeldung ohne Namen, angemeldet mit Namen. Telefonnummer, Adresse und
/// Kennung der Helfenden gibt es in der Vorschau nie; die Telefonnummer folgt erst nach der Zusage.
/// </summary>
public sealed record VorschauAntwort(
    Guid KategorieId,
    string Kategorie,
    string Code,
    bool Bezahlpflichtig,
    string Plz,
    string? Ort,
    int Prioritaet,
    int SuchradiusKm,
    bool Angemeldet,
    IReadOnlyList<VorschauEintragAntwort> Eintraege)
{
    public static VorschauAntwort Aus(VorschauErgebnis e, string plz, string? ort, bool angemeldet) => new(
        e.Kategorie.Id,
        e.Kategorie.Bezeichnung,
        e.Kategorie.Kennung,
        e.Kategorie.Kostenpflichtig,
        plz,
        ort,
        (int)e.Prioritaet,
        e.SuchradiusKm,
        angemeldet,
        e.Eintraege.Select(v => VorschauEintragAntwort.Aus(v, angemeldet)).ToList());
}

/// <summary>
/// Eintrag der Vorschau. <c>Name</c> nur angemeldet: bei Betrieben der Betriebsname, sonst der Name des Kontos.
/// </summary>
public sealed record VorschauEintragAntwort(
    int Rang,
    string? Name,
    bool IstBetrieb,
    int DistanzMeter,
    double Punktzahl,
    string Begruendung,
    IReadOnlyList<KriteriumAntwort> Kriterien,
    bool JetztVerfuegbar,
    double Bewertungsschnitt,
    int AnzahlBewertungen,
    int AnzahlEinsaetze,
    bool PartnerBevorzugt,
    PreisInfo? Preis)
{
    public static VorschauEintragAntwort Aus(VorschauEintrag v, bool angemeldet)
    {
        var profil = v.Profil;
        var istBetrieb = profil.Typ == Helfertyp.Betrieb;
        var name = istBetrieb ? profil.Betriebsname ?? profil.Konto?.Anzeigename : profil.Konto?.Anzeigename;
        return new(
            v.Rang,
            angemeldet ? name : null,
            istBetrieb,
            v.DistanzMeter,
            v.Punktzahl,
            v.Begruendung,
            v.Kriterien.Select(KriteriumAntwort.Aus).ToList(),
            profil.Einsatzbereit,
            profil.Bewertungsschnitt,
            profil.AnzahlBewertungen,
            profil.AnzahlEinsaetze,
            v.PartnerBevorzugt,
            v.Preis);
    }
}
