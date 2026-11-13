namespace Soforthilfe.Datenzugriff.Modell;

/// <summary>
/// Weg, auf dem eine Anfrage einer Kategorie zugeordnet wurde. Die Verständniskette geht in dieser Reihenfolge vor.
/// </summary>
public enum Erfassungsweg
{
    // Die anfragende Person hat die Kategorie selbst gewählt.
    Kategorieauswahl = 0,

    // Eindeutige Schlüsselwörter im Freitext.
    Regeln = 1,

    // TF-IDF-Klassifikator, trainiert mit fiktiven Beispielen.
    MaschinellesLernen = 2,

    // Lokales Sprachmodell, nur wenn die vorherigen Stufen nicht entscheiden.
    Sprachmodell = 3,
}
