# Helferprofil

Helfende Personen und Betriebe beschreiben im Helferprofil, was sie anbieten, wo und ob sie gerade verfügbar sind. Das Profil ist die Grundlage für Vorschläge an Hilfesuchende.

## Inhalt

| Angabe | Regel |
|---|---|
| Typ | `0` Privatperson oder `1` Betrieb; Betriebe geben ihren Namen an, bei Privatpersonen wird kein Betriebsname gespeichert |
| Beschreibung | freiwillig, höchstens 1000 Zeichen |
| Postleitzahl und Ort | Pflicht; die Postleitzahl steht im Ortschaftenverzeichnis und berührt eine Gemeinde der Region des Kontos |
| Einsatzradius | 1 bis 100 km |
| Fähigkeiten | mindestens eine bekannte Fähigkeit (`GET /api/skills`); beim Speichern werden alle Fähigkeiten ersetzt |
| Adresse | freiwillig; sieht nur die anfragende Person, und erst nach der Zusage |
| Preisangaben | Anfahrt, Stundenansatz, Nacht- und Wochenendzuschlag, MWST-Pflicht; Pflicht bei bezahlpflichtigen Fähigkeiten ([preise.md](preise.md)) |
| Verfügbarkeit | «jetzt verfügbar» lässt sich jederzeit umschalten, ohne das Profil neu zu speichern |
| Bewertung, Einsätze, Partner | führt die Plattform; nicht über das Profil änderbar |

## Standort

Für Entfernungen gilt der Punkt der Postleitzahl aus dem Ortschaftenverzeichnis, nie ein genauer Standort der Person. Das Profil nimmt keine Koordinaten entgegen; ändern sich Postleitzahl oder Ort, wird der Punkt neu bestimmt.

## Berechtigungen

Nur Konten mit der Rolle «Helfende» haben ein Helferprofil, höchstens eines je Konto, in der Region des Kontos. Andere Rollen erhalten `403`, ohne Anmeldung folgt `401`.

## Schnittstelle

| Methode | Pfad | Zweck |
|---|---|---|
| GET | `/api/helferprofil` | eigenes Profil; `404`, solange keines besteht |
| PUT | `/api/helferprofil` | Profil anlegen oder vollständig ersetzen |
| PUT | `/api/helferprofil/verfuegbarkeit` | «jetzt verfügbar» umschalten |

```
PUT /api/helferprofil
Authorization: Bearer <token>
Content-Type: application/json

{ "typ": 0, "beschreibung": "Hilft beim Einkaufen.", "plz": "3110", "ort": "Münsingen",
  "radiusKm": 8, "skillIds": ["<Kennung der Fähigkeit>"], "adresse": "Dorfstrasse 1",
  "anfahrtChf": null, "stundenansatzChf": null, "nachtzuschlagProzent": null,
  "wochenendzuschlagProzent": null, "mwstPflichtig": false }
```

Antwort `200` mit dem gespeicherten Profil: `id`, `typ`, `betriebsname`, `beschreibung`, `plz`, `ort`, `radiusKm`, `jetztVerfuegbar`, `partner`, `bewertungsschnitt`, `anzahlBewertungen`, `anzahlEinsaetze`, `skills`, `adresse`, die Preisangaben und `preiseErforderlich`. Ungültige Angaben ergeben `400` mit `feld`: `skillIds`, `preise`, `betriebsname`, `plz` oder `ort`.

## Protokoll

Anlegen und Ändern erzeugen die Einträge `Helferprofil.Erstellt` und `Helferprofil.Geaendert` mit der Kennung des Profils, ohne Personendaten.

## Tests

| Test | Prüft |
|---|---|
| Schnittstellentests `tests/api/helferprofil.test.mjs` | Rollen, Anlegen, Lesen, Ändern mit ersetzten Fähigkeiten, Verfügbarkeit, ungültige Angaben ohne Speicherung, Protokoll ohne Personendaten |
| Rauchtest | Profil anlegen, Rolle, Verfügbarkeit, Pflicht der Preisangaben |
