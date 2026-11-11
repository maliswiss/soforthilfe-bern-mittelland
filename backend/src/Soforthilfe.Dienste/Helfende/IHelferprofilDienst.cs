namespace Soforthilfe.Dienste.Helfende;

/// <summary>
/// Eigenes Helferprofil anlegen, ändern und die Verfügbarkeit umschalten.
/// </summary>
public interface IHelferprofilDienst
{
    /// <summary>Eigenes Profil, oder null, wenn noch keines besteht.</summary>
    Task<HelferprofilInfo?> LadenAsync(Guid kontoId, CancellationToken abbruch);

    /// <summary>Legt das Profil an oder ersetzt alle Angaben; Fähigkeiten werden vollständig ersetzt.</summary>
    Task<HelferprofilErgebnis> SpeichernAsync(Guid kontoId, HelferprofilDaten daten, CancellationToken abbruch);

    /// <summary>Setzt die Verfügbarkeit; null, wenn noch kein Profil besteht.</summary>
    Task<bool?> VerfuegbarkeitSetzenAsync(Guid kontoId, bool jetztVerfuegbar, CancellationToken abbruch);
}
