# Schnittstelle Release 1

Die Schnittstelle ist eine REST-API unter `/api`. Sie spricht JSON; Feldnamen beginnen mit einem Kleinbuchstaben (`eMail`, `kategorieId`). In der Containerplattform ist sie über den Lastverteiler unter `https://localhost/api` erreichbar, in der Entwicklung unter `http://localhost:5000/api`.

## Endpunkte

| Methode | Pfad | Zugriff | Zweck |
|---|---|---|---|
| GET | `/api/info` | offen | Name und Version |
| GET | `/api/mandanten` | offen | aktive Regionen für Anmeldung und Registrierung |
| POST | `/api/mandanten` | Plattformbetrieb | neue Region im laufenden Betrieb anlegen |
| POST | `/api/auth/registrierung` | offen | Konto in einer Region anlegen, liefert gleich ein Token |
| POST | `/api/auth/anmeldung` | offen | Anmeldung in der Region des Kontos, liefert ein Token |
| GET | `/api/auth/ich` | angemeldet | Angaben zum eigenen Konto |
| GET | `/api/kategorien` | offen | Kategorien der Startseite; `?bezahlpflichtig=false` oder `=true` filtert |
| GET | `/api/skills` | offen | Fähigkeiten, die Helfende anbieten können |
| GET | `/api/orte?plz=` | offen | Ortschaften zu einer Postleitzahl mit Gemeinde, Kanton, Region und Hinweis auf mehrere Gemeinden |
| POST | `/api/orte/umgebung` | offen | Postleitzahl und Ort zum freigegebenen Standort; Koordinaten nur im Rumpf, nicht gespeichert |
| POST | `/api/anfragen/verstehen` | offen | Verständnis-Schritt vor dem Erfassen: zuerst immer die Notfallprüfung, danach die Einordnung in eine Kategorie ([verstaendniskette.md](verstaendniskette.md)) |
| POST | `/api/anfragen` | Hilfesuchende | Anfrage erfassen; bei einem gesundheitlichen Notfall `451` statt Speicherung |
| GET | `/api/anfragen/meine` | Hilfesuchende | eigene Anfragen, neueste zuerst |
| GET | `/api/anfragen` | Hilfesuchende (eigene), Koordination, Mandantenadministration, Plattformbetrieb | Anfragen der Region |
| GET | `/api/anfragen/{id}` | wie oben | einzelne Anfrage; fremde Anfragen liefern `404` |
| GET | `/api/konten` | Koordination, Mandantenadministration, Plattformbetrieb | Konten der Region, ohne Telefonnummern |
| PUT | `/api/konten/{id}/rolle` | Mandantenadministration, Plattformbetrieb | Rolle eines Kontos ändern |
| GET | `/api/helferprofil` | Helfende | Eigenes Helferprofil |
| PUT | `/api/helferprofil` | Helfende | Helferprofil anlegen oder ersetzen ([helferprofil.md](helferprofil.md)) |
| PUT | `/api/helferprofil/verfuegbarkeit` | Helfende | «jetzt verfügbar» umschalten |
| POST | `/api/verifizierungen` | Helfende | Nachweis ohne Ausweisdokumente einreichen: UID-Nummer, Handelsregister, Telefonnummer, E-Mail-Adresse des Kontos oder Referenz ([verifizierung.md](verifizierung.md)) |
| GET | `/api/verifizierungen/meine` | Helfende | Eigene Nachweise mit Stand der Prüfung |
| GET | `/api/verifizierungen/offen` | Koordination | Offene Nachweise der eigenen Region mit Angaben zum Profil, älteste zuerst |
| POST | `/api/verifizierungen/{id}/entscheid` | Koordination | Nachweis freigeben oder ablehnen; fremde Region `404`, bereits entschieden `409` |
| POST | `/api/preise/richtpreis` | offen | Richtpreis für die erste Stunde zu bis zu zehn Zeitpunkten ([preise.md](preise.md)) |
| GET | `/api/protokoll` | Koordination, Mandantenadministration, Plattformbetrieb | Neueste Protokolleinträge der eigenen Region, für den Plattformbetrieb aller Regionen; Filter `aktion`, Anzahl `anzahl` (Standard 100, höchstens 500) |
| GET | `/health/live` | intern | Prozess läuft |
| GET | `/health/ready` | intern | Anwendung bereit, Datenbank erreichbar |

Rollen und Berechtigungen im Detail: [anmeldung-und-rollen.md](anmeldung-und-rollen.md). Regionen und Datentrennung: [mandanten.md](mandanten.md).

## Registrierung und Anmeldung

