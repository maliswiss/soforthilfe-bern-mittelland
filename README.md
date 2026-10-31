# Soforthilfe Bern/Mittelland

Hyperlokale Vermittlungsplattform für zeitkritische Alltagshilfe in Bern und Mittelland.
HF Informatik, gibb Bern.

[![CI](https://github.com/maliswiss/soforthilfe-bern-mittelland/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/maliswiss/soforthilfe-bern-mittelland/actions/workflows/ci.yml)

Hilfesuchende beschreiben ihre Lage in eigenen Worten. Die Plattform erkennt den Bedarf, stoppt bei gesundheitlichen Notfällen mit dem Hinweis «144 anrufen» und schlägt verifizierte Helfende aus der Nähe vor.

## Stand

**Im Aufbau.** Die Plattform ist noch nicht lauffähig.

| Release | Zeitraum | Inhalt | Stand |
|---|---|---|---|
| R1 Fundament | 19.10. bis 06.11.2026 | Repository, CI, Backend-Grundlage, Mandanten, Anmeldung, Anfrage, Containerplattform | in Arbeit |
| R2 Matching | 09.11. bis 27.11.2026 | Notfallregeln, Verständniskette, Orte, Helferprofile, Preise, Mehrsprachigkeit, Matching, Messprotokoll | geplant |
| R3 Prototyp | 30.11. bis 18.12.2026 | Auswahl und Zusage, Echtzeit, Abschluss, Datenschutz, Redundanz, Sicherung, Überwachung, Lasttest, Sicherheitsprüfung | geplant |

Die einzelnen Aufgaben stehen als Issues im Repository und sind nach Release und Zieltermin geordnet.

## Geplante Funktionen

- Anfrage per Kategorie oder Freitext, mit automatischer Erkennung des Bedarfs
- Notfall-Stopp bei gesundheitlichen Notfällen
- Ortsangabe über das amtliche Ortschaftenverzeichnis (swisstopo) oder per Standortfreigabe
- Helferprofile mit Verifizierung, Preisangaben und Verfügbarkeit
- Smart-Matching mit nachvollziehbarer Begründung
- Mehrsprachigkeit (Deutsch und Französisch), weitere Sprachen ohne Deployment
- Echtzeit-Hinweise zwischen Hilfesuchenden und Helfenden
- Mandantenfähigkeit: Regionen sind strikt getrennt
- Datenschutz, Redundanz mit zwei API-Instanzen, Sicherung, Überwachung

## Aufbau des Repositories

| Verzeichnis | Inhalt |
|---|---|
| `backend/` | ASP.NET Core Web API |
| `frontend/` | Vue 3 Web-App (PWA) |
| `deploy/` | Docker Compose, Traefik, Sicherung, Überwachung |
| `tests/` | Schnittstellen-, Oberflächen-, Last- und Sicherheitstests |
| `docs/` | Architektur, Konventionen, Drittkomponenten, Protokolle |
| `.github/` | Issue-Vorlage und CI |

## Technik

ASP.NET Core 10, Entity Framework Core, PostgreSQL 16 mit PostGIS, JWT, SignalR, Vue 3 mit Vite, Tailwind CSS und vue-i18n, Docker Compose mit Traefik, Prometheus und Alertmanager, Playwright. Details stehen in [docs/architektur.md](docs/architektur.md).

## Start

Die Startanleitung folgt mit Release 1, sobald die Containerplattform steht.
Zugangsdaten und Schlüssel gehören nie in das Repository: Vorlage ist die Datei [`.env.example`](.env.example), die echte `.env` bleibt lokal.

## Nummerierung

Im Repository und in den Issues kommen zwei Nummerierungen vor, die nichts miteinander zu tun haben:

- **«Anforderung A…»** (zum Beispiel «Anforderung A18») bezeichnet einen Punkt aus dem Anforderungskatalog des Projekts.
- **A01 bis A10** sind die Kategorien der [OWASP Top 10](https://owasp.org/Top10/) in der Ausgabe 2025. Die Sicherheitsprüfungen sind danach geordnet.

| Kennung | Kategorie |
|---|---|
| A01 | Zugriffskontrolle (Broken Access Control) |
| A02 | Fehlkonfiguration (Security Misconfiguration) |
| A03 | Software-Lieferkette (Software Supply Chain Failures) |
| A04 | Kryptografie (Cryptographic Failures) |
| A05 | Injection |
| A06 | Unsicherer Entwurf (Insecure Design) |
| A07 | Authentifizierung (Authentication Failures) |
| A08 | Software- und Datenintegrität (Software or Data Integrity Failures) |
| A09 | Protokollierung und Alarmierung (Security Logging and Alerting Failures) |
| A10 | Fehlerhafter Umgang mit Ausnahmesituationen (Mishandling of Exceptional Conditions) |

## Commits und Issues

Jede Aufgabe ist ein Issue, jeder Commit nennt sein Issue und trägt das Präfix seines Releases, zum Beispiel `R1: Kurzbeschreibung (Refs #5)`. Die Regeln stehen in [docs/commit-konvention.md](docs/commit-konvention.md).

## Lizenz

[MIT](LICENSE). Die verwendeten Drittkomponenten mit Lizenz und Quelle stehen in [docs/drittkomponenten.md](docs/drittkomponenten.md).
