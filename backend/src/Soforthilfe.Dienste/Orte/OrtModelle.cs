namespace Soforthilfe.Dienste.Orte;

/// <summary>
/// Ortschaft zu einer Postleitzahl. Gemeinde ist die Gemeinde mit dem grössten Adressenanteil; Region ist die Kennung
/// der aktiven Region, zu der diese Gemeinde gehört, oder leer, wenn die Plattform dort noch nicht tätig ist.
/// <c>PlzUeberMehrereGemeinden</c> ist wahr, wenn die Postleitzahl mehrere Gemeinden mit nennenswertem Anteil berührt.
/// </summary>
public sealed record OrtInfo(
    string Plz,
    string Ort,
    string Gemeinde,
    string Kanton,
    bool PlzUeberMehrereGemeinden,
    string? Region);

/// <summary>
/// Ergebnis der Prüfung, ob eine Postleitzahl zu einer Region passt.
/// </summary>
public enum PlzPruefung
{
    // Die Postleitzahl berührt mindestens eine Gemeinde der Region, oder die Region hat keine Gemeinden festgelegt.
    Passt,

    // Das Ortschaftenverzeichnis ist nicht eingelesen; es wird nicht geprüft.
    OhneVerzeichnis,

    // Die Postleitzahl steht nicht im Ortschaftenverzeichnis.
    Unbekannt,

    // Die Postleitzahl liegt vollständig ausserhalb der Region.
    AndereRegion,
}
