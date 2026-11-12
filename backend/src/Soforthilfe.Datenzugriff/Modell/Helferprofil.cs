using NetTopologySuite.Geometries;

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

    public Helfertyp Typ { get; set; } = Helfertyp.Privatperson;

    // Name des Betriebs; nur bei Typ «Betrieb».
    public string? Betriebsname { get; set; }

    public string? Beschreibung { get; set; }

    // Strasse und Nummer. Sieht nur die anfragende Person, und erst nach der Zusage.
    public string? Adresse { get; set; }

    public required string Postleitzahl { get; set; }

    public required string Ort { get; set; }

    // Punkt der Postleitzahl aus dem Ortschaftenverzeichnis (WGS84), nie ein genauer Standort.
    public Point? Standort { get; set; }

    public int EinsatzradiusKm { get; set; } = 10;

    // Gibt an, ob die Person derzeit Anfragen annimmt.
    public bool Einsatzbereit { get; set; } = true;

    // Preisangaben (Anforderung A9), vor der Zusage sichtbar. Pflicht bei bezahlpflichtigen Fähigkeiten.
    // Anfahrt als Pauschale je Einsatz; Zuschläge in Prozent auf den Stundenansatz.
    public decimal? AnfahrtChf { get; set; }

    public decimal? StundenansatzChf { get; set; }

    // Nachtzuschlag, Standard von 20 bis 7 Uhr.
    public int? NachtzuschlagProzent { get; set; }

    // Wochenendzuschlag für Samstag und Sonntag.
    public int? WochenendzuschlagProzent { get; set; }

    // Preise verstehen sich ohne MWST; bei MWST-Pflicht kommt sie im Richtpreis hinzu.
    public bool MwstPflichtig { get; set; }

    // Stand der Prüfung durch die Koordination; Änderungen an prüfrelevanten Angaben verlangen eine neue Prüfung.
    public Verifizierungsstatus Verifizierung { get; set; } = Verifizierungsstatus.Unverifiziert;

    public List<Verifizierung> Verifizierungen { get; set; } = [];

    // Partner-Abo; wirkt im Matching nur bei gleichwertigen Vorschlägen.
    public bool Partner { get; set; }

    public double Bewertungsschnitt { get; set; }

    public int AnzahlBewertungen { get; set; }

    public int AnzahlEinsaetze { get; set; }

    public List<Skill> Skills { get; set; } = [];
}
