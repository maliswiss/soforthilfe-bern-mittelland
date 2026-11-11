// Schnittstellentests: eigenes Helferprofil anlegen, ändern und die Verfügbarkeit umschalten.

import { test } from 'node:test';
import assert from 'node:assert/strict';
import { aufruf, neuesKonto, testkonto, BERN, HELFENDE, OHNE_TESTPASSWORT } from './hilfen.mjs';

// Fähigkeiten nach Kennung, zum Beispiel «nachbarschaftshilfe».
async function skills() {
  const { inhalt } = await aufruf('GET', '/api/skills');
  return Object.fromEntries(inhalt.map((s) => [s.code, s]));
}

const profil = (token, daten) => aufruf('PUT', '/api/helferprofil', { token, daten });

async function gueltigesProfil(zusatz = {}) {
  const s = await skills();
  return {
    typ: 0,
    beschreibung: 'Hilft gerne beim Einkaufen und begleitet zu Terminen.',
    plz: '3110',
    ort: 'Münsingen',
    radiusKm: 8,
    skillIds: [s.nachbarschaftshilfe.id, s.begleitung.id],
    ...zusatz,
  };
}

test('Helferprofil nur angemeldet und nur mit der Rolle Helfende', async () => {
  assert.equal((await aufruf('GET', '/api/helferprofil')).status, 401);

  const suchende = await neuesKonto(BERN);
  assert.equal((await aufruf('GET', '/api/helferprofil', { token: suchende.token })).status, 403);
  assert.equal((await profil(suchende.token, await gueltigesProfil())).status, 403);
});

test('Helfende legen ihr Profil an, sehen es und ändern es', async () => {
  const { token } = await neuesKonto(BERN, HELFENDE);
  const s = await skills();

  const leer = await aufruf('GET', '/api/helferprofil', { token });
  assert.equal(leer.status, 404);
  assert.equal(leer.inhalt.fehler, 'Es besteht noch kein Helferprofil.');

  const angelegt = await profil(token, await gueltigesProfil({ betriebsname: 'wird ignoriert', adresse: ' Dorfstrasse 1 ' }));
  assert.equal(angelegt.status, 200);
  assert.equal(angelegt.inhalt.typ, 0);
  assert.equal(angelegt.inhalt.betriebsname, null);
  assert.equal(angelegt.inhalt.plz, '3110');
  assert.equal(angelegt.inhalt.ort, 'Münsingen');
  assert.equal(angelegt.inhalt.radiusKm, 8);
  assert.equal(angelegt.inhalt.adresse, 'Dorfstrasse 1');
  assert.equal(angelegt.inhalt.jetztVerfuegbar, true);
  assert.equal(angelegt.inhalt.partner, false);
  assert.equal(angelegt.inhalt.anzahlEinsaetze, 0);
  assert.deepEqual(angelegt.inhalt.skills.map((x) => x.code).sort(), ['begleitung', 'nachbarschaftshilfe']);

  // Kein genauer Standort in der Antwort
  assert.ok(!('standort' in angelegt.inhalt) && !('breitengrad' in angelegt.inhalt));

  const gelesen = await aufruf('GET', '/api/helferprofil', { token });
  assert.deepEqual(gelesen.inhalt, angelegt.inhalt);

  // Fähigkeiten werden vollständig ersetzt; die Kennung des Profils bleibt
  const geaendert = await profil(token, await gueltigesProfil({ typ: 1, betriebsname: 'Muster Begleitdienst GmbH', skillIds: [s.begleitung.id] }));
  assert.equal(geaendert.status, 200);
  assert.equal(geaendert.inhalt.id, angelegt.inhalt.id);
  assert.equal(geaendert.inhalt.typ, 1);
  assert.equal(geaendert.inhalt.betriebsname, 'Muster Begleitdienst GmbH');
  assert.deepEqual(geaendert.inhalt.skills.map((x) => x.code), ['begleitung']);
});

test('Verfügbarkeit umschalten', async () => {
  const { token } = await neuesKonto(BERN, HELFENDE);
  const verfuegbarkeit = (jetztVerfuegbar) => aufruf('PUT', '/api/helferprofil/verfuegbarkeit', { token, daten: { jetztVerfuegbar } });

  assert.equal((await verfuegbarkeit(false)).status, 404);

  await profil(token, await gueltigesProfil());
  const aus = await verfuegbarkeit(false);
  assert.equal(aus.status, 200);
  assert.deepEqual(aus.inhalt, { jetztVerfuegbar: false });
  assert.equal((await aufruf('GET', '/api/helferprofil', { token })).inhalt.jetztVerfuegbar, false);
});

test('Ungültige Angaben werden mit Hinweis auf das Feld abgewiesen', async () => {
  const { token } = await neuesKonto(BERN, HELFENDE);
  const s = await skills();
  const fall = async (zusatz, feld) => {
    const { status, inhalt } = await profil(token, await gueltigesProfil(zusatz));
    assert.equal(status, 400, JSON.stringify(zusatz));
    if (feld) {
      assert.equal(inhalt.feld?.toLowerCase(), feld.toLowerCase(), JSON.stringify(zusatz));
    }
  };

  await fall({ skillIds: [] }, 'skillIds');
  await fall({ skillIds: [s.begleitung.id, '00000000-0000-0000-0000-000000000001'] }, 'skillIds');
  await fall({ typ: 1, betriebsname: '  ' }, 'betriebsname');
  await fall({ plz: 'abcd' }, 'plz');
  await fall({ plz: '3000', ort: 'Bern' }, 'plz');
  await fall({ plz: '8001', ort: 'Zürich' }, 'plz');
  await fall({ ort: ' ' }, 'ort');
  await fall({ radiusKm: 0 });
  await fall({ radiusKm: 101 });
  await fall({ typ: 5 });

  // Nichts gespeichert
  assert.equal((await aufruf('GET', '/api/helferprofil', { token })).status, 404);
});

test('Anlegen und Ändern stehen im Protokoll der Region, ohne Personendaten', { skip: OHNE_TESTPASSWORT }, async () => {
  const { token } = await neuesKonto(BERN, HELFENDE);
  const { inhalt } = await profil(token, await gueltigesProfil());
  await profil(token, await gueltigesProfil({ radiusKm: 12 }));

  const koordination = await testkonto('dora.probe@beispiel.invalid');
  const eintraege = (await aufruf('GET', '/api/protokoll?anzahl=500', { token: koordination })).inhalt
    .filter((e) => e.objektId === inhalt.id);
  assert.deepEqual(eintraege.map((e) => e.aktion).sort(), ['Helferprofil.Erstellt', 'Helferprofil.Geaendert']);
  assert.ok(eintraege.every((e) => e.region === BERN && e.detail === null && e.objekt === 'Helferprofil'));
});
