# Mandantenfähigkeit

Jede Region ist ein Mandant. Konten, Helferprofile und Anfragen gehören zu genau einer Region und sind strikt von den Daten anderer Regionen getrennt.

## Festlegen der Region

| Fall | Region der Anfrage |
|---|---|
| Angemeldet | aus dem Token; ein abweichender Kopf `X-Region` wird mit `403` abgewiesen |
| Plattformbetrieb | alle Regionen |
| Ohne Anmeldung | aus dem Kopf `X-Region` mit der Kennung der Region, zum Beispiel `X-Region: bern` |
| Unbekannte oder inaktive Region | Antwort `404`, es werden keine Daten gelesen |
| Ohne Kopf | keine Region; regionsbezogene Daten sind nicht sichtbar |

Die aktiven Regionen liefert `GET /api/regionen`, die Region der laufenden Anfrage `GET /api/regionen/aktuell`.

## Technische Umsetzung

| Ebene | Schutz |
|---|---|
| Schnittstelle | `MandantenMiddleware` legt die Region zu Beginn jeder Anfrage fest; sie kann innerhalb der Anfrage nicht mehr wechseln |
| Lesen | Globale Filter von Entity Framework Core liefern nur Zeilen der Region der Anfrage |
| Schreiben | Vor dem Speichern prüft der Datenbankkontext jede neue, geänderte oder gelöschte Zeile; Zeilen einer anderen Region lösen einen Fehler aus und werden nicht gespeichert |
| Datenbank | Jede regionsbezogene Tabelle hat einen Fremdschlüssel auf `Regionen`; Regionen mit Daten lassen sich nicht löschen |

Regionen sind Daten und keine Konfiguration. Eine neue Region braucht deshalb weder einen Neustart noch ein Deployment.

## Neue Region einrichten

Der Plattformbetrieb legt eine Region über die Schnittstelle an; sie ist sofort nutzbar.

```
POST /api/regionen
Authorization: Bearer <token-des-plattformbetriebs>
Content-Type: application/json

{ "kennung": "thun", "name": "Thun" }
```

Die Kennung besteht aus Kleinbuchstaben, Ziffern und Bindestrichen (2 bis 50 Zeichen) und ist eindeutig. Antwort `201` mit der neuen Region, `409`, wenn die Kennung schon vergeben ist.
