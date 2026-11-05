// Schnittstellentests: Mehrwertnummern (Anforderung A10), Trennung kostenlos und kostenpflichtig (Anforderung A12)
// und Erfassen einer Anfrage.

import { test } from 'node:test';
import assert from 'node:assert/strict';
import { aufruf, neueAdresse, neuesKonto, neuesPasswort, BERN, HELFENDE } from './hilfen.mjs';

const mitTelefon = (telefon) => aufruf('POST', '/api/auth/registrierung', {
  daten: { eMail: neueAdresse(), passwort: neuesPasswort(), anzeigename: 'Test Person', kontoart: HELFENDE, mandant: BERN, telefon },
});

const ort = { plz: '3011', ort: 'Bern' };

test('Mehrwertnummern werden in jeder Schreibweise abgewiesen', async () => {
  const nummern = [
    '0900', '0900 123 456', '0901 123 456', '0906 123 456',
    '+41 900 123 456', '+41 901 123 456', '+41 906 123 456',
    '0041 900 123 456', '0041-901-123-456', '0906/123.456', '(0900) 12 34 56',
  ];
  for (const nummer of nummern) {
    const { status, inhalt } = await mitTelefon(nummer);
    assert.equal(status, 400, nummer);
    assert.equal(inhalt.feld, 'telefon', nummer);
    assert.match(inhalt.fehler, /kostenpflichtig/, nummer);
  }
});

test('Ungültige Telefonnummern werden abgewiesen', async () => {
  for (const nummer of ['123', '031 123 45', '+49 30 1234567', 'keine Nummer']) {
    const { status, inhalt } = await mitTelefon(nummer);
    assert.equal(status, 400, nummer);
    assert.equal(inhalt.feld, 'telefon', nummer);
  }
});

test('Gültige Schweizer Nummern werden angenommen', async () => {
  for (const nummer of ['031 123 45 67', '+41 79 123 45 67', '0041 79 123 45 67', '079-123-45-67']) {
    const { status } = await mitTelefon(nummer);
    assert.equal(status, 200, nummer);
  }
});

test('Zehn Kategorien in fester Reihenfolge, kostenlose und kostenpflichtige getrennt abrufbar', async () => {
  const alle = await aufruf('GET', '/api/kategorien');
  const kostenlos = await aufruf('GET', '/api/kategorien?bezahlpflichtig=false');
  const kostenpflichtig = await aufruf('GET', '/api/kategorien?bezahlpflichtig=true');

  assert.equal(alle.status, 200);
  assert.equal(alle.inhalt.length, 10);
  assert.deepEqual(alle.inhalt.map((k) => k.reihenfolge), [1, 2, 3, 4, 5, 6, 7, 8, 9, 10]);
  assert.equal(alle.inhalt[0].code, 'schluesseldienst');
  assert.ok(kostenlos.inhalt.length > 0 && kostenlos.inhalt.every((k) => k.bezahlpflichtig === false));
  assert.ok(kostenpflichtig.inhalt.length > 0 && kostenpflichtig.inhalt.every((k) => k.bezahlpflichtig === true));
  assert.equal(kostenlos.inhalt.length + kostenpflichtig.inhalt.length, alle.inhalt.length);
  assert.ok(alle.inhalt.find((k) => k.code === 'begleitung').nurOrientierung);

  const skills = await aufruf('GET', '/api/skills');
  assert.equal(skills.status, 200);
  assert.equal(skills.inhalt.length, 10);
});

async function kategorie(bezahlpflichtig) {
  const { inhalt } = await aufruf('GET', `/api/kategorien?bezahlpflichtig=${bezahlpflichtig}`);
  return inhalt[0];
}

test('Anfrage nur angemeldet und nur als Hilfesuchende', async () => {
  const daten = { freitext: 'Einkauf für morgen', ...ort };
  const { token: helfende } = await neuesKonto(BERN, HELFENDE);

  assert.equal((await aufruf('POST', '/api/anfragen', { daten })).status, 401);
  assert.equal((await aufruf('POST', '/api/anfragen', { token: helfende, daten })).status, 403);
});

