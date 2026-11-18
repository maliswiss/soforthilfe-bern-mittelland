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
