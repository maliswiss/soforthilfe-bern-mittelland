# Testdaten

Testdaten machen die Plattform für Entwicklung, Tests und Vorführung sofort nutzbar: Konten aller Rollen in zwei Regionen, Helferprofile mit und ohne Preise, Nachweise in verschiedenen Ständen und zwei Anfragen. Alle Personen sind erfunden.

## Laden

| Punkt | Umsetzung |
|---|---|
| Einschalten | `Testdaten__Laden=true` in der `.env`; ohne diesen Wert lädt die Schnittstelle keine Testdaten |
| Zeitpunkt | beim Start der Schnittstelle, nach den Migrationen ([`TestdatenLader.cs`](../backend/src/Soforthilfe.Datenzugriff/Testdaten/TestdatenLader.cs)) |
| Keine Doppelungen | Konten, Profile, Nachweise und Anfragen nur in eine Datenbank ohne Konten; ein Neustart ändert nichts |
| Zweite Region | `biel-seeland` mit ihren Gemeinden wird bei jedem Start mit Testdaten angelegt oder ergänzt, als Datensatz wie jede weitere Region im laufenden Betrieb |
| Passwort | `TESTDATEN_PASSWORT` aus der `.env`, mindestens 10 Zeichen; es gilt für alle Testkonten ohne Passwort und nur für Adressen auf `beispiel.invalid`. Ohne diesen Wert lassen sich die Testkonten nicht anmelden |

Das Passwort steht nie im Repository; die Vorlage [`.env.example`](../.env.example) enthält nur einen Platzhalter. In der CI erzeugt jeder Lauf ein zufälliges Passwort.

## Regionen

| Region | Gemeinden | Herkunft |
|---|---|---|
| `bern-mittelland` | 74 | Stammdaten der Migration |
| `biel-seeland` | 61 | Testdaten |

Die Gemeinden sind BFS-Nummern der Verwaltungskreise des Kantons Bern ([orte-und-standort.md](orte-und-standort.md)).

## Konten

| Name | E-Mail-Adresse | Rolle | Region |
|---|---|---|---|
| Anna Beispiel | `anna.beispiel@beispiel.invalid` | Hilfesuchende | Bern/Mittelland |
| Bruno Muster | `bruno.muster@beispiel.invalid` | Helfende | Bern/Mittelland |
| Clara Fiktiv | `clara.fiktiv@beispiel.invalid` | Helfende | Bern/Mittelland |
| Dora Probe | `dora.probe@beispiel.invalid` | Koordination | Bern/Mittelland |
| Emil Exempel | `emil.exempel@beispiel.invalid` | Hilfesuchende | Biel/Seeland |
| Fritz Platzhalter | `fritz.platzhalter@beispiel.invalid` | Helfende | Biel/Seeland |
| Greta Muster | `greta.muster@beispiel.invalid` | Mandantenadministration | Biel/Seeland |
| Hans Betrieb | `hans.betrieb@beispiel.invalid` | Plattformbetrieb | keine, alle Regionen |

Die Konten haben keine Telefonnummer. Rollen und Berechtigungen: [anmeldung-und-rollen.md](anmeldung-und-rollen.md).

## Helferprofile

| Konto | Ort | Einsatzradius | Fähigkeiten | Preise | Verifizierung |
|---|---|---|---|---|---|
| Bruno Muster | 3012 Bern | 15 km | Sanitär, Heizung, Elektro; Kleine Reparatur | Anfahrt 40, Stundenansatz 95, Nacht 50 %, Wochenende 25 %, MWST-pflichtig | verifiziert |
| Clara Fiktiv | 3110 Münsingen | 5 km | Nachbarschaftshilfe; Begleitung und Orientierung | keine, kostenlose Kategorien | in Prüfung |
| Fritz Platzhalter | 2502 Biel/Bienne | 20 km | IT- und Handy-Soforthilfe | Anfahrt 20, Stundenansatz 70, Wochenende 20 % | nicht verifiziert |

Alle drei sind jetzt verfügbar, ohne Bewertungen und ohne Einsätze. Nur Bruno Muster ist verifiziert und damit Kandidat im [Matching](matching.md).

| Nachweis | Profil | Stand |
|---|---|---|
| E-Mail-Adresse des Kontos | Bruno Muster | freigegeben durch Dora Probe (Koordination) |
| Referenz «Kirchgemeinde Münsingen, Frau Beispiel» | Clara Fiktiv | offen, wartet auf die Koordination |

## Anfragen

| Konto | Ort | Kategorie | Weg | Dringlichkeit | Text |
|---|---|---|---|---|---|
| Anna Beispiel | 3011 Bern | Sanitär, Heizung, Elektro | Kategorieauswahl | mittel | «Der Wasserhahn in der Küche tropft stark.», heute zwischen 17 und 19 Uhr |
| Emil Exempel | 2502 Biel/Bienne | keine | ohne Kategorie | tief | «Ich brauche Hilfe beim Einrichten meines neuen Smartphones.» |

Beide Anfragen sind offen und haben noch keine Vorschläge.

## Messdaten

Für die [Messung des Matchings](messung-matching.md) kommen 14 weitere fiktive Helfende hinzu, zum Beispiel «Schlüsseldienst Fuchs GmbH» oder «Nina Hofer». Sie stehen in [`testfaelle.json`](../tests/messprotokoll/testfaelle.json) und werden nicht beim Start geladen, sondern vom Messskript über die Schnittstelle angelegt und von Dora Probe verifiziert (Adressen `messung.…@beispiel.invalid`). Gemessen wird auf einer frischen Datenbank mit diesen Testdaten.

## Datenschutz

| Regel | Umsetzung |
|---|---|
| Nur erfundene Personen | Namen wie «Anna Beispiel», Adressen auf `beispiel.invalid`, einer Domain, die es nicht geben kann |
| Keine Zugangsdaten im Code | Passwort nur aus der Umgebung, gespeichert wird nur der Hash |
| Nicht im Betrieb | Testdaten nur mit ausdrücklichem Schalter; ein Betrieb ohne `Testdaten__Laden` enthält keine Testkonten |
| Prüfung | die CI prüft, dass alle Konten auf `beispiel.invalid` enden |

## Prüfung

| Test | Prüft |
|---|---|
| CI, Schritt «Stammdaten und Testdaten prüfen» | 2 Regionen, 10 Kategorien, 10 Fähigkeiten, 8 Konten, 3 Helferprofile, 2 Anfragen, 5718 Ortschaften, nur Adressen auf `beispiel.invalid` |
| Schnittstellentests [`tests/api`](../tests/api) | melden sich mit den Testkonten an; ohne `TESTDATEN_PASSWORT` überspringen sich die Tests, die eine Anmeldung brauchen |
| Oberflächentests [`tests/oberflaeche`](../tests/oberflaeche) | Anmeldung, Vorschau mit Namen und Freigabe durch die Koordination mit den Testkonten |
