// Schnittstellentests: Verifizierung ohne Ausweisdokumente über UID-Nummer, Handelsregister, Telefonnummer,
// E-Mail-Adresse und Referenz.

import { test } from 'node:test';
import assert from 'node:assert/strict';
import { aufruf, BASIS, neuesKonto, testkonto, BERN, HELFENDE, OHNE_TESTPASSWORT } from './hilfen.mjs';

const UID = 0;
const HANDELSREGISTER = 1;
const TELEFON = 2;
const EMAIL = 3;
const REFERENZ = 4;

const einreichen = (token, art, nachweiswert) => aufruf('POST', '/api/verifizierungen', { token, daten: { art, nachweiswert } });
const meine = async (token) => (await aufruf('GET', '/api/verifizierungen/meine', { token })).inhalt;

// Helfende Person mit gespeichertem Profil (kostenlose Fähigkeit, damit keine Preise nötig sind).
async function helfendeMitProfil(zusatz = {}) {
  const konto = await neuesKonto(BERN, HELFENDE, zusatz);
  const skills = (await aufruf('GET', '/api/skills')).inhalt;
  const begleitung = skills.find((s) => s.code === 'begleitung');
  const profil = await aufruf('PUT', '/api/helferprofil', {
    token: konto.token,
    daten: { typ: 1, betriebsname: 'Muster Begleitdienst GmbH', plz: '3011', ort: 'Bern', radiusKm: 10, skillIds: [begleitung.id] },
  });
  assert.equal(profil.status, 200);
  assert.equal(profil.inhalt.verifizierung, 0);
  return konto;
}

test('Nachweise nur angemeldet, nur als Helfende und nur mit Helferprofil', async () => {
  assert.equal((await aufruf('POST', '/api/verifizierungen', { daten: { art: UID, nachweiswert: 'CHE-123.456.788' } })).status, 401);

  const suchende = await neuesKonto(BERN);
  assert.equal((await einreichen(suchende.token, UID, 'CHE-123.456.788')).status, 403);

  const ohneProfil = await neuesKonto(BERN, HELFENDE);
  const antwort = await einreichen(ohneProfil.token, UID, 'CHE-123.456.788');
  assert.equal(antwort.status, 400);
  assert.equal(antwort.inhalt.fehler, 'Es besteht noch kein Helferprofil.');
});

test('UID-Nummer wird geprüft, normalisiert und zur Prüfung vorgemerkt', async () => {
  const { token } = await helfendeMitProfil();

  const antwort = await einreichen(token, UID, 'che 123 456 788');
  assert.equal(antwort.status, 200);
  assert.equal(antwort.inhalt.art, UID);
  assert.equal(antwort.inhalt.nachweiswert, 'CHE-123.456.788');
  assert.equal(antwort.inhalt.status, 1);
  assert.equal(antwort.inhalt.geprueftAm, null);
  assert.equal(antwort.inhalt.anzeigename, 'Test Person');

  // Das Profil steht jetzt in Prüfung
  assert.equal((await aufruf('GET', '/api/helferprofil', { token })).inhalt.verifizierung, 1);
});

test('Handelsregister über die UID des eingetragenen Unternehmens', async () => {
  const { token } = await helfendeMitProfil();
  const antwort = await einreichen(token, HANDELSREGISTER, 'CHE-123.456.788 HR');
  assert.equal(antwort.status, 200);
  assert.equal(antwort.inhalt.art, HANDELSREGISTER);
  assert.equal(antwort.inhalt.nachweiswert, 'CHE-123.456.788');
});

test('Ungültige Nachweise werden mit Hinweis auf das Feld abgewiesen und nicht gespeichert', async () => {
  const { token } = await helfendeMitProfil();
  const fall = async (art, wert, feld) => {
    const { status, inhalt } = await einreichen(token, art, wert);
    assert.equal(status, 400, `${art} ${wert}`);
    assert.equal(inhalt.feld?.toLowerCase(), feld, `${art} ${wert}`);
  };

  await fall(UID, 'CHE-123.456.789', 'nachweiswert');   // falsche Prüfziffer
  await fall(UID, 'CHE-12.345.678', 'nachweiswert');
  await fall(HANDELSREGISTER, 'Muster AG, Bern', 'nachweiswert');
  await fall(UID, '   ', 'nachweiswert');
  await fall(UID, null, 'nachweiswert');
  await fall(9, 'CHE-123.456.788', 'art');

  assert.deepEqual(await meine(token), []);
  assert.equal((await aufruf('GET', '/api/helferprofil', { token })).inhalt.verifizierung, 0);
});

test('Ein offener Nachweis derselben Art wird ersetzt, nicht verdoppelt', async () => {
  const { token } = await helfendeMitProfil();
  await einreichen(token, UID, 'CHE-100.000.006');
  await einreichen(token, HANDELSREGISTER, 'CHE-100.000.006');
  await einreichen(token, UID, 'CHE-123.456.788');

  const liste = await meine(token);
  assert.equal(liste.length, 2);
  assert.equal(liste.find((v) => v.art === UID).nachweiswert, 'CHE-123.456.788');
});

