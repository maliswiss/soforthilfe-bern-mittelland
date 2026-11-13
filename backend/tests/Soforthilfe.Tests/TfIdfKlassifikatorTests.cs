using Soforthilfe.Dienste.Verstaendnis;

namespace Soforthilfe.Tests;

/// <summary>
/// TF-IDF-Klassifikator und Verständniskette aus Regeln und maschinellem Lernen. Der Kalibrierungssatz umfasst
/// 100 Sätze (je Kategorie sieben deutsche und drei französische), die nicht in den Trainingsdaten stehen. Regeln,
/// Trainingsdaten und Schwellen wurden daran abgestimmt; die Tests sichern diesen Stand gegen Rückschritte ab.
/// </summary>
public sealed class TfIdfKlassifikatorTests
{
    private static readonly TfIdfKlassifikator Klassifikator = new();

    public static readonly (string Kennung, string Freitext)[] Kalibrierungssatz =
    [
        ("schluesseldienst", "Ich stehe vor meiner Wohnung und komme nicht rein"),
        ("schluesseldienst", "Der Hausschlüssel ist weg und niemand hat einen Ersatz"),
        ("schluesseldienst", "Die Tür ist ins Schloss gefallen, mein Bund liegt in der Küche"),
        ("schluesseldienst", "Mein Zylinder an der Kellertür ist blockiert"),
        ("schluesseldienst", "Kann jemand heute Abend eine Tür öffnen?"),
        ("schluesseldienst", "Schlüssel im Briefkasten verloren"),
        ("schluesseldienst", "Ich habe mich auf dem Balkon ausgesperrt"),
        ("schluesseldienst", "J'ai perdu les clés de mon appartement"),
        ("schluesseldienst", "Ma porte ne s'ouvre plus, la serrure est coincée"),
        ("schluesseldienst", "Je suis bloqué devant ma porte"),
        ("kleine-reparatur", "Ein Stuhlbein ist locker und wackelt"),
        ("kleine-reparatur", "Die Kette meines Velos ist abgesprungen"),
        ("kleine-reparatur", "Ein Bild soll an die Wand gehängt werden"),
        ("kleine-reparatur", "Der Kleiderschrank klemmt und die Tür hängt schief"),
        ("kleine-reparatur", "Mein Rollator quietscht und ein Rad ist lose"),
        ("kleine-reparatur", "Eine Lampe muss an der Decke montiert werden"),
        ("kleine-reparatur", "Das Scharnier vom Küchenschrank ist kaputt"),
        ("kleine-reparatur", "Le pied de ma chaise est cassé"),
        ("kleine-reparatur", "Il faut réparer la poignée de l'armoire"),
        ("kleine-reparatur", "Mon vélo a besoin d'une petite réparation"),
        ("sanitaer-elektro", "Unter der Spüle ist alles nass, es tropft"),
        ("sanitaer-elektro", "In der Wohnung gibt es seit einer Stunde kein Licht"),
        ("sanitaer-elektro", "Der Boiler macht kein warmes Wasser mehr"),
        ("sanitaer-elektro", "Das Lavabo im Bad läuft nicht ab"),
        ("sanitaer-elektro", "Die Heizkörper bleiben kalt"),
        ("sanitaer-elektro", "Im Bad riecht es nach verbranntem Kabel und der Schalter ist heiss"),
        ("sanitaer-elektro", "Die Toilettenspülung hört nicht auf zu laufen"),
        ("sanitaer-elektro", "Il n'y a plus d'eau chaude dans la salle de bain"),
        ("sanitaer-elektro", "Le chauffage est en panne depuis hier"),
        ("sanitaer-elektro", "L'évier de la cuisine est bouché"),
        ("pannenhilfe", "Mein Wagen bleibt auf der Strasse stehen"),
        ("pannenhilfe", "Die Batterie vom Auto ist leer, ich brauche ein Überbrückungskabel"),
        ("pannenhilfe", "Platter Reifen an der Ausfahrt"),
        ("pannenhilfe", "Das Fahrzeug muss in die Garage geschleppt werden"),
        ("pannenhilfe", "Mein Motorrad springt nicht mehr an"),
        ("pannenhilfe", "Ich stehe mit dem Auto im Parkhaus und es geht nichts mehr"),
        ("pannenhilfe", "Wer kann mir mit Starthilfe helfen?"),
        ("pannenhilfe", "Ma voiture est en panne sur l'autoroute"),
        ("pannenhilfe", "J'ai un pneu crevé sur la route"),
        ("pannenhilfe", "La batterie de la voiture est à plat"),
        ("transporthilfe", "Ein Bett muss in eine andere Wohnung gebracht werden"),
        ("transporthilfe", "Wir ziehen am Samstag um und brauchen zwei Leute"),
        ("transporthilfe", "Ein Klavier muss ins Erdgeschoss"),
        ("transporthilfe", "Alte Möbel zur Entsorgung bringen"),
        ("transporthilfe", "Ich brauche jemanden mit einem Bus für einen Tisch"),
        ("transporthilfe", "Mehrere Kartons in den Estrich tragen"),
        ("transporthilfe", "Hilfe beim Zügeln in Köniz gesucht"),
        ("transporthilfe", "Il faut déménager un lit et une table"),
        ("transporthilfe", "Je cherche une camionnette pour transporter un canapé"),
        ("transporthilfe", "Aide pour porter des meubles au grenier"),
        ("it-soforthilfe", "Mein Tablet zeigt nur noch einen schwarzen Bildschirm"),
        ("it-soforthilfe", "Ich komme nicht mehr in mein E-Mail-Konto"),
        ("it-soforthilfe", "Das WLAN-Passwort ist vergessen"),
        ("it-soforthilfe", "Der Fernseher findet keine Sender mehr"),
        ("it-soforthilfe", "Mein Mobiltelefon lädt nicht mehr"),
        ("it-soforthilfe", "Ich brauche Hilfe beim Einrichten des neuen Computers"),
        ("it-soforthilfe", "Der Drucker druckt nur noch leere Seiten"),
        ("it-soforthilfe", "Mon ordinateur est très lent et plante"),
        ("it-soforthilfe", "Je n'arrive plus à me connecter à internet"),
        ("it-soforthilfe", "Mon téléphone ne s'allume plus"),
        ("reinigung", "Nach dem Hochwasser ist der Keller voller Schlamm"),
        ("reinigung", "Die Waschküche steht unter Wasser und muss gereinigt werden"),
        ("reinigung", "Nach einem kleinen Brand ist alles voller Russ"),
        ("reinigung", "Schimmel im Badezimmer entfernen"),
        ("reinigung", "Teppich nach einem Wasserschaden reinigen"),
        ("reinigung", "Die Wohnung muss nach einem Rohrbruch geputzt werden"),
        ("reinigung", "Nach dem Sturm ist der Sitzplatz voller Dreck"),
        ("reinigung", "La cave est pleine de boue après l'inondation"),
        ("reinigung", "Il faut nettoyer après un dégât d'eau"),
        ("reinigung", "Enlever la moisissure dans la chambre"),
        ("lieferdienst", "Ich bin krank im Bett und brauche Lebensmittel"),
        ("lieferdienst", "Wer kann mir meine Tabletten in der Apotheke abholen?"),
        ("lieferdienst", "Bitte Milch, Brot und Früchte vorbeibringen"),
        ("lieferdienst", "Ich kann wegen meines Beins nicht einkaufen gehen"),
        ("lieferdienst", "Mein Rezept liegt in der Apotheke bereit"),
        ("lieferdienst", "Einkäufe für das Wochenende nach Hause bringen"),
        ("lieferdienst", "Jemand sollte mir Wasser und Essen liefern"),
        ("lieferdienst", "Pouvez-vous m'apporter des courses?"),
        ("lieferdienst", "Je dois récupérer mes médicaments à la pharmacie"),
        ("lieferdienst", "Livraison de nourriture à la maison s'il vous plaît"),
        ("nachbarschaftshilfe", "Kann jemand meine Pflanzen giessen, während ich weg bin?"),
        ("nachbarschaftshilfe", "Ich suche jemanden, der mit meinem Hund spazieren geht"),
        ("nachbarschaftshilfe", "Wer hilft mir kurz, den Briefkasten zu leeren?"),
        ("nachbarschaftshilfe", "Jemand aus dem Quartier für eine kleine Hilfe gesucht"),
        ("nachbarschaftshilfe", "Bitte kurz nach meiner Katze schauen"),
        ("nachbarschaftshilfe", "Hilfe beim Ausfüllen eines Formulars gesucht"),
        ("nachbarschaftshilfe", "Ich brauche jemanden, der mir Gesellschaft leistet"),
        ("nachbarschaftshilfe", "Quelqu'un peut arroser mes plantes?"),
        ("nachbarschaftshilfe", "Je cherche un voisin pour promener mon chien"),
        ("nachbarschaftshilfe", "Un petit coup de main dans le quartier"),
        ("begleitung", "Ich muss morgen ins Spital und möchte nicht allein gehen"),
        ("begleitung", "Wer begleitet mich zur Gemeindeverwaltung?"),
        ("begleitung", "Ich brauche jemanden für den Weg zum Zahnarzt"),
        ("begleitung", "Begleitung zur Bank, ich bin unsicher"),
        ("begleitung", "Ich finde den Weg zur neuen Physiotherapie nicht"),
        ("begleitung", "Hilfe beim Gang zum Sozialamt"),
        ("begleitung", "Meine Mutter braucht eine Begleitperson zum Augenarzt"),
        ("begleitung", "J'ai besoin de quelqu'un pour m'accompagner chez le dentiste"),
        ("begleitung", "Je ne veux pas aller seul à l'hôpital"),
        ("begleitung", "Accompagner ma mère à la commune"),
    ];

