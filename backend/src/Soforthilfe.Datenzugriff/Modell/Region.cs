namespace Soforthilfe.Datenzugriff.Modell;

/// <summary>
/// Region als Mandant. Alle mandantenbezogenen Daten verweisen auf genau eine Region.
/// </summary>
public sealed class Region
{
    public Guid Id { get; set; }

    // Kurze, eindeutige Kennung für Adressen und Konfiguration, zum Beispiel «bern».
    public required string Kennung { get; set; }

    public required string Name { get; set; }

    public bool Aktiv { get; set; } = true;

    // Gemeinden der Region als BFS-Gemeindenummern. Über die Postleitzahl ergibt sich daraus die Region einer Person.
    // Eine Gemeinde gehört zu höchstens einer Region.
    public int[] Gemeinden { get; set; } = [];
}
