namespace Soforthilfe.Dienste.Verstaendnis;

/// <summary>
/// Ähnlichkeit eines Freitexts zu einer Kategorie (Kosinusähnlichkeit, 0 bis 1).
/// </summary>
public sealed record Kategorieaehnlichkeit(string Kennung, double Aehnlichkeit);

/// <summary>
/// Ergebnis der Klassifikation. «Kennung» ist nur gesetzt, wenn das Ergebnis sicher genug ist; «Rangfolge» nennt
/// alle Kategorien mit einer Ähnlichkeit über 0, die ähnlichste zuerst.
/// </summary>
public sealed record Klassifikationsergebnis(
    string? Kennung,
    double Aehnlichkeit,
    double Abstand,
    IReadOnlyList<Kategorieaehnlichkeit> Rangfolge);

/// <summary>
/// Ordnet Freitext einer Kategorie zu.
/// </summary>
public interface ITextKlassifikator
{
    Klassifikationsergebnis Klassifizieren(string? freitext);
}

/// <summary>
/// Klassischer Textklassifikator mit TF-IDF und Kosinusähnlichkeit, ohne externe Bibliothek. Für jede Kategorie
/// entsteht aus den Trainingsbeispielen ein Schwerpunktvektor; ein Freitext gehört zur Kategorie mit der höchsten
/// Ähnlichkeit, wenn diese hoch genug ist und sich genug von der zweitbesten abhebt.
/// </summary>
public sealed class TfIdfKlassifikator : ITextKlassifikator
{
    // Unterhalb dieser Ähnlichkeit gilt das Ergebnis als unsicher.
    public const double Mindestaehnlichkeit = 0.12;

    // Liegen die beiden besten Kategorien näher beieinander, entscheidet die nächste Stufe.
    public const double Mindestabstand = 0.03;

    private readonly Dictionary<string, double> inverseDokumenthaeufigkeit = new();
    private readonly Dictionary<string, Dictionary<string, double>> schwerpunkte = new();

    // Häufige Wörter ohne Aussage über die Kategorie, in normalisierter Form.
    private static readonly HashSet<string> Stoppwoerter =
    [
        "der", "die", "das", "ein", "eine", "einen", "und", "oder", "ich", "mir", "mich", "mein",
        "meine", "ist", "sind", "war", "nicht", "mehr", "sehr", "auch", "noch", "aber", "bin",
        "habe", "hat", "haben", "kann", "koennte", "muss", "brauche", "bitte", "fuer", "von",
        "mit", "bei", "auf", "in", "im", "am", "zu", "zum", "zur", "es", "sie", "er", "wir",
        "man", "dem", "den", "des", "als", "wie", "so", "da", "dass", "wenn",
        // Französisch
        "le", "la", "les", "et", "est", "pas", "je", "mon", "ma", "mes", "un", "une", "avec", "sur",
        "dans", "de", "du", "pour", "ai", "au", "aux", "ce", "cette", "il", "elle", "ne", "plus", "que",
        "qui", "me", "en", "se", "sans", "suis", "besoin", "quelqu", "peut", "faut", "tres", "chez",
    ];

    public TfIdfKlassifikator()
    {
        Trainieren(Trainingsdaten.BeispieleProKategorie);
    }

    private void Trainieren(IReadOnlyDictionary<string, string[]> beispiele)
    {
        var dokumente = beispiele
            .SelectMany(k => k.Value.Select(b => (Kategorie: k.Key, Woerter: Zerlegen(b))))
            .ToList();

        // Inverse Dokumenthäufigkeit über alle Beispiele, geglättet.
        var vorkommen = new Dictionary<string, int>();
        foreach (var wort in dokumente.SelectMany(d => d.Woerter.Distinct()))
        {
            vorkommen[wort] = vorkommen.GetValueOrDefault(wort) + 1;
        }

        foreach (var (wort, anzahl) in vorkommen)
        {
            inverseDokumenthaeufigkeit[wort] = Math.Log((double)(dokumente.Count + 1) / (anzahl + 1)) + 1.0;
        }

        // Schwerpunktvektor je Kategorie: Mittelwert der Beispielvektoren, auf Länge 1 gebracht.
        foreach (var gruppe in dokumente.GroupBy(d => d.Kategorie))
        {
            var summe = new Dictionary<string, double>();
            foreach (var dokument in gruppe)
            {
                foreach (var (wort, gewicht) in Vektorisieren(dokument.Woerter))
                {
                    summe[wort] = summe.GetValueOrDefault(wort) + gewicht;
                }
            }

            var anzahl = gruppe.Count();
            schwerpunkte[gruppe.Key] = Normieren(summe.ToDictionary(e => e.Key, e => e.Value / anzahl));
        }
    }

