# Commit-Konvention

## Format

```
<Release>: <Kurzbeschreibung> (Refs #<Issue>)
```

Beispiel:

```
R1: Anmeldung mit JWT umgesetzt (Refs #10)
```

## Regeln

- **Präfix:** `R1`, `R2` oder `R3`, passend zum Release der Aufgabe.
- **Sprache:** Deutsch, mit korrekten Umlauten.
- **Beschreibung:** kurz, im Perfekt oder als Ergebnis formuliert, höchstens etwa 70 Zeichen.
- **Bezug:** Jeder Commit nennt sein Issue mit `Refs #n`. Die Nummer steht im Commit-Hinweis des Issues.
- **Umfang:** Eine Aufgabe, ein Issue, ein Commit.
- **Inhalt:** Nur Quelltext, Tests, Konfiguration und Dokumentation. Keine Zugangsdaten, keine Build-Ausgaben, keine Datenbank-Abzüge.

## Branch und Tags

- Gearbeitet wird auf `main`.
- Jede Abnahme erhält einen Tag: `r1-abnahme`, `r2-abnahme`, `r3-abnahme`.

## Releases

| Release | Zeitraum | Abnahme |
|---|---|---|
| R1 | 19.10. bis 06.11.2026 | 06.11.2026 |
| R2 | 09.11. bis 27.11.2026 | 27.11.2026 |
| R3 | 30.11. bis 18.12.2026 | 18.12.2026 |

Eine Abnahme gilt als bestanden, wenn alle Aufgaben mit Priorität «Muss» erledigt sind, die CI grün ist, das Testprotokoll vorliegt und der Tag gesetzt ist.

## Issues

Der Titel beginnt mit der Aufgabennummer, zum Beispiel `[1.5] Titel`. Der Text enthält die Abschnitte «Aufgabe» und «Commit-Hinweis». Einordnung, Priorität und Zieltermin stehen im Kanban-Board.
