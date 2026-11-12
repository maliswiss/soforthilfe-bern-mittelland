using Soforthilfe.Datenzugriff.Modell;

namespace Soforthilfe.Dienste.Nachweise;

/// <summary>
/// Nachweise zum eigenen Helferprofil einreichen und einsehen; die Koordination der Region prüft sie und
/// entscheidet. Es werden keine Dokumente angenommen.
/// </summary>
public interface IVerifizierungDienst
{
    Task<VerifizierungErgebnis> EinreichenAsync(Guid kontoId, Nachweisart art, string? nachweiswert, CancellationToken abbruch);

    Task<IReadOnlyList<VerifizierungInfo>> MeineAsync(Guid kontoId, CancellationToken abbruch);

    /// <summary>Offene Nachweise der eigenen Region, älteste zuerst.</summary>
    Task<IReadOnlyList<PruefungInfo>> OffeneAsync(CancellationToken abbruch);

    /// <summary>Freigabe oder Ablehnung eines offenen Nachweises durch die Koordination.</summary>
    Task<EntscheidErgebnis> EntscheidenAsync(Guid nachweisId, Guid pruefendeId, bool freigeben, string? bemerkung,
        CancellationToken abbruch);
}
