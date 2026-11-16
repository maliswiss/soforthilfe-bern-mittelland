using Soforthilfe.Datenzugriff.Modell;

namespace Soforthilfe.Dienste.Verstaendnis;

/// <summary>
/// Dringlichkeit einer Anfrage mit kurzer Begründung.
/// </summary>
public sealed record DringlichkeitInfo(Prioritaet Prioritaet, string Grund);

/// <summary>
/// Leitet die Dringlichkeit aus Text und Kategorie ab. Signalwörter im Text haben Vorrang, Wörter für «keine Eile»
/// vor Wörtern für «sofort»; ohne Signalwort gilt die übliche Dringlichkeit der Kategorie. Eine ausdrücklich
/// gewählte Dringlichkeit ersetzt diese Ableitung.
/// </summary>
public static class Dringlichkeit
{
    // Normalisierte Begriffe (wie bei den Schlüsselwortregeln), Deutsch, Französisch, Italienisch und Englisch.
    public static readonly string[] Tief =
    [
        "keine eile", "eilt nicht", "nicht dringend", "kein stress", "irgendwann", "gelegentlich",
        "bei gelegenheit", "wenn moeglich", "naechste woche", "naechsten woche", "in den naechsten tagen",
        "pas presse", "pas pressee", "pas urgent", "semaine prochaine", "quand vous pouvez",
        "dans les prochains jours", "non urgente", "no rush", "next week",
    ];

    public static readonly string[] Hoch =
    [
        "sofort", "dringend", "notfall", "jetzt", "eilt", "schnellstmoeglich", "so schnell wie moeglich",
        "heute noch", "heute abend", "heute nacht", "nachts", "in einer stunde", "zug verpasst",
        "kalt", "kein strom", "kein wasser", "kein licht", "rohrbruch", "ueberschwemm",
        "kind", "kinder", "baby", "rollstuhl",
        "urgent", "urgence", "tout de suite", "immediatement", "au plus vite", "ce soir", "cette nuit",
        "plus de courant", "plus d eau", "inonde", "enfant", "bebe",
        "subito", "urgente", "asap", "right now", "tonight",
    ];

    // Ausgesperrt oder mit einer Panne unterwegs: meist sofort nötig. Transporte und Umzüge sind meist planbar.
    private static readonly HashSet<string> KategorienHoch = ["schluesseldienst", "pannenhilfe"];
    private static readonly HashSet<string> KategorienTief = ["transporthilfe"];

    public static DringlichkeitInfo Bestimmen(string? freitext, string? kategorieKennung)
    {
        if (!string.IsNullOrWhiteSpace(freitext))
        {
            var text = Textnormalisierung.Normalisieren(freitext);
            var woerter = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (Tief.FirstOrDefault(b => Textnormalisierung.Enthaelt(text, woerter, b)) is { } tief)
            {
                return new DringlichkeitInfo(Prioritaet.Niedrig,
                    $"Dringlichkeit tief wegen «{Textnormalisierung.Fundstelle(freitext, tief)}».");
            }

            if (Hoch.FirstOrDefault(b => Textnormalisierung.Enthaelt(text, woerter, b)) is { } hoch)
            {
                return new DringlichkeitInfo(Prioritaet.Hoch,
                    $"Dringlichkeit hoch wegen «{Textnormalisierung.Fundstelle(freitext, hoch)}».");
            }
        }

        if (kategorieKennung is not null && KategorienHoch.Contains(kategorieKennung))
        {
            return new DringlichkeitInfo(Prioritaet.Hoch, "Dringlichkeit hoch, weil diese Kategorie meist sofort nötig ist.");
        }

        if (kategorieKennung is not null && KategorienTief.Contains(kategorieKennung))
        {
            return new DringlichkeitInfo(Prioritaet.Niedrig, "Dringlichkeit tief, weil diese Kategorie meist planbar ist.");
        }

        return new DringlichkeitInfo(Prioritaet.Mittel, "Dringlichkeit mittel, ohne Hinweis auf Eile.");
    }
}
