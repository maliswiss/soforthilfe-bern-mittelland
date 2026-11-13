using System.Globalization;
using System.Text;

namespace Soforthilfe.Dienste.Verstaendnis;

/// <summary>
/// Gemeinsame Aufbereitung von Freitext für Notfallprüfung, Regeln und Klassifikation:
/// Kleinschreibung, Umlaute als ae/oe/ue, übrige Akzente entfernt (é → e, ç → c),
/// Satzzeichen als Leerzeichen.
/// </summary>
public static class Textnormalisierung
{
    public static string Normalisieren(string eingabe)
    {
        var ersetzt = eingabe.ToLower(CultureInfo.InvariantCulture)
            .Replace("ä", "ae").Replace("ö", "oe").Replace("ü", "ue").Replace("ß", "ss")
            .Replace("’", " ").Replace("'", " ");

        var zerlegt = ersetzt.Normalize(NormalizationForm.FormD);
        var bauer = new StringBuilder(zerlegt.Length);
        foreach (var zeichen in zerlegt)
        {
            // Akzente fallen weg, alles ausser Buchstaben und Ziffern wird zum Leerzeichen.
            if (CharUnicodeInfo.GetUnicodeCategory(zeichen) != UnicodeCategory.NonSpacingMark)
            {
                bauer.Append(char.IsLetterOrDigit(zeichen) ? zeichen : ' ');
            }
        }

        return string.Join(' ', bauer.ToString().Normalize(NormalizationForm.FormC)
            .Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }

    public static string[] Woerter(string eingabe) =>
        Normalisieren(eingabe).Split(' ', StringSplitOptions.RemoveEmptyEntries);

    /// <summary>
    /// Prüft, ob ein normalisierter Begriff im normalisierten Text vorkommt. Begriffe mit Leerzeichen gelten als
    /// Wortfolge am Wortanfang; kurze Begriffe (bis 4 Zeichen) nur als ganzes Wort, mittlere (5 und 6 Zeichen) als
    /// Wortanfang, längere auch innerhalb eines Worts, zum Beispiel «schluessel» in «hausschluessel».
    /// </summary>
    public static bool Enthaelt(string normalisierterText, string[] woerter, string begriff)
    {
        if (begriff.Contains(' '))
        {
            return $" {normalisierterText} ".Contains($" {begriff}", StringComparison.Ordinal);
        }

        return begriff.Length switch
        {
            <= 4 => woerter.Contains(begriff),
            < 7 => woerter.Any(w => w.StartsWith(begriff, StringComparison.Ordinal)),
            _ => woerter.Any(w => w.Contains(begriff, StringComparison.Ordinal)),
        };
    }

    /// <summary>
    /// Stelle im ursprünglichen Text, an der ein normalisierter Begriff vorkommt, zum Beispiel «Bewusstlos»
    /// für «bewusstlos». So nennt die Begründung das Wort so, wie die Person es geschrieben hat.
    /// Ohne Fund bleibt der Begriff unverändert.
    /// </summary>
    public static string Fundstelle(string original, string begriff)
    {
        var teile = new List<(string Original, int Beginn, int Ende)>();
        var normalisiert = new StringBuilder();
        foreach (var wort in WoerterImOriginal(original))
        {
            var form = Normalisieren(wort);
            if (form.Length == 0)
            {
                continue;
            }

            if (normalisiert.Length > 0)
            {
                normalisiert.Append(' ');
            }

            teile.Add((wort, normalisiert.Length, normalisiert.Length + form.Length));
            normalisiert.Append(form);
        }

        var stelle = normalisiert.ToString().IndexOf(begriff, StringComparison.Ordinal);
        if (stelle < 0)
        {
            return begriff;
        }

        var ende = stelle + begriff.Length;
        var gefunden = teile.Where(t => t.Beginn < ende && t.Ende > stelle).Select(t => t.Original).ToList();
        return gefunden.Count == 0 ? begriff : string.Join(' ', gefunden);
    }

    private static IEnumerable<string> WoerterImOriginal(string eingabe)
    {
        var bauer = new StringBuilder();
        foreach (var zeichen in eingabe.Normalize(NormalizationForm.FormC))
        {
            if (char.IsLetterOrDigit(zeichen) || CharUnicodeInfo.GetUnicodeCategory(zeichen) == UnicodeCategory.NonSpacingMark)
            {
                bauer.Append(zeichen);
            }
            else if (bauer.Length > 0)
            {
                yield return bauer.ToString();
                bauer.Clear();
            }
        }

        if (bauer.Length > 0)
        {
            yield return bauer.ToString();
        }
    }
}
