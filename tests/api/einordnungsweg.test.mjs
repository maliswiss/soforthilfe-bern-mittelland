// Schnittstellentests: Eine neue Anfrage speichert den Weg ihrer Einordnung, und das Protokoll hält ihn fest.

import { test } from 'node:test';
import assert from 'node:assert/strict';
import { aufruf, neuesKonto, testkonto, BERN, OHNE_TESTPASSWORT } from './hilfen.mjs';

const KATEGORIEAUSWAHL = 0;
const REGELN = 1;
const MASCHINELLES_LERNEN = 2;
const SPRACHMODELL = 3;

const ort = { plz: '3011', ort: 'Bern' };
const erfassen = (token, daten) => aufruf('POST', '/api/anfragen', { token, daten: { ...ort, ...daten } });

test('Die Anfrage speichert den Weg der Einordnung, auch beim Lesen', async () => {
  const { token } = await neuesKonto();
  const kategorien = (await aufruf('GET', '/api/kategorien')).inhalt;
  const begleitung = kategorien.find((k) => k.code === 'begleitung');

  const gewaehlt = await erfassen(token, { kategorieId: begleitung.id, freitext: 'Die Heizung ist ausgefallen.' });
  assert.equal(gewaehlt.inhalt.erfassungsweg, KATEGORIEAUSWAHL);
  assert.equal(gewaehlt.inhalt.kategorieId, begleitung.id);

  const regeln = await erfassen(token, { freitext: 'Die Heizung ist ausgefallen.' });
  assert.equal(regeln.inhalt.erfassungsweg, REGELN);

  const lernen = await erfassen(token, { freitext: 'Bitte Milch, Brot und Früchte vorbeibringen' });
  assert.equal(lernen.inhalt.erfassungsweg, MASCHINELLES_LERNEN);

  // Ohne laufendes Sprachmodell bleibt der Weg leer; mit Sprachmodell darf es entscheiden
  const offen = await erfassen(token, { freitext: 'Grüezi mitenand' });
  assert.ok(offen.inhalt.erfassungsweg === null || offen.inhalt.erfassungsweg === SPRACHMODELL);
  assert.equal(offen.inhalt.erfassungsweg === null, offen.inhalt.kategorieId === null);

  const gelesen = await aufruf('GET', `/api/anfragen/${regeln.inhalt.id}`, { token });
  assert.equal(gelesen.inhalt.erfassungsweg, REGELN);
  const liste = (await aufruf('GET', '/api/anfragen/meine', { token })).inhalt;
  assert.equal(liste.find((a) => a.id === lernen.inhalt.id).erfassungsweg, MASCHINELLES_LERNEN);
});

test('Der Weg steht im Protokoll der Region, ohne Freitext und ohne Personendaten', { skip: OHNE_TESTPASSWORT }, async () => {
  const { token } = await neuesKonto();
  const regeln = (await erfassen(token, { freitext: 'Die Heizung ist ausgefallen.' })).inhalt;
  const offen = (await erfassen(token, { freitext: 'Grüezi mitenand' })).inhalt;

  const koordination = await testkonto('dora.probe@beispiel.invalid');
  const eintraege = (await aufruf('GET', '/api/protokoll?aktion=Anfrage.Eingeordnet&anzahl=500', { token: koordination })).inhalt;

  const eintrag = eintraege.find((e) => e.objektId === regeln.id);
  assert.ok(eintrag);
  assert.equal(eintrag.detail, 'Regeln');
  assert.equal(eintrag.objekt, 'Anfrage');
  assert.equal(eintrag.region, BERN);
  assert.ok(!JSON.stringify(eintrag).includes('Heizung'));

  const ohneKategorie = eintraege.find((e) => e.objektId === offen.id);
  assert.ok(['Offen', 'Sprachmodell'].includes(ohneKategorie.detail));
});
