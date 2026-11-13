namespace Soforthilfe.Dienste.Verstaendnis;

/// <summary>
/// Fiktive Trainingsbeispiele für die Klassifikation von Freitext-Anfragen auf Deutsch und Französisch, mindestens
/// 17 je Kategorie. Die Beispiele sind bewusst kurz und alltagsnah formuliert und bilden die Grundlage des
/// TF-IDF-Modells. Sie enthalten keine echten Anfragen und keine Personendaten.
/// </summary>
public static class Trainingsdaten
{
    public static readonly IReadOnlyDictionary<string, string[]> BeispieleProKategorie =
        new Dictionary<string, string[]>
        {
            ["schluesseldienst"] =
            [
                "Ich habe mich ausgesperrt und komme nicht mehr in die Wohnung",
                "Mein Schlüssel ist im Schloss abgebrochen",
                "Die Wohnungstür ist zugefallen, der Schlüssel liegt drinnen",
                "Ich brauche jemanden, der die Haustür öffnen kann",
                "Schloss klemmt, ich komme nicht ins Haus",
                // Französisch
                "Je suis enfermé dehors et je ne peux plus entrer dans mon appartement",
                "Ma clé s'est cassée dans la serrure",
                "La porte a claqué et les clés sont restées à l'intérieur",
                "J'ai besoin d'un serrurier pour ouvrir la porte",
                "La serrure est bloquée, impossible d'entrer",
                // Ergänzung: weiterer Wortschatz
                "Der Schlüssel dreht sich im Zylinder nicht mehr",
                "Ich komme nicht in meine Wohnung, die Tür ist zu",
                "Schlüsselbund verloren, ich stehe vor dem Haus",
                "Die Tür lässt sich nicht mehr öffnen, das Schloss ist blockiert",
                "Jemand muss mir die Tür aufmachen",
                "J'ai perdu mon trousseau et je reste devant la porte",
                "La porte est fermée et je ne peux pas entrer"
            ],
            ["kleine-reparatur"] =
            [
                "Das Rad meines Koffers ist abgebrochen, ich brauche eine schnelle Reparatur",
                "Mein Velo hat einen platten Reifen",
                "Der Griff meiner Tasche ist gerissen",
                "Die Schublade ist kaputt und lässt sich nicht mehr schliessen",
                "Kleine Reparatur an einem Möbelstück nötig",
                // Französisch
                "La roue de ma valise est cassée",
                "Mon vélo a un pneu crevé",
                "La poignée de mon sac est arrachée",
                "Le tiroir est cassé et ne ferme plus",
                "Petite réparation nécessaire sur un meuble",
                // Ergänzung: weiterer Wortschatz
                "Ein Tisch wackelt und muss festgeschraubt werden",
                "Bild oder Spiegel an der Wand befestigen",
                "Die Schranktür hängt schief und klemmt",
                "Ein Rad am Rollator oder Kinderwagen ist lose",
                "Lampe an der Decke anbringen",
                "Velokette abgesprungen, Bremse schleift",
                "Réparer une chaise ou une table qui bouge",
                "Fixer un miroir au mur"
            ],
            ["sanitaer-elektro"] =
            [
                "Wasserhahn tropft stark und ich finde den Absperrhahn nicht",
                "Die Heizung fällt aus und es wird kalt",
                "Sicherung fliegt immer wieder heraus, kein Strom in der Küche",
                "Das WC läuft über, ich brauche einen Sanitärnotdienst",
                "Steckdose funktioniert nicht mehr, Elektriker gesucht",
                // Französisch
                "Le robinet fuit fortement et je ne trouve pas la vanne",
                "Le chauffage ne fonctionne plus et il fait froid",
                "Le fusible saute sans arrêt, plus de courant dans la cuisine",
                "Les toilettes débordent, il me faut un plombier",
                "La prise ne fonctionne plus, je cherche un électricien",
                // Ergänzung: weiterer Wortschatz
                "Unter dem Lavabo tropft Wasser auf den Boden",
                "Kein Licht in der ganzen Wohnung",
                "Heizkörper bleiben kalt, die Heizung läuft nicht",
                "Die WC-Spülung läuft ständig",
                "Kein warmes Wasser aus dem Boiler",
                "Ein Schalter funkt und das Kabel riecht verbrannt",
                "Pas de lumière dans tout l'appartement",
                "L'eau coule sous l'évier"
            ],
            ["pannenhilfe"] =
            [
                "Mein Auto springt nicht an, Batterie leer",
                "Reifenpanne auf dem Parkplatz, ich brauche Hilfe",
                "Das Fahrzeug bleibt liegen, Abschleppdienst nötig",
                "Starthilfe für das Auto gesucht",
                "Panne unterwegs, Wagen fährt nicht mehr",
                // Französisch
                "Ma voiture ne démarre pas, la batterie est vide",
                "Crevaison sur le parking, j'ai besoin d'aide",
                "Le véhicule est en panne, il faut un remorquage",
                "Je cherche quelqu'un pour une aide au démarrage",
                "Panne en route, la voiture n'avance plus",
                // Ergänzung: weiterer Wortschatz
                "Reifen am Auto platt, ich stehe am Strassenrand",
                "Der Wagen bleibt auf der Strasse stehen",
                "Motorrad oder Roller springt nicht an",
                "Überbrückungskabel für die Batterie gesucht",
                "Fahrzeug muss in die Garage geschleppt werden",
                "Le pneu de la voiture est à plat au bord de la route",
                "La voiture ne roule plus, il faut la remorquer"
            ],
            ["transporthilfe"] =
            [
                "Ich brauche Hilfe beim Transport eines Schranks",
                "Umzugshilfe für ein paar Kisten gesucht",
                "Möbel müssen in den dritten Stock getragen werden",
                "Transport eines Sofas mit einem Lieferwagen nötig",
                "Jemand mit Anhänger für einen kleinen Transport gesucht",
                "Ein Kühlschrank muss in den Keller getragen werden",
                "Waschmaschine in den ersten Stock tragen",
                "Schwere Kartons in den Keller tragen und Möbel verschieben",
                // Französisch
                "J'ai besoin d'aide pour transporter une armoire",
                "Aide au déménagement pour quelques cartons",
                "Des meubles doivent être portés au troisième étage",
                "Transport d'un canapé avec une camionnette",
                "Je cherche quelqu'un avec une remorque pour un petit transport",
                "Un réfrigérateur doit être porté à la cave",
                "Porter une machine à laver au premier étage",
                "Porter des cartons lourds à la cave et déplacer des meubles",
                // Ergänzung: weiterer Wortschatz
                "Ein Bett und ein Tisch müssen in eine andere Wohnung gebracht werden",
                "Wir ziehen um und brauchen Leute zum Tragen",
                "Ein Klavier oder schwere Möbel verschieben",
                "Jemand mit Bus oder Auto für einen grossen Transport",
                "Alte Möbel zur Entsorgung fahren",
                "Kartons auf den Estrich tragen",
                "Transporter un lit et une table dans un autre appartement",
                "Porter des meubles lourds au grenier"
            ],
            ["it-soforthilfe"] =
            [
                "Mein Laptop startet nicht mehr und ich brauche die Daten heute",
                "Das Handy lässt sich nicht mehr entsperren",
                "Der Drucker verbindet sich nicht mit dem WLAN",
                "Internet zuhause funktioniert nicht, Router streikt",
                "Hilfe bei einem Computerproblem gesucht",
                // Französisch
                "Mon ordinateur portable ne démarre plus et j'ai besoin des données aujourd'hui",
                "Je ne peux plus déverrouiller mon téléphone",
                "L'imprimante ne se connecte pas au wifi",
                "Internet ne fonctionne pas à la maison, le routeur est en panne",
                "Je cherche de l'aide pour un problème d'ordinateur",
                // Ergänzung: weiterer Wortschatz
                "Das Tablet zeigt einen schwarzen Bildschirm",
                "Ich komme nicht mehr in mein E-Mail-Konto",
                "Der Fernseher findet keine Sender",
                "Mobiltelefon lädt nicht und geht nicht mehr an",
                "Neuen Computer einrichten und Programme installieren",
                "Passwort vergessen, Konto gesperrt",
                "Mon téléphone ne s'allume plus",
                "Je n'arrive pas à configurer mon ordinateur"
            ],
            ["reinigung"] =
            [
                "Nach einem Wasserschaden muss die Wohnung gereinigt werden",
                "Keller ist überschwemmt und muss gereinigt werden",
                "Reinigung nach einem Rohrbruch nötig",
                "Starke Verschmutzung im Treppenhaus beseitigen",
                "Hilfe beim Aufräumen und Putzen nach einem Schaden",
                // Französisch
                "Après un dégât des eaux, l'appartement doit être nettoyé",
                "La cave est inondée et doit être nettoyée",
                "Nettoyage nécessaire après une rupture de conduite",
                "Grosse saleté à enlever dans la cage d'escalier",
                "Aide pour ranger et nettoyer après un sinistre",
                // Ergänzung: weiterer Wortschatz
                "Nach dem Hochwasser ist alles voller Schlamm",
                "Die Waschküche steht unter Wasser",
                "Russ und Gestank nach einem kleinen Brand entfernen",
                "Schimmel an der Wand im Badezimmer",
                "Teppich und Boden nach einem Schaden reinigen",
                "Nach dem Sturm liegt überall Dreck",
                "Enlever la boue et la saleté après une inondation",
                "Nettoyer la moisissure dans la salle de bain"
            ],
            ["lieferdienst"] =
            [
                "Ich kann nicht aus dem Haus und brauche Medikamente aus der Apotheke",
                "Einkauf müsste geliefert werden, ich bin nicht mobil",
                "Jemand könnte mir Brot und Milch bringen",
                "Abholung eines Rezepts in der Apotheke gesucht",
                "Lieferung von Lebensmitteln nach Hause",
                // Französisch
                "Je ne peux pas sortir et j'ai besoin de médicaments de la pharmacie",
                "Mes courses devraient être livrées, je ne suis pas mobile",
                "Quelqu'un pourrait m'apporter du pain et du lait",
                "Je cherche quelqu'un pour retirer une ordonnance à la pharmacie",
                "Livraison de produits alimentaires à domicile",
                // Ergänzung: weiterer Wortschatz
                "Ich bin krank und brauche Essen und Getränke",
                "Tabletten in der Apotheke abholen und bringen",
                "Einkäufe nach Hause liefern",
                "Ich kann nicht einkaufen gehen, bitte Milch und Brot",
                "Rezept abholen und vorbeibringen",
                "Wasser und Lebensmittel für das Wochenende",
                "Apporter des courses et de l'eau à la maison",
                "Récupérer des médicaments à la pharmacie"
            ],
            ["nachbarschaftshilfe"] =
            [
                "Ich brauche kurz Hilfe von jemandem aus der Nachbarschaft",
                "Kann jemand meine Katze für zwei Tage füttern",
                "Hilfe beim Tragen der Einkäufe in den vierten Stock",
                "Jemand könnte kurz beim Aufhängen einer Lampe helfen",
                "Nachbarschaftliche Unterstützung im Alltag gesucht",
                // Französisch
                "J'ai besoin d'un petit coup de main de quelqu'un du quartier",
                "Quelqu'un peut-il nourrir mon chat pendant deux jours",
                "Aide pour porter les courses au quatrième étage",
                "Quelqu'un pourrait m'aider à accrocher une lampe",
                "Je cherche un soutien entre voisins au quotidien",
                // Ergänzung: weiterer Wortschatz
                "Pflanzen giessen während den Ferien",
                "Mit dem Hund spazieren gehen",
                "Briefkasten leeren, solange ich weg bin",
                "Jemand aus dem Quartier für eine kurze Hilfe",
                "Ein Formular zusammen ausfüllen",
                "Gesellschaft leisten und etwas plaudern",
                "Arroser les plantes pendant les vacances",
                "Promener le chien d'une voisine"
            ],
            ["begleitung"] =
            [
                "Ich brauche Begleitung zu einem Arzttermin",
                "Fahrt zum Spital für eine Kontrolle gesucht",
                "Jemand sollte mich zum Amt begleiten, ich kenne mich nicht aus",
                "Orientierungshilfe für einen Behördengang",
                "Begleitung für einen älteren Menschen zum Termin",
                // Französisch
                "J'ai besoin d'un accompagnement pour un rendez-vous chez le médecin",
                "Je cherche un trajet vers l'hôpital pour un contrôle",
                "Quelqu'un devrait m'accompagner à l'administration, je ne m'y retrouve pas",
                "Aide à l'orientation pour une démarche administrative",
                "Accompagnement d'une personne âgée à un rendez-vous",
                // Ergänzung: weiterer Wortschatz
                "Begleitung zum Zahnarzt oder Augenarzt",
                "Ich möchte nicht allein ins Spital gehen",
                "Den Weg zur Physiotherapie finden",
                "Mit jemandem zur Bank oder zur Post gehen",
                "Begleitung zur Gemeindeverwaltung oder zum Sozialamt",
                "Unterstützung bei einem Termin auf dem Amt",
                "Accompagner quelqu'un chez le dentiste",
                "Aller à la commune avec une personne de confiance"
            ]
        };
}
