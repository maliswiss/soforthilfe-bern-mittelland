# Messprotokolle

Protokolle der Messung des Matchings, erzeugt mit [`messen.mjs`](../../tests/messprotokoll/messen.mjs). Ziel, Testfälle, Kriterien und die Auswertung der Ergebnisse beschreibt [messung-matching.md](../messung-matching.md).

| Messung | Datum | Protokoll | Tabellen | Ergebnis |
|---|---|---|---|---|
| 1 | 24.11.2026, 19:56 | [Protokoll](r2-matching-2026-11-24-19-56.md) | [Fälle](r2-matching-2026-11-24-19-56.csv) | 20 von 20 geeignet, 4 von 4 Notfälle gestoppt |
| 2, vor den Korrekturen | 25.11.2026, 08:00 | [Protokoll](r2-matching-2026-11-25-08-00.md) | [Fälle](r2-matching-2026-11-25-08-00.csv), [Zusatzfälle](r2-matching-2026-11-25-08-00-zusatz.csv) | 20 von 20 geeignet, 4 von 4 Notfälle gestoppt; Zusatzfälle mit 4 Fehlern |
| 3, nach den Korrekturen | 25.11.2026, 08:27 | [Protokoll](r2-matching-2026-11-25-08-27.md) | [Fälle](r2-matching-2026-11-25-08-27.csv), [Zusatzfälle](r2-matching-2026-11-25-08-27-zusatz.csv) | 20 von 20 geeignet, 4 von 4 Notfälle gestoppt; Zusatzfälle ohne Fehler |

Für die Abnahme von Release 2 gilt Messung 3.

## Aufbau

| Datei | Inhalt |
|---|---|
| `r2-matching-<Datum>-<Zeit>.md` | Ergebnis, Antwortzeit je Schritt, Messumgebung, Kriterien K1 bis K6, Testfälle mit Begründungen und Zeiten, Notfälle; ab Messung 2 die Zusatzmessung |
| `r2-matching-<Datum>-<Zeit>.csv` | ein Fall pro Zeile mit Weg, Sicherheit, Kriterien, Rang 1, Distanz und Zeiten je Schritt |
| `r2-matching-<Datum>-<Zeit>-zusatz.csv` | ein Zusatzfall pro Zeile mit erwarteter und erkannter Kategorie, Weg, Bewertung und Zeit |

Die Tabellen sind UTF-8 mit Semikolon als Trennzeichen und lassen sich direkt in einer Tabellenkalkulation öffnen.

## Daten

Die Protokolle enthalten nur fiktive Daten: die Helfenden der Messdaten aus [`testfaelle.json`](../../tests/messprotokoll/testfaelle.json) mit Adressen unter `beispiel.invalid`, die Testdaten der Plattform und die vor der Messung festgelegten Texte. Sie enthalten keine Zugangsdaten. Bei Notfällen steht nur der erkannte Begriff im Protokoll, nie Konto, Adresse oder Name.
