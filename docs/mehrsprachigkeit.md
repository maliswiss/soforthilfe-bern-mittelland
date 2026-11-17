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

## Tests

| Test | Prüft |
|---|---|
| Oberflächentests `tests/oberflaeche/sprache.spec.mjs` | Wechsel auf Französisch mit Texten, Kategorienamen, Sprache des Dokuments und Fussbereich; Wahl bleibt nach dem Neuladen; Rückwechsel; Sprache des Browsers ohne gespeicherte Wahl |
| Schnittstellentests `tests/api/anfragen.test.mjs` | Anfragen enthalten den Code der Kategorie |
