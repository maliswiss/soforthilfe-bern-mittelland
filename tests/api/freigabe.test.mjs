// Schnittstellentests: Die Koordination prüft Nachweise ihrer Region und gibt Helferprofile frei oder lehnt sie ab.

import { test } from 'node:test';
import assert from 'node:assert/strict';
import { aufruf, anmelden, neuesKonto, testkonto, BERN, BIEL, HELFENDE, OHNE_TESTPASSWORT } from './hilfen.mjs';

const UID = 0;
const REFERENZ = 4;
const IN_PRUEFUNG = 1;
const VERIFIZIERT = 2;
const ABGELEHNT = 3;

const einreichen = (token, art, nachweiswert) => aufruf('POST', '/api/verifizierungen', { token, daten: { art, nachweiswert } });
const offen = async (token) => (await aufruf('GET', '/api/verifizierungen/offen', { token })).inhalt;
const entscheiden = (token, id, freigeben, bemerkung = null) =>
  aufruf('POST', `/api/verifizierungen/${id}/entscheid`, { token, daten: { freigeben, bemerkung } });
const profilstatus = async (token) => (await aufruf('GET', '/api/helferprofil', { token })).inhalt.verifizierung;

// Betrieb mit gespeichertem Profil in Bern/Mittelland (kostenlose Fähigkeit, damit keine Preise nötig sind).
async function betrieb(betriebsname = 'Muster Begleitdienst GmbH') {
  const konto = await neuesKonto(BERN, HELFENDE);
  const skills = (await aufruf('GET', '/api/skills')).inhalt;
  const begleitung = skills.find((s) => s.code === 'begleitung');
  const profil = { typ: 1, betriebsname, plz: '3011', ort: 'Bern', radiusKm: 10, skillIds: [begleitung.id] };
  const speichern = (aenderung = {}) => aufruf('PUT', '/api/helferprofil', { token: konto.token, daten: { ...profil, ...aenderung } });
  assert.equal((await speichern()).status, 200);
  return { ...konto, speichern };
}

const koordination = () => testkonto('dora.probe@beispiel.invalid');

test('Offene Nachweise und Entscheide nur für die Koordination', async () => {
  const id = '00000000-0000-0000-0000-000000000001';
  assert.equal((await aufruf('GET', '/api/verifizierungen/offen')).status, 401);
  assert.equal((await entscheiden(undefined, id, true)).status, 401);

  for (const kontoart of [0, HELFENDE]) {
    const { token } = await neuesKonto(BERN, kontoart);
    assert.equal((await aufruf('GET', '/api/verifizierungen/offen', { token })).status, 403);
    assert.equal((await entscheiden(token, id, true)).status, 403);
  }
});

test('Die Koordination sieht offene Nachweise mit Angaben zum Profil, älteste zuerst', { skip: OHNE_TESTPASSWORT }, async () => {
  const { token } = await betrieb();
  const { inhalt } = await einreichen(token, UID, 'CHE-123.456.788');

  const liste = await offen(await koordination());
  const eintrag = liste.find((v) => v.id === inhalt.id);
  assert.ok(eintrag);
  assert.equal(eintrag.anzeigename, 'Test Person');
  assert.equal(eintrag.art, UID);
  assert.equal(eintrag.nachweiswert, 'CHE-123.456.788');
  assert.equal(eintrag.status, IN_PRUEFUNG);
  assert.equal(eintrag.typ, 1);
  assert.equal(eintrag.betriebsname, 'Muster Begleitdienst GmbH');
  assert.equal(eintrag.plz, '3011');
  assert.equal(eintrag.ort, 'Bern');
  assert.equal(eintrag.profilstatus, IN_PRUEFUNG);

  assert.ok(liste.every((v) => v.status === IN_PRUEFUNG));
  const zeiten = liste.map((v) => Date.parse(v.eingereichtAm));
  assert.deepEqual(zeiten, [...zeiten].sort((a, b) => a - b));
});

test('Freigabe macht das Profil verifiziert und steht im Protokoll ohne Wert und Bemerkung', { skip: OHNE_TESTPASSWORT }, async () => {
  const { token } = await betrieb();
  const { inhalt } = await einreichen(token, UID, 'CHE-123.456.788');
  const dora = await koordination();

  const antwort = await entscheiden(dora, inhalt.id, true, '  Eintrag im Register geprüft ');
  assert.equal(antwort.status, 200);
  assert.equal(antwort.inhalt.status, VERIFIZIERT);
  assert.equal(antwort.inhalt.bemerkung, 'Eintrag im Register geprüft');
  assert.ok(antwort.inhalt.geprueftAm);

  assert.equal(await profilstatus(token), VERIFIZIERT);
  const meine = (await aufruf('GET', '/api/verifizierungen/meine', { token })).inhalt;
  assert.equal(meine[0].status, VERIFIZIERT);
  assert.equal(meine[0].bemerkung, 'Eintrag im Register geprüft');
  assert.ok(!(await offen(dora)).some((v) => v.id === inhalt.id));

  // Ein zweiter Entscheid ist nicht möglich
  assert.equal((await entscheiden(dora, inhalt.id, false)).status, 409);

  const eintrag = (await aufruf('GET', '/api/protokoll?aktion=Verifizierung.Freigegeben&anzahl=500', { token: dora })).inhalt
    .find((e) => e.objektId === inhalt.id);
  assert.ok(eintrag);
  assert.equal(eintrag.detail, 'UidNummer');
  assert.ok(!JSON.stringify(eintrag).includes('CHE-') && !JSON.stringify(eintrag).includes('Register'));
});

