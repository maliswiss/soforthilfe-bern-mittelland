using System.Text.Json;
using System.Text.RegularExpressions;

namespace Soforthilfe.Dienste.Sprachen;

/// <summary>
/// Gründe, warum eine Sprachdatei abgewiesen wird.
/// </summary>
public enum SprachdateiFehler
{
    CodeUngueltig,
    NameUngueltig,
    KeinObjekt,
    Leer,
    SchluesselUngueltig,
    ZuTief,
    KeinText,
    TextZuLang,
    ZeichenUnzulaessig,
    PlatzhalterUngueltig,
    ZuViele,
}

/// <summary>
/// Ergebnis der Prüfung: Anzahl Texte oder Fehler mit dem Pfad des betroffenen Texts, zum Beispiel «start.titel1».
/// </summary>
public sealed record SprachdateiPruefung(int AnzahlTexte, SprachdateiFehler? Fehler, string? Pfad);

/// <summary>
/// Prüft importierte Sprachdateien, bevor sie gespeichert werden. Erlaubt sind nur verschachtelte Objekte mit
/// Texten; Zeichen, die als HTML oder als Steuerzeichen der Oberfläche (vue-i18n) wirken könnten, werden
/// abgewiesen. Platzhalter haben die Form {name}.
/// </summary>
public static partial class Sprachdatei
{
    public const int MaxTiefe = 5;
    public const int MaxTexte = 3000;
    public const int MaxTextlaenge = 1000;

    [GeneratedRegex("^[a-z]{2}$")]
    private static partial Regex CodeMuster();

    [GeneratedRegex("^[A-Za-z0-9_-]{1,60}$")]
    private static partial Regex SchluesselMuster();

    // Geschweifte Klammern nur als vollständiger Platzhalter {name}.
    [GeneratedRegex(@"\{[A-Za-z0-9_]+\}")]
    private static partial Regex Platzhalter();

    // < und > (HTML), @ und | (Verweise und Mehrzahl in vue-i18n) sind in Texten nicht erlaubt.
    private static readonly char[] Unzulaessig = ['<', '>', '@', '|'];

    public static bool CodeGueltig(string? code) => code is not null && CodeMuster().IsMatch(code);

    public static bool NameGueltig(string? name) =>
        !string.IsNullOrWhiteSpace(name) && name.Trim().Length is >= 2 and <= 40 && name.IndexOfAny(Unzulaessig) < 0;

    public static SprachdateiPruefung Pruefen(JsonElement texte)
    {
        if (texte.ValueKind != JsonValueKind.Object)
        {
            return new SprachdateiPruefung(0, SprachdateiFehler.KeinObjekt, null);
        }

        var anzahl = 0;
        var fehler = Durchlaufen(texte, string.Empty, 1, ref anzahl);
        if (fehler is not null)
        {
            return fehler;
        }

        return anzahl == 0
            ? new SprachdateiPruefung(0, SprachdateiFehler.Leer, null)
            : new SprachdateiPruefung(anzahl, null, null);
    }

    private static SprachdateiPruefung? Durchlaufen(JsonElement objekt, string pfad, int tiefe, ref int anzahl)
    {
        if (tiefe > MaxTiefe)
        {
            return new SprachdateiPruefung(0, SprachdateiFehler.ZuTief, pfad);
        }

        foreach (var eigenschaft in objekt.EnumerateObject())
        {
            var schluessel = pfad.Length == 0 ? eigenschaft.Name : $"{pfad}.{eigenschaft.Name}";
            if (!SchluesselMuster().IsMatch(eigenschaft.Name))
            {
                return new SprachdateiPruefung(0, SprachdateiFehler.SchluesselUngueltig, schluessel);
            }

            switch (eigenschaft.Value.ValueKind)
            {
                case JsonValueKind.Object:
                    var unten = Durchlaufen(eigenschaft.Value, schluessel, tiefe + 1, ref anzahl);
                    if (unten is not null)
                    {
                        return unten;
                    }

                    break;

                case JsonValueKind.String:
                    var text = eigenschaft.Value.GetString() ?? string.Empty;
                    if (text.Length > MaxTextlaenge)
                    {
                        return new SprachdateiPruefung(0, SprachdateiFehler.TextZuLang, schluessel);
                    }

                    if (text.IndexOfAny(Unzulaessig) >= 0)
                    {
                        return new SprachdateiPruefung(0, SprachdateiFehler.ZeichenUnzulaessig, schluessel);
                    }

                    // Nach dem Entfernen gültiger Platzhalter darf keine geschweifte Klammer übrig bleiben.
                    var ohnePlatzhalter = Platzhalter().Replace(text, string.Empty);
                    if (ohnePlatzhalter.Contains('{') || ohnePlatzhalter.Contains('}'))
                    {
                        return new SprachdateiPruefung(0, SprachdateiFehler.PlatzhalterUngueltig, schluessel);
                    }

                    if (++anzahl > MaxTexte)
                    {
                        return new SprachdateiPruefung(0, SprachdateiFehler.ZuViele, schluessel);
                    }

                    break;

                default:
                    return new SprachdateiPruefung(0, SprachdateiFehler.KeinText, schluessel);
            }
        }

        return null;
    }
}
