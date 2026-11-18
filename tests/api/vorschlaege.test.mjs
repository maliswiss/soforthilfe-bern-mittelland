// Schnittstellentests: Smart-Matching mit Gewichtung. Die Anfragen liegen in 3150 Schwarzenburg, weit weg von den
// Profilen anderer Tests. Distanzen ab Schwarzenburg: 3152 Mamishaus 2.6 km, 3148 Lanzenhäusern 3.2 km,
// 3145 Niederscherli 8.3 km.

import { test } from 'node:test';
import assert from 'node:assert/strict';
import { aufruf, neuesKonto, testkonto, helferprofil, BERN, HELFENDE, OHNE_TESTPASSWORT } from './hilfen.mjs';

const ort = { plz: '3150', ort: 'Schwarzenburg' };
const koordination = () => testkonto('dora.probe@beispiel.invalid');
const vorschlaege = (token, id) => aufruf('GET', `/api/anfragen/${id}/vorschlaege`, { token });

async function anfrage(token, kategorieCode, prioritaet) {
  const kategorien = (await aufruf('GET', '/api/kategorien')).inhalt;
  const kategorieId = kategorien.find((k) => k.code === kategorieCode).id;
  const erfasst = await aufruf('POST', '/api/anfragen', { token, daten: { kategorieId, prioritaet, ...ort } });
  assert.equal(erfasst.status, 201);
  return erfasst.inhalt;
}

// Punktzahl mit den Standardgewichten für Helfende ohne Bewertungen und Einsätze, die jetzt verfügbar sind.
const erwartetePunktzahl = (meter, radiusKm) =>
  0.35 * Math.max(0, 1 - meter / (radiusKm * 1000)) + 0.25 * 1 + 0.20 * 0.6 + 0.10 * 1 + 0.10 * 0;

test('Vorschläge sehen die anfragende Person, Koordination und Verwaltung', async () => {
  const id = '00000000-0000-0000-0000-000000000001';
  assert.equal((await vorschlaege(undefined, id)).status, 401);

  const helfende = await neuesKonto(BERN, HELFENDE);
  assert.equal((await vorschlaege(helfende.token, id)).status, 403);

  const fremde = await neuesKonto();
  assert.equal((await vorschlaege(fremde.token, id)).status, 404);
});

test('Die drei besten Kandidaten nach Punktzahl, Status «Vorschläge vorhanden»', { skip: OHNE_TESTPASSWORT }, async () => {
  const dora = await koordination();
  const skills = ['nachbarschaftshilfe'];
  await helferprofil({ ...ort, radiusKm: 10, skills, koordination: dora });
  await helferprofil({ plz: '3152', ort: 'Mamishaus', radiusKm: 10, skills, koordination: dora });
  await helferprofil({ plz: '3148', ort: 'Lanzenhäusern', radiusKm: 10, skills, koordination: dora });
  const weit = await helferprofil({ plz: '3145', ort: 'Niederscherli', radiusKm: 15, skills, koordination: dora });

  const { token } = await neuesKonto();
  const erfasst = await anfrage(token, 'nachbarschaftshilfe', 2);
  assert.equal(erfasst.status, 2, 'Vorschläge vorhanden');

  const antwort = await vorschlaege(token, erfasst.id);
  assert.equal(antwort.status, 200);
  const liste = antwort.inhalt;
  assert.equal(liste.length, 3);
  assert.deepEqual(liste.map((v) => v.rang), [1, 2, 3]);

  const punkte = liste.map((v) => v.punktzahl);
  assert.deepEqual(punkte, [...punkte].sort((a, b) => b - a));
  assert.ok(!liste.some((v) => v.helferprofilId === weit.profil.id), 'Niederscherli ist weiter weg');

  // Alle Vorschläge stammen aus der Kandidatensuche; der beste ist der nächste
  const kandidaten = (await aufruf('GET', `/api/anfragen/${erfasst.id}/kandidaten`, { token: dora })).inhalt.kandidaten;
  assert.ok(liste.every((v) => kandidaten.some((k) => k.helferprofilId === v.helferprofilId)));
  assert.equal(liste[0].distanzMeter, Math.min(...kandidaten.map((k) => k.distanzMeter)));

  // Gewichtete Punktzahl bei 15 km Suchradius (hohe Dringlichkeit)
  for (const v of liste) {
    assert.ok(Math.abs(v.punktzahl - erwartetePunktzahl(v.distanzMeter, 15)) < 0.001, `Punktzahl ${v.punktzahl}`);
    assert.equal(v.anzeigename, 'Test Person');
    assert.equal(v.partnerBevorzugt, false);
    assert.ok(!('telefon' in v) && !('adresse' in v));
  }

  // Dieselbe Liste für die Koordination; im Protokoll nur die Anzahl
  assert.deepEqual((await vorschlaege(dora, erfasst.id)).inhalt, liste);
  const protokoll = (await aufruf('GET', '/api/protokoll?aktion=Anfrage.Vorgeschlagen&anzahl=500', { token: dora })).inhalt;
  assert.equal(protokoll.find((e) => e.objektId === erfasst.id)?.detail, '3 Vorschläge');

  // Andere Hilfesuchende und andere Regionen sehen nichts
  assert.equal((await vorschlaege((await neuesKonto()).token, erfasst.id)).status, 404);
  assert.equal((await vorschlaege(await testkonto('greta.muster@beispiel.invalid'), erfasst.id)).status, 404);
});

test('Ohne passende Kandidaten bleibt die Anfrage offen', { skip: OHNE_TESTPASSWORT }, async () => {
  const { token } = await neuesKonto();
  const erfasst = await anfrage(token, 'transporthilfe', 0);
  assert.equal(erfasst.status, 1, 'offen');
  assert.deepEqual((await vorschlaege(token, erfasst.id)).inhalt, []);
});
