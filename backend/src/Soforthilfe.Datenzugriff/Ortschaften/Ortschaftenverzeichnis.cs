using System.Globalization;
using Microsoft.Extensions.Configuration;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using Soforthilfe.Datenzugriff.Modell;

namespace Soforthilfe.Datenzugriff.Ortschaften;

/// <summary>
/// Liest das amtliche Ortschaftenverzeichnis der Schweiz (swisstopo, CSV mit WGS84-Koordinaten, Trennzeichen «;»).
/// </summary>
public static class Ortschaftenverzeichnis
{
    // Dateiname der Ausgabe; bei mehreren Ausgaben gilt die neueste (das Datum steht im Namen).
    public const string Dateimuster = "amtliches_ortschaftenverzeichnis_wgs84*.csv";

    public static readonly string[] Pflichtspalten =
    [
        "Ortschaftsname", "PLZ4", "Zusatzziffer", "Gemeindename", "BFS-Nr",
        "Kantonskürzel", "Adressenanteil", "E", "N", "Sprache",
    ];

    private static readonly GeometryFactory Geometrie =
        NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);

    // Punkt aus Breiten- und Längengrad; PostGIS erwartet die Reihenfolge Länge, Breite.
    public static Point Punkt(double breitengrad, double laengengrad) =>
        Geometrie.CreatePoint(new Coordinate(laengengrad, breitengrad));

    /// <summary>
    /// Sucht die Datei: zuerst <c>Ortschaften:Datei</c> aus der Konfiguration, sonst die neueste Ausgabe im Ordner «Daten».
    /// </summary>
    public static string? DateiFinden(IConfiguration konfiguration)
    {
        var angegeben = konfiguration["Ortschaften:Datei"];
        if (!string.IsNullOrWhiteSpace(angegeben))
        {
            var pfad = Path.IsPathRooted(angegeben) ? angegeben : Path.Combine(AppContext.BaseDirectory, angegeben);
            return File.Exists(pfad) ? pfad : null;
        }

        var ordner = Path.Combine(AppContext.BaseDirectory, "Daten");
        if (!Directory.Exists(ordner))
        {
            return null;
        }

        return Directory.GetFiles(ordner, Dateimuster)
            .OrderByDescending(datei => Path.GetFileName(datei), StringComparer.Ordinal)
            .FirstOrDefault();
    }

    public static List<Ortschaft> Lesen(string datei)
    {
        using var leser = new StreamReader(datei, detectEncodingFromByteOrderMarks: true);
        return Lesen(leser);
    }

    /// <summary>
    /// Liest alle Zeilen. Fehlt eine Pflichtspalte oder ist ein Wert unlesbar, bricht das Einlesen mit
    /// <see cref="InvalidDataException"/> ab, damit kein halbes Verzeichnis entsteht.
    /// </summary>
    public static List<Ortschaft> Lesen(TextReader leser)
    {
        var kopf = leser.ReadLine() ?? throw new InvalidDataException("Das Ortschaftenverzeichnis ist leer.");
        var spalten = kopf.TrimStart('﻿').Split(';')
            .Select((name, index) => (Name: name.Trim(), Index: index))
            .ToDictionary(spalte => spalte.Name, spalte => spalte.Index);

        var fehlend = Pflichtspalten.Where(spalte => !spalten.ContainsKey(spalte)).ToList();
        if (fehlend.Count > 0)
        {
            throw new InvalidDataException($"Im Ortschaftenverzeichnis fehlen die Spalten: {string.Join(", ", fehlend)}.");
        }

        var ortschaften = new List<Ortschaft>();
        var nummer = 1;
        string? zeile;

        while ((zeile = leser.ReadLine()) is not null)
        {
            nummer++;
            if (string.IsNullOrWhiteSpace(zeile))
            {
                continue;
            }

            var werte = zeile.Split(';');
            if (werte.Length < spalten.Count)
            {
                throw new InvalidDataException($"Zeile {nummer} des Ortschaftenverzeichnisses ist unvollständig.");
            }

            string Wert(string spalte) => werte[spalten[spalte]].Trim();

            try
            {
                ortschaften.Add(new Ortschaft
                {
                    Postleitzahl = Wert("PLZ4"),
                    Zusatzziffer = Wert("Zusatzziffer"),
                    Name = Wert("Ortschaftsname"),
                    Gemeinde = Wert("Gemeindename"),
                    BfsNummer = int.Parse(Wert("BFS-Nr"), CultureInfo.InvariantCulture),
                    Kanton = Wert("Kantonskürzel"),
                    Adressenanteil = Zahl(Wert("Adressenanteil").Replace("%", string.Empty)),
                    Sprache = Wert("Sprache"),
                    Standort = Punkt(Zahl(Wert("N")), Zahl(Wert("E"))),
                });
            }
            catch (FormatException fehler)
            {
                throw new InvalidDataException($"Zeile {nummer} des Ortschaftenverzeichnisses enthält einen ungültigen Wert.", fehler);
            }
        }

        return ortschaften;
    }

    private static double Zahl(string wert) => double.Parse(wert.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture);
}
