// Schnittstellentests: Kandidatensuche nach Postleitzahl, Fähigkeit und Suchradius.
// Distanzen zwischen den Punkten der Postleitzahlen ab 3011 Bern: 3012 Bern 1.5 km, 3123 Belp 7.5 km,
// 3110 Münsingen 11.8 km.

import { test } from 'node:test';
import assert from 'node:assert/strict';
import { aufruf, neuesKonto, testkonto, helferprofil, BERN, HELFENDE, OHNE_TESTPASSWORT } from './hilfen.mjs';

const koordination = () => testkonto('dora.probe@beispiel.invalid');

async function anfrage(token, kategorieCode, prioritaet, plz = '3011', ort = 'Bern') {
  const kategorien = (await aufruf('GET', '/api/kategorien')).inhalt;
  const kategorieId = kategorien.find((k) => k.code === kategorieCode).id;
  const erfasst = await aufruf('POST', '/api/anfragen', { token, daten: { kategorieId, plz, ort, prioritaet } });
  assert.equal(erfasst.status, 201);
  return erfasst.inhalt;
}

const kandidaten = (token, id) => aufruf('GET', `/api/anfragen/${id}/kandidaten`, { token });

test('Kandidaten sehen nur Koordination und Verwaltung', async () => {
  const id = '00000000-0000-0000-0000-000000000001';
  assert.equal((await kandidaten(undefined, id)).status, 401);

  for (const kontoart of [0, HELFENDE]) {
    const { token } = await neuesKonto(BERN, kontoart);
    assert.equal((await kandidaten(token, id)).status, 403);
  }
});

test('Verifizierte Helfende im Such- und Einsatzradius, nächste zuerst', { skip: OHNE_TESTPASSWORT }, async () => {
  const dora = await koordination();
  const skills = ['nachbarschaftshilfe'];
  const nah = await helferprofil({ plz: '3012', ort: 'Bern', radiusKm: 10, skills, koordination: dora });
  const belp = await helferprofil({ plz: '3123', ort: 'Belp', radiusKm: 10, skills, koordination: dora });
  const ungeprueft = await helferprofil({ plz: '3011', ort: 'Bern', skills, verifiziert: false });
  const eigenerRadiusZuKlein = await helferprofil({ plz: '3123', ort: 'Belp', radiusKm: 5, skills, koordination: dora });
  const muensingen = await helferprofil({ plz: '3110', ort: 'Münsingen', radiusKm: 20, skills, koordination: dora });
  const andereFaehigkeit = await helferprofil({ plz: '3011', ort: 'Bern', skills: ['begleitung'], koordination: dora });

  const { token } = await neuesKonto();
  const mittel = await anfrage(token, 'nachbarschaftshilfe', 1);

  const antwort = await kandidaten(dora, mittel.id);
  assert.equal(antwort.status, 200);
  assert.equal(antwort.inhalt.suchradiusKm, 10);
  assert.equal(antwort.inhalt.kategorieFehlt, false);

  const liste = antwort.inhalt.kandidaten;
  const finde = (h) => liste.find((k) => k.helferprofilId === h.profil.id);
  assert.ok(finde(nah));
  assert.ok(Math.abs(finde(nah).distanzMeter - 1460) < 100, `Distanz ${finde(nah).distanzMeter}`);
  assert.equal(finde(nah).anzeigename, 'Test Person');
  assert.equal(finde(nah).plz, '3012');
  assert.ok(finde(belp));
  assert.ok(!finde(ungeprueft), 'nicht verifiziert');
  assert.ok(!finde(eigenerRadiusZuKlein), 'ausserhalb des eigenen Einsatzradius');
  assert.ok(!finde(muensingen), 'ausserhalb des Suchradius');
  assert.ok(!finde(andereFaehigkeit), 'andere Fähigkeit');

  // Nächste zuerst, alle im Suchradius; keine Telefonnummern oder Adressen
  const distanzen = liste.map((k) => k.distanzMeter);
  assert.deepEqual(distanzen, [...distanzen].sort((a, b) => a - b));
  assert.ok(distanzen.every((d) => d <= 10_000));
  assert.ok(liste.every((k) => !('telefon' in k) && !('adresse' in k)));

  // Hohe Dringlichkeit: Suchradius 15 km, Münsingen liegt jetzt darin
  const hoch = await anfrage(token, 'nachbarschaftshilfe', 2);
  const weiter = (await kandidaten(dora, hoch.id)).inhalt;
  assert.equal(weiter.suchradiusKm, 15);
  assert.ok(weiter.kandidaten.some((k) => k.helferprofilId === muensingen.profil.id));
  assert.ok(!weiter.kandidaten.some((k) => k.helferprofilId === eigenerRadiusZuKlein.profil.id));

  // Tiefe Dringlichkeit: 5 km, Belp fällt weg
  const tief = await anfrage(token, 'nachbarschaftshilfe', 0);
  const nahe = (await kandidaten(dora, tief.id)).inhalt;
  assert.equal(nahe.suchradiusKm, 5);
  assert.ok(nahe.kandidaten.some((k) => k.helferprofilId === nah.profil.id));
  assert.ok(!nahe.kandidaten.some((k) => k.helferprofilId === belp.profil.id));
});

test('Kostenpflichtige Kategorie: Kandidaten mit Preisangaben', { skip: OHNE_TESTPASSWORT }, async () => {
  const dora = await koordination();
  const betrieb = await helferprofil({
    plz: '3011', ort: 'Bern', skills: ['sanitaer-elektro'], koordination: dora,
    preise: { anfahrtChf: 40, stundenansatzChf: 95 },
  });

  const { token } = await neuesKonto();
  const erfasst = await anfrage(token, 'sanitaer-elektro', 1);
  const liste = (await kandidaten(dora, erfasst.id)).inhalt.kandidaten;
  assert.ok(liste.some((k) => k.helferprofilId === betrieb.profil.id));
});

test('Ohne Kategorie keine Suche; Anfragen anderer Regionen gibt es nicht', { skip: OHNE_TESTPASSWORT }, async () => {
  const { token } = await neuesKonto();
  const offen = await aufruf('POST', '/api/anfragen', { token, daten: { freitext: 'Xylofon Quader Zebra', plz: '3011', ort: 'Bern' } });
  assert.equal(offen.status, 201);

  // Mit laufendem Sprachmodell kann der Text doch eingeordnet sein; dann gibt es nichts zu prüfen.
  const dora = await koordination();
  if (offen.inhalt.kategorieId === null) {
    const ohne = (await kandidaten(dora, offen.inhalt.id)).inhalt;
    assert.equal(ohne.kategorieFehlt, true);
    assert.deepEqual(ohne.kandidaten, []);
  }

  const greta = await testkonto('greta.muster@beispiel.invalid');
  assert.equal((await kandidaten(greta, offen.inhalt.id)).status, 404);
});
