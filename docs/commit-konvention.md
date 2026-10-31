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

## Prüfung vor dem Commit

Vor jedem Commit laufen zwei Prüfungen über die gestagten Dateien:

1. **Hygieneprüfung** (`scripts/hygiene/pruefe-dateien.mjs`): verhindert Build-Ausgaben (`bin/`, `obj/`, `node_modules/`, `dist/`, Testberichte), lokale Umgebungsdateien (`.env`), lokale Konfigurationen mit Zugangsdaten, Schlüssel und Zertifikate, Datenbank-Abzüge, vollständige Sicherheitsberichte sowie Office- und PDF-Dateien. In Konfigurationsdateien sucht sie zusätzlich nach eingetragenen Kennwörtern, Tokens und Schlüsseln. Platzhalter und Verweise auf Umgebungsvariablen sind erlaubt.
2. **Geheimnisprüfung** mit Gitleaks und der Konfiguration `.gitleaks.toml`.

Einrichtung einmal pro Klon:

```
git config core.hooksPath .githooks
```

Gitleaks und Node.js müssen installiert sein. Schlägt eine Prüfung fehl, entsteht kein Commit; die betroffene Datei wird mit `git restore --staged <datei>` aus dem Index genommen.

Die Tests der Hygieneprüfung laufen mit:

```
node --test "tests/hygiene/*.test.mjs"
```

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
