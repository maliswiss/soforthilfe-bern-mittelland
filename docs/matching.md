# Matching

Das Matching findet zu einer Anfrage passende Helfende. Es läuft in zwei Schritten: Die Kandidatensuche bestimmt, wer überhaupt in Frage kommt; danach ordnet das Smart-Matching die Kandidaten nach einer gewichteten Punktzahl und speichert die drei besten als Vorschläge.

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

## Smart-Matching

Jeder Kandidat erhält je Kriterium Teilpunkte zwischen 0 und 1. Die Punktzahl ist die Summe der gewichteten Teilpunkte und liegt ebenfalls zwischen 0 und 1.

| Kriterium | Teilpunkte | Gewicht |
|---|---|---|
| Distanz | 1 − Distanz / Suchradius, also 1 bei gleicher Postleitzahl und 0 am Rand des Suchradius | 0.35 |
| Fähigkeit | 1; die Fähigkeit ist Voraussetzung der Kandidatensuche | 0.25 |
| Bewertung | Durchschnitt / 5; ohne Bewertungen 0.6, damit neue Helfende eine Chance haben | 0.20 |
| Verfügbarkeit | 1, wenn jetzt verfügbar, sonst 0.4 | 0.10 |
| Erfahrung | Anzahl Einsätze / 10, höchstens 1 | 0.10 |

Beispiel: 2.5 km bei 10 km Suchradius, keine Bewertungen, jetzt verfügbar, 4 Einsätze ergibt 0.75 · 0.35 + 1 · 0.25 + 0.6 · 0.20 + 1 · 0.10 + 0.4 · 0.10 = 0.7725.

Die Gewichte sind einstellbar, im Abschnitt `Matching:Gewichte` von `appsettings.json` oder über die Umgebung, zum Beispiel `Matching__Gewichte__Distanz=0.5`. Die Schnittstelle bringt sie auf die Summe 1; negative Gewichte oder lauter Nullen verhindern den Start.

| Regel | Umsetzung |
|---|---|
| Reihenfolge | nach Punktzahl, bei gleicher Punktzahl nach Distanz, dann nach Kennung |
| Partner-Abo | wirkt nur bei gleichwertiger Punktzahl (Abstand höchstens 0.02): Ein Partner rückt dann vor Helfende ohne Abo und ist gekennzeichnet |
| Anzahl | die drei besten Kandidaten |
| Zeitpunkt | gleich beim Erfassen einer Anfrage mit Kategorie; mit mindestens einem Vorschlag wechselt der Status auf «Vorschläge vorhanden» |
| Benachrichtigung | keine; die anfragende Person entscheidet, wen sie anfragt |
| Nachvollziehbarkeit | gespeichert werden Rang, Punktzahl, Distanz, Teilpunkte und die verwendeten Gewichte; das Protokoll nennt nur die Anzahl (`Anfrage.Vorgeschlagen`, zum Beispiel «3 Vorschläge») |

Die anfragende Person sieht die Vorschläge ihrer Anfrage unter `GET /api/anfragen/{id}/vorschlaege`, die Koordination und die Verwaltung die Vorschläge der Anfragen ihrer Region. Die Vorschläge nennen Name, Postleitzahl und Ort, Distanz, Bewertung, Einsätze und Verfügbarkeit, nie Telefonnummer oder Adresse.

## Tests

| Test | Prüft |
|---|---|
| Komponententests `KandidatenSucheTests` | Suchradius je Dringlichkeit; Grenzen von Such- und Einsatzradius; Reihenfolge |
| Komponententests `PunktzahlTests` | Teilpunkte je Kriterium; gewichtete Punktzahl mit Beispiel; Gewichte auf Summe 1, ungültige Gewichte; Reihenfolge; Partner nur bei gleichwertiger Punktzahl |
| Schnittstellentests `tests/api/vorschlaege.test.mjs` | Zugriff je Rolle; drei beste nach Punktzahl; Punktzahl bei hoher Dringlichkeit; Status «Vorschläge vorhanden»; Protokoll mit Anzahl; ohne Kandidaten offen |
| Schnittstellentests `tests/api/kandidaten.test.mjs` | Zugriff je Rolle; verifiziert, Fähigkeit, Such- und Einsatzradius je Dringlichkeit; Preise bei kostenpflichtigen Kategorien; ohne Kategorie; fremde Region |
