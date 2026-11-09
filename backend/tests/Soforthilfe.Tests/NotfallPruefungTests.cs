using Soforthilfe.Dienste.Notfall;
using Soforthilfe.Dienste.Verstaendnis;

namespace Soforthilfe.Tests;

/// <summary>
/// Testfälle der Notfallregeln: akute gesundheitliche Hinweise werden erkannt, alltägliche Anliegen
/// sowie Brand (118) und Polizei (117) lösen keinen Notfall-Stopp aus.
/// </summary>
public sealed class NotfallPruefungTests
{
    private readonly NotfallPruefung _pruefung = new();

    [Theory]
    // Deutsch
    [InlineData("Mein Mann ist bewusstlos auf dem Boden.", "bewusstlos")]
    [InlineData("Ich glaube, mein Vater hat einen Herzinfarkt!", "Herzinfarkt")]
    [InlineData("Meine Nachbarin bekommt keine Luft mehr.", "bekommt keine Luft")]
    [InlineData("Er hat starke Brustschmerzen seit einer Stunde.", "Brustschmerzen")]
    [InlineData("Verdacht auf Schlaganfall, die Sprache ist undeutlich.", "Schlaganfall")]
    [InlineData("Das Kind hat Putzmittel getrunken, Vergiftung?", "Vergiftung")]
    [InlineData("Meine Mutter ist plötzlich zusammengebrochen.", "zusammengebrochen")]
    [InlineData("Die Wunde blutet stark und hört nicht auf.", "blutet stark")]
    [InlineData("Bitte schnell, die Wehen haben angefangen.", "Wehen")]
    [InlineData("Nach dem Wespenstich: allergischer Schock!", "allergischer Schock")]
    [InlineData("Herzinfarktverdacht bei meinem Nachbarn", "Herzinfarktverdacht")]
    [InlineData("ER ATMET NICHT", "ATMET NICHT")]
    // Französisch
    [InlineData("Mon voisin est inconscient dans la cuisine.", "inconscient")]
    [InlineData("Ma mère ne respire plus normalement.", "ne respire plus")]
    [InlineData("Je pense que c'est un AVC.", "AVC")]
    [InlineData("Il a une douleur thoracique très forte.", "douleur thoracique")]
    // Englisch und Italienisch
    [InlineData("My father is having a heart attack", "heart attack")]
    [InlineData("Mia madre non respira bene", "non respira")]
    public void Erkennt_akute_gesundheitliche_Hinweise(string freitext, string erwartetesWort)
    {
        var ergebnis = _pruefung.Pruefen(freitext);

        Assert.True(ergebnis.IstNotfall);
        Assert.Equal(erwartetesWort, ergebnis.Schluesselwort);
    }

    [Theory]
    [InlineData("Mein Schlüssel ist im Schloss abgebrochen, ich komme nicht in die Wohnung.")]
    [InlineData("Notfall: Wasserrohrbruch im Keller, alles steht unter Wasser.")]
    [InlineData("Der Wasserhahn in der Küche tropft stark.")]
    [InlineData("Ich brauche Begleitung zum Arzttermin am Dienstag.")]
    [InlineData("Mein Auto springt nicht an, Panne auf dem Parkplatz.")]
    [InlineData("Bitte Medikamente in der Apotheke abholen.")]
    [InlineData("Mon téléphone ne fonctionne plus, besoin d'aide.")]
    [InlineData("Die Fahnen verwehen im Wind, bitte Mast reparieren.")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Alltaegliche_Anliegen_sind_kein_Notfall(string? freitext)
    {
        var ergebnis = _pruefung.Pruefen(freitext);

        Assert.False(ergebnis.IstNotfall);
        Assert.Null(ergebnis.Schluesselwort);
    }

    [Theory]
    [InlineData("In der Küche brennt es, Rauch überall.")]
    [InlineData("Bei uns wurde eingebrochen, die Tür ist kaputt.")]
    [InlineData("Ein Feuer im Treppenhaus.")]
    [InlineData("Jemand bedroht mich vor dem Haus.")]
    public void Brand_und_Polizei_gehoeren_nicht_zur_Notfallpruefung(string freitext)
    {
        Assert.False(_pruefung.Pruefen(freitext).IstNotfall);
    }

    [Theory]
    [InlineData("Herzinfarktverdacht bei meinem Nachbarn", "herzinfarkt")]
    [InlineData("Herr Muster ist BEWUSSTLOS.", "bewusstlos")]
    [InlineData("Ma mère ne respire plus normalement.", "ne respire plus")]
    public void Fuer_das_Protokoll_gilt_der_Begriff_aus_der_Liste_ohne_Wortlaut_aus_dem_Text(string freitext, string begriff)
    {
        var ergebnis = _pruefung.Pruefen(freitext);

        Assert.Equal(begriff, ergebnis.Begriff);
        Assert.Contains(begriff, NotfallPruefung.Schluesselwoerter);
        Assert.Null(_pruefung.Pruefen("Der Wasserhahn tropft.").Begriff);
    }

    [Fact]
    public void Alle_Begriffe_sind_normalisiert_und_eindeutig()
    {
        foreach (var begriff in NotfallPruefung.Schluesselwoerter)
        {
            Assert.Equal(Textnormalisierung.Normalisieren(begriff), begriff);
        }

        Assert.Equal(NotfallPruefung.Schluesselwoerter.Count, NotfallPruefung.Schluesselwoerter.Distinct().Count());
    }
}