    public Klassifikationsergebnis Klassifizieren(string? freitext)
    {
        var woerter = Zerlegen(freitext ?? string.Empty);
        if (woerter.Count == 0)
        {
            return new Klassifikationsergebnis(null, 0, 0, []);
        }

        var vektor = Normieren(Vektorisieren(woerter));
        var rangfolge = schwerpunkte
            .Select(s => new Kategorieaehnlichkeit(s.Key, Math.Round(Kosinus(vektor, s.Value), 3)))
            .Where(a => a.Aehnlichkeit > 0)
            .OrderByDescending(a => a.Aehnlichkeit)
            .ToList();

        var beste = rangfolge.Count > 0 ? rangfolge[0].Aehnlichkeit : 0;
        var zweitbeste = rangfolge.Count > 1 ? rangfolge[1].Aehnlichkeit : 0;
        var abstand = Math.Round(beste - zweitbeste, 3);
        var sicher = beste >= Mindestaehnlichkeit && abstand >= Mindestabstand;

        return new Klassifikationsergebnis(sicher ? rangfolge[0].Kennung : null, beste, abstand, rangfolge);
    }

    private Dictionary<string, double> Vektorisieren(List<string> woerter)
    {
        var vektor = new Dictionary<string, double>();
        foreach (var gruppe in woerter.GroupBy(w => w))
        {
            // Termhäufigkeit mal inverse Dokumenthäufigkeit; unbekannte Wörter zählen mit 1.
            var tf = (double)gruppe.Count() / woerter.Count;
            vektor[gruppe.Key] = tf * inverseDokumenthaeufigkeit.GetValueOrDefault(gruppe.Key, 1.0);
        }

        return vektor;
    }

    private static Dictionary<string, double> Normieren(Dictionary<string, double> vektor)
    {
        var laenge = Math.Sqrt(vektor.Values.Sum(w => w * w));
        return laenge == 0 ? vektor : vektor.ToDictionary(e => e.Key, e => e.Value / laenge);
    }

    private static double Kosinus(Dictionary<string, double> a, Dictionary<string, double> b)
    {
        var (kleiner, groesser) = a.Count <= b.Count ? (a, b) : (b, a);
        var summe = 0.0;
        foreach (var (wort, gewicht) in kleiner)
        {
            if (groesser.TryGetValue(wort, out var anderes))
            {
                summe += gewicht * anderes;
            }
        }

        return summe;
    }

    /// <summary>Wörter mit mehr als zwei Zeichen, ohne Stoppwörter, auf eine einfache Stammform gekürzt.</summary>
    public static List<string> Zerlegen(string text) =>
        Textnormalisierung.Woerter(text)
            .Where(w => w.Length > 2 && !Stoppwoerter.Contains(w))
            .Select(Stammform)
            .ToList();

    /// <summary>Sehr einfache Stammform für häufige deutsche Endungen, zum Beispiel «Heizungen» zu «heiz».</summary>
    public static string Stammform(string wort)
    {
        string[] endungen = ["ungen", "enden", "ende", "ung", "est", "ern", "en", "er", "es", "em", "e", "s"];
        foreach (var endung in endungen)
        {
            if (wort.Length - endung.Length >= 4 && wort.EndsWith(endung, StringComparison.Ordinal))
            {
                return wort[..^endung.Length];
            }
        }

        return wort;
    }
}
