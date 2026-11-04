// Schnittstellentests: Rollen und Berechtigungen.

import { test } from 'node:test';
import assert from 'node:assert/strict';
import { randomUUID } from 'node:crypto';
import { aufruf, neuesKonto, testkonto, OHNE_TESTPASSWORT } from './hilfen.mjs';

test('Region anlegen: ohne Anmeldung 401, mit falscher Rolle 403, als Plattformbetrieb 201', { skip: OHNE_TESTPASSWORT }, async () => {
  const region = { kennung: `test-${randomUUID().slice(0, 8)}`, name: 'Testregion' };
  const { token: hilfesuchende } = await neuesKonto();
  const koordination = await testkonto('dora.probe@beispiel.invalid');
  const betrieb = await testkonto('hans.betrieb@beispiel.invalid');

  assert.equal((await aufruf('POST', '/api/regionen', { daten: region })).status, 401);
  assert.equal((await aufruf('POST', '/api/regionen', { token: hilfesuchende, daten: region })).status, 403);
  assert.equal((await aufruf('POST', '/api/regionen', { token: koordination, daten: region })).status, 403);
  assert.equal((await aufruf('POST', '/api/regionen', { token: betrieb, daten: region })).status, 201);
  assert.equal((await aufruf('POST', '/api/regionen', { token: betrieb, daten: region })).status, 409);

  // Neue Region ohne Neustart nutzbar.
  const aktuell = await aufruf('GET', '/api/regionen/aktuell', { region: region.kennung });
  assert.equal(aktuell.status, 200);
  assert.equal(aktuell.inhalt.kennung, region.kennung);
});

test('Ungültige Kennungen für Regionen werden abgewiesen', { skip: OHNE_TESTPASSWORT }, async () => {
  const betrieb = await testkonto('hans.betrieb@beispiel.invalid');
  for (const kennung of ['A', 'Gross', 'mit leerzeichen', 'ü-zeichen']) {
    const { status } = await aufruf('POST', '/api/regionen', { token: betrieb, daten: { kennung, name: 'Test' } });
    assert.equal(status, 400, kennung);
  }
});

test('Kontenliste nur für Koordination, Mandantenadministration und Plattformbetrieb', { skip: OHNE_TESTPASSWORT }, async () => {
  const { token: hilfesuchende } = await neuesKonto();
  const { token: helfende } = await neuesKonto('bern', 'Helfende');

  assert.equal((await aufruf('GET', '/api/konten')).status, 401);
  assert.equal((await aufruf('GET', '/api/konten', { token: hilfesuchende })).status, 403);
  assert.equal((await aufruf('GET', '/api/konten', { token: helfende })).status, 403);

  for (const adresse of ['dora.probe@beispiel.invalid', 'greta.muster@beispiel.invalid', 'hans.betrieb@beispiel.invalid']) {
    const { status } = await aufruf('GET', '/api/konten', { token: await testkonto(adresse) });
    assert.equal(status, 200, adresse);
  }
});

test('Rollen vergeben nur Mandantenadministration und Plattformbetrieb, nie den Plattformbetrieb selbst', { skip: OHNE_TESTPASSWORT }, async () => {
  const { konto } = await neuesKonto('mittelland', 'Helfende');
  const koordination = await testkonto('dora.probe@beispiel.invalid');
  const admin = await testkonto('greta.muster@beispiel.invalid');
  const pfad = `/api/konten/${konto.id}/rolle`;

  assert.equal((await aufruf('PUT', pfad, { token: koordination, daten: { rolle: 'Koordination' } })).status, 403);
  assert.equal((await aufruf('PUT', pfad, { token: admin, daten: { rolle: 'Plattformbetrieb' } })).status, 400);
  assert.equal((await aufruf('PUT', pfad, { token: admin, daten: { rolle: 'Koordination' } })).status, 204);
});
