// Prüft die Auswertung der Messung ohne laufende Schnittstelle: Kriterien je Fall, Kennzahlen und Zellen.

import { test } from 'node:test';
import assert from 'node:assert/strict';
import {
  kriterienPruefen, geeignet, gehoertZuSoll, median, perzentil, mdZelle, csvZelle, stempel, schrittKennzahlen, notfallEintraegePruefen, zusatzBewerten, ZUSATZ_BEWERTUNG, KRITERIEN,
} from './auswertung.mjs';

const fall = { erwartet: 'sanitaer-elektro', soll: ['Wyss Sanitär und Heizung'] };
const kriterien = (teilpunkte, gewicht = 0.2) => KRITERIEN.map((kriterium) => ({
  kriterium, teilpunkte, gewicht, beitrag: Math.round(teilpunkte * gewicht * 10000) / 10000,
}));
const vorschlag = (zusatz = {}) => ({
  rang: 1, punktzahl: 0.8, distanzMeter: 1200, betriebsname: 'Wyss Sanitär und Heizung', anzeigename: 'Konto Wyss',
  begruendung: '1.2 km entfernt · Fähigkeit «Sanitär, Heizung, Elektro»', kriterien: kriterien(0.8),
  preis: { richtpreisErsteStunde: 155 }, ...zusatz,
});
const kostenpflichtig = { code: 'sanitaer-elektro', bezahlpflichtig: true };

test('Ein passender, begründeter Vorschlag erfüllt alle Kriterien', () => {
  const k = kriterienPruefen(fall, kostenpflichtig, 1, [vorschlag()]);
  assert.deepEqual(k, { K1: true, K2: true, K3: true, K4: true, K5: true, K6: true });
  assert.equal(geeignet(k), true);
});

test('Jedes Kriterium kann den Fall scheitern lassen', () => {
  assert.equal(kriterienPruefen(fall, { ...kostenpflichtig, code: 'kleine-reparatur' }, 1, [vorschlag()]).K1, false);
  assert.equal(kriterienPruefen(fall, kostenpflichtig, 1, []).K2, false);
  assert.equal(kriterienPruefen(fall, kostenpflichtig, 1, Array(4).fill(vorschlag())).K2, false);
  // Suchradius: tief 5 km, mittel 10 km, hoch 15 km
  assert.equal(kriterienPruefen(fall, kostenpflichtig, 0, [vorschlag({ distanzMeter: 6000 })]).K3, false);
  assert.equal(kriterienPruefen(fall, kostenpflichtig, 2, [vorschlag({ distanzMeter: 14000 })]).K3, true);
  assert.equal(kriterienPruefen(fall, kostenpflichtig, 1, [vorschlag({ begruendung: ' ' })]).K4, false);
  assert.equal(kriterienPruefen(fall, kostenpflichtig, 1, [vorschlag({ kriterien: kriterien(0.8).slice(1) })]).K4, false);
  assert.equal(kriterienPruefen(fall, kostenpflichtig, 1, [vorschlag({ punktzahl: 0.9 })]).K4, false);
  assert.equal(kriterienPruefen(fall, kostenpflichtig, 1, [vorschlag({ betriebsname: 'Elektro Schneider AG' })]).K5, false);
  assert.equal(kriterienPruefen(fall, kostenpflichtig, 1, [vorschlag({ preis: null })]).K6, false);
  assert.equal(kriterienPruefen(fall, { code: 'sanitaer-elektro', bezahlpflichtig: false }, 1, [vorschlag()]).K6, false);
  assert.equal(geeignet(kriterienPruefen(fall, null, 1, [])), false);
});

test('Soll: voller Name oder gekürzter Nachname, Betriebe über den Betriebsnamen', () => {
  assert.equal(gehoertZuSoll('Nina Hofer', ['Nina Hofer']), true);
  assert.equal(gehoertZuSoll('Nina H.', ['Nina Hofer']), true);
  assert.equal(gehoertZuSoll('Nina', ['Nina Hofer']), false);
  assert.equal(gehoertZuSoll('Jonas Berger', ['Nina Hofer', 'Kunz IT-Support']), false);
});

test('Median und Perzentil der Antwortzeiten', () => {
  assert.equal(median([300, 100, 200]), 200);
  assert.equal(median([100, 200, 300, 400]), 250);
  assert.equal(median([]), null);
  const zeiten = Array.from({ length: 20 }, (_, i) => (i + 1) * 100);
  assert.equal(perzentil(zeiten, 90), 1800);
  assert.equal(perzentil(zeiten, 100), 2000);
  assert.equal(perzentil([700], 90), 700);
});

