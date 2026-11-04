// Schnittstellentests: Registrierung und Anmeldung.

import { test } from 'node:test';
import assert from 'node:assert/strict';
import { aufruf, anmelden, neueAdresse, neuesKonto, neuesPasswort, tokenInhalt } from './hilfen.mjs';

const registrierung = (eMail, passwort, weiteres = {}) => ({
  eMail, passwort, anzeigename: 'Test Person', rolle: 'Hilfesuchende', ...weiteres,
});

test('Registrierung ohne Region wird abgewiesen', async () => {
  const { status } = await aufruf('POST', '/api/konto/registrieren', {
    daten: registrierung(neueAdresse(), neuesPasswort()),
  });
  assert.equal(status, 400);
});

test('Registrierung in einer unbekannten Region wird abgewiesen', async () => {
  const { status } = await aufruf('POST', '/api/konto/registrieren', {
    region: 'gibt-es-nicht',
    daten: registrierung(neueAdresse(), neuesPasswort()),
  });
  assert.equal(status, 404);
});

test('Registrierung liefert das Konto ohne Passwort-Hash', async () => {
  const { status, inhalt } = await aufruf('POST', '/api/konto/registrieren', {
    region: 'bern',
    daten: registrierung(neueAdresse(), neuesPasswort()),
  });
  assert.equal(status, 201);
  assert.equal(inhalt.region, 'bern');
  assert.equal(inhalt.rolle, 'Hilfesuchende');
  assert.ok(!('passwortHash' in inhalt));
  assert.ok(!('passwort' in inhalt));
});

test('Eine E-Mail-Adresse gehört zu genau einem Konto, auch in anderer Schreibweise', async () => {
  const adresse = neueAdresse();
  const erste = await aufruf('POST', '/api/konto/registrieren', {
    region: 'bern', daten: registrierung(adresse, neuesPasswort()),
  });
  assert.equal(erste.status, 201);

  const zweite = await aufruf('POST', '/api/konto/registrieren', {
    region: 'mittelland', daten: registrierung(adresse.toUpperCase(), neuesPasswort()),
  });
  assert.equal(zweite.status, 409);
});

test('Nur Hilfesuchende und Helfende können sich selbst registrieren', async () => {
  for (const rolle of ['Koordination', 'Mandantenadministration', 'Plattformbetrieb']) {
    const { status } = await aufruf('POST', '/api/konto/registrieren', {
      region: 'bern', daten: registrierung(neueAdresse(), neuesPasswort(), { rolle }),
    });
    assert.equal(status, 400, rolle);
  }
});

test('Zu kurze Passwörter werden abgewiesen', async () => {
  const { status } = await aufruf('POST', '/api/konto/registrieren', {
    region: 'bern', daten: registrierung(neueAdresse(), 'kurz'),
  });
  assert.equal(status, 400);
});

test('Falsches Passwort und unbekannte Adresse erhalten dieselbe Antwort', async () => {
  const { adresse } = await neuesKonto();
  const falsch = await aufruf('POST', '/api/konto/anmelden', { daten: { eMail: adresse, passwort: neuesPasswort() } });
  const unbekannt = await aufruf('POST', '/api/konto/anmelden', { daten: { eMail: neueAdresse(), passwort: neuesPasswort() } });

  assert.equal(falsch.status, 401);
  assert.equal(unbekannt.status, 401);
  assert.equal(falsch.inhalt.detail, unbekannt.inhalt.detail);
});

test('Das Token enthält nur Kennung, Rolle und Region', async () => {
  const { adresse, passwort, konto } = await neuesKonto();
  const inhalt = tokenInhalt(await anmelden(adresse, passwort));

  assert.equal(inhalt.sub, konto.id);
  assert.equal(inhalt.role, 'Hilfesuchende');
  assert.equal(inhalt.region, 'bern');
  assert.ok(!JSON.stringify(inhalt).includes(adresse), 'keine E-Mail-Adresse im Token');
  assert.ok(!JSON.stringify(inhalt).includes('Test Person'), 'kein Name im Token');
});

test('Eigenes Konto nur mit gültigem Token', async () => {
  const { token, konto } = await neuesKonto();

  assert.equal((await aufruf('GET', '/api/konto/ich')).status, 401);
  assert.equal((await aufruf('GET', '/api/konto/ich', { token: `${token}x` })).status, 401);

  const ich = await aufruf('GET', '/api/konto/ich', { token });
  assert.equal(ich.status, 200);
  assert.equal(ich.inhalt.id, konto.id);
});
