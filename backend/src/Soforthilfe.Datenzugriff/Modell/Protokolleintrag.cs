namespace Soforthilfe.Datenzugriff.Modell;

/// <summary>
/// Nachvollziehbarer Eintrag zu einem sicherheitsrelevanten Vorgang, zum Beispiel einem Notfall-Stopp.
/// Ohne Personendaten: kein Konto, keine IP-Adresse und kein Wortlaut aus Eingaben.
/// </summary>
public sealed class Protokolleintrag
{
    public long Id { get; set; }

    // Region der laufenden Anfrage; leer, wenn noch keine Region gewählt ist.
    public Guid? RegionId { get; set; }

    public Region? Region { get; set; }

    // Art des Vorgangs, zum Beispiel «Notfall.Stopp».
    public required string Aktion { get; set; }

    // Betroffener Bereich, zum Beispiel «Verstaendnis» oder «Anfrage».
    public string? Objekt { get; set; }

    // Kennung eines gespeicherten Objekts, falls es eines gibt.
    public string? ObjektId { get; set; }

    public string? Detail { get; set; }

    public DateTimeOffset Zeitpunkt { get; set; }
}
