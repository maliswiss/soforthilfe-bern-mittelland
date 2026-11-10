// Schnittstellentests: strikte Datentrennung zwischen den Regionen (Anforderung A18).

import { test } from 'node:test';
import assert from 'node:assert/strict';
import { aufruf, neuesKonto, testkonto, BERN, BIEL, HELFENDE, TESTPASSWORT, OHNE_TESTPASSWORT } from './hilfen.mjs';

const anfrage = { freitext: 'Hilfe beim Tragen von Einkäufen', plz: '3011', ort: 'Bern' };

test('Aktive Regionen sind ohne Anmeldung abrufbar', async () => {
  const { status, inhalt } = await aufruf('GET', '/api/mandanten');
  assert.equal(status, 200);
  const bern = inhalt.find((region) => region.schluessel === BERN);
  assert.equal(bern.name, 'Soforthilfe Bern/Mittelland');
  assert.equal(bern.standardsprache, 'de');
});

test('Unbekannte Region im Kopf X-Region wird abgewiesen', async () => {
  assert.equal((await aufruf('GET', '/api/kategorien', { region: 'gibt-es-nicht' })).status, 404);
  assert.equal((await aufruf('GET', '/api/kategorien', { region: BERN })).status, 200);
});

test('Angemeldet gilt die Region des Kontos; ein abweichender Kopf wird abgewiesen', async () => {
  const { token } = await neuesKonto(BERN);
  assert.equal((await aufruf('GET', '/api/auth/ich', { token, region: BIEL })).status, 403);
  assert.equal((await aufruf('GET', '/api/auth/ich', { token, region: BERN })).status, 200);
});

test('Anmeldung nur in der Region des Kontos; ohne Angabe gilt diese Region', { skip: OHNE_TESTPASSWORT }, async () => {
  const daten = (mandant) => ({ daten: { eMail: 'emil.exempel@beispiel.invalid', passwort: TESTPASSWORT, mandant } });
  assert.equal((await aufruf('POST', '/api/auth/anmeldung', daten(BERN))).status, 401);
  assert.equal((await aufruf('POST', '/api/auth/anmeldung', daten(BIEL))).status, 200);

  const ohneRegion = await aufruf('POST', '/api/auth/anmeldung', daten(undefined));
  assert.equal(ohneRegion.status, 200);
  assert.equal(ohneRegion.inhalt.benutzer.mandantSchluessel, BIEL);
});

test('Koordination und Mandantenadministration sehen nur Konten der eigenen Region', { skip: OHNE_TESTPASSWORT }, async () => {
  const bern = await aufruf('GET', '/api/konten', { token: await testkonto('dora.probe@beispiel.invalid') });
  assert.ok(bern.inhalt.length > 0);
  assert.ok(bern.inhalt.every((konto) => konto.region === BERN));

  const biel = await aufruf('GET', '/api/konten', { token: await testkonto('greta.muster@beispiel.invalid') });
  assert.ok(biel.inhalt.length > 0);
  assert.ok(biel.inhalt.every((konto) => konto.region === BIEL));
});

test('Der Plattformbetrieb sieht alle Regionen', { skip: OHNE_TESTPASSWORT }, async () => {
  const alle = await aufruf('GET', '/api/konten', { token: await testkonto('hans.betrieb@beispiel.invalid') });
  const regionen = new Set(alle.inhalt.map((konto) => konto.region));
  assert.ok(regionen.has(BERN) && regionen.has(BIEL));
});

test('Konto einer fremden Region gilt als nicht vorhanden', { skip: OHNE_TESTPASSWORT }, async () => {
  const { konto } = await neuesKonto(BERN, HELFENDE);
  const adminBiel = await testkonto('greta.muster@beispiel.invalid');
  const { status } = await aufruf('PUT', `/api/konten/${konto.id}/rolle`, {
    token: adminBiel, daten: { rolle: 'Koordination' },
  });
  assert.equal(status, 404);
});

test('Anfragen einer fremden Region sind weder in der Liste noch einzeln sichtbar', { skip: OHNE_TESTPASSWORT }, async () => {
  const { token } = await neuesKonto(BERN);
  const erfasst = await aufruf('POST', '/api/anfragen', { token, daten: anfrage });
  assert.equal(erfasst.status, 201);

  const adminBiel = await testkonto('greta.muster@beispiel.invalid');
  const liste = await aufruf('GET', '/api/anfragen', { token: adminBiel });
  assert.equal(liste.status, 200);
  assert.ok(!liste.inhalt.some((eintrag) => eintrag.id === erfasst.inhalt.id));
  assert.equal((await aufruf('GET', `/api/anfragen/${erfasst.inhalt.id}`, { token: adminBiel })).status, 404);

  const koordinationBern = await testkonto('dora.probe@beispiel.invalid');
  assert.equal((await aufruf('GET', `/api/anfragen/${erfasst.inhalt.id}`, { token: koordinationBern })).status, 200);
});
