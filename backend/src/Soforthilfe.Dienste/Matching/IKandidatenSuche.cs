namespace Soforthilfe.Dienste.Matching;

/// <summary>
/// Sucht geeignete Helfende nach Postleitzahl, Fähigkeit und Suchradius (PostGIS).
/// </summary>
public interface IKandidatenSuche
{
    /// <summary>Kandidaten zu einer Anfrage der eigenen Region; null, wenn es die Anfrage dort nicht gibt.</summary>
    Task<KandidatenErgebnis?> FuerAnfrageAsync(Guid anfrageId, CancellationToken abbruch);

    Task<IReadOnlyList<Kandidat>> SuchenAsync(KandidatenKriterien kriterien, CancellationToken abbruch);
}
