// Schnittstellentests: Mehrwertnummern (Anforderung A10), Trennung kostenlos und kostenpflichtig (Anforderung A12)
// und Erfassen einer Anfrage.

import { test } from 'node:test';
import assert from 'node:assert/strict';
import { aufruf, neueAdresse, neuesKonto, neuesPasswort } from './hilfen.mjs';

const mitTelefon = (telefon) => aufruf('POST', '/api/konto/registrieren', {
  region: 'bern',
  daten: { eMail: neueAdresse(), passwort: neuesPasswort(), anzeigename: 'Test Person', rolle: 'Helfende', telefon },
});

test('Mehrwertnummern werden in jeder Schreibweise abgewiesen', async () => {
  const nummern = [
    '0900 123 456', '0901 123 456', '0906 123 456',
    '+41 900 123 456', '+41 901 123 456', '+41 906 123 456',
    '0041 900 123 456', '0041-901-123-456', '0906/123.456', '(0900) 12 34 56',
  ];
  for (const nummer of nummern) {
    const { status, inhalt } = await mitTelefon(nummer);
    assert.equal(status, 400, nummer);
    assert.match(inhalt.title, /Kostenpflichtige Telefonnummer/, nummer);
  }
});

test('Ungültige Telefonnummern werden abgewiesen', async () => {
  for (const nummer of ['123', '031 123 45', '+49 30 1234567', 'keine Nummer']) {
    const { status } = await mitTelefon(nummer);
    assert.equal(status, 400, nummer);
  }
});

test('Gültige Schweizer Nummern werden angenommen', async () => {
  for (const nummer of ['031 123 45 67', '+41 79 123 45 67', '0041 79 123 45 67', '079-123-45-67']) {
    const { status } = await mitTelefon(nummer);
    assert.equal(status, 201, nummer);
  }
});

test('Kostenlose und kostenpflichtige Kategorien sind getrennt abrufbar', async () => {
  const alle = await aufruf('GET', '/api/kategorien');
  const kostenlos = await aufruf('GET', '/api/kategorien?kostenpflichtig=false');
  const kostenpflichtig = await aufruf('GET', '/api/kategorien?kostenpflichtig=true');

  assert.equal(alle.status, 200);
  assert.ok(kostenlos.inhalt.length > 0 && kostenlos.inhalt.every((k) => k.kostenpflichtig === false));
  assert.ok(kostenpflichtig.inhalt.length > 0 && kostenpflichtig.inhalt.every((k) => k.kostenpflichtig === true));
  assert.equal(kostenlos.inhalt.length + kostenpflichtig.inhalt.length, alle.inhalt.length);
  assert.ok(alle.inhalt.every((k) => Array.isArray(k.skills) && k.skills.length > 0));
});

async function kategorie(kostenpflichtig) {
  const { inhalt } = await aufruf('GET', `/api/kategorien?kostenpflichtig=${kostenpflichtig}`);
  return inhalt[0];
}

test('Anfrage nur angemeldet und nur als Hilfesuchende', async () => {
  const daten = { freitext: 'Einkauf für morgen', postleitzahl: '3011' };
  const { token: helfende } = await neuesKonto('bern', 'Helfende');

  assert.equal((await aufruf('POST', '/api/anfragen', { daten })).status, 401);
  assert.equal((await aufruf('POST', '/api/anfragen', { token: helfende, daten })).status, 403);
});

test('Anfrage per Kategorie, per Freitext oder mit beidem', async () => {
  const { token } = await neuesKonto('bern');
  const kostenlos = await kategorie(false);
  const kostenpflichtig = await kategorie(true);

  const perKategorie = await aufruf('POST', '/api/anfragen', { token, daten: { kategorieId: kostenlos.id, postleitzahl: '3011' } });
  assert.equal(perKategorie.status, 201);
  assert.equal(perKategorie.inhalt.kategorieKennung, kostenlos.kennung);
  assert.equal(perKategorie.inhalt.kostenpflichtig, false);
  assert.equal(perKategorie.inhalt.status, 'Offen');

  const perFreitext = await aufruf('POST', '/api/anfragen', { token, daten: { freitext: '  Die Heizung ist ausgefallen.  ', postleitzahl: '3012' } });
  assert.equal(perFreitext.status, 201);
  assert.equal(perFreitext.inhalt.kategorieKennung, null);
  assert.equal(perFreitext.inhalt.freitext, 'Die Heizung ist ausgefallen.');

  const beides = await aufruf('POST', '/api/anfragen', {
    token, daten: { kategorieId: kostenpflichtig.id, freitext: 'Wasserhahn tropft', postleitzahl: '3098' },
  });
  assert.equal(beides.status, 201);
  assert.equal(beides.inhalt.kostenpflichtig, true);
});

test('Unvollständige oder ungültige Anfragen werden abgewiesen', async () => {
  const { token } = await neuesKonto('bern');
  const faelle = [
    { postleitzahl: '3011' },
    { freitext: '   ', postleitzahl: '3011' },
    { freitext: 'Hilfe', postleitzahl: '0123' },
    { freitext: 'Hilfe', postleitzahl: '301' },
    { freitext: 'Hilfe', postleitzahl: 'abcd' },
    { freitext: 'x'.repeat(2001), postleitzahl: '3011' },
    { kategorieId: '00000000-0000-0000-0000-000000000001', postleitzahl: '3011' },
  ];
  for (const daten of faelle) {
    const { status } = await aufruf('POST', '/api/anfragen', { token, daten });
    assert.equal(status, 400, JSON.stringify(daten).slice(0, 60));
  }
});

test('Hilfesuchende sehen nur ihre eigenen Anfragen', async () => {
  const erste = await neuesKonto('bern');
  const zweite = await neuesKonto('bern');
  const erfasst = await aufruf('POST', '/api/anfragen', {
    token: erste.token, daten: { freitext: 'Begleitung zum Arzttermin', postleitzahl: '3011' },
  });

  const eigene = await aufruf('GET', '/api/anfragen', { token: erste.token });
  assert.ok(eigene.inhalt.some((anfrage) => anfrage.id === erfasst.inhalt.id));
  assert.equal((await aufruf('GET', `/api/anfragen/${erfasst.inhalt.id}`, { token: erste.token })).status, 200);

  const fremde = await aufruf('GET', '/api/anfragen', { token: zweite.token });
  assert.ok(!fremde.inhalt.some((anfrage) => anfrage.id === erfasst.inhalt.id));
  assert.equal((await aufruf('GET', `/api/anfragen/${erfasst.inhalt.id}`, { token: zweite.token })).status, 404);
});
