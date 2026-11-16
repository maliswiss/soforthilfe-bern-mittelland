// Schnittstellentests: Dringlichkeit einer Anfrage aus Text und Kategorie, sofern keine gewählt ist.

import { test } from 'node:test';
import assert from 'node:assert/strict';
import { aufruf, neuesKonto } from './hilfen.mjs';

const TIEF = 0;
const MITTEL = 1;
const HOCH = 2;

const verstehen = (daten) => aufruf('POST', '/api/anfragen/verstehen', { daten });

test('Der Verständnis-Schritt nennt die abgeleitete Dringlichkeit mit Begründung', async () => {
  const sofort = (await verstehen({ freitext: 'Der Wasserhahn tropft, bitte sofort' })).inhalt;
  assert.equal(sofort.prioritaet, HOCH);
  assert.equal(sofort.prioritaetGrund, 'Dringlichkeit hoch wegen «sofort».');

  const ohneEile = (await verstehen({ freitext: 'Der Wasserhahn tropft, keine Eile' })).inhalt;
  assert.equal(ohneEile.prioritaet, TIEF);

  const ueblich = (await verstehen({ freitext: 'Der Wasserhahn tropft' })).inhalt;
  assert.equal(ueblich.prioritaet, MITTEL);

  // Ohne Signalwort entscheidet die Kategorie
  const ausgesperrt = (await verstehen({ freitext: 'Ich habe mich ausgesperrt' })).inhalt;
  assert.equal(ausgesperrt.prioritaet, HOCH);
  assert.match(ausgesperrt.prioritaetGrund, /meist sofort nötig/);
});

test('Bei einem Notfallhinweis gibt es keine Dringlichkeit, sondern den Hinweis auf 144', async () => {
  const { inhalt } = await verstehen({ freitext: 'Er ist bewusstlos, bitte sofort' });
  assert.equal(inhalt.notfall, true);
  assert.equal(inhalt.prioritaet, null);
});

test('Ohne gewählte Dringlichkeit wird sie beim Erfassen abgeleitet, eine gewählte gilt', async () => {
  const { token } = await neuesKonto();
  const ort = { plz: '3011', ort: 'Bern' };
  const erfassen = (daten) => aufruf('POST', '/api/anfragen', { token, daten: { ...ort, ...daten } });

  assert.equal((await erfassen({ freitext: 'Ich habe mich ausgesperrt' })).inhalt.prioritaet, HOCH);
  assert.equal((await erfassen({ freitext: 'Umzug, keine Eile' })).inhalt.prioritaet, TIEF);
  assert.equal((await erfassen({ freitext: 'Der Wasserhahn tropft' })).inhalt.prioritaet, MITTEL);
  assert.equal((await erfassen({ freitext: 'Ich habe mich ausgesperrt', prioritaet: TIEF })).inhalt.prioritaet, TIEF);
  assert.equal((await erfassen({ freitext: 'Hilfe', prioritaet: 7 })).status, 400);
});
