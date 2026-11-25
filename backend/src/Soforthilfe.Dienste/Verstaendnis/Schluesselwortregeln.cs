namespace Soforthilfe.Dienste.Verstaendnis;

/// <summary>
/// Treffer der Regelstufe: Kennung der Kategorie, erstes gefundenes Schlüsselwort und Sicherheit. Bei einem
/// Tippfehler nennt «Schreibweise» das Wort aus dem Text (normalisiert), das dem Schlüsselwort ähnlich ist.
/// </summary>
public sealed record Regelergebnis(string Kennung, string Begriff, double Konfidenz, string? Schreibweise = null);

/// <summary>
/// Regelstufe der Verständniskette: eindeutige Schlüsselwörter in Deutsch, Französisch, Italienisch und Englisch.
/// Trifft nur eine Kategorie zu oder hat eine Kategorie klar mehr Treffer, gilt sie; sonst entscheidet die nächste
/// Stufe. Die Regeln sind nachvollziehbar und funktionieren ohne Modell.
/// </summary>
public static class Schluesselwortregeln
{
    // Begriffe in normalisierter Form (Kleinschreibung, ae/oe/ue, ohne Akzente), je Kennung der Kategorie.
    public static readonly IReadOnlyDictionary<string, string[]> Begriffe = new Dictionary<string, string[]>
    {
        ["schluesseldienst"] =
        [
            "ausgesperrt", "schluessel", "schloss", "haustuer", "wohnungstuer", "tuer zugefallen",
            "cle", "clef", "cles", "serrure", "serrurier", "porte claquee", "enferme dehors", "enfermee dehors",
            "chiave", "chiavi", "serratura", "locked out", "lost my key", "keys",
            // Zylinder, Tür öffnen, vor der Tür stehen
            "zylinder", "tuer oeffnen", "komme nicht rein", "bloque devant", "bloquee devant",
        ],
        ["kleine-reparatur"] =
        [
            "velo", "fahrrad", "schublade", "koffer", "moebelstueck", "bicyclette", "tiroir", "valise",
            "bicicletta", "bike", "bicycle",
            // Montagearbeiten
            "regal", "montieren", "aufhaengen", "bohren", "duebel", "etagere", "refixer", "percer", "accrocher",
            "storen", "rollladen", "jalousie", "store", "volet",
            // Möbel und Hilfsmittel, die klemmen, wackeln oder lose sind
            "montiert", "an die wand", "scharnier", "wackelt", "quietscht", "klemmt", "rollator", "stuhl",
            "chaise", "poignee",
        ],
        ["sanitaer-elektro"] =
        [
            "wasserhahn", "rohrbruch", "heizung", "sicherung", "steckdose", "stromausfall", "kein strom",
            "abfluss", "verstopft", "elektriker", "sanitaer", "boiler", "robinet", "fuite", "chauffage",
            "fusible", "prise electrique", "electricien", "plombier", "canalisation", "bouche",
            "panne de courant", "plus de courant", "rubinetto", "riscaldamento", "plumber", "leak", "heating",
            "electrician", "power outage",
            // Abläufe, Lavabo und Geräte am Wasser
            "laeuft nicht ab", "lavabo", "evier", "ecoule", "waschmaschine", "geschirrspueler", "lave linge",
            "lave vaisselle", "chauffe eau", "eau chaude",
            // Tropfen, Licht, WC und Heizkörper
            "tropf", "kein licht", "heizkoerper", "toilett", "wc", "spuele", "warmes wasser", "kabel",
        ],
        ["pannenhilfe"] =
        [
            "auto", "starthilfe", "abschlepp", "reifenpanne", "autobatterie", "voiture", "depannage",
            "remorquage", "crevaison", "batterie de voiture", "macchina", "car battery", "tow", "flat tyre",
            "flat tire",
            // Fahrzeuge und Überbrücken
            "wagen", "fahrzeug", "motorrad", "ueberbrueck", "parkhaus", "pneu crev",
        ],
        ["transporthilfe"] =
        [
            "umzug", "zuegeln", "transport", "lieferwagen", "anhaenger", "demenagement", "demenager",
            "camionnette", "trasloco", "moving", "move a sofa",
            // Tragen und Entsorgen
            "kisten", "hochtragen", "schleppen", "entsorg", "cartons", "dechetterie", "debarrass", "vieux meubles",
            "karton", "klavier", "estrich", "grenier", "umzieh",
        ],
        ["it-soforthilfe"] =
        [
            "laptop", "computer", "handy", "smartphone", "drucker", "wlan", "router", "ordinateur",
            "telephone portable", "imprimante", "wifi", "stampante", "printer", "phone",
            "datensicherung", "backup", "passwort", "internet", "mot de passe",
            // Geräte und Konten
            "tablet", "fernseher", "mobiltelefon", "bildschirm", "telephone", "e mail", "email",
        ],
        ["reinigung"] =
        [
            "reinigung", "putzen", "wasserschaden", "ueberschwemm", "schimmel", "nettoyage", "nettoyer",
            "degat des eaux", "inondation", "inonde", "moisissure", "pulizia", "cleaning", "flood",
            // Russ nach einem Brand; ein laufendes Feuer gehört zur Feuerwehr, nicht auf die Plattform
            "russ", "brandschaden", "suie",
            // Schlamm und Dreck nach Hochwasser oder Sturm
            "schlamm", "hochwasser", "unter wasser", "dreck", "boue", "geputzt", "gereinigt",
        ],
        ["lieferdienst"] =
        [
            "apotheke", "medikament", "einkauf", "einkaufen", "lebensmittel", "pharmacie", "courses",
            "medicament", "livraison", "farmacia", "spesa", "groceries", "pharmacy",
            // Tabletten, Rezept und Einkäufe bringen
            "tabletten", "rezept", "liefern", "einkaeuf", "nourriture",
        ],
        ["nachbarschaftshilfe"] =
        [
            "nachbar", "nachbarschaft", "katze", "hund", "giessen", "voisin", "voisine", "voisinage", "chat",
            "chien", "arroser", "vicino", "gatto", "neighbour", "neighbor",
            // Pflanzen, Haustiere, Briefkasten und Gesellschaft
            "pflanzen", "spazieren", "briefkasten", "quartier", "gesellschaft", "formular", "plantes", "promener",
        ],
        ["begleitung"] =
        [
            "begleit", "arzttermin", "behoerde", "behoerdengang", "orientierung", "accompagner",
            "accompagnement", "rendez vous", "accompagnare", "accompany", "appointment",
            // nicht allein gehen, Spitaltermin
            "nicht allein", "pas seul", "aller seul", "spital", "hopital",
            // Arztpraxen und Ämter
            "zahnarzt", "augenarzt", "arztpraxis", "physiotherap", "sozialamt", "verwaltung", "dentiste", "medecin",
        ],
    };

