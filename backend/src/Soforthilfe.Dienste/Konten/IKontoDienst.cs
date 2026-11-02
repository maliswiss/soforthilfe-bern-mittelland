namespace Soforthilfe.Dienste.Konten;

/// <summary>
/// Registrierung, Anmeldung und Lesen von Konten.
/// </summary>
public interface IKontoDienst
{
    Task<KontoErgebnis> RegistrierenAsync(RegistrierungsDaten daten, CancellationToken abbruch);

    // Liefert null, wenn E-Mail-Adresse oder Passwort nicht stimmen.
    Task<KontoInfo?> AnmeldenAsync(string eMail, string passwort, CancellationToken abbruch);

    // Liest ein Konto der aktuellen Region.
    Task<KontoInfo?> LiesAsync(Guid id, CancellationToken abbruch);
}
