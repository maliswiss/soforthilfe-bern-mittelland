namespace Soforthilfe.Datenzugriff.Modell;

/// <summary>
/// Profil einer helfenden Person oder eines Betriebs mit Einsatzgebiet und Fähigkeiten.
/// </summary>
public sealed class Helferprofil
{
    public Guid Id { get; set; }

    // Jedes Konto hat höchstens ein Helferprofil.
    public Guid KontoId { get; set; }

    public Konto? Konto { get; set; }

    public Guid RegionId { get; set; }

    public Region? Region { get; set; }

    public required string Postleitzahl { get; set; }

    public required string Ort { get; set; }

    public int EinsatzradiusKm { get; set; } = 10;

    // Gibt an, ob die Person derzeit Anfragen annimmt.
    public bool Einsatzbereit { get; set; } = true;

    public List<Skill> Skills { get; set; } = [];
}