test('Ablehnung: in Prüfung, solange weitere Nachweise offen sind, danach abgelehnt', { skip: OHNE_TESTPASSWORT }, async () => {
  const { token } = await betrieb();
  const uid = (await einreichen(token, UID, 'CHE-123.456.788')).inhalt;
  const referenz = (await einreichen(token, REFERENZ, 'Gemeinde Muri, Frau Muster')).inhalt;
  const dora = await koordination();

  assert.equal((await entscheiden(dora, uid.id, false, 'Nummer gehört zu einem anderen Betrieb')).status, 200);
  assert.equal(await profilstatus(token), IN_PRUEFUNG);

  assert.equal((await entscheiden(dora, referenz.id, false)).inhalt.status, ABGELEHNT);
  assert.equal(await profilstatus(token), ABGELEHNT);

  // Mit einem neuen Nachweis steht das Profil wieder in Prüfung
  await einreichen(token, UID, 'CHE-100.000.006');
  assert.equal(await profilstatus(token), IN_PRUEFUNG);
});

test('Die Ablehnung eines Zusatznachweises lässt ein verifiziertes Profil verifiziert', { skip: OHNE_TESTPASSWORT }, async () => {
  const { token } = await betrieb();
  const dora = await koordination();
  await entscheiden(dora, (await einreichen(token, UID, 'CHE-123.456.788')).inhalt.id, true);

  const referenz = (await einreichen(token, REFERENZ, 'Gemeinde Muri, Frau Muster')).inhalt;
  assert.equal(await profilstatus(token), IN_PRUEFUNG);
  await entscheiden(dora, referenz.id, false);
  assert.equal(await profilstatus(token), VERIFIZIERT);
});

test('Nach einer prüfrelevanten Änderung liegen die freigegebenen Nachweise wieder zur Prüfung vor', { skip: OHNE_TESTPASSWORT }, async () => {
  const { token, speichern } = await betrieb();
  const dora = await koordination();
  const uid = (await einreichen(token, UID, 'CHE-123.456.788')).inhalt;
  await entscheiden(dora, uid.id, true);

  // Radius und Beschreibung verlangen keine neue Prüfung
  assert.equal((await speichern({ radiusKm: 25, beschreibung: 'Begleitung zu Terminen' })).inhalt.verifizierung, VERIFIZIERT);

  // Ein neuer Betriebsname schon
  assert.equal((await speichern({ betriebsname: 'Neue Begleitung AG' })).inhalt.verifizierung, IN_PRUEFUNG);
  const wieder = (await offen(dora)).find((v) => v.id === uid.id);
  assert.ok(wieder);
  assert.equal(wieder.betriebsname, 'Neue Begleitung AG');
  assert.equal(wieder.geprueftAm, null);

  await entscheiden(dora, uid.id, true);
  assert.equal(await profilstatus(token), VERIFIZIERT);
});

test('Nachweise einer anderen Region gelten als nicht vorhanden', { skip: OHNE_TESTPASSWORT }, async () => {
  const { token } = await betrieb();
  const { inhalt } = await einreichen(token, UID, 'CHE-123.456.788');

  // Koordination für Biel/Seeland: neues Konto, Rolle durch die Mandantenadministration der Region
  const biel = await neuesKonto(BIEL);
  const rolle = await aufruf('PUT', `/api/konten/${biel.konto.id}/rolle`, {
    token: await testkonto('greta.muster@beispiel.invalid'), daten: { rolle: 'Koordination' },
  });
  assert.equal(rolle.status, 204);
  const koordinationBiel = await anmelden(biel.adresse, biel.passwort, BIEL);

  assert.ok(!(await offen(koordinationBiel)).some((v) => v.id === inhalt.id));
  assert.equal((await entscheiden(koordinationBiel, inhalt.id, true)).status, 404);
  assert.equal(await profilstatus(token), IN_PRUEFUNG);

  assert.equal((await entscheiden(await koordination(), '00000000-0000-0000-0000-000000000001', true)).status, 404);
});

test('Die Bemerkung ist auf 500 Zeichen begrenzt', { skip: OHNE_TESTPASSWORT }, async () => {
  const { token } = await betrieb();
  const { inhalt } = await einreichen(token, UID, 'CHE-123.456.788');
  const dora = await koordination();

  assert.equal((await entscheiden(dora, inhalt.id, false, 'x'.repeat(501))).status, 400);
  assert.equal((await entscheiden(dora, inhalt.id, false, 'x'.repeat(500))).status, 200);
});