    // Reihenfolge der Verständniskette ohne Datenbank: zuerst Regeln, dann maschinelles Lernen.
    private static string? Kette(string freitext) =>
        Schluesselwortregeln.Anwenden(freitext)?.Kennung ?? Klassifikator.Klassifizieren(freitext).Kennung;

    [Fact]
    public void Die_Kette_ordnet_den_Kalibrierungssatz_fast_vollstaendig_richtig_ein()
    {
        var ergebnisse = Kalibrierungssatz.Select(s => (s.Kennung, Erkannt: Kette(s.Freitext))).ToList();

        Assert.True(ergebnisse.Count(e => e.Erkannt == e.Kennung) >= 95);
        Assert.True(ergebnisse.Count(e => e.Erkannt is not null && e.Erkannt != e.Kennung) <= 2);
    }

    [Fact]
    public void Das_maschinelle_Lernen_allein_ordnet_die_meisten_Saetze_richtig_ein()
    {
        var ergebnisse = Kalibrierungssatz
            .Select(s => (s.Kennung, Erkannt: Klassifikator.Klassifizieren(s.Freitext).Kennung))
            .ToList();

        Assert.True(ergebnisse.Count(e => e.Erkannt == e.Kennung) >= 75);
        Assert.True(ergebnisse.Count(e => e.Erkannt is not null && e.Erkannt != e.Kennung) <= 7);
    }

