namespace Soforthilfe.Datenzugriff.Modell;

/// <summary>
/// Eingereichter Nachweis zu einem Helferprofil, den die Koordination der Region prüft. Gespeichert werden nur
/// überprüfbare Angaben wie eine UID-Nummer, nie Ausweisdokumente oder Dateien.
/// </summary>
public sealed class Verifizierung
{
    public Guid Id { get; set; }

    public Guid RegionId { get; set; }

    public Guid HelferprofilId { get; set; }

    public Helferprofil? Helferprofil { get; set; }

    public Nachweisart Art { get; set; }

    // Normalisierter Wert, zum Beispiel «CHE-123.456.788».
    public string? Nachweiswert { get; set; }

    public Verifizierungsstatus Status { get; set; } = Verifizierungsstatus.InPruefung;

    public string? Bemerkung { get; set; }

    public Guid? GeprueftVonId { get; set; }

    public DateTimeOffset EingereichtAm { get; set; }

    public DateTimeOffset? GeprueftAm { get; set; }
}
