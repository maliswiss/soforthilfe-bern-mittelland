// Schnittstellentests: Protokolleintrag bei Notfallerkennung, ohne Personendaten.
// Jeder Test verwendet einen eigenen Begriff, damit parallel laufende Testdateien die Einträge nicht verwechseln.

import { test } from 'node:test';
import assert from 'node:assert/strict';
import { aufruf, neuesKonto, testkonto, BERN, BIEL, OHNE_TESTPASSWORT } from './hilfen.mjs';

const FELDER = ['aktion', 'detail', 'id', 'objekt', 'objektId', 'region', 'zeitpunkt'];

const protokoll = (token) => aufruf('GET', '/api/protokoll?aktion=Notfall.Stopp&anzahl=500', { token });

// Höchste Kennung, die der Plattformbetrieb vor dem Test sieht; neue Einträge liegen darüber.
async function letzteKennung(betrieb) {
  const { inhalt } = await protokoll(betrieb);
  return inhalt.reduce((hoechste, eintrag) => Math.max(hoechste, eintrag.id), 0);
}

const neue = (eintraege, ab, begriff) =>
  eintraege.filter((eintrag) => eintrag.id > ab && eintrag.detail === `Erkanntes Schlüsselwort: ${begriff}`);

test('Protokoll lesen nur mit Koordination, Mandantenadministration oder Plattformbetrieb', async () => {
  assert.equal((await aufruf('GET', '/api/protokoll')).status, 401);

  const hilfesuchende = await neuesKonto(BERN);
  assert.equal((await aufruf('GET', '/api/protokoll', { token: hilfesuchende.token })).status, 403);
});

test('Notfall-Stopp im Verständnis-Schritt wird in der Region protokolliert, ohne Personendaten', { skip: OHNE_TESTPASSWORT }, async () => {
  const betrieb = await testkonto('hans.betrieb@beispiel.invalid');
  const ab = await letzteKennung(betrieb);

  const freitext = 'Herr Testmann hat einen Herzstillstand, Telefon 031 555 66 77.';
  const antwort = await aufruf('POST', '/api/anfragen/verstehen', { region: BERN, daten: { freitext } });
  assert.equal(antwort.inhalt.notfall, true);

  const koordination = await testkonto('dora.probe@beispiel.invalid');
  const { status, inhalt } = await protokoll(koordination);
  assert.equal(status, 200);

  const [eintrag] = neue(inhalt, ab, 'herzstillstand');
  assert.ok(eintrag, 'Eintrag für die Koordination der Region sichtbar');
  assert.equal(eintrag.aktion, 'Notfall.Stopp');
  assert.equal(eintrag.objekt, 'Verstaendnis');
  assert.equal(eintrag.objektId, null);
  assert.equal(eintrag.region, BERN);
  assert.ok(Date.parse(eintrag.zeitpunkt) > 0);

  // Nur feste Felder, kein Wortlaut aus dem Freitext.
  assert.deepEqual(Object.keys(eintrag).sort(), FELDER);
  const text = JSON.stringify(eintrag);
  for (const teil of ['Testmann', 'Herzstillstand', '031', '555']) {
    assert.ok(!text.includes(teil), `kein «${teil}» im Protokoll`);
  }

  // Andere Regionen sehen den Eintrag nicht.
  const adminBiel = await testkonto('greta.muster@beispiel.invalid');
  const biel = await protokoll(adminBiel);
  assert.equal(biel.status, 200);
  assert.equal(neue(biel.inhalt, ab, 'herzstillstand').length, 0);
  assert.ok(biel.inhalt.every((e) => e.region === BIEL));
});

test('Notfall-Stopp beim Erfassen wird ohne Konto protokolliert', { skip: OHNE_TESTPASSWORT }, async () => {
  const betrieb = await testkonto('hans.betrieb@beispiel.invalid');
  const ab = await letzteKennung(betrieb);

  const person = await neuesKonto(BERN);
  const erfassen = await aufruf('POST', '/api/anfragen', {
    token: person.token,
    daten: { freitext: 'Frau Beispiel ist kollabiert.', plz: '3011', ort: 'Bern' },
  });
  assert.equal(erfassen.status, 451);

  const koordination = await testkonto('dora.probe@beispiel.invalid');
  const [eintrag] = neue((await protokoll(koordination)).inhalt, ab, 'kollabiert');
  assert.ok(eintrag, 'Eintrag für die Koordination der Region sichtbar');
  assert.equal(eintrag.objekt, 'Anfrage');
  assert.equal(eintrag.region, BERN);

  const text = JSON.stringify(eintrag);
  for (const teil of [person.konto.id, person.adresse, 'Beispiel', '3011']) {
    assert.ok(!text.includes(teil), `kein «${teil}» im Protokoll`);
  }
});

test('Notfall-Stopp ohne gewählte Region sieht nur der Plattformbetrieb', { skip: OHNE_TESTPASSWORT }, async () => {
  const betrieb = await testkonto('hans.betrieb@beispiel.invalid');
  const ab = await letzteKennung(betrieb);

  const antwort = await aufruf('POST', '/api/anfragen/verstehen', { daten: { freitext: 'Er hat einen Krampfanfall.' } });
  assert.equal(antwort.inhalt.notfall, true);

  const [eintrag] = neue((await protokoll(betrieb)).inhalt, ab, 'krampfanfall');
  assert.ok(eintrag, 'Eintrag für den Plattformbetrieb sichtbar');
  assert.equal(eintrag.region, null);

  const koordination = await testkonto('dora.probe@beispiel.invalid');
  assert.equal(neue((await protokoll(koordination)).inhalt, ab, 'krampfanfall').length, 0);
});

test('Neueste Einträge zuerst, Anzahl begrenzt, Filter nach Aktion', { skip: OHNE_TESTPASSWORT }, async () => {
  const betrieb = await testkonto('hans.betrieb@beispiel.invalid');

  const zwei = await aufruf('GET', '/api/protokoll?anzahl=2', { token: betrieb });
  assert.equal(zwei.status, 200);
  assert.ok(zwei.inhalt.length <= 2);
  if (zwei.inhalt.length === 2) {
    assert.ok(Date.parse(zwei.inhalt[0].zeitpunkt) >= Date.parse(zwei.inhalt[1].zeitpunkt));
  }

  const unbekannt = await aufruf('GET', '/api/protokoll?aktion=Gibt.Es.Nicht', { token: betrieb });
  assert.equal(unbekannt.status, 200);
  assert.deepEqual(unbekannt.inhalt, []);
});
