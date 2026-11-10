// Schnittstellentests: Rollen und Berechtigungen.

import { test } from 'node:test';
import assert from 'node:assert/strict';
import { randomUUID } from 'node:crypto';
import {
  aufruf, neueAdresse, neuesKonto, neuesPasswort, testkonto, BIEL, HELFENDE, OHNE_TESTPASSWORT,
} from './hilfen.mjs';

test('Region anlegen: ohne Anmeldung 401, mit falscher Rolle 403, als Plattformbetrieb 201', { skip: OHNE_TESTPASSWORT }, async () => {
  const region = { schluessel: `test-${randomUUID().slice(0, 8)}`, name: 'Soforthilfe Testregion' };
  const { token: hilfesuchende } = await neuesKonto();
  const koordination = await testkonto('dora.probe@beispiel.invalid');
  const betrieb = await testkonto('hans.betrieb@beispiel.invalid');

  assert.equal((await aufruf('POST', '/api/mandanten', { daten: region })).status, 401);
  assert.equal((await aufruf('POST', '/api/mandanten', { token: hilfesuchende, daten: region })).status, 403);
  assert.equal((await aufruf('POST', '/api/mandanten', { token: koordination, daten: region })).status, 403);

  const angelegt = await aufruf('POST', '/api/mandanten', { token: betrieb, daten: region });
  assert.equal(angelegt.status, 201);
  assert.equal(angelegt.inhalt.schluessel, region.schluessel);

  const doppelt = await aufruf('POST', '/api/mandanten', { token: betrieb, daten: region });
  assert.equal(doppelt.status, 409);
  assert.equal(doppelt.inhalt.feld, 'schluessel');

  // Neue Region ohne Neustart nutzbar: in der Liste und für die Registrierung.
  const liste = await aufruf('GET', '/api/mandanten');
  assert.ok(liste.inhalt.some((eintrag) => eintrag.schluessel === region.schluessel));
  const konto = await aufruf('POST', '/api/auth/registrierung', {
    daten: { eMail: neueAdresse(), passwort: neuesPasswort(), anzeigename: 'Test Person', mandant: region.schluessel },
  });
  assert.equal(konto.status, 200);
  assert.equal(konto.inhalt.benutzer.mandantSchluessel, region.schluessel);
});

test('Ungültige Schlüssel für Regionen werden abgewiesen', { skip: OHNE_TESTPASSWORT }, async () => {
  const betrieb = await testkonto('hans.betrieb@beispiel.invalid');
  for (const schluessel of ['a', 'Gross', 'mit leerzeichen', 'ü-zeichen', '-vorne', 'doppelt--strich']) {
    const { status } = await aufruf('POST', '/api/mandanten', { token: betrieb, daten: { schluessel, name: 'Test' } });
    assert.equal(status, 400, schluessel);
  }
});

test('Gemeinden einer neuen Region dürfen zu keiner anderen Region gehören', { skip: OHNE_TESTPASSWORT }, async () => {
  const betrieb = await testkonto('hans.betrieb@beispiel.invalid');
  const neu = (gemeinden) => aufruf('POST', '/api/mandanten', {
    token: betrieb, daten: { schluessel: `test-${randomUUID().slice(0, 8)}`, name: 'Soforthilfe Testregion', gemeinden },
  });

  // 351 ist die Gemeinde Bern und gehört zu Bern/Mittelland.
  const doppelt = await neu([351]);
  assert.equal(doppelt.status, 409);
  assert.equal(doppelt.inhalt.feld, 'gemeinden');

  assert.equal((await neu([0])).status, 400);
  assert.equal((await neu([10000])).status, 400);
});

test('Kontenliste nur für Koordination, Mandantenadministration und Plattformbetrieb', { skip: OHNE_TESTPASSWORT }, async () => {
  const { token: hilfesuchende } = await neuesKonto();
  const { token: helfende } = await neuesKonto(undefined, HELFENDE);

  assert.equal((await aufruf('GET', '/api/konten')).status, 401);
  assert.equal((await aufruf('GET', '/api/konten', { token: hilfesuchende })).status, 403);
  assert.equal((await aufruf('GET', '/api/konten', { token: helfende })).status, 403);

  for (const adresse of ['dora.probe@beispiel.invalid', 'greta.muster@beispiel.invalid', 'hans.betrieb@beispiel.invalid']) {
    const { status, inhalt } = await aufruf('GET', '/api/konten', { token: await testkonto(adresse) });
    assert.equal(status, 200, adresse);
    assert.ok(inhalt.every((konto) => !('telefon' in konto)), 'keine Telefonnummern in der Liste');
  }
});

test('Rollen vergeben nur Mandantenadministration und Plattformbetrieb, nie den Plattformbetrieb selbst', { skip: OHNE_TESTPASSWORT }, async () => {
  const { konto } = await neuesKonto(BIEL, HELFENDE);
  const koordination = await testkonto('dora.probe@beispiel.invalid');
  const admin = await testkonto('greta.muster@beispiel.invalid');
  const pfad = `/api/konten/${konto.id}/rolle`;

  assert.equal((await aufruf('PUT', pfad, { token: koordination, daten: { rolle: 'Koordination' } })).status, 403);
  assert.equal((await aufruf('PUT', pfad, { token: admin, daten: { rolle: 'Plattformbetrieb' } })).status, 400);
  assert.equal((await aufruf('PUT', pfad, { token: admin, daten: { rolle: 'Koordination' } })).status, 204);
});
