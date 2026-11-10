# Mandantenfähigkeit

Jede Region ist ein Mandant. Konten, Helferprofile und Anfragen gehören zu genau einer Region und sind strikt von den Daten anderer Regionen getrennt.

## Festlegen der Region

| Fall | Region der Anfrage |
|---|---|
| Angemeldet | aus dem Token; ein abweichender Kopf `X-Region` wird mit `403` abgewiesen |
| Plattformbetrieb | alle Regionen |
| Registrierung | aus dem Feld `mandant`, zum Beispiel `"mandant": "bern-mittelland"`; die Oberfläche bestimmt es aus der Postleitzahl |
| Anmeldung | aus dem Konto; ein angegebenes `mandant` muss zur Region des Kontos passen |
| Ohne Anmeldung | aus dem Kopf `X-Region` mit dem Schlüssel der Region, zum Beispiel `X-Region: bern-mittelland` |
| Unbekannte oder inaktive Region | Antwort `404`, es werden keine Daten gelesen |
| Ohne Kopf | keine Region; regionsbezogene Daten sind nicht sichtbar |

Die aktiven Regionen liefert `GET /api/mandanten` mit `schluessel`, `name` und `standardsprache`. Mit den Stammdaten kommt die Region `bern-mittelland`; die Testdaten legen zusätzlich `biel-seeland` an.

## Region aus der Postleitzahl

Jede Region nennt ihre Gemeinden als BFS-Gemeindenummern (`Regionen.Gemeinden`); eine Gemeinde gehört zu höchstens einer Region. Das Ortschaftenverzeichnis verbindet jede Postleitzahl mit ihren Gemeinden, daraus folgt die Region ([orte-und-standort.md](orte-und-standort.md)).

| Region | Gemeinden | Gebiet |
|---|---|---|
| `bern-mittelland` | 74 | Verwaltungskreis Bern-Mittelland |
| `biel-seeland` (Testdaten) | 61 | Verwaltungskreise Biel/Bienne (19) und Seeland (42) |

Eine Anfrage wird nur mit einer Postleitzahl angenommen, die mindestens eine Gemeinde der Region des Kontos berührt. Eine Region ohne Gemeinden schränkt die Postleitzahl nicht ein.

## Technische Umsetzung

| Ebene | Schutz |
|---|---|
| Schnittstelle | `MandantenMiddleware` legt die Region zu Beginn jeder Anfrage fest; sie kann innerhalb der Anfrage nicht mehr wechseln |
| Lesen | Globale Filter von Entity Framework Core liefern nur Zeilen der Region der Anfrage |
| Schreiben | Vor dem Speichern prüft der Datenbankkontext jede neue, geänderte oder gelöschte Zeile; Zeilen einer anderen Region lösen einen Fehler aus und werden nicht gespeichert |
| Datenbank | Jede regionsbezogene Tabelle hat einen Fremdschlüssel auf `Regionen`; Regionen mit Daten lassen sich nicht löschen |
| Protokoll | Einträge tragen die Region der Anfrage; ein Notfall-Stopp ohne Anmeldung wird ohne Region gespeichert und ist nur für den Plattformbetrieb sichtbar |

Regionen sind Daten und keine Konfiguration. Eine neue Region braucht deshalb weder einen Neustart noch ein Deployment.

## Neue Region einrichten

Der Plattformbetrieb legt eine Region über die Schnittstelle an; sie ist sofort nutzbar.

```
POST /api/mandanten
Authorization: Bearer <token-des-plattformbetriebs>
Content-Type: application/json

{ "schluessel": "thun", "name": "Soforthilfe Thun", "gemeinden": [939, 942] }
```

Der Schlüssel besteht aus Kleinbuchstaben, Ziffern und einzelnen Bindestrichen (2 bis 50 Zeichen) und ist eindeutig. `gemeinden` ist freiwillig und enthält BFS-Gemeindenummern (1 bis 9999). Antwort `201` mit der neuen Region, `409` mit `"feld": "schluessel"`, wenn der Schlüssel schon vergeben ist, und `409` mit `"feld": "gemeinden"`, wenn eine Gemeinde bereits zu einer anderen Region gehört.