    [Fact]
    public void Kein_Trainingsbeispiel_wird_einer_falschen_Kategorie_zugeordnet()
    {
        var falsch = Trainingsdaten.BeispieleProKategorie
            .SelectMany(k => k.Value.Select(b => (k.Key, Erkannt: Klassifikator.Klassifizieren(b).Kennung)))
            .Where(e => e.Erkannt is not null && e.Erkannt != e.Key)
            .ToList();

        Assert.Empty(falsch);
    }

    [Theory]
    [InlineData("Die Kette meines Velos ist abgesprungen", "kleine-reparatur")]
    [InlineData("Bitte Milch, Brot und Früchte vorbeibringen", "lieferdienst")]
    [InlineData("Wir ziehen am Samstag um und brauchen zwei Leute", "transporthilfe")]
    public void Ohne_Schluesselwort_entscheidet_das_maschinelle_Lernen(string freitext, string kennung)
    {
        Assert.Null(Schluesselwortregeln.Anwenden(freitext));
        var ergebnis = Klassifikator.Klassifizieren(freitext);
        Assert.Equal(kennung, ergebnis.Kennung);
        Assert.True(ergebnis.Aehnlichkeit >= TfIdfKlassifikator.Mindestaehnlichkeit);
        Assert.True(ergebnis.Abstand >= TfIdfKlassifikator.Mindestabstand);
    }

    [Fact]
    public void Bei_knappem_Abstand_bleibt_das_Ergebnis_offen_mit_Rangfolge()
    {
        var ergebnis = Klassifikator.Klassifizieren("Aide pour porter les courses au quatrième étage");

        Assert.Null(ergebnis.Kennung);
        Assert.True(ergebnis.Abstand < TfIdfKlassifikator.Mindestabstand);
        Assert.True(ergebnis.Rangfolge.Count >= 2);
        Assert.Equal(ergebnis.Rangfolge.OrderByDescending(r => r.Aehnlichkeit).ToList(), ergebnis.Rangfolge.ToList());
    }

    [Theory]
    [InlineData("Grüezi mitenand")]
    [InlineData("")]
    [InlineData(null)]
    public void Ohne_bekannte_Woerter_gibt_es_weder_Kategorie_noch_Rangfolge(string? freitext)
    {
        var ergebnis = Klassifikator.Klassifizieren(freitext);
        Assert.Null(ergebnis.Kennung);
        Assert.Empty(ergebnis.Rangfolge);
    }

    [Theory]
    [InlineData("heizungen", "heiz")]
    [InlineData("kisten", "kist")]
    [InlineData("wohnung", "wohn")]
    [InlineData("auto", "auto")]
    public void Stammform_kuerzt_haeufige_Endungen(string wort, string stamm) =>
        Assert.Equal(stamm, TfIdfKlassifikator.Stammform(wort));
}
