using System.Text.RegularExpressions;
using Soforthilfe.Dienste.Konten;

namespace Soforthilfe.Dienste.Nachweise;

/// <summary>
/// Prüft und vereinheitlicht die Angaben zu Telefonnummer und Referenz, bevor die Koordination sie prüft.
/// </summary>
public static partial class Nachweiswerte
{
    // Eine Referenz nennt eine Organisation oder Gemeinde und eine Kontaktperson; ein einzelnes Wort genügt nicht.
    public const int ReferenzMindestlaenge = 5;

    public const int ReferenzHoechstlaenge = 200;

    [GeneratedRegex(@"\s+")]
    private static partial Regex Leerraum();

    /// <summary>
    /// Schweizer Telefonnummer in gut lesbarer Form, zum Beispiel «031 123 45 67». Liefert null, wenn die Nummer
    /// ungültig ist. Mehrwertnummern prüft der Aufrufer vorher, damit er den passenden Hinweis geben kann.
    /// </summary>
    public static string? Telefon(string? eingabe)
    {
        var normiert = Telefonnummer.Normieren(eingabe ?? string.Empty);
        return normiert is null
            ? null
            : $"{normiert[..3]} {normiert[3..6]} {normiert[6..8]} {normiert[8..]}";
    }

    /// <summary>
    /// Referenz mit einfachen Leerzeichen und ohne Rand. Liefert null, wenn sie zu kurz oder zu lang ist.
    /// </summary>
    public static string? Referenz(string? eingabe)
    {
        var text = Leerraum().Replace(eingabe ?? string.Empty, " ").Trim();
        return text.Length is >= ReferenzMindestlaenge and <= ReferenzHoechstlaenge ? text : null;
    }
}
