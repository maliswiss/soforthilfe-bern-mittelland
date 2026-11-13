namespace Soforthilfe.Dienste.Verstaendnis;

/// <summary>
/// Treffer der Regelstufe: Kennung der Kategorie, erstes gefundenes Schlüsselwort und Sicherheit.
/// </summary>
public sealed record Regelergebnis(string Kennung, string Begriff, double Konfidenz);

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

        // Eindeutig nur, wenn eine Kategorie mehr Treffer hat als jede andere.
        if (treffer.Count == 0 || (treffer.Count > 1 && treffer[0].Begriffe.Count == treffer[1].Begriffe.Count))
        {
            return null;
        }

        // Sicherheit: 0.8 bei einem Treffer, je weiterem Treffer mehr, höchstens 0.95; Mitbewerber senken sie.
        var beste = treffer[0];
        var konfidenz = Math.Min(0.95, 0.7 + 0.1 * beste.Begriffe.Count - (treffer.Count > 1 ? 0.1 : 0));
        return new Regelergebnis(beste.Kennung, beste.Begriffe[0], Math.Round(konfidenz, 2));
    }
}
