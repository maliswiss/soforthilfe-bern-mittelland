# Matching

Das Matching findet zu einer Anfrage passende Helfende. Es läuft in zwei Schritten: Die Kandidatensuche bestimmt, wer überhaupt in Frage kommt; danach ordnet das Smart-Matching die Kandidaten.

## Kandidatensuche

Geeignet sind Helferprofile, die alle Bedingungen erfüllen:

| Bedingung | Umsetzung |
|---|---|
| Region | dieselbe Region wie die Anfrage (Mandantentrennung) |
| Verifizierung | Profil verifiziert; Profile in Prüfung, abgelehnt oder gesperrt fallen weg |
| Fähigkeit | mindestens eine Fähigkeit der Kategorie der Anfrage; ohne Kategorie wird nicht gesucht |
| Preise | bei kostenpflichtigen Kategorien Anfahrt und Stundenansatz angegeben (Anforderung A9) |
| Suchradius | Distanz höchstens so gross wie der Suchradius der Anfrage |
| Einsatzradius | Distanz höchstens so gross wie der Einsatzradius im Helferprofil |

Die Distanz gilt zwischen den Punkten der Postleitzahlen aus dem Ortschaftenverzeichnis, nie zwischen genauen Standorten. PostGIS rechnet mit `geography` in Metern; die Vorauswahl `ST_DWithin` nutzt den räumlichen Index. Ohne Ortschaftenverzeichnis kommen nur Profile mit derselben Postleitzahl in Frage.

| Dringlichkeit | Suchradius |
|---|---|
| hoch | 15 km |
| mittel | 10 km |
| tief | 5 km |

Die Kandidaten sind nach Distanz geordnet, bei gleicher Distanz nach Kennung. Helferprofile ohne Punkt, etwa aus Testdaten, erhalten beim Start der Schnittstelle den Punkt ihrer Postleitzahl.

Koordination und Verwaltung sehen die Kandidaten einer Anfrage ihrer Region unter `GET /api/anfragen/{id}/kandidaten`: Name, Postleitzahl und Ort, Distanz, Einsatzradius und Verfügbarkeit, ohne Telefonnummer und Adresse.

## Tests

| Test | Prüft |
|---|---|
| Komponententests `KandidatenSucheTests` | Suchradius je Dringlichkeit; Grenzen von Such- und Einsatzradius; Reihenfolge |
| Schnittstellentests `tests/api/kandidaten.test.mjs` | Zugriff je Rolle; verifiziert, Fähigkeit, Such- und Einsatzradius je Dringlichkeit; Preise bei kostenpflichtigen Kategorien; ohne Kategorie; fremde Region |
