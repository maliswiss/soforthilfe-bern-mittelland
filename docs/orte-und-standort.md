# Orte, Postleitzahlen und Standort

Orte und Postleitzahlen stammen aus dem amtlichen Ortschaftenverzeichnis der Schweiz von swisstopo. Die Plattform liest es selbst ein und fragt keinen fremden Dienst ab.

## Datenquelle

| Angabe | Wert |
|---|---|
| Quelle | Amtliches Ortschaftenverzeichnis mit Postleitzahl, swisstopo |
| Ausgabe | 01.09.2026, CSV mit Koordinaten in WGS84, Trennzeichen `;` |
| Datei | [`backend/src/Soforthilfe.Datenzugriff/Daten/`](../backend/src/Soforthilfe.Datenzugriff/Daten) |
| Umfang | 5718 Zeilen: Ortschaft (Postleitzahl und Zusatzziffer) je berührte Gemeinde |
| Nutzung | freie Nutzung, Quellenangabe Pflicht; die Oberfläche nennt «© swisstopo» in der Fusszeile |
| Bezug | [swisstopo](https://www.swisstopo.admin.ch/de/amtliches-ortschaftenverzeichnis), [opendata.swiss](https://opendata.swiss/de/dataset/amtliches-ortschaftenverzeichnis-mit-postleitzahl-und-perimeter) |

## Einlesen

1. Beim Start prüft die Schnittstelle, ob die Tabelle `Ortschaften` leer ist.
2. Eine Sperre in der Datenbank sorgt dafür, dass bei mehreren Instanzen nur eine einliest.
3. Gelesen wird die neueste Datei `amtliches_ortschaftenverzeichnis_wgs84*.csv` im Ordner `Daten` neben der Anwendung; `Ortschaften__Datei` kann eine andere Datei angeben.
4. Fehlt eine Pflichtspalte oder ist ein Wert unlesbar, wird nichts gespeichert. Die Schnittstelle startet trotzdem und meldet den Fehler im Anwendungsprotokoll.
5. Nach dem Einlesen steht im Protokoll der Eintrag `Ortschaften.Eingelesen` mit Anzahl und Dateiname.

| Spalte der Datei | Feld in `Ortschaften` |
|---|---|
| `PLZ4`, `Zusatzziffer` | `Postleitzahl`, `Zusatzziffer` |
| `Ortschaftsname` | `Name` |
| `Gemeindename`, `BFS-Nr` | `Gemeinde`, `BfsNummer` |
| `Kantonskürzel` | `Kanton` |
| `Adressenanteil` | `Adressenanteil` in Prozent |
| `E`, `N` | `Standort` als Punkt (Länge, Breite) mit räumlichem Index |
| `Sprache` | `Sprache` |

## Neue Ausgabe

swisstopo veröffentlicht das Verzeichnis regelmässig neu. Für eine neue Ausgabe die Datei im Ordner `Daten` ersetzen, die Tabelle leeren (`TRUNCATE "Ortschaften"`) und die Schnittstelle neu starten; das Verzeichnis wird dann neu eingelesen.

## Tests

| Test | Prüft |
|---|---|
| Einheitentests `OrtschaftenverzeichnisTests` | Format, Koordinaten in der richtigen Reihenfolge, mehrere Gemeinden je Ortschaft, Abbruch bei fehlender Spalte oder ungültigem Wert, Vollständigkeit der mitgelieferten Ausgabe |
| CI, Schritt «Stammdaten und Testdaten prüfen» | 5718 Zeilen in `Ortschaften` und der Protokolleintrag nach dem Start |
| Oberflächentest | Quellenangabe «© swisstopo» in der Fusszeile |
