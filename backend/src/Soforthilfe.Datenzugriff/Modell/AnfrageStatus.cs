namespace Soforthilfe.Datenzugriff.Modell;

/// <summary>
/// Verlauf einer Anfrage von der Erfassung bis zum Abschluss. Die Zahlenwerte gelten auch an der Schnittstelle;
/// gespeichert wird der Name.
/// </summary>
public enum AnfrageStatus
{
    Entwurf = 0,
    Offen = 1,
    Vorgeschlagen = 2,
    Angenommen = 3,
    Unterwegs = 4,
    Erledigt = 5,
    Abgebrochen = 6,
    Notfallgestoppt = 7,
}
