# Messung des Matchings

Die Messung zeigt, ob das Matching zu typischen Anliegen geeignete und begründete Vorschläge liefert. Die Testfälle und ihre erwarteten Ergebnisse sind vor der Messung festgelegt, in [`tests/messprotokoll/testfaelle.json`](../tests/messprotokoll/testfaelle.json).

## Ziel und Bewertung

| Punkt | Festlegung |
|---|---|
| Ziel | mindestens 16 von 20 Fällen mit geeignetem, begründetem Vorschlag |
| Fall erfüllt | erkannte Kategorie wie erwartet, Rang 1 unter den Helfenden der Spalte «Soll», Vorschlag mit Begründung |
| Notfälle | alle vier werden gestoppt (Hinweis auf 144) und protokolliert, ohne Anfrage und ohne Vorschlag |
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
| Neue Texte | kein Text aus den Trainingsdaten oder dem Kalibrierungssatz |
| Erreichbarkeit | jede erwartete helfende Person hat die Fähigkeit und liegt im eigenen Einsatzradius und im Suchradius |
| Erwartung | die nächste geeignete helfende Person gehört zu den erwarteten |
