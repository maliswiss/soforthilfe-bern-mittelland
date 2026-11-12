namespace Soforthilfe.Datenzugriff.Modell;

/// <summary>
/// Überprüfbare Angaben, auf die sich die Verifizierung stützt. Ausweisdokumente gehören bewusst nicht dazu.
/// </summary>
public enum Nachweisart
{
    // Unternehmens-Identifikationsnummer, zum Beispiel «CHE-123.456.788».
    UidNummer = 0,

    // Eintrag im Handelsregister, nachgewiesen über die UID des eingetragenen Unternehmens.
    Handelsregister = 1,

    // Telefonnummer, bestätigt durch die Koordination, zum Beispiel mit einem Rückruf.
    TelefonBestaetigt = 2,

    // E-Mail-Adresse des Kontos, bestätigt durch die Koordination.
    EMailBestaetigt = 3,

    // Referenz einer Organisation oder Gemeinde mit Kontaktperson.
    Referenz = 4,
}
