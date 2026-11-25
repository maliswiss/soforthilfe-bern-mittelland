using Soforthilfe.Dienste.Verstaendnis;

namespace Soforthilfe.Dienste.Notfall;

/// <summary>
/// Ergebnis der Notfallprüfung. «Schluesselwort» nennt den erkannten Begriff so, wie er im Text steht,
/// «Begriff» denselben Eintrag aus der festen Liste (für das Protokoll, ohne Wortlaut aus dem Text).
/// </summary>
public sealed record Notfallergebnis(bool IstNotfall, string? Schluesselwort, string? Begriff = null)
{
    public static readonly Notfallergebnis KeinNotfall = new(false, null);
}

/// <summary>
/// Regelbasierte Prüfung auf akute gesundheitliche Hinweise in Deutsch, Mundart, Französisch, Italienisch
/// und Englisch. Sie läuft immer, unabhängig vom Erfassungsweg; bei einem Treffer stoppt die
/// Vermittlung und die Plattform verweist auf die Notrufnummer 144.
/// </summary>
public interface INotfallPruefung
{
    Notfallergebnis Pruefen(string? freitext);
}

/// <summary>
/// Bewusst empfindlich: lieber einmal zu oft auf 144 verweisen als einen gesundheitlichen Notfall vermitteln.
/// Feuerwehr (118) und Polizei (117) gehören nicht dazu; dort rufen Betroffene direkt an.
/// </summary>
public sealed class NotfallPruefung : INotfallPruefung
{
    // Begriffe in normalisierter Form (Kleinschreibung, ae/oe/ue, ohne Akzente und Satzzeichen).
    // Das allgemeine Wort «Notfall» fehlt bewusst: «Notfall mit dem Schlüssel» ist kein medizinischer Notfall.
    public static readonly IReadOnlyList<string> Schluesselwoerter =
    [
        // Deutsch
        "herzinfarkt", "herzstillstand", "herzattacke", "schlaganfall", "hirnschlag",
        "bewusstlos", "ohnmaechtig", "nicht ansprechbar", "reagiert nicht mehr", "kollabiert", "zusammengebrochen",
        "atemnot", "atmet nicht", "atmet kaum", "keine atmung", "bekommt keine luft", "kriegt keine luft", "erstickt",
        "starke blutung", "blutet stark", "verblutet", "vergiftung", "vergiftet", "ueberdosis",
        "krampfanfall", "epileptischer anfall", "brustschmerzen", "schmerzen in der brust",
        "notarzt", "ambulanz", "rettungsdienst", "reanimation", "wiederbelebung",
        "schwere verbrennung", "unfall mit verletzten", "schwer verletzt", "sturz aus hoehe",
        "anaphylaktisch", "allergischer schock", "geburt hat eingesetzt", "wehen", "lebensgefahr",
        // Anzeichen eines Hirnschlags ohne Fachbegriff
        "spricht undeutlich", "spricht verwaschen", "arm nicht mehr bewegen", "bein nicht mehr bewegen",
        "gesicht haengt", "haengender mundwinkel",

        // Mundart (Berndeutsch und weitere)
        "schnuufet nuemm", "schnuufet nuem", "schnuufet nid", "schnuufet chuum", "chunnt kei luft",
        "bechunnt kei luft", "nuemm aasprechbar", "nuemm ansprechbar", "zaemebroche",

        // Französisch
        "urgence medicale", "crise cardiaque", "arret cardiaque", "infarctus", "inconscient", "inconsciente",
        "evanoui", "evanouie", "ne respire plus", "respire mal", "hemorragie", "saigne beaucoup", "avc",
        "accident vasculaire", "overdose", "intoxication", "empoisonnement", "convulsion", "crise d epilepsie",
        "douleur thoracique", "douleurs dans la poitrine", "ambulance", "brulure grave", "choc anaphylactique",
        "accouchement", "danger de mort", "ne peut plus bouger le bras",

        // Englisch
        "heart attack", "stroke", "unconscious", "not breathing", "severe bleeding", "chest pain", "cant breathe",
        "can t breathe", "cannot breathe", "difficulty breathing", "trouble breathing", "passed out", "seizure",
        "heavy bleeding", "bleeding heavily", "slurred speech", "anaphylactic",

        // Italienisch
        "infarto", "incosciente", "emorragia", "non respira", "dolore al petto", "non riesce a respirare",
    ];

    public Notfallergebnis Pruefen(string? freitext)
    {
        if (string.IsNullOrWhiteSpace(freitext))
        {
            return Notfallergebnis.KeinNotfall;
        }

        var text = $" {Textnormalisierung.Normalisieren(freitext)} ";
        foreach (var begriff in Schluesselwoerter)
        {
            if (Enthaelt(text, begriff))
            {
                return new Notfallergebnis(true, Textnormalisierung.Fundstelle(freitext, begriff), begriff);
            }
        }

        return Notfallergebnis.KeinNotfall;
    }

    // Kurze Begriffe (bis fünf Buchstaben, zum Beispiel «avc», «wehen») nur als ganzes Wort,
    // längere auch innerhalb zusammengesetzter Wörter (zum Beispiel «herzinfarktverdacht»).
    private static bool Enthaelt(string text, string begriff) =>
        begriff.Length <= 5
            ? text.Contains($" {begriff} ", StringComparison.Ordinal)
            : text.Contains(begriff, StringComparison.Ordinal);
}
