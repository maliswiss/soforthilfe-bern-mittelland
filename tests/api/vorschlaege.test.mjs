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
    assert.equal(v.preis, null, 'kostenlose Kategorie ohne Preis');
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

test('Kostenpflichtige Kategorie: Vorschläge mit Preisangaben vor der Zusage', { skip: OHNE_TESTPASSWORT }, async () => {
  const dora = await koordination();
  const betrieb = await helferprofil({
    ...ort, radiusKm: 10, skills: ['lieferdienst'], koordination: dora, typ: 1, betriebsname: 'Vorschlag Lieferdienst AG',
    preise: { anfahrtChf: 15, stundenansatzChf: 60, mwstPflichtig: true },
  });

  const { token } = await neuesKonto();
  const erfasst = await anfrage(token, 'lieferdienst', 2);
  const liste = (await vorschlaege(token, erfasst.id)).inhalt;
  const eintrag = liste.find((v) => v.helferprofilId === betrieb.profil.id);
  assert.ok(eintrag, 'Betrieb unter den Vorschlägen');
  assert.equal(eintrag.betriebsname, 'Vorschlag Lieferdienst AG');
  assert.equal(eintrag.preis.anfahrtChf, 15);
  assert.equal(eintrag.preis.stundenansatzChf, 60);
  assert.equal(eintrag.preis.mwstPflichtig, true);
  assert.equal(typeof eintrag.preis.richtpreisErsteStunde, 'number');
});

test('Ohne passende Kandidaten bleibt die Anfrage offen', { skip: OHNE_TESTPASSWORT }, async () => {
  const { token } = await neuesKonto();
  const erfasst = await anfrage(token, 'transporthilfe', 0);
  assert.equal(erfasst.status, 1, 'offen');
  assert.deepEqual((await vorschlaege(token, erfasst.id)).inhalt, []);
});

// 3155 Helgisried-Rohrbach liegt abseits; die Profile ziehen am Ende weg und sind dann wieder in Prüfung.
const helgisried = { plz: '3155', ort: 'Helgisried-Rohrbach' };

async function wegziehen(helfer) {
  const profil = (await aufruf('GET', '/api/helferprofil', { token: helfer.token })).inhalt;
  await aufruf('PUT', '/api/helferprofil', {
    token: helfer.token,
    daten: { typ: profil.typ, betriebsname: profil.betriebsname, plz: '3011', ort: 'Bern', radiusKm: profil.radiusKm, skillIds: profil.skills.map((s) => s.id) },
  });
}

async function anfrageHelgisried(token) {
  const kategorien = (await aufruf('GET', '/api/kategorien')).inhalt;
  const kategorieId = kategorien.find((k) => k.code === 'begleitung').id;
  const erfasst = await aufruf('POST', '/api/anfragen', { token, daten: { kategorieId, prioritaet: 0, ...helgisried } });
  assert.equal(erfasst.status, 201);
  return erfasst.inhalt;
}

test('Nicht verfügbare Helfende bleiben Kandidaten, mit weniger Punkten für die Verfügbarkeit', { skip: OHNE_TESTPASSWORT }, async (t) => {
  const dora = await koordination();
  const bereit = await helferprofil({ ...helgisried, radiusKm: 10, skills: ['begleitung'], koordination: dora });
  const pause = await helferprofil({ ...helgisried, radiusKm: 10, skills: ['begleitung'], koordination: dora });
  t.after(() => Promise.all([wegziehen(bereit), wegziehen(pause)]));
  await aufruf('PUT', '/api/helferprofil/verfuegbarkeit', { token: pause.token, daten: { jetztVerfuegbar: false } });

  const { token } = await neuesKonto();
  const erfasst = await anfrageHelgisried(token);
  const kandidaten = (await aufruf('GET', `/api/anfragen/${erfasst.id}/kandidaten`, { token: dora })).inhalt.kandidaten;
  assert.equal(kandidaten.find((k) => k.helferprofilId === pause.profil.id)?.jetztVerfuegbar, false, 'nicht verfügbar, aber Kandidat');

  // Verfügbar 1, nicht verfügbar 0.4 Teilpunkte; bei gleicher Distanz 0.10 × 0.6 = 0.06 Punkte weniger
  const liste = (await vorschlaege(token, erfasst.id)).inhalt;
  const a = liste.find((v) => v.helferprofilId === bereit.profil.id);
  const b = liste.find((v) => v.helferprofilId === pause.profil.id);
  assert.ok(a && b, 'beide unter den Vorschlägen');
  const teil = (v) => v.kriterien.find((k) => k.kriterium === 'verfuegbarkeit').teilpunkte;
  assert.equal(teil(a), 1);
  assert.equal(teil(b), 0.4);
  assert.ok(Math.abs(a.punktzahl - b.punktzahl - 0.06) < 0.001, `${a.punktzahl} gegen ${b.punktzahl}`);
  assert.ok(a.rang < b.rang);
  assert.match(b.begruendung, /derzeit nicht verfügbar/);
});

test('Gespeicherte Vorschläge ändern sich nicht, wenn sich das Profil danach ändert', { skip: OHNE_TESTPASSWORT }, async (t) => {
  const dora = await koordination();
  const helfer = await helferprofil({ ...helgisried, radiusKm: 10, skills: ['begleitung'], koordination: dora });
  t.after(() => wegziehen(helfer));
  const { token } = await neuesKonto();
  const erfasst = await anfrageHelgisried(token);
  const vorher = (await vorschlaege(token, erfasst.id)).inhalt;
  assert.ok(vorher.some((v) => v.helferprofilId === helfer.profil.id && v.jetztVerfuegbar));

  await aufruf('PUT', '/api/helferprofil/verfuegbarkeit', { token: helfer.token, daten: { jetztVerfuegbar: false } });
  const nachher = (await vorschlaege(token, erfasst.id)).inhalt;

  // Rang, Punktzahl, Teilpunkte und Begründung bleiben; nur die angezeigte Verfügbarkeit ist die aktuelle
  const fest = (v) => ({ id: v.id, rang: v.rang, punktzahl: v.punktzahl, begruendung: v.begruendung, kriterien: v.kriterien });
  assert.deepEqual(nachher.map(fest), vorher.map(fest));
  const eigener = nachher.find((v) => v.helferprofilId === helfer.profil.id);
  assert.equal(eigener.jetztVerfuegbar, false);
  assert.match(eigener.begruendung, /jetzt verfügbar/);
});
