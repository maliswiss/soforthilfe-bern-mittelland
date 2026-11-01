namespace Soforthilfe.Datenzugriff.Modell;

/// <summary>
/// Anfrage einer hilfesuchenden Person, per Kategorie oder als Freitext.
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

    public required string Postleitzahl { get; set; }

    public AnfrageStatus Status { get; set; } = AnfrageStatus.Offen;

    public DateTimeOffset ErstelltAm { get; set; }
}
