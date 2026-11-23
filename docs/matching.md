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
| Nachvollziehbarkeit | gespeichert werden Rang, Punktzahl, Distanz, Teilpunkte, die verwendeten Gewichte und die Begründung; das Protokoll nennt nur die Anzahl (`Anfrage.Vorgeschlagen`, zum Beispiel «3 Vorschläge») |

Die anfragende Person sieht die Vorschläge ihrer Anfrage unter `GET /api/anfragen/{id}/vorschlaege`, die Koordination und die Verwaltung die Vorschläge der Anfragen ihrer Region. Die Vorschläge nennen Name, Postleitzahl und Ort, Distanz, Bewertung, Einsätze und Verfügbarkeit, nie Telefonnummer oder Adresse.

## Begründung

Jeder Vorschlag erklärt sich selbst: als kurzer Text und mit dem Beitrag jedes Kriteriums zur Punktzahl.

| Teil | Inhalt |
|---|---|
| `begruendung` | Distanz, Fähigkeit mit Kategorie, Bewertung, «verifiziert», Verfügbarkeit, Einsätze; bei Partnern der Hinweis auf den Vorzug bei gleichwertiger Punktzahl |
| `kriterien` | je Kriterium `teilpunkte` (0 bis 1), `gewicht` und `beitrag` = Teilpunkte · Gewicht; die Beiträge ergeben zusammen die Punktzahl |

Die Distanz steht unter 1 km in Metern, sonst in Kilometern mit einer Stelle; bei gleicher Postleitzahl steht «gleiche Postleitzahl». Der Text wird beim Vorschlag auf Deutsch gespeichert und hält fest, was zu diesem Zeitpunkt galt. Die Oberfläche kann die Kriterien über ihre Codes in jeder Sprache darstellen.

```json
{
  "rang": 1,
  "punktzahl": 0.7725,
  "distanzMeter": 2500,
  "begruendung": "2.5 km entfernt · Fähigkeit «Nachbarschaftshilfe» · noch keine Bewertungen · verifiziert · jetzt verfügbar · 4 Einsätze",
  "kriterien": [
    { "kriterium": "distanz", "teilpunkte": 0.75, "gewicht": 0.35, "beitrag": 0.2625 },
    { "kriterium": "faehigkeit", "teilpunkte": 1, "gewicht": 0.25, "beitrag": 0.25 },
    { "kriterium": "bewertung", "teilpunkte": 0.6, "gewicht": 0.2, "beitrag": 0.12 },
    { "kriterium": "verfuegbarkeit", "teilpunkte": 1, "gewicht": 0.1, "beitrag": 0.1 },
    { "kriterium": "erfahrung", "teilpunkte": 0.4, "gewicht": 0.1, "beitrag": 0.04 }
  ]
}
```

## Vorschlagsliste

Die Anfrage zeigt ihre Vorschläge unter «Vorschläge» ([screenflow.md](screenflow.md), Bildschirm 05):

| Teil | Inhalt |
|---|---|
| Kopf | Rang und Name, bei Betrieben der Betriebsname; Betrieb oder Privatperson; Punktzahl |
| Begründung | der gespeicherte Text des Vorschlags |
| Angaben | Entfernung («im gleichen Postleitzahlgebiet», in Metern oder Kilometern) und Verfügbarkeit; bei Partnern der Hinweis auf den Vorzug bei gleichwertiger Eignung |
| Preis | bei kostenpflichtigen Kategorien Richtpreis für die erste Stunde mit Anfahrt, Stundenansatz und Zuschlägen, berechnet beim Abruf aus dem aktuellen Profil (Anforderung A9) |
| Kontakt | Hinweis, dass die Telefonnummer erst nach der Zusage sichtbar wird |
| Reihenfolge | zugeklappt unter «Wie kommt diese Reihenfolge zustande?» mit Pfeil: je Kriterium die Teilpunkte, darunter die Punktzahl |

Die Kriterien erscheinen über ihre Codes in der gewählten Sprache; der Begründungstext bleibt deutsch.

## Vorschau ohne Anmeldung

