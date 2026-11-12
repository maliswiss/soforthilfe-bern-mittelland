using Soforthilfe.Dienste.Nachweise;

namespace Soforthilfe.Tests;

/// <summary>
/// Telefonnummer und Referenz als Nachweis: Schreibweisen, Vereinheitlichung und Grenzen.
/// </summary>
public sealed class NachweiswerteTests
{
    [Theory]
    [InlineData("031 123 45 67", "031 123 45 67")]
    [InlineData("0311234567", "031 123 45 67")]
    [InlineData("+41 31 123 45 67", "031 123 45 67")]
    [InlineData("0041 79 123 45 67", "079 123 45 67")]
    [InlineData("079/123.45.67", "079 123 45 67")]
    public void Telefonnummer_wird_lesbar_vereinheitlicht(string eingabe, string erwartet) =>
        Assert.Equal(erwartet, Nachweiswerte.Telefon(eingabe));

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("031 123 45")]
    [InlineData("+49 30 1234567")]
    [InlineData("031 123 45 6x")]
    public void Ungueltige_Telefonnummer_ergibt_null(string? eingabe) =>
        Assert.Null(Nachweiswerte.Telefon(eingabe));

    [Fact]
    public void Referenz_wird_ohne_Rand_und_mit_einfachen_Leerzeichen_gespeichert() =>
        Assert.Equal("Gemeinde Muri, Frau Muster", Nachweiswerte.Referenz("  Gemeinde   Muri,\n Frau Muster "));

    [Theory]
    [InlineData(null)]
    [InlineData("    ")]
    [InlineData("SRK")]
    public void Zu_kurze_Referenz_ergibt_null(string? eingabe) =>
        Assert.Null(Nachweiswerte.Referenz(eingabe));

    [Fact]
    public void Referenz_hat_eine_Hoechstlaenge()
    {
        Assert.NotNull(Nachweiswerte.Referenz(new string('a', Nachweiswerte.ReferenzHoechstlaenge)));
        Assert.Null(Nachweiswerte.Referenz(new string('a', Nachweiswerte.ReferenzHoechstlaenge + 1)));
    }
}