    public static Regelergebnis? Anwenden(string? freitext)
    {
        if (string.IsNullOrWhiteSpace(freitext))
        {
            return null;
        }

        var text = Textnormalisierung.Normalisieren(freitext);
        var woerter = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        var treffer = Begriffe
            .Select(k => (Kennung: k.Key, Begriffe: k.Value.Where(b => Textnormalisierung.Enthaelt(text, woerter, b)).ToList()))
            .Where(t => t.Begriffe.Count > 0)
            .OrderByDescending(t => t.Begriffe.Count)
            .ToList();

        // Ohne genauen Treffer: Tippfehler in längeren Wörtern zulassen, zum Beispiel «heitzung» für «heizung».
        if (treffer.Count == 0)
        {
            return Aehnlich(woerter);
        }

        // Eindeutig nur, wenn eine Kategorie mehr Treffer hat als jede andere.
        if (treffer.Count > 1 && treffer[0].Begriffe.Count == treffer[1].Begriffe.Count)
        {
            return null;
        }

        // Sicherheit: 0.8 bei einem Treffer, je weiterem Treffer mehr, höchstens 0.95; Mitbewerber senken sie.
        var beste = treffer[0];
        var konfidenz = Math.Min(0.95, 0.7 + 0.1 * beste.Begriffe.Count - (treffer.Count > 1 ? 0.1 : 0));
        return new Regelergebnis(beste.Kennung, beste.Begriffe[0], Math.Round(konfidenz, 2));
    }

    // Kürzere Schlüsselwörter bleiben aussen vor: dort ergibt ein anderer Buchstabe zu oft ein anderes Wort,
    // zum Beispiel «storen» und «stören» oder «bohren» und «Bohnen».
    private const int MinLaengeBegriff = 7;
    private const int MinLaengeWort = 6;
    private const double KonfidenzAehnlich = 0.7;

    // Ein Wort, das genau einen Buchstaben von einem einzelnen Schlüsselwort abweicht. Gilt nur, wenn alle
    // solchen Wörter zur selben Kategorie gehören; die Sicherheit ist tiefer als bei einem genauen Treffer.
    private static Regelergebnis? Aehnlich(string[] woerter)
    {
        var treffer = Begriffe
            .SelectMany(k => k.Value
                .Where(b => b.Length >= MinLaengeBegriff && !b.Contains(' '))
                .SelectMany(b => woerter
                    .Where(w => w.Length >= MinLaengeWort && EinBuchstabeAnders(w, b))
                    .Select(w => (Kennung: k.Key, Begriff: b, Wort: w))))
            .ToList();

        if (treffer.Count == 0 || treffer.Select(t => t.Kennung).Distinct().Count() > 1)
        {
            return null;
        }

        return new Regelergebnis(treffer[0].Kennung, treffer[0].Begriff, KonfidenzAehnlich, treffer[0].Wort);
    }

    /// <summary>
    /// Weichen zwei Wörter um genau einen Buchstaben ab: einer zu viel, einer zu wenig, ein anderer oder zwei
    /// benachbarte vertauscht?
    /// </summary>
    public static bool EinBuchstabeAnders(string a, string b)
    {
        if (a == b || Math.Abs(a.Length - b.Length) > 1)
        {
            return false;
        }

        if (a.Length == b.Length)
        {
            var unterschiede = Enumerable.Range(0, a.Length).Where(i => a[i] != b[i]).ToList();
            return unterschiede.Count == 1
                || (unterschiede.Count == 2 && unterschiede[1] == unterschiede[0] + 1
                    && a[unterschiede[0]] == b[unterschiede[1]] && a[unterschiede[1]] == b[unterschiede[0]]);
        }

        var (lang, kurz) = a.Length > b.Length ? (a, b) : (b, a);
        for (var i = 0; i < kurz.Length; i++)
        {
            if (lang[i] != kurz[i])
            {
                return lang[(i + 1)..] == kurz[i..];
            }
        }

        // Nur der letzte Buchstabe ist zusätzlich.
        return true;
    }
}
