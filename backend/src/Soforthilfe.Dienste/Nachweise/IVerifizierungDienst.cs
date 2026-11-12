using Soforthilfe.Datenzugriff.Modell;

namespace Soforthilfe.Dienste.Nachweise;

/// <summary>
/// Nachweise zum eigenen Helferprofil einreichen und einsehen. Es werden keine Dokumente angenommen.
/// </summary>
public interface IVerifizierungDienst
{
    Task<VerifizierungErgebnis> EinreichenAsync(Guid kontoId, Nachweisart art, string? nachweiswert, CancellationToken abbruch);

    Task<IReadOnlyList<VerifizierungInfo>> MeineAsync(Guid kontoId, CancellationToken abbruch);
}
