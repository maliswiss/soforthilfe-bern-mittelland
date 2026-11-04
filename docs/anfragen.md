# Anfragen und Kategorien

Hilfesuchende erfassen eine Anfrage per Kategorie, per Freitext oder mit beidem; die Region kommt aus dem Token.

## Schnittstelle

| Schnittstelle | Zweck | Berechtigt |
|---|---|---|
| `GET /api/kategorien` | Kategorien mit Fähigkeiten; `?kostenpflichtig=false` oder `=true` liefert nur kostenlose beziehungsweise kostenpflichtige | alle, auch ohne Anmeldung |
| `POST /api/anfragen` | Anfrage erfassen | Hilfesuchende |
| `GET /api/anfragen` | Liste der Anfragen, neueste zuerst | Hilfesuchende (eigene), Koordination, Mandantenadministration, Plattformbetrieb (Region) |
| `GET /api/anfragen/{id}` | Eine Anfrage; fremde oder regionsfremde Anfragen gelten als nicht vorhanden (`404`) | wie oben |

## Regeln

| Regel | Umsetzung |
|---|---|
| Inhalt | Mindestens eine Kategorie oder ein Freitext (höchstens 2000 Zeichen); beides zusammen ist erlaubt |
| Postleitzahl | Vierstellig, beginnt nicht mit 0 |
| Status | Eine neue Anfrage ist `Offen`; die Verständniskette (Regeln, Klassifikator, Sprachmodell) folgt in Release 2 |
| Kostenlos und kostenpflichtig (Anforderung A12) | Jede Kategorie ist entweder kostenlos oder kostenpflichtig; die Anfrage zeigt, welcher Art sie ist |
| Mehrwertnummern (Anforderung A10) | Telefonnummern mit 0900, 0901 oder 0906 werden abgewiesen, auch als +41 oder 0041 und mit Trennzeichen; gilt bei der Registrierung und beim Nachweis |