Schon vor der Anmeldung zeigt `POST /api/anfragen/vorschau`, wer helfen könnte: dieselbe Kandidatensuche und Punktzahl wie bei den Vorschlägen, aber ohne Anfrage und ohne Speichern. Ohne gewählte Dringlichkeit gilt die übliche der Kategorie.

| Angabe | ohne Anmeldung | angemeldet | nach der Zusage |
|---|---|---|---|
| Begründung, Punktzahl, Distanz, Bewertung, Verfügbarkeit, Preisangaben | ja | ja | ja |
| Betrieb oder Privatperson | ja | ja | ja |
| Name, bei Betrieben der Betriebsname | nein | ja | ja |
| Telefonnummer | nein | nein | ja (Release 3) |
| Adresse, Kennung des Profils | nein | nein | nein |

Die gestaffelte Preisgabe entscheidet die Schnittstelle, nicht die Oberfläche: Ohne Anmeldung enthält die Antwort keinen Namen, die Oberfläche zeigt nur einen verwischten Platzhalter. Ohne Anmeldung bestimmt die Postleitzahl die Region, wie bei der Registrierung; Postleitzahlen ohne Region oder ausserhalb des Ortschaftenverzeichnisses werden mit Hinweis auf das Feld `plz` abgewiesen. Angemeldet gilt die Region des Kontos, für den Plattformbetrieb die Region der Postleitzahl.

## Messung

Testfälle, erwartete Ergebnisse und Messdaten der Messung stehen in [messung-matching.md](messung-matching.md).

## Tests

| Test | Prüft |
|---|---|
| Komponententests `KandidatenSucheTests` | Suchradius je Dringlichkeit; Grenzen von Such- und Einsatzradius; Reihenfolge |
| Komponententests `PunktzahlTests` | Teilpunkte je Kriterium; gewichtete Punktzahl mit Beispiel; Gewichte auf Summe 1, ungültige Gewichte; Reihenfolge; Partner nur bei gleichwertiger Punktzahl |
| Schnittstellentests `tests/api/vorschau.test.mjs` | ungültige Eingaben mit Feld; Dringlichkeit und Ort aus Kategorie und Postleitzahl; ohne Anmeldung ohne Namen, angemeldet mit Namen, Telefonnummer nie; Preise bei kostenpflichtigen Kategorien; Region aus der Postleitzahl |
| Oberflächentests `tests/oberflaeche/vorschlaege.spec.mjs` | Vorschlagsliste mit Begründung, Entfernung, Preis, Hinweis zur Telefonnummer und zugeklappten Teilpunkten je Kriterium mit Punktzahl; Sprachwechsel auf der Liste ohne neue Reihenfolge; Hinweis, wenn niemand Passendes in Reichweite ist |
| Oberflächentests `tests/oberflaeche/vorschau.spec.mjs` | Vorschau ohne Anmeldung mit verdecktem Namen und Preis; nach der Anmeldung mit Namen und weiter zum Erfassen |
| Komponententests `BegruendungTests` | Text mit und ohne Bewertungen, Einzahl, Partner; Distanz in Metern und Kilometern; Kriterien mit Beitrag und Summe |
| Schnittstellentests `tests/api/begruendung.test.mjs` | Text je Vorschlag; alle Kriterien mit Teilpunkten, Gewicht und Beitrag; Summe gleich Punktzahl |
| Schnittstellentests `tests/api/vorschlaege.test.mjs` | Zugriff je Rolle; drei beste nach Punktzahl; Punktzahl bei hoher Dringlichkeit; Status «Vorschläge vorhanden»; Protokoll mit Anzahl; Preisangaben bei kostenpflichtigen Kategorien; ohne Kandidaten offen; nicht verfügbare Helfende mit 0.4 Teilpunkten und 0.06 Punkten weniger; gespeicherte Vorschläge bleiben nach Profiländerungen gleich |
| Schnittstellentests `tests/api/kandidaten.test.mjs` | Zugriff je Rolle; verifiziert, Fähigkeit, Such- und Einsatzradius je Dringlichkeit; Preise bei kostenpflichtigen Kategorien; ohne Kategorie; fremde Region; kleinerer Einsatzradius wirkt sofort, neuer Ort erst nach erneuter Prüfung |