```
POST /api/auth/registrierung
Content-Type: application/json

{ "eMail": "anna@beispiel.invalid", "passwort": "<mindestens-10-zeichen>", "anzeigename": "Anna Beispiel",
  "telefon": "031 123 45 67", "kontoart": 0, "mandant": "bern-mittelland" }
```

| Feld | Regel |
|---|---|
| `kontoart` | `0` Hilfe suchen (Rolle Hilfesuchende), `1` Hilfe anbieten (Rolle Helfende); Standard `0` |
| `mandant` | Schlüssel der Region; die Oberfläche bestimmt ihn aus der Postleitzahl (`GET /api/orte`); Standard `bern-mittelland` |
| `telefon` | freiwillig; Schweizer Nummer, Mehrwertnummern (0900, 0901, 0906) in jeder Schreibweise abgelehnt (Anforderung A10) |
| `passwort` | mindestens 10 Zeichen |

`POST /api/auth/anmeldung` erwartet `eMail` und `passwort`; ohne `mandant` gilt die Region des Kontos. Beide Aufrufe antworten mit `200`:

```
{ "token": "<jwt>", "gueltigBis": "2026-11-05T10:00:00+00:00",
  "benutzer": { "id": "…", "eMail": "anna@beispiel.invalid", "anzeigename": "Anna Beispiel", "telefon": "0311234567",
                "rollen": ["Hilfesuchende"], "hatHelferProfil": false,
                "mandantSchluessel": "bern-mittelland", "mandantName": "Soforthilfe Bern/Mittelland" } }
```

Wird `mandant` angegeben und passt nicht zum Konto, folgt dieselbe Antwort `401` wie bei einem falschen Passwort.

## Anfrage erfassen

```
POST /api/anfragen
Authorization: Bearer <token>
Content-Type: application/json

{ "kategorieId": "…", "freitext": "Der Wasserhahn tropft stark.", "plz": "3011", "ort": "Bern",
  "adresse": "Beispielweg 1", "zeitfenster": "heute zwischen 17 und 19 Uhr", "prioritaet": 1 }
```

| Feld | Regel |
|---|---|
| `kategorieId`, `freitext` | mindestens eines von beiden; Freitext höchstens 2000 Zeichen |
| `plz` | vierstellig, beginnt nicht mit 0, steht im Ortschaftenverzeichnis und berührt eine Gemeinde der Region des Kontos |
| `ort` | Pflicht, höchstens 120 Zeichen |
| `adresse`, `zeitfenster` | freiwillig |
| `prioritaet` | `0` tief, `1` mittel (Standard), `2` hoch |

Antwort `201` mit der Anfrage: `id`, `kategorieId`, `kategorieBezeichnung`, `freitext`, `plz`, `ort`, `adresse`, `zeitfenster`, `prioritaet`, `status`, `erstelltAm` und `bezahlpflichtig`.

## Orte und Postleitzahlen

```
GET /api/orte?plz=3174
```

Antwort `200`, je Ortschaft ein Eintrag; bei unbekannter Postleitzahl eine leere Liste:

```
[ { "plz": "3174", "ort": "Thörishaus", "gemeinde": "Köniz", "kanton": "BE",
    "plzUeberMehrereGemeinden": true, "region": "bern-mittelland" } ]
```

`gemeinde` ist die Gemeinde mit dem grössten Anteil der Adressen, `region` die zuständige Region oder `null` ausserhalb der Regionen. Eine Postleitzahl, die nicht vierstellig ist oder mit 0 beginnt, ergibt `400` mit `"feld": "plz"`.

```
POST /api/orte/umgebung
Content-Type: application/json

{ "breitengrad": 46.9466, "laengengrad": 7.444 }
```

Antwort `200` mit einem Eintrag wie oben (hier `3011` Bern), ohne Koordinaten. Liegt keine Ortschaft näher als 15 km, folgt `404`; Breitengrad ausserhalb von −90 bis 90, Längengrad ausserhalb von −180 bis 180 oder fehlende Werte ergeben `400`. `GET` auf diesen Pfad ergibt `405`, damit Koordinaten nie in einer Adresse stehen. Details: [orte-und-standort.md](orte-und-standort.md).

## Notfall-Stopp

```
POST /api/anfragen/verstehen
Content-Type: application/json

{ "freitext": "Mein Mann ist bewusstlos auf dem Boden." }
```

Antwort `200`:

```
{ "notfall": true, "notfallGrund": "Erkanntes Schlüsselwort: bewusstlos",
  "notfallHinweis": "Bitte sofort 144 anrufen. Diese Plattform vermittelt keine medizinischen Leistungen." }
```

