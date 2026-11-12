using System.Text.RegularExpressions;

namespace Soforthilfe.Dienste.Nachweise;

/// <summary>
/// Unternehmens-Identifikationsnummer (UID) der Schweiz: «CHE» und neun Ziffern, die letzte ist eine Prüfziffer
/// nach Modulo 11 mit den Gewichten 5, 4, 3, 2, 7, 6, 5, 4. Ergibt die Rechnung 10, gibt es die Nummer nicht.
/// </summary>
public static partial class Uid
{
    private static readonly int[] Gewichte = [5, 4, 3, 2, 7, 6, 5, 4];

    // «CHE-123.456.788», «CHE123456788» oder mit Leerzeichen; ein Zusatz wie «HR» oder «MWST» ist erlaubt.
    [GeneratedRegex(@"^CHE[\s\-]*(\d{3})[\s.]*(\d{3})[\s.]*(\d{3})(?:\s+(?:HR|MWST|TVA|IVA|RC|RI))?$", RegexOptions.IgnoreCase)]
    private static partial Regex Muster();

    /// <summary>Normalisierte Form «CHE-123.456.788», oder null, wenn Format oder Prüfziffer nicht stimmen.</summary>
    public static string? Normalisieren(string? eingabe)
    {
        var treffer = Muster().Match(eingabe?.Trim() ?? string.Empty);
        if (!treffer.Success)
        {
            return null;
        }

        var ziffern = treffer.Groups[1].Value + treffer.Groups[2].Value + treffer.Groups[3].Value;
        return PruefzifferStimmt(ziffern) ? $"CHE-{ziffern[..3]}.{ziffern[3..6]}.{ziffern[6..]}" : null;
    }

    public static bool IstGueltig(string? eingabe) => Normalisieren(eingabe) is not null;

    private static bool PruefzifferStimmt(string ziffern)
    {
        var summe = 0;
        for (var i = 0; i < Gewichte.Length; i++)
        {
            summe += (ziffern[i] - '0') * Gewichte[i];
        }

        var pruefziffer = 11 - (summe % 11);
        if (pruefziffer == 10)
        {
            return false;
        }

        return (pruefziffer == 11 ? 0 : pruefziffer) == ziffern[8] - '0';
    }
}
