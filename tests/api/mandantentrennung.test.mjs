// Schnittstellentests: strikte Datentrennung zwischen den Regionen (Anforderung A18).

import { test } from 'node:test';
import assert from 'node:assert/strict';
import { aufruf, neuesKonto, testkonto, OHNE_TESTPASSWORT } from './hilfen.mjs';

test('Region aus dem Kopf X-Region; unbekannte Region 404, ohne Region 400', async () => {
  const bern = await aufruf('GET', '/api/regionen/aktuell', { region: 'bern' });
  assert.equal(bern.status, 200);
  assert.equal(bern.inhalt.kennung, 'bern');
  assert.equal((await aufruf('GET', '/api/regionen/aktuell', { region: 'gibt-es-nicht' })).status, 404);
  assert.equal((await aufruf('GET', '/api/regionen/aktuell')).status, 400);
});

test('Angemeldet gilt die Region des Kontos; ein abweichender Kopf wird abgewiesen', async () => {
  const { token } = await neuesKonto('bern');
  const aktuell = await aufruf('GET', '/api/regionen/aktuell', { token });
  assert.equal(aktuell.inhalt.kennung, 'bern');
  assert.equal((await aufruf('GET', '/api/konto/ich', { token, region: 'mittelland' })).status, 403);
  assert.equal((await aufruf('GET', '/api/konto/ich', { token, region: 'bern' })).status, 200);
});

test('Koordination und Mandantenadministration sehen nur Konten der eigenen Region', { skip: OHNE_TESTPASSWORT }, async () => {
  const bern = await aufruf('GET', '/api/konten', { token: await testkonto('dora.probe@beispiel.invalid') });
  assert.ok(bern.inhalt.length > 0);
  assert.ok(bern.inhalt.every((konto) => konto.region === 'bern'));

  const mittelland = await aufruf('GET', '/api/konten', { token: await testkonto('greta.muster@beispiel.invalid') });
  assert.ok(mittelland.inhalt.length > 0);
  assert.ok(mittelland.inhalt.every((konto) => konto.region === 'mittelland'));
});

test('Der Plattformbetrieb sieht alle Regionen', { skip: OHNE_TESTPASSWORT }, async () => {
  const alle = await aufruf('GET', '/api/konten', { token: await testkonto('hans.betrieb@beispiel.invalid') });
  const regionen = new Set(alle.inhalt.map((konto) => konto.region));
  assert.ok(regionen.has('bern') && regionen.has('mittelland'));
});

test('Konto einer fremden Region gilt als nicht vorhanden', { skip: OHNE_TESTPASSWORT }, async () => {
  const { konto } = await neuesKonto('bern', 'Helfende');
  const adminMittelland = await testkonto('greta.muster@beispiel.invalid');
  const { status } = await aufruf('PUT', `/api/konten/${konto.id}/rolle`, {
    token: adminMittelland, daten: { rolle: 'Koordination' },
  });
  assert.equal(status, 404);
});

test('Anfragen einer fremden Region sind weder in der Liste noch einzeln sichtbar', { skip: OHNE_TESTPASSWORT }, async () => {
  const { token } = await neuesKonto('bern');
  const erfasst = await aufruf('POST', '/api/anfragen', {
    token, daten: { freitext: 'Hilfe beim Tragen von Einkäufen', postleitzahl: '3011' },
  });
  assert.equal(erfasst.status, 201);

  const adminMittelland = await testkonto('greta.muster@beispiel.invalid');
  const liste = await aufruf('GET', '/api/anfragen', { token: adminMittelland });
  assert.equal(liste.status, 200);
  assert.ok(!liste.inhalt.some((anfrage) => anfrage.id === erfasst.inhalt.id));
  assert.equal((await aufruf('GET', `/api/anfragen/${erfasst.inhalt.id}`, { token: adminMittelland })).status, 404);

  const koordinationBern = await testkonto('dora.probe@beispiel.invalid');
  assert.equal((await aufruf('GET', `/api/anfragen/${erfasst.inhalt.id}`, { token: koordinationBern })).status, 200);
});
