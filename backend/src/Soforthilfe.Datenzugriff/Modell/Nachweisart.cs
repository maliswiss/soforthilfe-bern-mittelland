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

    TelefonBestaetigt = 2,

    EMailBestaetigt = 3,

    Referenz = 4,
}
