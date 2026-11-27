# Testprotokoll Release 2

Release 2 (09.11. bis 27.11.2026): Notfall-Stopp mit Hinweis «144 anrufen», Orte über das Ortschaftenverzeichnis von swisstopo und Standortfreigabe, Helferprofile mit Verifizierung und Preisangaben, Verständniskette mit Regeln, TF-IDF-Klassifikator und Sprachmodell, Mehrsprachigkeit Deutsch und Französisch sowie Smart-Matching mit Begründung. Alle automatischen Tests laufen bei jedem Push in der CI ([GitHub Actions](https://github.com/maliswiss/soforthilfe-bern-mittelland/actions/workflows/ci.yml)); jeder Job zeigt am Ende eine Testübersicht. Die Messung des Matchings lief auf dem Arbeitsgerät mit echtem Sprachmodell.

## Testumgebung

| Bereich | CI (GitHub Actions) | Lokal |
|---|---|---|
| Betriebssystem | Ubuntu (`ubuntu-latest`) | Windows mit Docker Desktop |
| Schnittstelle | .NET SDK 10, ASP.NET Core 10 im Release-Build, Umgebung `Production` wie im Container, mit Testdaten | `dotnet run`, Umgebung `Development`, mit Testdaten |
| Datenbank | PostgreSQL 16 mit PostGIS 3.4 als Service-Container, Schema aus den Migrationen | gleiches Image über Docker Compose, Port 5433 |
| Sprachmodell | Attrappe mit festen Antworten ([`tests/attrappe`](../tests/attrappe)) | Ollama mit `llama3.2:3b` nur auf dem Prozessor (Intel Core i7-8650U, 32 GB) |
| Oberfläche | Node.js 22, Vite-Vorschau des Builds mit den Sicherheitsköpfen des Betriebs | `npm run dev` |
| Browser | Chromium über Playwright 1.65.0, Desktop und Mobiltelefon (Pixel 7) | Chrome |
| Containerplattform | Docker Compose mit Profil `plattform`, Traefik, nginx | Docker Desktop |
| Testdaten | fiktive Personen auf `beispiel.invalid` ([testdaten.md](testdaten.md)), Passwort und Schlüssel je Lauf zufällig erzeugt | lokale `.env` |

## Testumfang

| Testart | Ort | Anzahl | Prüft |
|---|---|---|---|
| Geheimnisprüfung | Gitleaks über die gesamte Historie | 1 Lauf | keine Zugangsdaten in irgendeinem Commit |
| Hygieneprüfung | [`tests/hygiene`](../tests/hygiene) | 11 Tests | keine Geheimnisse, Build-Ausgaben, Office-Dateien oder vollständigen Prüfberichte im Repository |
| Testübersicht der CI | [`tests/ci`](../tests/ci) | 10 Tests | Auswertung der Testausgaben von node:test, dotnet mit Farbcodes, Playwright und Rauchtest |
| Testfälle der Messung | [`tests/messprotokoll`](../tests/messprotokoll) | 13 Tests | Testfälle passen zu Stammdaten und Ortschaftenverzeichnis, Auswertung von Messung, Antwortzeit, Notfällen und Zusatzfällen |
| Attrappe des Sprachmodells | [`tests/attrappe`](../tests/attrappe) | 5 Tests | feste Antworten, Fehlerfälle und Zählung der Aufrufe |
| Einheitentests | [`backend/tests/Soforthilfe.Tests`](../backend/tests/Soforthilfe.Tests) | 273 Tests | Notfallprüfung, Textnormalisierung, Regeln, TF-IDF-Klassifikator mit Kalibrierungssatz, Sprachmodell, Dringlichkeit, Ortschaftenverzeichnis, Preisrechner, UID und Nachweise, Profilstatus, Kandidatensuche, Punktzahl, Begründung, Sprachdateien |
| Zustandsprüfung und Stammdaten | CI-Schritte im Job «Backend bauen und testen» | 15 Prüfungen | `/health/live`, `/health/ready`, `/api/info`, Fehlerantwort, Anzahl Zeilen je Tabelle, nur fiktive Adressen |
| Rauchtest der Schnittstelle | [`tests/rauchtest/schnittstelle.sh`](../tests/rauchtest/schnittstelle.sh) | 99 Prüfungen | jeder Endpunkt mit Status und Inhalt |
| Schnittstellentests | [`tests/api`](../tests/api) | 132 Tests in 20 Dateien | Release 1 sowie Notfall, Protokoll, Orte, Verständnis, Einordnungsweg, Dringlichkeit, Sprachmodell, Helferprofil, Verifizierung, Freigabe, Preise, Sprachen, Kandidaten, Vorschläge, Begründung, Vorschau |
| Oberflächentests | [`tests/oberflaeche`](../tests/oberflaeche) | 38 Tests in 11 Dateien | Startseite, Notfall, Anfrage, Standort, Helferprofil, Preise, Sprache, Sprachverwaltung, Vorschau, Vorschläge, Mobiltelefon |
| Containerplattform | CI-Job «Containerplattform starten» | 7 Prüfungen | Start mit einem Befehl, Schnittstelle und Ortschaften über HTTPS, Oberfläche mit Content-Security-Policy und Standort nur für die eigene Seite, Umleitung HTTP auf HTTPS |
| Messung des Matchings | [`tests/messprotokoll/messen.mjs`](../tests/messprotokoll/messen.mjs) | 20 Fälle, 4 Notfälle, 17 Zusatzfälle | Ziel 2 mit echtem Sprachmodell: geeigneter, begründeter Vorschlag, Antwortzeit, Notfall-Stopp ([messung-matching.md](messung-matching.md)) |

## Abdeckung der Anforderungen

| Bereich | Schnittstelle | Oberfläche | Dokumentation |
|---|---|---|---|
| Notfall-Stopp mit Hinweis «144 anrufen» und Protokolleintrag ohne Personendaten | Deutsch, Französisch, Englisch, Italienisch, Mundart; Stopp vor der Einordnung und beim Erfassen (`451`); Protokoll nur mit Begriff | Notfallseite mit `tel:144`, erkanntes Wort nicht in der Adresse | [notfallpruefung.md](notfallpruefung.md) |
| Orte über das Ortschaftenverzeichnis, Region aus der Postleitzahl, Standortfreigabe | 5718 Ortschaften, Region aus Postleitzahl, Standort ohne Speicherung | Ort zur Postleitzahl, Standortfreigabe im Browser | [orte-und-standort.md](orte-und-standort.md) |
| Helferprofile mit Verifizierung und Preisangaben | UID, Nachweise, Freigabe durch die Koordination, Richtpreis mit Zuschlägen und MWST | Profil, Verfügbarkeit, Preisangaben | [helferprofil.md](helferprofil.md), [verifizierung.md](verifizierung.md), [preise.md](preise.md) |
| Verständniskette und Dringlichkeit | alle vier Wege, Begründung, Alternativen, Sprachmodell mit Attrappe | Einordnung mit Begründung | [verstaendniskette.md](verstaendniskette.md) |
| Mehrsprachigkeit Deutsch und Französisch, Sprachimport ohne Deployment | Sprachdateien prüfen, importieren, aktivieren | Sprachwechsel, Sprachverwaltung | [mehrsprachigkeit.md](mehrsprachigkeit.md) |
| Smart-Matching mit Begründung (Ziel 2) | Kandidaten, Punktzahl, Vorschläge, Vorschau ohne Anmeldung mit gestaffelter Preisgabe | Vorschlagsliste und Vorschau | [matching.md](matching.md), [messung-matching.md](messung-matching.md) |

## Ergebnisse

| Testart | Ergebnis |
|---|---|
| Geheimnisprüfung | keine Funde |
| Hygieneprüfung | 11 von 11 bestanden |
| Testübersicht der CI | 10 von 10 bestanden |
| Testfälle der Messung | 13 von 13 bestanden |
| Attrappe des Sprachmodells | 5 von 5 bestanden |
| Einheitentests | 273 von 273 bestanden |
| Zustandsprüfung und Stammdaten | alle Prüfungen bestanden |
| Rauchtest der Schnittstelle | 99 von 99 bestanden |
| Schnittstellentests | 132 von 132 bestanden, keiner übersprungen |
| Oberflächentests | 38 von 38 bestanden |
| Containerplattform | alle Prüfungen bestanden |
| Messung des Matchings | 20 von 20 geeignet (Ziel 16), Median 81 ms, höchstens 363 ms (Ziel 7 s), 4 von 4 Notfällen gestoppt; Zusatzfälle ohne Fehler |

Die Ergebnisse der automatischen Tests stammen aus dem CI-Lauf vom 27.11.2026 mit diesem Testprotokoll (Aufgabe 2.44). Lokal liefen die Schnittstellentests am 25.11.2026 mit 126 von 132 bestanden; die 6 übrigen brauchen die Attrappe des Sprachmodells und laufen nur in der CI. Die Messung lief am 25.11.2026 auf dem Arbeitsgerät ([Messprotokoll](messprotokolle/r2-matching-2026-11-25-08-27.md)).

## Befunde und Massnahmen

| Nr. | Datum | Befund | Massnahme | Stand |
|---|---|---|---|---|
| 1 | 20.11.2026 | Ein zu grosser Anfrageinhalt führte zu einer Antwort `500` statt einer verständlichen Fehlermeldung | zentrale Fehlerbehandlung antwortet mit `413` und Problem-Dokument (Aufgabe 2.30) | erledigt |
| 2 | 25.11.2026 | Zusatzmessung: Tippfehler «heitzung» sicher, aber falsch eingeordnet | ein Buchstabe Abweichung in Schlüsselwörtern ab 7 Zeichen (Aufgabe 2.37) | erledigt |
| 3 | 25.11.2026 | Zusatzmessung: drei Notfälle in Mundart, Englisch und Alltagssprache nicht erkannt | 30 Notfallbegriffe ergänzt, mit Gegenbeispielen in den Einheitentests (Aufgabe 2.37) | erledigt |
| 4 | 25.11.2026 | Zusatzmessung: erste Frage an das Sprachmodell nach 10061 ms im Zeitlimit | Vorladen mit der Anweisung, feste Reihenfolge der Kategorien, kürzere Antwort, Zeitlimit 7 s; danach 4118 ms (Aufgabe 2.37) | erledigt |
| 5 | 23.11.2026 | Lokal überspringen die Schnittstellentests 6 Fälle ohne Attrappe des Sprachmodells | in der CI mit Attrappe ausgeführt; lokal bewusst übersprungen | bekannt |
| 6 | 24.11.2026 | Schnittstellentests vor einer Messung verändern die Daten der Rangfolge | Messung immer auf frischer Datenbank mit Testdaten, ohne Tests dazwischen ([messung-matching.md](messung-matching.md)) | bekannt |
| 7 | 26.11.2026 | Fehlertexte der Schnittstelle erscheinen in der französischen Oberfläche auf Deutsch | Fehlertexte über den Fehlercode in der gewählten Sprache, geplant in Release 3 | offen, Release 3 |
| 8 | 27.11.2026 | Im Gesamtlauf schlug ein Schnittstellentest der Vorschau einmal fehl: Die Testdateien laufen gleichzeitig, und ein anderer Test legte zwischen zwei Abfragen ein Profil am selben Ort an | die Vorschau-Tests liegen jetzt in 1797 Münchenwiler, mehr als 15 km von allen anderen Testprofilen (Aufgabe 2.44) | erledigt |
| 9 | 27.11.2026 | Die Testübersicht erkannte die Ausgabe der Einheitentests in der CI nicht, wegen der Farbcodes der Konsole | Farbcodes werden vor der Auswertung entfernt, mit eigenem Test (Aufgabe 2.44) | erledigt |

## Ausführen

| Test | Befehl |
|---|---|
| Hygieneprüfung | `node --test "tests/hygiene/*.test.mjs"` |
| Testübersicht, Messung, Attrappe | `node --test "tests/ci/*.test.mjs"`, `node --test "tests/messprotokoll/*.test.mjs"`, `node --test "tests/attrappe/*.test.mjs"` |
| Einheitentests | im Ordner `backend`: `dotnet test --solution Soforthilfe.slnx` |
| Rauchtest | `API_URL=http://localhost:5000 bash tests/rauchtest/schnittstelle.sh` |
| Schnittstellentests | `node --test "tests/api/*.test.mjs"` mit `API_URL` und `TESTDATEN_PASSWORT`, für alle Fälle zusätzlich `SPRACHMODELL_ATTRAPPE` mit laufender Attrappe |
| Oberflächentests | in `tests/oberflaeche`: `npm ci`, `npx playwright install chromium`, `npx playwright test` mit `OBERFLAECHE_URL` und `TESTDATEN_PASSWORT` |
| Messung des Matchings | `node tests/messprotokoll/messen.mjs` mit `API_URL` und `TESTDATEN_PASSWORT`, frische Datenbank, Sprachmodell geladen ([messung-matching.md](messung-matching.md)) |
