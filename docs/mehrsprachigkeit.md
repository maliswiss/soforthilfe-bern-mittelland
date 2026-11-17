# Mehrsprachigkeit

Die Oberfläche gibt es auf Deutsch und Französisch. Alle sichtbaren Texte stehen als Daten in Sprachdateien, nie direkt in den Komponenten; Deutsch ist die Grundsprache.

## Sprachdateien

| Datei | Inhalt |
|---|---|
| [`frontend/src/sprachen/de.json`](../frontend/src/sprachen/de.json) | Grundsprache Deutsch, alle Texte |
| [`frontend/src/sprachen/fr.json`](../frontend/src/sprachen/fr.json) | Französisch, gleiche Schlüssel und gleiche Platzhalter wie Deutsch |

Die Dateien sind nach Seiten gegliedert (`start`, `erfassen`, `profil` …). Platzhalter stehen in geschweiften Klammern, zum Beispiel `{plz}`, und sind in jeder Sprache gleich. Fehlt ein Text in einer Sprache, erscheint der deutsche.

Die Bereiche `kategorien` und `faehigkeiten` enthalten die Namen der zehn Kategorien und ihrer Fähigkeiten, je unter dem Code aus der Schnittstelle (zum Beispiel `sanitaer-elektro`). Die Schnittstelle liefert die Namen auf Deutsch; die Oberfläche zeigt über den Code den Namen der gewählten Sprache ([`datentexte.js`](../frontend/src/api/datentexte.js)). Anfragen enthalten dafür `kategorieCode`.

## Wahl der Sprache

| Regel | Umsetzung |
|---|---|
| Umschalter | Kurzzeichen `DE` und `FR` in der Kopfzeile, die gewählte Sprache ist hervorgehoben |
| Erste Wahl | ohne gespeicherte Wahl die Sprache des Browsers, sonst Deutsch |
| Merken | die Wahl bleibt im Browser gespeichert (lokaler Speicher); ohne Speicher gilt sie bis zum Neuladen |
| Dokument | `lang` der Seite folgt der Sprache, damit Vorleseprogramme richtig aussprechen |
| Zahlen und Daten | Schweizer Format der Sprache (`de-CH`, `fr-CH`) |

Meldungen und Begründungen, die die Schnittstelle als Text liefert (zum Beispiel Fehlermeldungen oder die Begründung der Einordnung), sind auf Deutsch.

## Sprachimport ohne Deployment

Der Plattformbetrieb importiert weitere Sprachen oder korrigierte Texte im laufenden Betrieb über die Schnittstelle. Die Texte liegen in der Tabelle `Sprachen` und gelten beim nächsten Abruf, ohne Neustart und ohne neues Image.

| Regel | Umsetzung |
|---|---|
| Format | dasselbe JSON-Objekt wie `de.json`; fehlende Schlüssel zeigt die Oberfläche auf Deutsch |
| Code | zwei Kleinbuchstaben nach ISO 639-1, zum Beispiel `it`; der Import von `de` oder `fr` ergänzt die mitgelieferten Texte |
| Name | eigener Name der Sprache, 2 bis 40 Zeichen, zum Beispiel «Italiano» |
| Prüfung | höchstens 5 Ebenen, 3000 Texte und 1000 Zeichen je Text; Schlüssel aus Buchstaben, Ziffern, `_` und `-`; keine Zeichen `<`, `>`, `@` und `\|`; geschweifte Klammern nur als Platzhalter wie `{plz}` |
| Grösse | Anfrage höchstens 1 MB |
| Ersetzen | ein erneuter Import ersetzt alle Texte der Sprache |
| Ein- und ausschalten | ausgeschaltete Sprachen erscheinen nicht im Umschalter und liefern keine Texte |
| Protokoll | `Sprache.Importiert` mit Anzahl Texte, `Sprache.Aktiviert`, `Sprache.Deaktiviert`, `Sprache.Geloescht` |

Ein fehlerhafter Import antwortet mit `400`, nennt das Feld (`code`, `name` oder `texte`) und bei den Texten die betroffene Stelle, zum Beispiel `start.titel1`. Die Prüfung verhindert, dass ein Text die Oberfläche verändert oder die Ausgabe von vue-i18n stört.

```
PUT /api/sprachen/it
{ "name": "Italiano", "aktiv": true, "texte": { "start": { "titel1": "Aiuto immediato" } } }
```

## Tests

| Test | Prüft |
|---|---|
| Oberflächentests `tests/oberflaeche/sprache.spec.mjs` | Wechsel auf Französisch mit Texten, Kategorienamen, Sprache des Dokuments und Fussbereich; Wahl bleibt nach dem Neuladen; Rückwechsel; Sprache des Browsers ohne gespeicherte Wahl |
| Schnittstellentests `tests/api/anfragen.test.mjs` | Anfragen enthalten den Code der Kategorie |
| Schnittstellentests `tests/api/sprachen.test.mjs` | Zugriff je Rolle; Import gilt sofort; erneuter Import ersetzt die Texte; ausschalten, löschen und Protokoll; ungültige Dateien mit Feld und Stelle |
| Komponententests `SprachdateiTests` | Code, Name, Ebenen, Schlüssel, Längen, unzulässige Zeichen und Platzhalter |
