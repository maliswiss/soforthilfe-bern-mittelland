using Soforthilfe.Datenzugriff.Modell;
using Soforthilfe.Datenzugriff.Ortschaften;

namespace Soforthilfe.Tests;

/// <summary>
/// Einlesen des amtlichen Ortschaftenverzeichnisses (swisstopo): Format, Fehlerfälle und die mitgelieferte Ausgabe.
/// </summary>
public sealed class OrtschaftenverzeichnisTests
{
    private const string Kopf = "﻿Ortschaftsname;PLZ4;Zusatzziffer;ZIP_ID;Gemeindename;BFS-Nr;Kantonskürzel;Adressenanteil;E;N;Sprache;Validity";

    private static List<Ortschaft> Lesen(params string[] zeilen) =>
        Ortschaftenverzeichnis.Lesen(new StringReader(string.Join("\r\n", new[] { Kopf }.Concat(zeilen))));

    [Fact]
    public void Zeile_wird_mit_allen_Angaben_gelesen()
    {
        var ortschaft = Assert.Single(Lesen("Bern;3011;00;4363;Bern;351;BE;100 %;7.4446;46.9480;de;2008-07-01"));

        Assert.Equal("3011", ortschaft.Postleitzahl);
        Assert.Equal("00", ortschaft.Zusatzziffer);
        Assert.Equal("Bern", ortschaft.Name);
        Assert.Equal("Bern", ortschaft.Gemeinde);
        Assert.Equal(351, ortschaft.BfsNummer);
        Assert.Equal("BE", ortschaft.Kanton);
        Assert.Equal(100, ortschaft.Adressenanteil);
        Assert.Equal("de", ortschaft.Sprache);

        // PostGIS erwartet Länge (E) als X und Breite (N) als Y, im Bezugssystem WGS84.
        Assert.Equal(7.4446, ortschaft.Standort.X, 4);
        Assert.Equal(46.9480, ortschaft.Standort.Y, 4);
        Assert.Equal(4326, ortschaft.Standort.SRID);
    }

    [Fact]
    public void Eine_Ortschaft_ueber_zwei_Gemeinden_ergibt_zwei_Zeilen_mit_Anteil()
    {
        var zeilen = Lesen(
            "Thörishaus;3174;00;4500;Köniz;355;BE;58.602 %;7.35;46.89;de;2008-07-01",
            "Thörishaus;3174;00;4500;Neuenegg;670;BE;41.398 %;7.34;46.89;de;2008-07-01");

        Assert.Equal(2, zeilen.Count);
        Assert.Equal(58.602, zeilen[0].Adressenanteil, 3);
        Assert.Equal(670, zeilen[1].BfsNummer);
    }

    [Fact]
    public void Leere_Zeilen_werden_uebersprungen()
    {
        Assert.Single(Lesen("", "Bern;3011;00;4363;Bern;351;BE;100 %;7.44;46.94;de;2008-07-01", "   "));
    }

    [Fact]
    public void Fehlende_Spalte_bricht_das_Einlesen_ab()
    {
        var fehler = Assert.Throws<InvalidDataException>(() =>
            Ortschaftenverzeichnis.Lesen(new StringReader("Ortschaftsname;PLZ4\nBern;3011")));
        Assert.Contains("Zusatzziffer", fehler.Message);
    }

    [Theory]
    [InlineData("Bern;3011;00;4363;Bern;keine-zahl;BE;100 %;7.44;46.94;de;2008-07-01")]
    [InlineData("Bern;3011;00;4363;Bern;351;BE;100 %;sieben;46.94;de;2008-07-01")]
    [InlineData("Bern;3011;00")]
    public void Ungueltige_Zeile_bricht_das_Einlesen_ab(string zeile)
    {
        var fehler = Assert.Throws<InvalidDataException>(() => Lesen(zeile));
        Assert.Contains("Zeile 2", fehler.Message);
    }

    [Fact]
    public void Leere_Datei_wird_abgewiesen()
    {
        Assert.Throws<InvalidDataException>(() => Ortschaftenverzeichnis.Lesen(new StringReader(string.Empty)));
    }

    [Fact]
    public void Mitgelieferte_Ausgabe_ist_vollstaendig_und_eindeutig()
    {
        var datei = Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, "Daten"), Ortschaftenverzeichnis.Dateimuster).Single();
        var ortschaften = Ortschaftenverzeichnis.Lesen(datei);

        Assert.True(ortschaften.Count > 5000, $"nur {ortschaften.Count} Zeilen");
        Assert.Equal(ortschaften.Count, ortschaften.Select(o => (o.Postleitzahl, o.Zusatzziffer, o.BfsNummer)).Distinct().Count());
        Assert.All(ortschaften, o => Assert.Matches("^[1-9][0-9]{3}$", o.Postleitzahl));

        // Stichproben aus beiden Pilotregionen.
        Assert.Contains(ortschaften, o => o is { Postleitzahl: "3011", Name: "Bern", Gemeinde: "Bern", Kanton: "BE" });
        Assert.Contains(ortschaften, o => o is { Postleitzahl: "2502", Name: "Biel/Bienne", BfsNummer: 371 });

        // Alle Punkte liegen in der Schweiz und ihrer unmittelbaren Umgebung.
        Assert.All(ortschaften, o =>
        {
            Assert.InRange(o.Standort.X, 5.9, 10.5);
            Assert.InRange(o.Standort.Y, 45.8, 47.9);
        });
    }
}
