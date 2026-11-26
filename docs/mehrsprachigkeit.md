# Mehrsprachigkeit

Die Oberfläche gibt es auf Deutsch und Französisch. Alle sichtbaren Texte stehen als Daten in Sprachdateien, nie direkt in den Komponenten; Deutsch ist die Grundsprache. Weitere Sprachen und korrigierte Texte importiert der Plattformbetrieb im laufenden Betrieb, ohne neues Deployment.

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
| Umschalter | Kurzzeichen der mitgelieferten und der aktiven importierten Sprachen in der Kopfzeile, die gewählte Sprache ist hervorgehoben |
| Erste Wahl | ohne gespeicherte Wahl die Sprache des Browsers, auch eine importierte, sonst Deutsch |
| Merken | eine Wahl im Umschalter bleibt im Browser gespeichert (lokaler Speicher); ohne Speicher gilt sie bis zum Neuladen |
| Dokument | `lang` der Seite folgt der Sprache, damit Vorleseprogramme richtig aussprechen |
| Zahlen und Daten | Schweizer Format der Sprache (`de-CH`, `fr-CH`) |

Meldungen und Begründungen, die die Schnittstelle als Text liefert (zum Beispiel Fehlermeldungen oder die Begründung der Einordnung), sind auf Deutsch.

## Beschreibung in anderen Sprachen

Die Oberfläche gibt es auf Deutsch und Französisch, die Beschreibung in eigenen Worten ist aber an keine Sprache gebunden. Wer Englisch, Italienisch oder Mundart schreibt, wird trotzdem verstanden; die Stufen der [Verständniskette](verstaendniskette.md) decken dabei unterschiedliche Sprachen ab.

| Stufe | Sprachen |
|---|---|
| [Notfallprüfung](notfallpruefung.md) | Deutsch, Französisch, Englisch, Italienisch und häufige Wendungen in Mundart («schnuufet nümm») |
| Regeln | Deutsch, Französisch, Italienisch und Englisch; Mundart, soweit sie Wörter der Liste enthält (zum Beispiel «verstopft», «Apotheke») |
| Maschinelles Lernen | Deutsch und Französisch, die Sprachen der Trainingsbeispiele |
| Sprachmodell | auch weitere Sprachen, soweit das Modell sie versteht |

Die erkannte Kategorie erscheint in der Sprache der Oberfläche, die Begründung auf Deutsch. In der [Messung](messung-matching.md) wurden alle fünf Texte in Mundart und alle vier in Englisch oder Italienisch richtig eingeordnet oder als Notfall gestoppt.

## Sprachimport ohne Deployment

Der Plattformbetrieb importiert weitere Sprachen oder korrigierte Texte im laufenden Betrieb über die Schnittstelle. Die Texte liegen in der Tabelle `Sprachen` und gelten beim nächsten Abruf, ohne Neustart und ohne neues Image.

| Regel | Umsetzung |
|---|---|
| Format | dasselbe JSON-Objekt wie `de.json`; fehlende Schlüssel zeigt die Oberfläche auf Deutsch |
| Code | zwei Kleinbuchstaben nach ISO 639-1, zum Beispiel `it`; der Import von `de` oder `fr` ergänzt die mitgelieferten Texte |
| Name | eigener Name der Sprache, 2 bis 40 Zeichen, zum Beispiel «Italiano» |
| Prüfung | höchstens 5 Ebenen, 3000 Texte und 1000 Zeichen je Text; Schlüssel aus Buchstaben, Ziffern, `_` und `-`; keine Zeichen `<`, `>`, `@` und `\|`; geschweifte Klammern nur als Platzhalter wie `{plz}` |
| Grösse | Anfrage höchstens 1 MB, sonst Status 413 |
| Ersetzen | ein erneuter Import ersetzt alle Texte der Sprache |
| Ein- und ausschalten | ausgeschaltete Sprachen erscheinen nicht im Umschalter und liefern keine Texte |
| Protokoll | `Sprache.Importiert` mit Anzahl Texte, `Sprache.Aktiviert`, `Sprache.Deaktiviert`, `Sprache.Geloescht` |

Ein fehlerhafter Import antwortet mit `400`, nennt das Feld (`code`, `name` oder `texte`) und bei den Texten die betroffene Stelle, zum Beispiel `start.titel1`. Die Prüfung verhindert, dass ein Text die Oberfläche verändert oder die Ausgabe von vue-i18n stört.

```
PUT /api/sprachen/it
{ "name": "Italiano", "aktiv": true, "texte": { "start": { "titel1": "Aiuto immediato" } } }
```

## Sprachverwaltung

Der Plattformbetrieb verwaltet die Sprachen unter `/sprachen` (Link «Sprachen» in der Kopfzeile).

| Funktion | Umsetzung |
|---|---|
| Übersicht | importierte Sprachen mit Code, Name, Anzahl Texte, Stand und Zeitpunkt der letzten Änderung |
| Vorlage | die deutschen Texte als Datei `sprachvorlage-de.json` zum Übersetzen |
| Import | Code, eigener Name und Sprachdatei; wahlweise sofort im Umschalter |
| Prüfung im Browser | dieselben Regeln wie die Schnittstelle ([`sprachpruefung.js`](../frontend/src/api/sprachpruefung.js)); dazu der Vergleich mit der Vorlage: fehlende Texte (erscheinen auf Deutsch), unbekannte Schlüssel (werden nie angezeigt) und andere Platzhalter |
| Texte ersetzen | übernimmt Code und Name einer Zeile; der Import ersetzt alle Texte |
| Ein- und ausschalten, löschen | sofort wirksam; Löschen mit Rückfrage |

Beim Start lädt die Oberfläche die Liste der aktiven Sprachen und die Texte der gewählten importierten Sprache. Importierte Texte zu `de` oder `fr` liegen über den mitgelieferten. Nach jeder Änderung in der Sprachverwaltung lädt die Oberfläche Liste und Texte neu; andere Browser sehen die Änderung beim nächsten Laden der Seite. Ist die Schnittstelle nicht erreichbar, bleiben Deutsch und Französisch.

## Tests

| Test | Prüft |
|---|---|
| Oberflächentests `tests/oberflaeche/sprache.spec.mjs` | Wechsel auf Französisch mit Texten, Kategorienamen, Sprache des Dokuments und Fussbereich; Wahl bleibt nach dem Neuladen; Rückwechsel; Sprache des Browsers ohne gespeicherte Wahl; Wechsel mitten im Formular mit erhaltenen Eingaben und Meldungen in der gewählten Sprache |
| Schnittstellentests `tests/api/anfragen.test.mjs` | Anfragen enthalten den Code der Kategorie |
| Schnittstellentests `tests/api/sprachen.test.mjs` | Zugriff je Rolle; Import gilt sofort; erneuter Import ersetzt die Texte; ausschalten, löschen und Protokoll; ungültige Dateien mit Feld und Stelle; Grenzen für Anzahl, Länge, Ebenen und Schlüssel; über 1 MB Status 413; ein ungültiger erneuter Import lässt die bisherigen Texte unverändert |
| Oberflächentests `tests/oberflaeche/sprachen.spec.mjs` | Prüfung im Browser mit betroffener Stelle; Import mit fehlenden und unbekannten Texten; neue Sprache sofort im Umschalter und für Besuchende; deutsche Texte als Ersatz; ausschalten und löschen; Zugang nur für den Plattformbetrieb |
| Einheitentests `SprachdateiTests` | Code, Name, Ebenen, Schlüssel, Längen, unzulässige Zeichen und Platzhalter |
