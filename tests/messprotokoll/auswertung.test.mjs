// Prüft die Auswertung der Messung ohne laufende Schnittstelle: Kriterien je Fall, Kennzahlen und Zellen.

import { test } from 'node:test';
import assert from 'node:assert/strict';
import {
  kriterienPruefen, geeignet, gehoertZuSoll, median, perzentil, mdZelle, csvZelle, stempel, KRITERIEN,
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
