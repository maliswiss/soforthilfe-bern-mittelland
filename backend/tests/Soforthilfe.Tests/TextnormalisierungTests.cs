using Soforthilfe.Dienste.Verstaendnis;

namespace Soforthilfe.Tests;

/// <summary>
/// Testfälle der Textaufbereitung, auf der Notfallprüfung und Klassifikation aufbauen.
/// </summary>
public sealed class TextnormalisierungTests
{
    [Theory]
    [InlineData("Schlüssel abgebrochen!", "schluessel abgebrochen")]
    [InlineData("  Grösse  und   Mass ", "groesse und mass")]
    [InlineData("Ma clé est cassée, ça bloque.", "ma cle est cassee ca bloque")]
    [InlineData("Straße", "strasse")]
    [InlineData("l'ambulance", "l ambulance")]
    [InlineData("", "")]
    public void Normalisiert_Schreibweise_Umlaute_und_Satzzeichen(string eingabe, string erwartet)
    {
        Assert.Equal(erwartet, Textnormalisierung.Normalisieren(eingabe));
    }

    [Theory]
    [InlineData("Er ist BEWUSSTLOS.", "bewusstlos", "BEWUSSTLOS")]
    [InlineData("Ma clé est cassée", "cle est", "clé est")]
    [InlineData("Nichts davon", "fehlt", "fehlt")]
    public void Fundstelle_nennt_das_Wort_wie_geschrieben(string original, string begriff, string erwartet)
    {
        Assert.Equal(erwartet, Textnormalisierung.Fundstelle(original, begriff));
    }
}
