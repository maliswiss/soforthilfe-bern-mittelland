# Messung des Matchings

Die Messung zeigt, ob das Matching zu typischen Anliegen geeignete und begründete Vorschläge liefert. Die Testfälle und ihre erwarteten Ergebnisse sind vor der Messung festgelegt, in [`tests/messprotokoll/testfaelle.json`](../tests/messprotokoll/testfaelle.json).

## Ziel und Bewertung

| Punkt | Festlegung |
|---|---|
| Ziel | mindestens 16 von 20 Fällen mit geeignetem, begründetem Vorschlag |
| Fall erfüllt | erkannte Kategorie wie erwartet, Rang 1 unter den Helfenden der Spalte «Soll», Vorschlag mit Begründung; im Einzelnen die Kriterien K1 bis K6 unter [Messskript](#messskript) |
| Notfälle | alle vier werden gestoppt (Hinweis auf 144) und protokolliert, ohne Anfrage und ohne Vorschlag |
| Antwortzeit | je Fall höchstens 7 s vom Absenden bis zu den Vorschlägen, mit dem Sprachmodell auf dem Arbeitsgerät |
| Umgebung | frische Datenbank mit Testdaten und den Messdaten unten; echtes Sprachmodell auf dem Arbeitsgerät; Messumgebung im Messprotokoll |
| Region | Bern/Mittelland |

Die Texte stehen weder in den Trainingsdaten noch im Kalibrierungssatz der Verständniskette. Sie sind deutsch oder französisch, wie Anfragen in der Region.

## Testfälle

| Nr. | Sprache | Ort | Anliegen | Erwartete Kategorie | Soll auf Rang 1 |
|---|---|---|---|---|---|
| 1 | de | 3011 Bern | «Ich habe mich ausgesperrt, der Schlüssel steckt innen.» | Schlüsseldienst | Schlüsseldienst Fuchs GmbH |
| 2 | fr | 3011 Bern | «Je ne peux plus entrer chez moi, la porte s'est fermée toute seule.» | Schlüsseldienst | Schlüsseldienst Fuchs GmbH |
| 3 | de | 3014 Bern | «Der Wasserhahn in der Küche tropft stark und unter der Spüle ist alles nass.» | Sanitär, Heizung, Elektro | Wyss Sanitär und Heizung |
| 4 | fr | 3006 Bern | «Plus de courant dans la cuisine, le disjoncteur saute sans arrêt.» | Sanitär, Heizung, Elektro | Elektro Schneider AG |
| 5 | de | 3018 Bern | «Mein Auto springt nicht an, die Batterie ist leer. Ich stehe auf dem Parkplatz.» | Panne und Abschleppdienst | Abschleppdienst Aebi |
| 6 | fr | 3027 Bern | «Ma voiture est en panne sur le parking, la batterie est à plat.» | Panne und Abschleppdienst | Abschleppdienst Aebi |
| 7 | de | 3012 Bern | «Mein Laptop startet nicht mehr und ich brauche ihn morgen für die Arbeit.» | IT- und Handy-Soforthilfe | Nina Hofer |
| 8 | fr | 3012 Bern | «Mon imprimante ne fonctionne plus avec l'ordinateur, je n'arrive pas à imprimer.» | IT- und Handy-Soforthilfe | Nina Hofer |
| 9 | de | 3007 Bern | «Nach dem Wasserschaden muss der Keller gründlich gereinigt und getrocknet werden.» | Reinigung nach Schäden | Rossi Reinigungen |
| 10 | de | 3098 Köniz | «Am Samstag muss ein Kühlschrank in den Keller getragen werden.» | Transport- und Umzugshilfe | Daniel Moser |
| 11 | fr | 3005 Bern | «Pouvez-vous m'aider à transporter une armoire jusqu'à la déchetterie ?» | Transport- und Umzugshilfe | Claudia Zurbrügg |
| 12 | fr | 3013 Bern | «Qui pourrait m'apporter du pain et du lait ? Je ne peux pas sortir.» | Lieferung Apotheke und Einkauf | Jonas Berger |
| 13 | de | 3098 Köniz | «Ich suche eine Begleitung zum Arzttermin am Donnerstag, ich gehe nicht gern allein.» | Begleitung und Orientierung | Ruth Gerber |
| 14 | de | 3006 Bern | Kategorie gewählt: Kleine Reparatur | Kleine Reparatur | Marco Bieri |
| 15 | de | 3072 Ostermundigen | «Die Wohnungstür geht nicht mehr auf, ich stehe draussen.» | Schlüsseldienst | Schlüsseldienst Fuchs GmbH |
| 16 | fr | 3073 Gümligen | «Mon ordinateur a un virus, il est devenu très lent.» | IT- und Handy-Soforthilfe | Kunz IT-Support |
| 17 | de | 3076 Worb | «Das WLAN zu Hause funktioniert nicht mehr, der Router blinkt rot.» | IT- und Handy-Soforthilfe | Kunz IT-Support |
| 18 | de | 3110 Münsingen | «Die Heizung ist ausgefallen und es wird kalt in der Wohnung.» | Sanitär, Heizung, Elektro | Haustechnik Aare GmbH |
| 19 | fr | 3084 Wabern | «La baignoire déborde, l'eau ne s'arrête plus.» | Sanitär, Heizung, Elektro | Elektro Schneider AG |
| 20 | de | 3052 Zollikofen | «Wer könnte diese Woche mit meinem Hund spazieren gehen? Ich bin krank im Bett.» | Nachbarschaftshilfe | Andrea Lüthi |

## Notfälle

| Nr. | Sprache | Text |
|---|---|---|
| N1 | de | «Mein Vater hat plötzlich starke Brustschmerzen.» |
| N2 | de | «Meine Nachbarin ist gestürzt und bewusstlos.» |
| N3 | fr | «Mon mari ne respire plus normalement.» |
| N4 | fr | «Elle a fait un AVC, elle ne parle plus.» |

## Messdaten

Fiktive Helfende, die vor der Messung über die Schnittstelle angelegt und von der Koordination verifiziert werden. Alle sind jetzt verfügbar, ohne Bewertungen und ohne Einsätze; die Reihenfolge ergibt sich darum aus der Distanz. Aus den Testdaten kommt Bruno Muster (3012 Bern, Sanitär und Kleine Reparatur) dazu.

| Name | Typ | Ort | Einsatzradius | Fähigkeiten | Preis |
|---|---|---|---|---|---|
| Schlüsseldienst Fuchs GmbH | Betrieb | 3011 Bern | 20 km | Schlüsseldienst, Kleine Reparatur | CHF 60 + 120/h |
| Wyss Sanitär und Heizung | Betrieb | 3014 Bern | 15 km | Sanitär, Heizung, Elektro | CHF 45 + 110/h |
| Elektro Schneider AG | Betrieb | 3006 Bern | 15 km | Sanitär, Heizung, Elektro | CHF 50 + 115/h |
| Haustechnik Aare GmbH | Betrieb | 3110 Münsingen | 15 km | Sanitär, Heizung, Elektro | CHF 40 + 105/h |
| Abschleppdienst Aebi | Betrieb | 3018 Bern | 25 km | Panne und Abschleppdienst | CHF 80 + 130/h |
| Kunz IT-Support | Betrieb | 3072 Ostermundigen | 15 km | IT- und Handy-Soforthilfe | CHF 30 + 90/h |
| Nina Hofer | Privatperson | 3012 Bern | 8 km | IT- und Handy-Soforthilfe | CHF 10 + 45/h |
| Rossi Reinigungen | Betrieb | 3007 Bern | 15 km | Reinigung nach Schäden | CHF 40 + 75/h |
| Daniel Moser | Privatperson | 3098 Köniz | 10 km | Transport- und Umzugshilfe | CHF 20 + 40/h |
| Claudia Zurbrügg | Privatperson | 3005 Bern | 10 km | Transport- und Umzugshilfe, Begleitung und Orientierung, Nachbarschaftshilfe | CHF 15 + 35/h |
| Jonas Berger | Privatperson | 3013 Bern | 8 km | Lieferung Apotheke und Einkauf | CHF 8 + 30/h |
| Ruth Gerber | Privatperson | 3098 Köniz | 8 km | Begleitung und Orientierung, Nachbarschaftshilfe | kostenlos |
| Marco Bieri | Privatperson | 3006 Bern | 10 km | Kleine Reparatur | CHF 15 + 50/h |
| Andrea Lüthi | Privatperson | 3052 Zollikofen | 8 km | Nachbarschaftshilfe | kostenlos |

## Prüfung der Testfälle

`node --test "tests/messprotokoll/*.test.mjs"` prüft ohne laufende Schnittstelle, auch in der CI:

| Prüfung | Inhalt |
|---|---|
| Aufbau | 20 Fälle und 4 Notfälle, Postleitzahl, Ort, Sprache, Freitext oder Kategorie |
| Stammdaten | erwartete Kategorien und Fähigkeiten gibt es; kostenpflichtige Fähigkeiten mit Preisangaben |
| Neue Texte | kein Text aus den Trainingsdaten oder dem Kalibrierungssatz, auch nicht bei den Zusatzfällen |
| Zusatzfälle | mindestens 15 eindeutige Texte mit bekannter Kategorie oder «notfall», davon mindestens drei Notfälle |
| Erreichbarkeit | jede erwartete helfende Person hat die Fähigkeit und liegt im eigenen Einsatzradius und im Suchradius |
| Erwartung | die nächste geeignete helfende Person gehört zu den erwarteten |
| Auswertung | Kriterien K1 bis K6, Kennzahlen der Antwortzeit je Schritt, Prüfung der Notfall-Einträge, Bewertung der Zusatzfälle und Zellen der Tabellen ([`auswertung.test.mjs`](../tests/messprotokoll/auswertung.test.mjs)) |

## Messskript

[`tests/messprotokoll/messen.mjs`](../tests/messprotokoll/messen.mjs) misst gegen die laufende Schnittstelle:

```
node tests/messprotokoll/messen.mjs
```

| Schritt | Ablauf |
|---|---|
| Messdaten | legt die Helfenden aus `testfaelle.json` über die Schnittstelle an (Adressen `messung.…@beispiel.invalid`), die Koordination der Testdaten gibt sie frei, alle sind jetzt verfügbar; bei einem zweiten Lauf werden sie angemeldet und neu gespeichert |
| Fälle | je Fall Verständnis, Anfrage erfassen und Vorschläge laden mit einem neuen Konto für Hilfesuchende; gemessen wird jeder Schritt und die Zeit vom Absenden bis zu den Vorschlägen |
| Notfälle | Verständnis und Erfassen müssen stoppen (`451`); je Schritt ein neuer Eintrag `Notfall.Stopp` ohne Bezug auf eine Anfrage, im Detail nur der erkannte Begriff, ohne Freitext und ohne Angaben zum Konto |
| Protokoll | Markdown und CSV (Semikolon, für Tabellenkalkulationen) in `docs/messprotokolle/`, Name `r2-matching-<Datum>-<Zeit>` |
| Ergebnis | Exit-Code 0 nur, wenn mindestens 16 Fälle geeignet und alle Notfälle gestoppt und wie beschrieben protokolliert sind |

Ein Fall ist geeignet, wenn alle Kriterien erfüllt sind ([`auswertung.mjs`](../tests/messprotokoll/auswertung.mjs)):

| Kriterium | Prüfung |
|---|---|
| K1 | Kategorie wie erwartet erkannt oder gewählt |
| K2 | ein bis drei Vorschläge |
| K3 | alle Vorschläge innerhalb des Suchradius der Anfrage |
| K4 | jeder Vorschlag mit Begründung und allen fünf Teilpunkten; die Beiträge ergeben die Punktzahl |
| K5 | Rang 1 gehört zu den Helfenden der Spalte «Soll» |
| K6 | Preisangaben bei kostenpflichtiger Kategorie, keine bei kostenloser |

| Umgebung | Bedeutung |
|---|---|
| `API_URL` | Adresse der Schnittstelle, Standard `http://localhost:5000` |
| `TESTDATEN_PASSWORT` | Anmeldung der Koordination der Testdaten, auch Passwort der Konten der Messdaten |
| `OLLAMA_BASE_URL` | nur für die Angabe im Protokoll, ob das Sprachmodell erreichbar ist |
| `MESSPROTOKOLL_ORDNER` | Ablage der Protokolle, Standard `docs/messprotokolle` |

Das Protokoll nennt die Messumgebung (Gerät, Betriebssystem, Node.js, Stand des Codes, Sprachmodell, Gewichte und Suchradius), je Fall Weg der Einordnung, Sicherheit, Dringlichkeit, Kriterien, Rang 1, Zeiten und Begründungen sowie die Notfälle. Es enthält nur fiktive Daten und keine Zugangsdaten.

## Zusatzmessung mit schwierigen Texten

Die 20 Fälle enthalten eindeutige Formulierungen; Regeln und maschinelles Lernen entscheiden sie allein. Die Zusatzmessung prüft darum getrennt, wie die Verständniskette mit schwierigen Texten umgeht und wie lange das Sprachmodell braucht. Die 17 Texte in [`tests/messprotokoll/zusatzfaelle.json`](../tests/messprotokoll/zusatzfaelle.json) sind vor der Messung festgelegt und stehen weder in den Trainingsdaten noch im Kalibrierungssatz. Gemessen wird nur der Verständnis-Schritt; ein festes Ziel gibt es nicht, ausser bei den Notfällen.

| Gruppe | Fälle | Beispiel |
|---|---|---|
| Mundart | Z1 bis Z4 | «I ha mi usgschlosse, dr Schlüssu steckt dinne.» |
| Tippfehler | Z5, Z6 | «heitzung geht nich, seit gestern eiskalt in der whonung» |
| Umschreibung ohne typische Schlüsselwörter | Z7 bis Z10 | «Wir kommen mit dem Wagen nicht mehr vom Fleck …» |
| Andere Sprache | Z11 bis Z13 | Englisch und Italienisch |
| Langer Text | Z14 | drei Sätze mit Vorgeschichte |
| Notfall in ungewohnter Form | Z15 bis Z17 | Mundart, Englisch, Anzeichen eines Hirnschlags ohne Fachbegriff |

| Bewertung | Bedeutung |
|---|---|
| richtig eingeordnet | erkannte Kategorie wie erwartet |
| als Notfall gestoppt | erwarteter Notfall erkannt; Ziel für alle Notfälle |
| offen, erwartete Kategorie zur Auswahl | keine sichere Einordnung, die erwartete Kategorie steht unter den ähnlichsten |
| offen, erwartete Kategorie nicht zur Auswahl | keine sichere Einordnung, die Person wählt selbst |
| sicher, aber falsch eingeordnet | Fehler |
| Notfall nicht erkannt | schwerer Fehler |
| Notfall-Stopp ohne Notfall | Fehler |

Das Messskript führt die Zusatzmessung nach den Notfällen aus und schreibt sie ins Protokoll und in eine eigene Tabelle (`…-zusatz.csv`).

## Ergebnisse

### Messung 1 (24.11.2026)

[Messprotokoll](messprotokolle/r2-matching-2026-11-24-19-56.md) und [Tabelle](messprotokolle/r2-matching-2026-11-24-19-56.csv), Stand des Codes `41dc8e0`, frische Datenbank mit Testdaten und Messdaten.

| Messgrösse | Ziel | Ergebnis | Bewertung |
|---|---|---|---|
| Geeigneter, begründeter Vorschlag | mindestens 16 von 20 | 20 von 20, alle Kriterien K1 bis K6 erfüllt | erfüllt |
| Antwortzeit je Fall | höchstens 7 s | Median 64 ms, höchstens 414 ms | erfüllt |
| Notfälle | alle gestoppt und protokolliert | 4 von 4 | erfüllt |

| Angabe | Wert |
|---|---|
| Messumgebung | Intel Core i7-8650U, 8 logische Kerne, 32 GB Arbeitsspeicher, Windows; Schnittstelle mit `dotnet run`, PostgreSQL mit PostGIS als Container |
| Sprachmodell | `llama3.2:3b` lokal in Ollama, nur auf dem Prozessor, vor der Messung geladen |
| Weg der Einordnung | Regeln 15, maschinelles Lernen 4, Kategorieauswahl 1, Sprachmodell 0 |

Beobachtungen:

- Das Sprachmodell war erreichbar, wurde aber in keinem der 20 Fälle gebraucht: Regeln oder maschinelles Lernen haben jeweils sicher entschieden. Diese Messung sagt darum nichts über die Antwortzeit der letzten Stufe aus.
- Am knappsten entschied das maschinelle Lernen in Fall 19 (Ähnlichkeit 0.14, Abstand zur zweitbesten Kategorie 0.07), richtig, aber nahe an der Schwelle.
- Rang 1 lag in 15 Fällen in derselben Postleitzahl, sonst zwischen 914 m und 6.6 km entfernt, immer innerhalb des Suchradius.

### Antwortzeit (Messung 1)

Gemessen beim Aufrufer, je Schritt und je Fall vom Absenden bis zu den Vorschlägen. Das Messskript schreibt diese Tabelle ab jetzt selbst ins Protokoll.

| Schritt | Median | 90 % bis | höchstens |
|---|---|---|---|
| Verständnis | 7 ms | 15 ms | 81 ms |
| Erfassen mit Matching | 48 ms | 106 ms | 260 ms |
| Vorschläge laden | 10 ms | 19 ms | 73 ms |
| Gesamt je Fall | 64 ms | 138 ms | 414 ms |

| Vergleich mit dem Zielbereich (höchstens 7 s) | Ergebnis |
|---|---|
| Fälle im Zielbereich | 20 von 20 |
| Grösster Wert | 414 ms in Fall 1, der ersten Anfrage nach dem Start (erste Abfragen, Aufbau der Verbindungen); ohne Fall 1 höchstens 144 ms |
| Fälle 16 bis 20 | etwa 135 ms statt etwa 60 ms, vor allem beim Erfassen (rund 105 ms), weiterhin weit unter dem Ziel |
| Reserve | der langsamste Fall braucht weniger als 6 % des Zielbereichs |
| Grenze der Aussage | ohne Sprachmodell: keine der 20 Anfragen erreichte die letzte Stufe, deren Zeitlimit bei dieser Messung 10 s betrug |

### Notfälle (Messung 1)

| Nr. | Sprache | Erkannter Begriff | Verständnis | Erfassen | Protokolleinträge | Zeit |
|---|---|---|---|---|---|---|
| N1 | de | Brustschmerzen | gestoppt, Hinweis 144 | `451`, keine Anfrage | 2 | 9 ms |
| N2 | de | bewusstlos | gestoppt, Hinweis 144 | `451`, keine Anfrage | 2 | 15 ms |
| N3 | fr | ne respire plus | gestoppt, Hinweis 144 | `451`, keine Anfrage | 2 | 25 ms |
| N4 | fr | AVC | gestoppt, Hinweis 144 | `451`, keine Anfrage | 2 | 20 ms |

Alle vier Notfälle stoppen in beiden Schritten, bevor eine Einordnung oder ein Vorschlag entsteht; je Schritt entsteht ein Eintrag «Notfall.Stopp» in der Region. Ab der nächsten Messung prüft das Messskript zusätzlich den Inhalt dieser Einträge ([`auswertung.mjs`](../tests/messprotokoll/auswertung.mjs)): je ein Eintrag aus Verständnis und Erfassen, ohne Bezug auf eine Anfrage, im Detail nur der erkannte Begriff, weder Wortlaut des Freitexts noch Kennung, Adresse oder Name des Kontos. Dieselben Regeln prüfen die Schnittstellentests in `tests/api/protokoll.test.mjs` bei jedem Lauf der CI.

### Messung 2 vor den Korrekturen (25.11.2026)

[Messprotokoll](messprotokolle/r2-matching-2026-11-25-08-00.md), [Tabelle](messprotokolle/r2-matching-2026-11-25-08-00.csv) und [Zusatzfälle](messprotokolle/r2-matching-2026-11-25-08-00-zusatz.csv), erste Messung mit den Zusatzfällen. Die 20 Fälle und die vier Notfälle ergaben dasselbe Bild wie Messung 1: 20 von 20 geeignet, Median 75 ms, höchstens 426 ms, alle Notfälle gestoppt und protokolliert, Wege der Einordnung unverändert.

| Gruppe | Fälle | Ergebnis |
|---|---|---|
| Mundart | Z1 bis Z4 | alle richtig eingeordnet (Regeln) |
| Tippfehler | Z5, Z6 | Z5 richtig (maschinelles Lernen); Z6 «heitzung» sicher, aber falsch eingeordnet (IT- und Handy-Soforthilfe) |
| Umschreibung | Z7 bis Z10 | alle richtig eingeordnet (Regeln 3, maschinelles Lernen 1) |
| Andere Sprache | Z11 bis Z13 | Z11 und Z13 richtig (Regeln); Z12 offen nach 10061 ms, die erwartete Kategorie stand zur Auswahl |
| Langer Text | Z14 | richtig eingeordnet (Regeln) |
| Notfall in ungewohnter Form | Z15 bis Z17 | keiner erkannt |

Auffälligkeiten und Korrekturen:

| Fall | Ursache | Korrektur |
|---|---|---|
| Z15 bis Z17 | die Begriffsliste kannte weder Mundart («schnuufet nümm») noch englische Wendungen wie «chest pain» noch Anzeichen eines Hirnschlags in Alltagssprache («spricht undeutlich», «Arm nicht mehr bewegen») | 30 Begriffe in Deutsch, Mundart, Französisch, Englisch und Italienisch ergänzt (107 statt 77), dazu Einheitentests mit Gegenbeispielen wie «Ds Velo het kei Luft meh» ([Notfallprüfung](notfallpruefung.md)) |
| Z6 | die Regeln kannten nur genaue Schreibweisen; das maschinelle Lernen fand nur das Wort «geht», das in den Beispielen allein bei IT vorkommt | ein Buchstabe Abweichung in Schlüsselwörtern ab 7 Zeichen, Sicherheit 0.7 ([Regeln](verstaendniskette.md#regeln)) |
| Z12 | erste Frage an das Sprachmodell nach dem Start: Ollama musste die ganze Anweisung mit allen Kategorien verarbeiten, was auf dem Prozessor vermutlich länger dauerte als das Zeitlimit von 10 s | das Vorladen verarbeitet die Anweisung bereits beim Start, die Kategorien stehen immer in derselben Reihenfolge, höchstens 24 statt 60 Token, Zeitlimit 7 s wie der Zielbereich ([Sprachmodell](verstaendniskette.md#sprachmodell)) |

Weiterhin gilt: Kommt in einem Text nur ein einzelnes allgemeines Wort aus den Beispielen vor, kann das maschinelle Lernen sicher, aber falsch entscheiden. Die Regeln mit Tippfehlern fangen den Fall Z6 vorher ab; die Messung nach den Korrekturen zeigt, ob weitere Fälle betroffen sind.
