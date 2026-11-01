namespace Soforthilfe.Datenzugriff.Modell;

/// <summary>
/// Konto einer Person. Enthält Personendaten und unterliegt dem Datenschutzkonzept.
/// </summary>
public sealed class Konto
{
    public Guid Id { get; set; }

    // Region des Kontos; nur der Plattformbetrieb arbeitet regionsübergreifend und hat keine Region.
    public Guid? RegionId { get; set; }

    public Region? Region { get; set; }

    public required string EMail { get; set; }

    public required string Anzeigename { get; set; }

    public string? Telefon { get; set; }

    // Nur der Hash wird gespeichert, nie das Passwort selbst. Gesetzt bei der Registrierung.
    public string? PasswortHash { get; set; }

    public Rolle Rolle { get; set; }

    public DateTimeOffset ErstelltAm { get; set; }
}
