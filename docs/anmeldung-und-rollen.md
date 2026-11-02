# Anmeldung und Rollen

## Anmeldung

| Schnittstelle | Zweck |
|---|---|
| `POST /api/konto/registrieren` | Konto anlegen, in der Region aus dem Kopf `X-Region`; selbst registrieren können sich nur Hilfesuchende und Helfende |
| `POST /api/konto/anmelden` | Anmeldung mit E-Mail-Adresse und Passwort; liefert ein Token (JWT) |
| `GET /api/konto/ich` | Eigenes Konto, nur mit Token |

| Thema | Umsetzung |
|---|---|
| Passwörter | Nur als Hash gespeichert: PBKDF2 mit HMAC-SHA512 und Salz je Passwort (Verfahren von ASP.NET Core Identity); mindestens 10 Zeichen |
| Token | JWT mit HS256, gültig 60 Minuten; Schlüssel, Herausgeber und Zielgruppe aus `JWT_KEY`, `JWT_ISSUER` und `JWT_AUDIENCE` |
| Inhalt des Tokens | Nur Kontokennung (`sub`), Rolle (`role`) und Region (`region`), keine Namen und keine Adressen |
| Schlüssel | Ohne `JWT_KEY` mit mindestens 32 Zeichen startet die Anwendung nicht |
| Fehlversuche | Gleiche Meldung für unbekannte Adresse und falsches Passwort; auch ohne Konto wird ein Hash geprüft, damit die Antwortzeit nichts verrät |
| Region | Angemeldet gilt die Region aus dem Token; ein abweichender Kopf `X-Region` wird mit `403` abgewiesen |

## Rollen und Berechtigungen

| Schnittstelle | Hilfesuchende | Helfende | Koordination | Mandantenadministration | Plattformbetrieb |
|---|---|---|---|---|---|
| `GET /api/regionen`, `GET /api/regionen/aktuell` | ja | ja | ja | ja | ja |
| `GET /api/konto/ich` | ja | ja | ja | ja | ja |
| `GET /api/konten` | nein | nein | eigene Region | eigene Region | alle Regionen |
| `PUT /api/konten/{id}/rolle` | nein | nein | nein | eigene Region | alle Regionen |
| `POST /api/regionen` | nein | nein | nein | nein | ja |

| Regel | Umsetzung |
|---|---|
| Prüfung der Rolle | An der Schnittstelle mit `[Authorize(Roles = …)]`; ohne Token `401`, mit falscher Rolle `403` |
| Prüfung der Region | Im Datenzugriff (siehe [mandanten.md](mandanten.md)); Konten einer fremden Region gelten als nicht vorhanden (`404`) |
| Vergebbare Rollen | Hilfesuchende, Helfende, Koordination und Mandantenadministration; der Plattformbetrieb ist an keine Region gebunden und wird nicht über die Schnittstelle vergeben |
| Plattformbetrieb | Arbeitet regionsübergreifend; ein Kopf `X-Region` wird dann nicht ausgewertet |
| Testkonten | Die fiktiven Testkonten erhalten nur ein Passwort, wenn `TESTDATEN_PASSWORT` in der Umgebung gesetzt ist |
