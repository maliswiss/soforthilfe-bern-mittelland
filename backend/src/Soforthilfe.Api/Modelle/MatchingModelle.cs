using Soforthilfe.Datenzugriff.Modell;
using Soforthilfe.Dienste.Matching;

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
/// Vorschlag des Smart-Matchings nach Rang. Name erst für angemeldete Personen; Telefonnummer und Adresse gibt es
/// hier nie.
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
    bool PartnerBevorzugt)
{
    public static VorschlagAntwort Aus(VorschlagInfo v) => new(
        v.Id, v.HelferprofilId, v.Rang, v.Punktzahl, v.DistanzMeter, v.Anzeigename, v.Typ, v.Betriebsname, v.Plz, v.Ort,
        v.JetztVerfuegbar, v.Bewertungsschnitt, v.AnzahlBewertungen, v.AnzahlEinsaetze, v.PartnerBevorzugt);
}
