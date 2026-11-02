# Mandantenfähigkeit

Jede Region ist ein Mandant. Konten, Helferprofile und Anfragen gehören zu genau einer Region und sind strikt von den Daten anderer Regionen getrennt.

## Festlegen der Region

| Fall | Region der Anfrage |
|---|---|
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
