using Soforthilfe.Dienste.Nachweise;

namespace Soforthilfe.Tests;

/// <summary>
/// Unternehmens-Identifikationsnummer: Formate, Prüfziffer nach Modulo 11 und Normalisierung.
/// </summary>
public sealed class UidTests
{
    [Theory]
    [InlineData("CHE-123.456.788", "CHE-123.456.788")]   // Beispiel aus dem UID-Standard
    [InlineData("CHE123456788", "CHE-123.456.788")]
    [InlineData("che 123 456 788", "CHE-123.456.788")]
    [InlineData(" CHE-123.456.788 HR ", "CHE-123.456.788")]
    [InlineData("CHE-123.456.788 MWST", "CHE-123.456.788")]
    [InlineData("CHE-100.000.070", "CHE-100.000.070")]   // Prüfziffer 0 aus Rest 0
    [InlineData("CHE-100.000.006", "CHE-100.000.006")]
    public void Gueltige_UID_wird_normalisiert(string eingabe, string erwartet)
    {
        Assert.Equal(erwartet, Uid.Normalisieren(eingabe));
        Assert.True(Uid.IstGueltig(eingabe));
    }

    [Theory]
    [InlineData("CHE-123.456.789")]   // falsche Prüfziffer
    [InlineData("CHE-100.000.160")]   // Rechnung ergibt 10: diese Nummer gibt es nicht
    [InlineData("CHE-100.000.161")]
    [InlineData("CHE-12.345.678")]
    [InlineData("CHE-1234.567.890")]
    [InlineData("CH-123.456.788")]
    [InlineData("ADM-123.456.788")]
    [InlineData("123.456.788")]
    [InlineData("CHE-123.456.788 XYZ")]
    [InlineData("")]
    [InlineData(null)]
    public void Ungueltige_UID_wird_abgewiesen(string? eingabe)
    {
        Assert.Null(Uid.Normalisieren(eingabe));
        Assert.False(Uid.IstGueltig(eingabe));
    }
}
