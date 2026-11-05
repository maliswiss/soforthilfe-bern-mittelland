// Schnittstellentests: Registrierung und Anmeldung.

import { test } from 'node:test';
import assert from 'node:assert/strict';
import {
  aufruf, anmelden, neueAdresse, neuesKonto, neuesPasswort, tokenInhalt, BERN, BIEL, HELFENDE,
} from './hilfen.mjs';

const registrierung = (eMail, passwort, weiteres = {}) => ({
  eMail, passwort, anzeigename: 'Test Person', kontoart: 0, mandant: BERN, ...weiteres,
});

test('Registrierung in einer unbekannten Region wird abgewiesen', async () => {
  const { status, inhalt } = await aufruf('POST', '/api/auth/registrierung', {
    daten: registrierung(neueAdresse(), neuesPasswort(), { mandant: 'gibt-es-nicht' }),
  });
  assert.equal(status, 400);
  assert.equal(inhalt.feld, 'mandant');
  assert.ok(inhalt.fehler);
});

test('Registrierung liefert Token und Konto ohne Passwort-Hash', async () => {
  const { status, inhalt } = await aufruf('POST', '/api/auth/registrierung', {
    daten: registrierung(neueAdresse(), neuesPasswort()),
  });
  assert.equal(status, 200);
  assert.ok(inhalt.token);
  assert.ok(inhalt.gueltigBis);
  assert.equal(inhalt.benutzer.mandantSchluessel, BERN);
  assert.equal(inhalt.benutzer.mandantName, 'Soforthilfe Bern/Mittelland');
  assert.deepEqual(inhalt.benutzer.rollen, ['Hilfesuchende']);
  assert.equal(inhalt.benutzer.hatHelferProfil, false);
  const text = JSON.stringify(inhalt);
  assert.ok(!text.includes('passwortHash') && !text.includes('"passwort"'));
});

test('Ohne Angabe gilt die Region Bern/Mittelland, Helfende erhalten die Rolle Helfende', async () => {
  const daten = registrierung(neueAdresse(), neuesPasswort(), { kontoart: HELFENDE });
  delete daten.mandant;
  const { status, inhalt } = await aufruf('POST', '/api/auth/registrierung', { daten });
  assert.equal(status, 200);
  assert.equal(inhalt.benutzer.mandantSchluessel, BERN);
  assert.deepEqual(inhalt.benutzer.rollen, ['Helfende']);
});

test('Eine E-Mail-Adresse gehört zu genau einem Konto, auch in anderer Schreibweise und Region', async () => {
  const adresse = neueAdresse();
  const erste = await aufruf('POST', '/api/auth/registrierung', { daten: registrierung(adresse, neuesPasswort()) });
  assert.equal(erste.status, 200);

  const zweite = await aufruf('POST', '/api/auth/registrierung', {
    daten: registrierung(adresse.toUpperCase(), neuesPasswort(), { mandant: BIEL }),
  });
  assert.equal(zweite.status, 409);
  assert.equal(zweite.inhalt.feld, 'eMail');
});

test('Nur «Hilfe suchen» und «Hilfe anbieten» sind als Kontoart erlaubt', async () => {
  for (const kontoart of [2, 3, 4]) {
    const { status } = await aufruf('POST', '/api/auth/registrierung', {
      daten: registrierung(neueAdresse(), neuesPasswort(), { kontoart }),
    });
    assert.equal(status, 400, String(kontoart));
  }
});

test('Zu kurze Passwörter werden mit Hinweis auf das Feld abgewiesen', async () => {
  const { status, inhalt } = await aufruf('POST', '/api/auth/registrierung', {
    daten: registrierung(neueAdresse(), 'kurz'),
  });
  assert.equal(status, 400);
  assert.equal(inhalt.feld, 'passwort');
  assert.ok(inhalt.fehler);
});

test('Falsches Passwort, unbekannte Adresse und fremde Region erhalten dieselbe Antwort', async () => {
  const { adresse, passwort } = await neuesKonto();
  const falsch = await aufruf('POST', '/api/auth/anmeldung', { daten: { eMail: adresse, passwort: neuesPasswort(), mandant: BERN } });
  const unbekannt = await aufruf('POST', '/api/auth/anmeldung', { daten: { eMail: neueAdresse(), passwort: neuesPasswort(), mandant: BERN } });
  const fremd = await aufruf('POST', '/api/auth/anmeldung', { daten: { eMail: adresse, passwort, mandant: BIEL } });

  for (const antwort of [falsch, unbekannt, fremd]) {
    assert.equal(antwort.status, 401);
    assert.equal(antwort.inhalt.fehler, falsch.inhalt.fehler);
  }
  assert.equal((await aufruf('POST', '/api/auth/anmeldung', { daten: { eMail: adresse, passwort, mandant: BERN } })).status, 200);
});

test('Das Token enthält nur Kennung, Rolle und Region', async () => {
  const { adresse, passwort, konto } = await neuesKonto();
  const inhalt = tokenInhalt(await anmelden(adresse, passwort));

  assert.equal(inhalt.sub, konto.id);
  assert.equal(inhalt.role, 'Hilfesuchende');
  assert.equal(inhalt.region, BERN);
  assert.ok(!JSON.stringify(inhalt).includes(adresse), 'keine E-Mail-Adresse im Token');
  assert.ok(!JSON.stringify(inhalt).includes('Test Person'), 'kein Name im Token');
});

test('Eigenes Konto nur mit gültigem Token', async () => {
  const { token, konto } = await neuesKonto();

  const ohne = await aufruf('GET', '/api/auth/ich');
  assert.equal(ohne.status, 401);
  assert.ok(ohne.inhalt.fehler, 'auch ohne Anmeldung ein Text für die Oberfläche');
  assert.equal((await aufruf('GET', '/api/auth/ich', { token: `${token}x` })).status, 401);

  const ich = await aufruf('GET', '/api/auth/ich', { token });
  assert.equal(ich.status, 200);
  assert.equal(ich.inhalt.id, konto.id);
  assert.equal(ich.inhalt.mandantSchluessel, BERN);
});