test('Anfrage per Kategorie, per Freitext oder mit beidem', async () => {
  const { token } = await neuesKonto();
  const kostenlos = await kategorie(false);
  const kostenpflichtig = await kategorie(true);

  const perKategorie = await aufruf('POST', '/api/anfragen', { token, daten: { kategorieId: kostenlos.id, ...ort } });
  assert.equal(perKategorie.status, 201);
  assert.equal(perKategorie.inhalt.kategorieId, kostenlos.id);
  assert.equal(perKategorie.inhalt.kategorieBezeichnung, kostenlos.bezeichnung);
  assert.equal(perKategorie.inhalt.bezahlpflichtig, false);
  assert.equal(perKategorie.inhalt.status, 1, 'offen');
  assert.equal(perKategorie.inhalt.prioritaet, 1, 'mittel');
  assert.equal(perKategorie.inhalt.plz, '3011');
  assert.equal(perKategorie.inhalt.ort, 'Bern');

  const perFreitext = await aufruf('POST', '/api/anfragen', {
    token, daten: { freitext: '  Die Heizung ist ausgefallen.  ', plz: '3012', ort: 'Bern', prioritaet: 2 },
  });
  assert.equal(perFreitext.status, 201);
  assert.equal(perFreitext.inhalt.kategorieId, null);
  assert.equal(perFreitext.inhalt.freitext, 'Die Heizung ist ausgefallen.');
  assert.equal(perFreitext.inhalt.prioritaet, 2);

  const beides = await aufruf('POST', '/api/anfragen', {
    token,
    daten: {
      kategorieId: kostenpflichtig.id, freitext: 'Wasserhahn tropft', plz: '3098', ort: 'Köniz',
      adresse: 'Beispielweg 1', zeitfenster: 'heute zwischen 17 und 19 Uhr',
    },
  });
  assert.equal(beides.status, 201);
  assert.equal(beides.inhalt.bezahlpflichtig, true);
  assert.equal(beides.inhalt.adresse, 'Beispielweg 1');
  assert.equal(beides.inhalt.zeitfenster, 'heute zwischen 17 und 19 Uhr');
});

test('Unvollständige oder ungültige Anfragen werden abgewiesen', async () => {
  const { token } = await neuesKonto();
  const faelle = [
    [{ ...ort }, 'freitext'],
    [{ freitext: '   ', ...ort }, 'freitext'],
    [{ freitext: 'Hilfe', plz: '0123', ort: 'Bern' }, 'plz'],
    [{ freitext: 'Hilfe', plz: '301', ort: 'Bern' }, 'plz'],
    [{ freitext: 'Hilfe', plz: 'abcd', ort: 'Bern' }, 'plz'],
    [{ freitext: 'Hilfe', plz: '3011', ort: '   ' }, 'ort'],
    [{ freitext: 'Hilfe', plz: '3011' }, 'ort'],
    [{ freitext: 'Hilfe', ...ort, prioritaet: 5 }, 'prioritaet'],
    [{ freitext: 'x'.repeat(2001), ...ort }, 'freitext'],
    [{ kategorieId: '00000000-0000-0000-0000-000000000001', ...ort }, 'kategorieId'],
  ];
  for (const [daten, feld] of faelle) {
    const { status, inhalt } = await aufruf('POST', '/api/anfragen', { token, daten });
    const name = JSON.stringify(daten).slice(0, 60);
    assert.equal(status, 400, name);
    assert.equal(inhalt.feld, feld, name);
    assert.ok(inhalt.fehler, name);
  }
});

test('Hilfesuchende sehen nur ihre eigenen Anfragen', async () => {
  const erste = await neuesKonto();
  const zweite = await neuesKonto();
  const erfasst = await aufruf('POST', '/api/anfragen', {
    token: erste.token, daten: { freitext: 'Begleitung zum Arzttermin', ...ort },
  });

  for (const pfad of ['/api/anfragen/meine', '/api/anfragen']) {
    const eigene = await aufruf('GET', pfad, { token: erste.token });
    assert.ok(eigene.inhalt.some((anfrage) => anfrage.id === erfasst.inhalt.id), pfad);
    const fremde = await aufruf('GET', pfad, { token: zweite.token });
    assert.ok(!fremde.inhalt.some((anfrage) => anfrage.id === erfasst.inhalt.id), pfad);
  }

  assert.equal((await aufruf('GET', `/api/anfragen/${erfasst.inhalt.id}`, { token: erste.token })).status, 200);
  assert.equal((await aufruf('GET', `/api/anfragen/${erfasst.inhalt.id}`, { token: zweite.token })).status, 404);
});
