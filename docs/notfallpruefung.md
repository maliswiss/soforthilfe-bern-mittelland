# Notfallprüfung

Die Plattform vermittelt keine medizinischen Leistungen. Jede Beschreibung in eigenen Worten wird deshalb vor allem anderen auf akute gesundheitliche Hinweise geprüft. Bei einem Treffer stoppt die Vermittlung, und die Plattform verweist auf die Notrufnummer 144.

## Grundsätze

| Nr. | Grundsatz | Begründung |
|---|---|---|
| 1 | Die Prüfung läuft immer zuerst, unabhängig vom Erfassungsweg | ein Notfall darf nie in die Vermittlung gelangen |
| 2 | Feste Regeln statt maschinellem Lernen | nachvollziehbar, sofort (ohne Wartezeit) und ohne Trainingsdaten |
| 3 | Bewusst empfindlich | lieber einmal zu oft auf 144 verweisen als einen Notfall vermitteln |
| 4 | Nur gesundheitliche Notfälle (144) | Feuerwehr (118) und Polizei (117) rufen Betroffene direkt an; Brand und Einbruch lösen keinen Notfall-Stopp aus |
| 5 | Das allgemeine Wort «Notfall» allein genügt nicht | «Notfall: Wasserrohrbruch» ist kein medizinischer Notfall |
| 6 | Sprachen Deutsch, Französisch, Englisch und Italienisch | Bern/Mittelland und Biel/Seeland sind zweisprachig, Gäste schreiben oft Englisch oder Italienisch |

## Ablauf

1. Der Text wird aufbereitet: Kleinschreibung, Umlaute als ae/oe/ue, übrige Akzente entfernt, Satzzeichen als Leerzeichen ([`Textnormalisierung.cs`](../backend/src/Soforthilfe.Dienste/Verstaendnis/Textnormalisierung.cs)).
2. Die Begriffsliste wird der Reihe nach geprüft ([`NotfallPruefung.cs`](../backend/src/Soforthilfe.Dienste/Notfall/NotfallPruefung.cs)). Kurze Begriffe bis fünf Buchstaben (zum Beispiel «avc», «wehen») zählen nur als ganzes Wort, längere auch innerhalb zusammengesetzter Wörter (zum Beispiel «Herzinfarktverdacht»).
3. Der erste Treffer wird als Fundstelle so zurückgegeben, wie die Person ihn geschrieben hat.

## Notfall-Stopp

| Stelle | Verhalten |
|---|---|
| Startseite, «Hilfe finden» | `POST /api/anfragen/verstehen` prüft den Text auch ohne Anmeldung; bei einem Treffer öffnet die Oberfläche die Notfallseite |
| Anfrage erfassen | `POST /api/anfragen` prüft den Text erneut; bei einem Treffer wird nichts gespeichert, Antwort `451` |
| Notfallseite | «Bitte sofort 144 anrufen», das erkannte Wort, ein Anrufknopf (`tel:144`) und die Abgrenzung: keine medizinischen Leistungen, gesundheitliche Anliegen nur für Orientierung, Transport und Begleitung |

Das erkannte Wort steht nur im Speicher der Seite und nie in der Adresse, damit es weder im Browserverlauf noch in Zugriffsprotokollen landet.

## Protokoll

Jeder Notfall-Stopp erzeugt einen Eintrag in der Tabelle `Protokoll`, im Verständnis-Schritt (`Objekt` «Verstaendnis») wie beim Erfassen (`Objekt` «Anfrage»).

| Feld | Inhalt |
|---|---|
| `Aktion` | `Notfall.Stopp` |
| `Detail` | `Erkanntes Schlüsselwort: …` mit dem Begriff aus der festen Liste in normalisierter Form, nie mit dem Wortlaut der Eingabe |
| `Region` | Region der Anfrage; leer, wenn noch keine Region gewählt ist |
| `Zeitpunkt` | Zeitpunkt in UTC |

Nicht gespeichert werden Konto, Name, E-Mail-Adresse, Telefonnummer, IP-Adresse, Ort und Freitext. Scheitert das Speichern des Eintrags, erhält die Person den Hinweis auf 144 trotzdem; der Fehler steht im Anwendungsprotokoll. Lesen: `GET /api/protokoll` ([schnittstelle.md](schnittstelle.md)).

## Begriffe

| Bereich | Beispiele |
|---|---|
| Herz und Kreislauf | Herzinfarkt, Herzstillstand, Brustschmerzen, Schlaganfall, crise cardiaque, AVC, heart attack, infarto |
| Bewusstsein | bewusstlos, ohnmächtig, nicht ansprechbar, kollabiert, zusammengebrochen, inconscient, évanoui, unconscious |
| Atmung | Atemnot, atmet nicht, bekommt keine Luft, erstickt, ne respire plus, not breathing, non respira |
| Blutung und Verletzung | starke Blutung, blutet stark, schwer verletzt, schwere Verbrennung, Sturz aus Höhe, hémorragie |
| Vergiftung und Allergie | Vergiftung, Überdosis, allergischer Schock, intoxication, choc anaphylactique |
| Weitere | Krampfanfall, Wehen, Notarzt, Ambulanz, Rettungsdienst, Reanimation, Lebensgefahr, danger de mort |

Die vollständige Liste mit 77 Begriffen steht im Code; ein Test stellt sicher, dass jeder Begriff normalisiert und nur einmal vorhanden ist.

## Testfälle

Die Einheitentests in [`NotfallPruefungTests.cs`](../backend/tests/Soforthilfe.Tests/NotfallPruefungTests.cs) prüfen:

| Gruppe | Anzahl | Beispiel | Erwartet |
|---|---|---|---|
| Akute Hinweise Deutsch | 12 | «Mein Mann ist bewusstlos auf dem Boden.» | Notfall, Fundstelle «bewusstlos» |
| Akute Hinweise Französisch | 4 | «Je pense que c'est un AVC.» | Notfall, Fundstelle «AVC» |
| Akute Hinweise Englisch und Italienisch | 2 | «Mia madre non respira bene» | Notfall, Fundstelle «non respira» |
| Alltägliche Anliegen und leere Eingaben | 11 | «Notfall: Wasserrohrbruch im Keller» | kein Notfall |
| Brand und Polizei | 4 | «In der Küche brennt es» | kein Notfall |
| Begriffsliste | 1 | jeder Begriff normalisiert und eindeutig | bestanden |

Ausführen im Ordner `backend` mit `dotnet test --solution Soforthilfe.slnx`; die CI führt die Tests bei jedem Push aus.