test('Telefonnummer: ohne Angabe die des Kontos, sonst die angegebene, immer lesbar vereinheitlicht', async () => {
  const { token } = await helfendeMitProfil({ telefon: '+41 31 123 45 67' });

  const ausKonto = await einreichen(token, TELEFON, null);
  assert.equal(ausKonto.status, 200);
  assert.equal(ausKonto.inhalt.nachweiswert, '031 123 45 67');
  assert.equal(ausKonto.inhalt.status, 1);

  const andere = await einreichen(token, TELEFON, '0041 79 123 45 67');
  assert.equal(andere.status, 200);
  assert.equal(andere.inhalt.nachweiswert, '079 123 45 67');

  // Der offene Nachweis wurde ersetzt
  assert.equal((await meine(token)).length, 1);
});

test('Mehrwertnummern und ungültige Nummern werden abgewiesen, ohne Nummer im Konto fehlt der Wert', async () => {
  const { token } = await helfendeMitProfil();
  const fall = async (wert, meldung) => {
    const { status, inhalt } = await einreichen(token, TELEFON, wert);
    assert.equal(status, 400, String(wert));
    assert.equal(inhalt.feld?.toLowerCase(), 'nachweiswert', String(wert));
    assert.match(inhalt.fehler, meldung, String(wert));
  };

  await fall(null, /erforderlich/);
  await fall('0900 123 456', /kostenpflichtig/);
  await fall('+41 901 123 456', /kostenpflichtig/);
  await fall('+49 30 1234567', /Schweizer Telefonnummer/);
  await fall('031 123 45', /Schweizer Telefonnummer/);

  assert.deepEqual(await meine(token), []);
});

test('E-Mail-Adresse: bestätigt wird immer die Adresse des Kontos', async () => {
  const { token, adresse } = await helfendeMitProfil();

  const ohneAngabe = await einreichen(token, EMAIL, null);
  assert.equal(ohneAngabe.status, 200);
  assert.equal(ohneAngabe.inhalt.nachweiswert, adresse);

  assert.equal((await einreichen(token, EMAIL, adresse.toUpperCase())).status, 200);

  const andere = await einreichen(token, EMAIL, 'andere.adresse@beispiel.invalid');
  assert.equal(andere.status, 400);
  assert.equal(andere.inhalt.feld?.toLowerCase(), 'nachweiswert');
  assert.equal((await meine(token)).length, 1);
});

test('Referenz einer Organisation oder Gemeinde mit Kontaktperson', async () => {
  const { token } = await helfendeMitProfil();

  const antwort = await einreichen(token, REFERENZ, '  Gemeinde   Muri,\n Frau Muster ');
  assert.equal(antwort.status, 200);
  assert.equal(antwort.inhalt.art, REFERENZ);
  assert.equal(antwort.inhalt.nachweiswert, 'Gemeinde Muri, Frau Muster');

  for (const wert of [null, '   ', 'SRK']) {
    const { status, inhalt } = await einreichen(token, REFERENZ, wert);
    assert.equal(status, 400, String(wert));
    assert.equal(inhalt.feld?.toLowerCase(), 'nachweiswert', String(wert));
  }
});

test('Keine Ausweisdokumente: Uploads werden nicht angenommen', async () => {
  const { token } = await helfendeMitProfil();
  const formular = new FormData();
  formular.append('art', '0');
  formular.append('nachweiswert', 'CHE-123.456.788');
  formular.append('dokument', new Blob(['%PDF-1.4 Ausweis'], { type: 'application/pdf' }), 'ausweis.pdf');

  const antwort = await fetch(`${BASIS}/api/verifizierungen`, {
    method: 'POST',
    headers: { Authorization: `Bearer ${token}` },
    body: formular,
  });
  assert.equal(antwort.status, 415);
  assert.deepEqual(await meine(token), []);
});

test('Einreichen steht im Protokoll, ohne den Wert des Nachweises', { skip: OHNE_TESTPASSWORT }, async () => {
  const { token } = await helfendeMitProfil();
  const { inhalt } = await einreichen(token, UID, 'CHE-123.456.788');

  const koordination = await testkonto('dora.probe@beispiel.invalid');
  const eintrag = (await aufruf('GET', '/api/protokoll?aktion=Verifizierung.Eingereicht&anzahl=500', { token: koordination })).inhalt
    .find((e) => e.objektId === inhalt.id);
  assert.ok(eintrag);
  assert.equal(eintrag.detail, 'UidNummer');
  assert.equal(eintrag.region, BERN);
  assert.ok(!JSON.stringify(eintrag).includes('CHE-'));
});
