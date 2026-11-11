// Schnittstellentests: Preisangaben im Helferprofil und Richtpreis für die erste Stunde (Anforderung A9).

import { test } from 'node:test';
import assert from 'node:assert/strict';
import { aufruf, neuesKonto, BERN, HELFENDE } from './hilfen.mjs';

const richtpreis = (daten) => aufruf('POST', '/api/preise/richtpreis', { daten });

// Anfahrt 40, Stundenansatz 95, Nacht 50 %, Wochenende 25 %, MWST-pflichtig.
const BETRIEB = { anfahrtChf: 40, stundenansatzChf: 95, nachtzuschlagProzent: 50, wochenendzuschlagProzent: 25, mwstPflichtig: true };

test('Richtpreis werktags, nachts und am Wochenende, ohne Anmeldung', async () => {
  const { status, inhalt } = await richtpreis({
    ...BETRIEB,
    // Schweizer Ortszeit im November: Mittwoch 10 Uhr, Mittwoch 22 Uhr, Samstag 10 Uhr, Samstag 23 Uhr
    zeitpunkte: ['2026-11-11T10:00:00+01:00', '2026-11-11T22:00:00+01:00', '2026-11-14T10:00:00+01:00', '2026-11-14T23:00:00+01:00'],
  });
  assert.equal(status, 200);
  assert.deepEqual(inhalt.map((p) => p.richtpreisErsteStunde), [145.95, 197.3, 171.6, 197.3]);
  assert.deepEqual(inhalt.map((p) => p.angewandterZuschlag), [null, 'nacht', 'wochenende', 'nacht']);
  assert.deepEqual(inhalt.map((p) => p.angewandterZuschlagProzent), [0, 50, 25, 50]);
  assert.ok(inhalt.every((p) => p.mwstSatzProzent === 8.1 && p.mwstPflichtig === true));
});

test('Ohne MWST-Pflicht und mit fehlenden Angaben', async () => {
  const ohneMwst = (await richtpreis({ ...BETRIEB, mwstPflichtig: false, zeitpunkte: ['2026-11-11T22:00:00+01:00'] })).inhalt;
  assert.equal(ohneMwst[0].richtpreisErsteStunde, 182.5);

  const unvollstaendig = (await richtpreis({ anfahrtChf: 40, zeitpunkte: ['2026-11-11T10:00:00+01:00'] })).inhalt;
  assert.equal(unvollstaendig[0].richtpreisErsteStunde, null);
});

test('Höchstens zehn Zeitpunkte, ungültige Beträge werden abgewiesen', async () => {
  const zeitpunkte = Array.from({ length: 12 }, (_, i) => `2026-11-${String(i + 1).padStart(2, '0')}T10:00:00+01:00`);
  assert.equal((await richtpreis({ ...BETRIEB, zeitpunkte })).inhalt.length, 10);

  for (const falsch of [{ anfahrtChf: -1 }, { stundenansatzChf: 501 }, { nachtzuschlagProzent: 201 }, { wochenendzuschlagProzent: -5 }]) {
    assert.equal((await richtpreis({ ...BETRIEB, ...falsch })).status, 400, JSON.stringify(falsch));
  }
});

test('Bezahlpflichtige Fähigkeiten verlangen Anfahrt und Stundenansatz im Helferprofil', async () => {
  const { token } = await neuesKonto(BERN, HELFENDE);
  const skills = Object.fromEntries((await aufruf('GET', '/api/skills')).inhalt.map((s) => [s.code, s]));
  const profil = (zusatz) => aufruf('PUT', '/api/helferprofil', {
    token,
    daten: { typ: 1, betriebsname: 'Muster Haustechnik AG', plz: '3011', ort: 'Bern', radiusKm: 15, skillIds: [skills['sanitaer-elektro'].id, skills.nachbarschaftshilfe.id], ...zusatz },
  });

  const ohnePreise = await profil({});
  assert.equal(ohnePreise.status, 400);
  assert.equal(ohnePreise.inhalt.feld, 'preise');

  assert.equal((await profil({ anfahrtChf: 40 })).status, 400);

  const mitPreisen = await profil(BETRIEB);
  assert.equal(mitPreisen.status, 200);
  assert.equal(mitPreisen.inhalt.preiseErforderlich, true);
  assert.equal(mitPreisen.inhalt.anfahrtChf, 40);
  assert.equal(mitPreisen.inhalt.stundenansatzChf, 95);
  assert.equal(mitPreisen.inhalt.nachtzuschlagProzent, 50);
  assert.equal(mitPreisen.inhalt.wochenendzuschlagProzent, 25);
  assert.equal(mitPreisen.inhalt.mwstPflichtig, true);
});

test('Kostenlose Fähigkeiten brauchen keine Preisangaben', async () => {
  const { token } = await neuesKonto(BERN, HELFENDE);
  const skills = Object.fromEntries((await aufruf('GET', '/api/skills')).inhalt.map((s) => [s.code, s]));
  const { status, inhalt } = await aufruf('PUT', '/api/helferprofil', {
    token,
    daten: { typ: 0, plz: '3011', ort: 'Bern', radiusKm: 5, skillIds: [skills.nachbarschaftshilfe.id] },
  });
  assert.equal(status, 200);
  assert.equal(inhalt.preiseErforderlich, false);
  assert.equal(inhalt.anfahrtChf, null);
  assert.equal(inhalt.mwstPflichtig, false);
});
