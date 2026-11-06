# Abnahme Release 1

| Angabe | Wert |
|---|---|
| Release | R1 Fundament, 19.10. bis 06.11.2026 |
| Aufgaben | 1.1 bis 1.22 (Issues #1 bis #22) |
| Abnahme | 06.11.2026 |
| Tag | `r1-abnahme` |
| Testprotokoll | [testprotokoll-r1.md](testprotokoll-r1.md) |

## Abnahmekriterien

| Nr. | Kriterium | Nachweis | Erfüllt |
|---|---|---|---|
| 1 | Alle Aufgaben von Release 1 sind umgesetzt, je Aufgabe ein Commit | Kanban, Commits `R1: … (Refs #1)` bis `(Refs #22)` | ja |
| 2 | Alle automatischen Prüfungen laufen in der CI grün | Jobs Geheimnisprüfung, Hygieneprüfung, Frontend, Backend mit Schnittstellen- und Oberflächentests, Containerplattform | ja |
| 3 | Backend in drei Schichten mit Datenmodell und Migrationen | [architektur.md](architektur.md), [datenmodell.md](datenmodell.md) | ja |
| 4 | Mandantenfähigkeit mit strikter Datentrennung (Anforderung A18) | Schnittstellentests Mandantentrennung, [mandanten.md](mandanten.md) | ja |
| 5 | Anmeldung mit JWT, Rollen und Berechtigungen | Schnittstellentests Anmeldung und Rollen, [anmeldung-und-rollen.md](anmeldung-und-rollen.md) | ja |
| 6 | Anfrage per Kategorie, Freitext oder beidem; Mehrwertnummern abgelehnt (Anforderung A10); kostenlos und kostenpflichtig getrennt (Anforderung A12) | Schnittstellen- und Oberflächentests, [schnittstelle.md](schnittstelle.md) | ja |
| 7 | Oberfläche mit Notfallband 144, Regionswahl, Registrierung, Anmeldung und Anfrageformular, auch auf dem Mobiltelefon | Oberflächentests, [screenflow.md](screenflow.md) | ja |
| 8 | Containerplattform startet mit einem Befehl, Zugriff nur über HTTPS | CI-Job Containerplattform, [containerplattform.md](containerplattform.md) | ja |
| 9 | Keine Geheimnisse, Build-Ausgaben oder Office-Dateien im Repository | Gitleaks über die gesamte Historie, Hygieneprüfung | ja |
| 10 | README und Dokumentation entsprechen dem Code | Durchsicht von Gliederung, Links, Diagrammen und Badges | ja |

## Bekannte Punkte

| Punkt | Behandlung |
|---|---|
| Paketprüfung meldet `deepmerge` (tief) im Build-Werkzeug Workbox, ohne korrigierte Version | betrifft nur den Build; Beobachtung über Dependabot (Testprotokoll, Befund 5) |
| Lokale Testläufe hinterlassen Testkonten und Testregionen | lokale Datenbank bei Bedarf zurücksetzen (Testprotokoll, Befund 6) |

## Übergabe an Release 2

Release 2 (09.11. bis 27.11.2026) baut auf diesem Stand auf: Notfall-Stopp mit Hinweis «144 anrufen», Orte über das Ortschaftenverzeichnis von swisstopo und Standortfreigabe, Helferprofile mit Verifizierung und Preisangaben, Verständniskette mit Regeln, TF-IDF-Klassifikator und Sprachmodell, Mehrsprachigkeit Deutsch und Französisch sowie Smart-Matching mit Begründung.

## Entscheid

Release 1 ist abgenommen. Der abgenommene Stand trägt den Tag `r1-abnahme`.
