# Abnahme Release 2

| Angabe | Wert |
|---|---|
| Release | R2, 09.11. bis 27.11.2026 |
| Aufgaben | 2.1 bis 2.45 (Issues #23 bis #67) |
| Abnahme | 27.11.2026 |
| Tag | `r2-abnahme` |
| Testprotokoll | [testprotokoll-r2.md](testprotokoll-r2.md) |
| Messprotokoll | [messung-matching.md](messung-matching.md), [messprotokolle](messprotokolle/README.md) |

## Abnahmekriterien

| Nr. | Kriterium | Nachweis | Erfüllt |
|---|---|---|---|
| 1 | Alle Aufgaben von Release 2 sind umgesetzt, je Aufgabe ein Commit | Kanban, Commits `R2: … (Refs #23)` bis `(Refs #67)` | ja |
| 2 | Alle automatischen Prüfungen laufen in der CI grün | Jobs Geheimnisprüfung, Hygieneprüfung, Frontend, Backend mit Einheiten-, Schnittstellen- und Oberflächentests, Containerplattform; Testübersicht je Job | ja |
| 3 | Gesundheitliche Notfälle werden nie vermittelt: Stopp mit Hinweis «144 anrufen» und Protokolleintrag ohne Personendaten | Einheiten- und Schnittstellentests, 4 von 4 Notfällen und 3 von 3 Zusatzfällen in der Messung gestoppt, [notfallpruefung.md](notfallpruefung.md) | ja |
| 4 | Orte aus dem Ortschaftenverzeichnis von swisstopo, Region aus der Postleitzahl, Standort ohne Speicherung | Schnittstellen- und Oberflächentests, [orte-und-standort.md](orte-und-standort.md) | ja |
| 5 | Helferprofile mit Verifizierung durch die Koordination und Preisangaben | Schnittstellen- und Oberflächentests, [helferprofil.md](helferprofil.md), [verifizierung.md](verifizierung.md), [preise.md](preise.md) | ja |
| 6 | Verständniskette mit Kategorie, Regeln, TF-IDF-Klassifikator und Sprachmodell, mit Begründung | Einheitentests mit Kalibrierungssatz, Schnittstellentests mit Attrappe, [verstaendniskette.md](verstaendniskette.md) | ja |
| 7 | Mehrsprachigkeit Deutsch und Französisch, Sprachimport ohne Deployment | Schnittstellen- und Oberflächentests, [mehrsprachigkeit.md](mehrsprachigkeit.md) | ja |
| 8 | Ziel 2: in mindestens 16 von 20 vorab festgelegten Testfällen ein geeigneter, begründeter Vorschlag; Antwortzeit je Testfall gemessen | Messung 3: 20 von 20, Median 81 ms, höchstens 363 ms, Zielbereich 7 s eingehalten ([Messprotokoll](messprotokolle/r2-matching-2026-11-25-08-27.md)) | ja |
| 9 | Keine Geheimnisse, Build-Ausgaben, Office-Dateien oder Probemessungen im Repository | Gitleaks über die gesamte Historie, Hygieneprüfung | ja |
| 10 | README und Dokumentation entsprechen dem Code | Durchsicht von Funktionen, Testständen, Schnittstelle, Testdaten und Links | ja |

## Bekannte Punkte

| Punkt | Behandlung |
|---|---|
| Fehlertexte der Schnittstelle erscheinen in der französischen Oberfläche auf Deutsch | in Release 3 über den Fehlercode in der gewählten Sprache (Testprotokoll, Befund 7) |
| Lokal überspringen die Schnittstellentests 6 Fälle ohne Attrappe des Sprachmodells | in der CI vollständig ausgeführt (Testprotokoll, Befund 5) |
| Vorschläge von Dependabot zur Aktualisierung von Abhängigkeiten | Prüfung in der Paketprüfung von Release 3 |
| Die Antwortzeit des Sprachmodells beruht auf einer einzigen Entscheidung auf dem Prozessor des Arbeitsgeräts | 4.1 s, im Zielbereich; die übrigen Stufen antworten in Millisekunden ([messung-matching.md](messung-matching.md)) |

## Übergabe an Release 3

Release 3 (30.11. bis 18.12.2026) baut auf diesem Stand auf: Auswahl und Zusage in der Vorschlagsliste, «Suche erweitern», Eskalation und Hinweise in Echtzeit, Chat je Anfrage, Abschluss mit Auftragswert, Vermittlungsgebühr und Bewertung, Koordinations-Dashboard und Auswertung, Schutz der Standortdaten, Auskunft, Löschung und Aufbewahrungsfristen, Redundanz mit zwei API-Instanzen, Sicherung, Überwachung und Alarmierung sowie die Sicherheitsprüfung nach den OWASP Top 10.

## Entscheid

Release 2 ist abgenommen. Der abgenommene Stand trägt den Tag `r2-abnahme`.
