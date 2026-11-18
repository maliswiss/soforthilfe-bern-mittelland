using NetTopologySuite.Geometries;
using Soforthilfe.Datenzugriff.Modell;

namespace Soforthilfe.Dienste.Matching;

/// <summary>
/// Kriterien der Kandidatensuche: Kategorie, Punkt der Postleitzahl und Suchradius. Ohne Punkt (Ortschaftenverzeichnis
/// fehlt) gilt dieselbe Postleitzahl mit Distanz 0.
/// </summary>
public sealed record KandidatenKriterien(
    Guid RegionId,
    Guid KategorieId,
    bool Kostenpflichtig,
    string Postleitzahl,
    Point? Standort,
    int SuchradiusKm);

/// <summary>
/// Geeignete helfende Person mit Distanz zwischen den Punkten der Postleitzahlen in Metern.
/// </summary>
public sealed record Kandidat(Helferprofil Profil, double DistanzMeter);

/// <summary>
/// Kandidaten zu einer Anfrage, nächste zuerst. <c>KategorieFehlt</c>: Ohne Kategorie wird nicht gesucht.
/// </summary>
public sealed record KandidatenErgebnis(
    int SuchradiusKm,
    bool KategorieFehlt,
    IReadOnlyList<Kandidat> Kandidaten);
