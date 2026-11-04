namespace Soforthilfe.Dienste.Konten;

/// <summary>
/// Prüft Schweizer Telefonnummern (Anforderung A10). Gebraucht bei der Registrierung und später beim Nachweis.
/// </summary>
public static class Telefonnummer
{
    // Kostenpflichtige Mehrwertnummern; sie werden nie angenommen.
    private static readonly string[] Mehrwertvorwahlen = ["0900", "0901", "0906"];

    /// <summary>
    /// Bringt eine Nummer in die Form 0XXXXXXXXX. Liefert null, wenn es keine Schweizer Nummer ist.
    /// </summary>
    public static string? Normieren(string eingabe)
    {
        // Übliche Trennzeichen entfernen: Leerzeichen, Bindestrich, Schrägstrich, Punkt, Klammern.
        var text = new string(eingabe.Where(z => !" -/.()".Contains(z)).ToArray());

        // Internationale Schweizer Vorwahl (+41 oder 0041) durch die führende 0 ersetzen.
        if (text.StartsWith("+41", StringComparison.Ordinal))
        {
            text = "0" + text[3..];
        }
        else if (text.StartsWith("0041", StringComparison.Ordinal))
        {
            text = "0" + text[4..];
        }

        return text.Length == 10 && text[0] == '0' && text.All(char.IsAsciiDigit) ? text : null;
    }

    /// <summary>
    /// Prüft eine bereits normierte Nummer auf eine Mehrwertvorwahl.
    /// </summary>
    public static bool IstMehrwertnummer(string normiert) =>
        Mehrwertvorwahlen.Any(vorwahl => normiert.StartsWith(vorwahl, StringComparison.Ordinal));
}
