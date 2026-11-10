namespace Soforthilfe.Dienste.Konten;

/// <summary>
/// Registrierung, Anmeldung und Lesen von Konten.
/// </summary>
public interface IKontoDienst
{
    // Registriert ein Konto in der Region der laufenden Anfrage.
    Task<KontoErgebnis> RegistrierenAsync(RegistrierungsDaten daten, CancellationToken abbruch);

    // Liefert null, wenn E-Mail-Adresse oder Passwort nicht stimmen oder das Konto nicht zur angegebenen Region gehört.
    Task<KontoInfo?> AnmeldenAsync(string eMail, string passwort, string? region, CancellationToken abbruch);

    // Liest ein Konto der aktuellen Region.
    Task<KontoInfo?> LiesAsync(Guid id, CancellationToken abbruch);

    // Konten der aktuellen Region; für den Plattformbetrieb alle Konten.
    Task<IReadOnlyList<KontoInfo>> ListeAsync(CancellationToken abbruch);

    Task<RollenErgebnis> RolleSetzenAsync(Guid id, string rolle, CancellationToken abbruch);
}
