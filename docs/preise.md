# Preisangaben

Wer bezahlpflichtige Leistungen anbietet, nennt seine Preise im Helferprofil. Anfragende sehen den Preis vor der Zusage (Anforderung A9); bei kostenlosen Kategorien wird kein Preis angezeigt.

## Angaben im Helferprofil

| Angabe | Bedeutung | Bereich |
|---|---|---|
| `anfahrtChf` | Pauschale je Einsatz | 0 bis 500 Franken |
| `stundenansatzChf` | Preis je Stunde | 0 bis 500 Franken |
| `nachtzuschlagProzent` | Zuschlag auf den Stundenansatz von 20 bis 7 Uhr | 0 bis 200 % |
| `wochenendzuschlagProzent` | Zuschlag auf den Stundenansatz am Samstag und Sonntag | 0 bis 200 % |
| `mwstPflichtig` | Preise verstehen sich ohne MWST; bei MWST-Pflicht kommt sie im Richtpreis hinzu | ja oder nein |

Enthält das Profil mindestens eine bezahlpflichtige Fähigkeit, sind Anfahrt und Stundenansatz Pflicht; sonst folgt `400` mit `"feld": "preise"`. `preiseErforderlich` in der Antwort zeigt, ob das Profil Preise braucht.

## Richtpreis für die erste Stunde

```
Richtpreis = Anfahrt + Stundenansatz × (1 + Zuschlag) [× (1 + MWST-Satz)]
```

| Regel | Umsetzung |
|---|---|
| Zeit | Schweizer Ortszeit (`Europe/Zurich`), auch wenn der Zeitpunkt in UTC kommt |
| Nacht | 20 bis 7 Uhr |
| Wochenende | Samstag und Sonntag |
| Nacht am Wochenende | es gilt der höhere Zuschlag; Zuschläge werden nicht addiert |
| MWST | Normalsatz 8,1 %, nur bei MWST-Pflicht |
| Rundung | auf 5 Rappen |
| Fehlende Angaben | ohne Anfahrt oder Stundenansatz kein Richtpreis |

MWST-Satz und Nachtzeit lassen sich über `Preise__MwstSatzProzent`, `Preise__NachtVonStunde` und `Preise__NachtBisStunde` einstellen.

| Beispiel: Anfahrt 40, Stundenansatz 95, Nacht 50 %, Wochenende 25 %, MWST-pflichtig | Richtpreis |
|---|---|
| Mittwoch, 10 Uhr | 145.95 |
| Mittwoch, 22 Uhr | 197.30 (Nachtzuschlag) |
| Samstag, 10 Uhr | 171.60 (Wochenendzuschlag) |
| Samstag, 23 Uhr | 197.30 (höherer Zuschlag: Nacht) |
| Mittwoch, 22 Uhr, ohne MWST-Pflicht | 182.50 |

## Schnittstelle

`POST /api/preise/richtpreis` berechnet den Richtpreis ohne Anmeldung und ohne etwas zu speichern, zum Beispiel für die Vorschau im Profil. Es gilt dieselbe Berechnung wie später in den Vorschlägen.

```
POST /api/preise/richtpreis
Content-Type: application/json

{ "anfahrtChf": 40, "stundenansatzChf": 95, "nachtzuschlagProzent": 50, "wochenendzuschlagProzent": 25,
  "mwstPflichtig": true, "zeitpunkte": ["2026-11-11T22:00:00+01:00"] }
```

Antwort `200`, je Zeitpunkt (höchstens zehn; ohne Angabe der aktuelle):

```
[ { "anfahrtChf": 40, "stundenansatzChf": 95, "nachtzuschlagProzent": 50, "wochenendzuschlagProzent": 25,
    "mwstPflichtig": true, "mwstSatzProzent": 8.1, "richtpreisErsteStunde": 197.30,
    "angewandterZuschlag": "nacht", "angewandterZuschlagProzent": 50 } ]
```

## Oberfläche

Die Preise sind vor der Zusage sichtbar, aber nur bei bezahlpflichtigen Kategorien (Anforderungen A9 und A12).

| Stelle | Bezahlpflichtige Kategorie | Kostenlose Kategorie |
|---|---|---|
| «Wo und wann?» vor dem Absenden | Hinweis «Preise vor der Zusage»: Anfahrt und Stundenansatz, Zuschläge, Richtpreis der ersten Stunde, MWST; «Verbindlich wird erst Ihre Zusage.» | «Diese Hilfe ist kostenlos.» |
| Anfrage nach dem Absenden | derselbe Hinweis unter den Angaben zur Anfrage | kein Preishinweis |
| Helferprofil | Vorschau zu vier Beispielzeiten in der Darstellung, die Anfragende bei den Vorschlägen sehen | keine Preisangaben nötig |

Beim Freitext bestimmt die [Verständniskette](verstaendniskette.md) die Kategorie und damit, ob der Preishinweis erscheint; die Person kann die Kategorie vor dem Absenden ändern. Die Preise einzelner Helfender zeigt die Komponente [`PreisAngaben.vue`](../frontend/src/components/PreisAngaben.vue): Richtpreis mit angewandtem Zuschlag und MWST, Anfahrt, Stundenansatz, beide Zuschläge und den MWST-Satz.

## Tests

| Test | Prüft |
|---|---|
| Einheitentests `PreisrechnerTests` | Werktag, Nacht, Wochenende, höherer Zuschlag, Grenzen 20 und 7 Uhr, Ortszeit aus UTC, ohne MWST, fehlende Angaben, kostenlose Leistungen, Rundung auf 5 Rappen, einstellbarer MWST-Satz |
| Schnittstellentests `tests/api/preise.test.mjs` | Richtpreis zu vier Zeitpunkten, ohne MWST, fehlende Angaben, höchstens zehn Zeitpunkte, ungültige Beträge, Pflicht bei bezahlpflichtigen Fähigkeiten, keine Pflicht bei kostenlosen |
| Rauchtest | Richtpreis nachts mit MWST, Profil ohne Preise abgewiesen |
| Oberflächentests `tests/oberflaeche/preise.spec.mjs` | Preishinweis bei erkannter bezahlpflichtiger Kategorie vor und nach dem Absenden, kein Preishinweis bei kostenloser Kategorie, Wahl aus den ähnlichsten Kategorien bestimmt den Preishinweis |
| Oberflächentests `tests/oberflaeche/helferprofil.spec.mjs` | Vorschau im Helferprofil mit Richtpreis, Zuschlag, Stundenansatz und MWST |
