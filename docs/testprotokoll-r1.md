# Testprotokoll Release 1

Release 1 (19.10. bis 06.11.2026): Fundament mit CI, Backend in drei Schichten, Datenmodell, Mandantenfähigkeit, Anmeldung und Rollen, Anfrage erfassen, Oberfläche und Containerplattform. Alle Tests laufen bei jedem Push automatisch in der CI ([GitHub Actions](https://github.com/maliswiss/soforthilfe-bern-mittelland/actions/workflows/ci.yml)).

## Testumgebung

| Bereich | CI (GitHub Actions) | Lokal |
|---|---|---|
| Betriebssystem | Ubuntu (`ubuntu-latest`) | Windows mit Docker Desktop |
| Schnittstelle | .NET SDK 10, ASP.NET Core 10 im Release-Build, Umgebung `Production` wie im Container, mit Testdaten | `dotnet run`, Umgebung `Development` |
| Datenbank | PostgreSQL 16 mit PostGIS 3.4 als Service-Container, Schema aus den Migrationen | gleiches Image über Docker Compose, Port 5433 |
| Oberfläche | Node.js 22, Vite-Vorschau des Builds mit den Sicherheitsköpfen des Betriebs | `npm run dev` |
| Browser | Chromium über Playwright 1.64.1, Desktop und Mobiltelefon (Pixel 7) | Chrome |
| Containerplattform | Docker Compose mit Profil `plattform`, Traefik, nginx | Docker Desktop |
| Testdaten | fiktive Personen auf `beispiel.invalid`, Passwort und Schlüssel je Lauf zufällig erzeugt | lokale `.env` |

## Testumfang

| Testart | Ort | Anzahl | Prüft |
|---|---|---|---|
| Hygieneprüfung | [`tests/hygiene`](../tests/hygiene) | 11 Tests | keine Geheimnisse, Build-Ausgaben oder Office-Dateien im Repository |
| Geheimnisprüfung | Gitleaks über die gesamte Historie | 1 Lauf | keine Zugangsdaten in irgendeinem Commit |
| Zustandsprüfung und Stammdaten | CI-Schritte im Job «Backend bauen und testen» | 13 Prüfungen | `/health/live`, `/health/ready`, `/api/info`, Fehlerantwort mit `fehler`, Anzahl Zeilen je Tabelle, nur fiktive Adressen |
| Rauchtest der Schnittstelle | [`tests/rauchtest/schnittstelle.sh`](../tests/rauchtest/schnittstelle.sh) | 39 Prüfungen | jeder Endpunkt mit Status und Inhalt |
| Schnittstellentests | [`tests/api`](../tests/api) | 29 Tests | Anmeldung, Rollen, Mandantentrennung, Mehrwertnummern, Kategorien, Anfragen |
| Oberflächentests | [`tests/oberflaeche`](../tests/oberflaeche) | 12 Tests | Notfallband, Startseite, Regionswahl, Registrierung, Anmeldung, Anfrageformular, Mobiltelefon |
| Containerplattform | CI-Job «Containerplattform starten» | 5 Prüfungen | Start mit einem Befehl, Zustandsprüfung und Schnittstelle über HTTPS, Oberfläche mit Content-Security-Policy, Umleitung HTTP auf HTTPS |

## Abdeckung der Anforderungen

| Anforderung | Schnittstelle | Oberfläche |
|---|---|---|
| Mehrwertnummern werden abgelehnt (Anforderung A10) | 11 Schreibweisen abgewiesen, 4 gültige angenommen | Warnung schon während der Eingabe, Registrierung wird nicht gesendet |
| Kostenlos und kostenpflichtig getrennt (Anforderung A12) | Filter `bezahlpflichtig`, Anfrage meldet `bezahlpflichtig` | Kennzeichnung «kostenlos» auf der Startseite und in der Anfrage |
| Strikte Datentrennung der Regionen (Anforderung A18) | fremde Konten und Anfragen liefern `404`, abweichender Kopf `X-Region` liefert `403` | Anmeldung nur in der Region des Kontos |
| Anfrage per Kategorie, Freitext oder beidem | alle drei Wege, Pflichtfelder Postleitzahl und Ort | beide Wege von der Startseite bis «Meine Anfragen» |
| Anmeldung und Rollen | Token mit Kennung, Rolle und Region; Rechte je Endpunkt | Schutz der Seiten, Rückkehr zum Ziel nach der Anmeldung |
| Notfall: Hinweis auf 144 | | Notfallband mit `tel:144` auf jeder Seite |
| Sicherheitsköpfe | | Content-Security-Policy ohne Verstösse in der Browserkonsole |

## Ergebnisse

| Testart | Ergebnis |
|---|---|
| Hygieneprüfung | 11 von 11 bestanden |
| Geheimnisprüfung | keine Funde |
| Zustandsprüfung und Stammdaten | alle Prüfungen bestanden |
| Rauchtest der Schnittstelle | 39 von 39 bestanden |
| Schnittstellentests | 29 von 29 bestanden, keiner übersprungen |
| Oberflächentests | 12 von 12 bestanden |
| Containerplattform | alle Prüfungen bestanden |

Die Ergebnisse stammen aus dem CI-Lauf vom 06.11.2026 nach dem Commit der Oberflächentests (Aufgabe 1.20). Die Schnittstellentests liefen zusätzlich lokal gegen PostgreSQL mit 29 von 29 bestanden.

## Befunde und Massnahmen

| Nr. | Datum | Befund | Massnahme | Stand |
|---|---|---|---|---|
| 1 | 01.11.2026 | Port 5432 lokal belegt, die Datenbank startet nicht | Port über `POSTGRES_PORT` in der `.env` wählbar, lokal 5433 | erledigt |
| 2 | 04.11.2026 | Backend-Job der CI schlägt nach den Aufgaben 1.11 bis 1.13 fehl | Schnittstelle, Rauchtest und Tests in Aufgabe 1.14 bereinigt | erledigt |
| 3 | 05.11.2026 | Schnittstellentest findet 8 statt 10 Fähigkeiten: beim Löschen alter Kategorien entfernte die Löschweitergabe zwei weiterverwendete Fähigkeiten | neue feste Kennungen für beide Fähigkeiten, Migration neu erzeugt, vor dem Push behoben | erledigt |
| 4 | 05.11.2026 | Paketprüfung meldet `glob` und `minimatch` (mittel) im Build-Werkzeug Workbox | `glob` 14.0.3 für Workbox festgelegt (`overrides` in `frontend/package.json`) | erledigt |
| 5 | 05.11.2026 | Paketprüfung meldet `deepmerge` (tief) im Build-Werkzeug Workbox; es gibt keine korrigierte Version | betrifft nur den Build, nicht die ausgelieferte Oberfläche; Beobachtung über Dependabot | akzeptiert |
| 6 | 05.11.2026 | Lokale Testläufe legen jedes Mal neue Konten und Regionen an | Datenbank bei Bedarf mit `docker compose down -v` zurücksetzen; die CI beginnt immer mit einer leeren Datenbank | bekannt |

## Ausführen

| Test | Befehl |
|---|---|
| Hygieneprüfung | `node --test tests/hygiene/*.test.mjs` |
| Rauchtest | `API_URL=http://localhost:5000 bash tests/rauchtest/schnittstelle.sh` |
| Schnittstellentests | `node --test "tests/api/*.test.mjs"` mit `API_URL` und `TESTDATEN_PASSWORT` |
| Oberflächentests | in `tests/oberflaeche`: `npm ci`, `npx playwright install chromium`, `npx playwright test` mit `OBERFLAECHE_URL` und `TESTDATEN_PASSWORT` |
