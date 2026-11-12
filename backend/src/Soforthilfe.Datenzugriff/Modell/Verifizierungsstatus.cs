namespace Soforthilfe.Datenzugriff.Modell;

/// <summary>
/// Stand der Prüfung eines Helferprofils oder eines einzelnen Nachweises.
/// </summary>
public enum Verifizierungsstatus
{
    Unverifiziert = 0,
    InPruefung = 1,
    Verifiziert = 2,
    Abgelehnt = 3,
    Gesperrt = 4,
}