test('Zellen für Markdown und CSV, Zeitstempel in Schweizer Zeit', () => {
  assert.equal(mdZelle('a | b\nc'), 'a / b c');
  assert.equal(csvZelle('Bern'), 'Bern');
  assert.equal(csvZelle('Sanitär; Heizung'), '"Sanitär; Heizung"');
  assert.equal(csvZelle('«sagt "ja"»'), '"«sagt ""ja""»"');
  assert.equal(csvZelle(null), '');
  assert.equal(stempel(new Date('2026-11-24T08:15:00Z')), '2026-11-24-09-15');
  assert.equal(stempel(new Date('2026-07-01T22:30:00Z')), '2026-07-02-00-30');
});

test('Kennzahlen je Schritt, Fälle ohne einen Schritt zählen dort nicht mit', () => {
  const k = schrittKennzahlen([
    { verstehen: 80, erfassen: 260, vorschlaege: 70, gesamt: 410 },
    { verstehen: 5, erfassen: 40, vorschlaege: 10, gesamt: 55 },
    { verstehen: 7, erfassen: 50, vorschlaege: 9, gesamt: 66 },
    { verstehen: 9, gesamt: 9 },
  ]);
  assert.deepEqual(k.verstehen, { anzahl: 4, median: 8, p90: 80, max: 80 });
  assert.deepEqual(k.erfassen, { anzahl: 3, median: 50, p90: 260, max: 260 });
  assert.equal(k.gesamt.median, 61);
  assert.deepEqual(schrittKennzahlen([]).gesamt, { anzahl: 0, median: null, p90: null, max: null });
});

test('Notfall im Protokoll: beide Schritte, ohne Anfrage, nur der Begriff, ohne Freitext und ohne Konto', () => {
  const freitext = 'Meine Nachbarin ist gestürzt und bewusstlos.';
  const konto = { adresse: 'test.123@beispiel.invalid', anzeigename: 'Test Person' };
  const eintrag = (objekt, zusatz = {}) => ({
    id: 1, aktion: 'Notfall.Stopp', objekt, objektId: null, detail: 'Erkanntes Schlüsselwort: bewusstlos', region: 'bern-mittelland', ...zusatz,
  });
  const gut = notfallEintraegePruefen([eintrag('Verstaendnis'), eintrag('Anfrage')], freitext, konto);
  assert.equal(gut.erfuellt, true);
  assert.equal(gut.anzahl, 2);

  assert.equal(notfallEintraegePruefen([eintrag('Verstaendnis')], freitext, konto).erfuellt, false);
  assert.equal(notfallEintraegePruefen([eintrag('Verstaendnis'), eintrag('Anfrage', { objektId: '42' })], freitext, konto).ohneAnfrage, false);
  const mitText = notfallEintraegePruefen([eintrag('Verstaendnis', { detail: freitext }), eintrag('Anfrage')], freitext, konto);
  assert.equal(mitText.ohneFreitext, false);
  assert.equal(mitText.nurBegriff, false);
  assert.equal(notfallEintraegePruefen([eintrag('Verstaendnis', { detail: 'Erkanntes Schlüsselwort: bewusstlos, Test Person' }), eintrag('Anfrage')], freitext, konto).ohneKonto, false);
});

test('Zusatzfälle: richtig, hilfreich offen, verfehlt, falsch, Notfall gestoppt, verpasst oder Fehlalarm', () => {
  const fall = { erwartet: 'transporthilfe' };
  const alternativen = [{ code: 'kleine-reparatur' }, { code: 'transporthilfe' }];
  assert.equal(zusatzBewerten(fall, { notfall: false, code: 'transporthilfe' }), 'richtig');
  assert.equal(zusatzBewerten(fall, { notfall: false, code: 'it-soforthilfe' }), 'falsch');
  assert.equal(zusatzBewerten(fall, { notfall: false, code: null, alternativen }), 'hilfreich');
  assert.equal(zusatzBewerten(fall, { notfall: false, code: null, alternativen: alternativen.slice(0, 1) }), 'verfehlt');
  assert.equal(zusatzBewerten(fall, { notfall: false, code: null }), 'verfehlt');
  assert.equal(zusatzBewerten(fall, { notfall: true }), 'fehlalarm');
  assert.equal(zusatzBewerten({ erwartet: 'notfall' }, { notfall: true }), 'gestoppt');
  assert.equal(zusatzBewerten({ erwartet: 'notfall' }, { notfall: false, code: 'begleitung' }), 'verpasst');
  for (const b of ['richtig', 'gestoppt', 'hilfreich', 'verfehlt', 'falsch', 'verpasst', 'fehlalarm']) {
    assert.ok(ZUSATZ_BEWERTUNG[b], b);
  }
});