Ohne Hinweis auf einen Notfall sind `notfall` falsch und die beiden Texte `null`; dazu kommt die Einordnung mit Kategorie, Weg, Sicherheit und Begründung ([verstaendniskette.md](verstaendniskette.md)). Bei einem Notfall bleibt die Einordnung leer. Enthält der Freitext beim Erfassen (`POST /api/anfragen`) einen Notfallhinweis, wird die Anfrage nicht gespeichert; die Antwort ist `451` mit `notfall`, `hinweis` und `grund`. Der Status 451 steht dafür, dass die Plattform aus rechtlichen Gründen keine medizinischen Leistungen vermittelt. Regeln und Testfälle: [notfallpruefung.md](notfallpruefung.md).

Jeder Notfall-Stopp wird ohne Personendaten protokolliert:

```
GET /api/protokoll?aktion=Notfall.Stopp
Authorization: Bearer <token>
```

Antwort `200`:

```
[ { "id": 42, "aktion": "Notfall.Stopp", "objekt": "Verstaendnis", "objektId": null,
    "detail": "Erkanntes Schlüsselwort: bewusstlos", "zeitpunkt": "2026-11-09T14:03:11+00:00",
    "region": "bern-mittelland" } ]
```

## Zahlenwerte

| Feld | Werte |
|---|---|
| `status` | `1` offen, `2` Vorschläge vorhanden, `3` angenommen, `4` unterwegs, `5` erledigt, `6` abgebrochen, `7` wegen Notfall gestoppt |
| `prioritaet` | `0` tief, `1` mittel, `2` hoch |

Eine neue Anfrage ist offen (`1`). Die weiteren Werte setzen Matching, Zusage und Abschluss.

## Fehlerantworten

Jeder Fehler ist ein Problem-Dokument (`application/problem+json`) mit zwei zusätzlichen Feldern:

| Feld | Inhalt |
|---|---|
| `fehler` | Text für die Oberfläche, zum Beispiel «Diese E-Mail-Adresse ist bereits registriert.» |
| `feld` | betroffenes Eingabefeld, zum Beispiel `telefon` oder `plz`, sofern es eines gibt |

```
{ "status": 400, "title": "Kostenpflichtige Telefonnummer",
  "detail": "Nummern mit 0900 (sowie 0901 und 0906) sind kostenpflichtig und werden nicht akzeptiert.",
  "fehler": "Nummern mit 0900 (sowie 0901 und 0906) sind kostenpflichtig und werden nicht akzeptiert.",
  "feld": "telefon" }
```

| Status | Bedeutung |
|---|---|
| `400` | Eingabe ungültig, `feld` nennt das Feld |
| `401` | nicht angemeldet, Token ungültig oder Anmeldung fehlgeschlagen |
| `403` | Rolle fehlt oder Kopf `X-Region` passt nicht zum Konto |
| `404` | nicht vorhanden, auch für Daten einer fremden Region |
| `405` | Methode nicht erlaubt, zum Beispiel Koordinaten per `GET` |
| `409` | E-Mail-Adresse oder Region besteht bereits |
| `415` | Medientyp nicht unterstützt, zum Beispiel ein Upload statt JSON |
| `451` | Notfall-Stopp: Hinweis auf einen gesundheitlichen Notfall, die Anfrage wird nicht vermittelt |
| `500` | interner Fehler, ohne technische Details, mit Verfolgungsnummer |

## Sicherheitsverhalten

| Nr. | Regel | Umsetzung |
|---|---|---|
| 1 | Passwörter nie im Klartext | PBKDF2 mit HMAC-SHA512 und Salz je Passwort |
| 2 | Datensparsames Token | nur Kontokennung, Rolle und Region |
| 3 | Gleiche Antwort bei Fehlversuchen | unbekannte Adresse, falsches Passwort und fremde Region: `401` mit demselben Text, gleiche Antwortzeit |
| 4 | Mandantentrennung (Anforderung A18) | jede Abfrage filtert über die Region aus dem Token; fremde Daten liefern `404` |
| 5 | Rollenprüfung | Rolle als Anspruch im Token, Prüfung je Endpunkt |
| 6 | Keine Mehrwertnummern (Anforderung A10) | 0900, 0901 und 0906 auch als +41 oder 0041 und mit Trennzeichen abgelehnt, Feld `telefon` |
| 7 | Kostenlos und kostenpflichtig getrennt (Anforderung A12) | jede Kategorie ist entweder kostenlos oder kostenpflichtig; die Anfrage meldet `bezahlpflichtig` |
| 8 | Genaue Adresse getrennt | `adresse` getrennt von `plz` und `ort` gespeichert |

## Prüfung

Die Schnittstellentests in [`tests/api`](../tests/api) und der Rauchtest in [`tests/rauchtest/schnittstelle.sh`](../tests/rauchtest/schnittstelle.sh) prüfen jeden Endpunkt dieser Liste in der CI.
