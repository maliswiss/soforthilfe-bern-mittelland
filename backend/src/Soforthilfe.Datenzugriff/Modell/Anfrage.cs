namespace Soforthilfe.Datenzugriff.Modell;

/// <summary>
/// Anfrage einer hilfesuchenden Person, per Kategorie, als Freitext oder mit beidem.
/// </summary>
public sealed class Anfrage
{
    public Guid Id { get; set; }

    public Guid RegionId { get; set; }

    public Region? Region { get; set; }

    // Konto der hilfesuchenden Person.
    public Guid KontoId { get; set; }

    public Konto? Konto { get; set; }

    // Leer, solange die Kategorie noch nicht gewählt oder erkannt ist.
    public Guid? KategorieId { get; set; }

    public Kategorie? Kategorie { get; set; }

    public string? Freitext { get; set; }

    // Ort grob öffentlich (Postleitzahl und Ort), die genaue Adresse wird getrennt gespeichert.
    public required string Postleitzahl { get; set; }

    public required string Ort { get; set; }

    public string? Adresse { get; set; }

    // Wann die Hilfe passt, in eigenen Worten, zum Beispiel «heute zwischen 17 und 19 Uhr».
    public string? Zeitfenster { get; set; }

    public Prioritaet Prioritaet { get; set; } = Prioritaet.Mittel;

    public AnfrageStatus Status { get; set; } = AnfrageStatus.Offen;

    public DateTimeOffset ErstelltAm { get; set; }
}
