// Schnittstellentests: unverbindliche Vorschau ohne Anmeldung mit gestaffelter Preisgabe. Ohne Anmeldung keine
// Namen, angemeldet mit Namen; Telefonnummer, Adresse und Kennung der Helfenden nie. Die Anfragen liegen in
// 3150 Schwarzenburg, weit weg von den Profilen anderer Tests.

import { test } from 'node:test';
import assert from 'node:assert/strict';
import { aufruf, neuesKonto, testkonto, helferprofil, OHNE_TESTPASSWORT } from './hilfen.mjs';

const ort = { plz: '3150', ort: 'Schwarzenburg' };
const vorschau = (daten, token) => aufruf('POST', '/api/anfragen/vorschau', { daten, token });

async function kategorie(code) {
  const kategorien = (await aufruf('GET', '/api/kategorien')).inhalt;
  return kategorien.find((k) => k.code === code);
}

// Diese Angaben gibt es in der Vorschau nie
function ohneKontaktdaten(eintrag) {
  for (const feld of ['telefon', 'eMail', 'adresse', 'helferprofilId', 'kontoId']) {
    assert.ok(!(feld in eintrag), `${feld} darf nicht in der Vorschau stehen`);
  }
}

test('Ungültige Eingaben werden mit Hinweis auf das Feld abgewiesen', async () => {
  const nachbarschaftshilfe = await kategorie('nachbarschaftshilfe');

  const kurz = await vorschau({ kategorieId: nachbarschaftshilfe.id, plz: '30' });
  assert.equal(kurz.status, 400);
  assert.equal(kurz.inhalt.feld, 'plz');

  const unbekannt = await vorschau({ kategorieId: nachbarschaftshilfe.id, plz: '0000' });
  assert.equal(unbekannt.status, 400);
  assert.equal(unbekannt.inhalt.feld, 'plz');

  const ohneRegion = await vorschau({ kategorieId: nachbarschaftshilfe.id, plz: '8001' });
  assert.equal(ohneRegion.status, 400);
  assert.match(ohneRegion.inhalt.fehler, /noch keine Region/);

  const ohneKategorie = await vorschau({ kategorieId: '00000000-0000-0000-0000-000000000001', ...ort });
  assert.equal(ohneKategorie.status, 400);
  assert.equal(ohneKategorie.inhalt.feld, 'kategorieId');

  const dringlichkeit = await vorschau({ kategorieId: nachbarschaftshilfe.id, ...ort, prioritaet: 3 });
  assert.equal(dringlichkeit.status, 400);
  assert.equal(dringlichkeit.inhalt.feld, 'prioritaet');
});

test('Ohne Dringlichkeit gilt die übliche der Kategorie; der Ort folgt aus der Postleitzahl', async () => {
  const schluesseldienst = await kategorie('schluesseldienst');
  const antwort = await vorschau({ kategorieId: schluesseldienst.id, plz: '3150' });
  assert.equal(antwort.status, 200);
  assert.equal(antwort.inhalt.prioritaet, 2);
  assert.equal(antwort.inhalt.suchradiusKm, 15);
  assert.equal(antwort.inhalt.ort, 'Schwarzenburg');
  assert.equal(antwort.inhalt.code, 'schluesseldienst');
  assert.equal(antwort.inhalt.bezahlpflichtig, true);
  assert.equal(antwort.inhalt.angemeldet, false);
});

test('Ohne Anmeldung ohne Namen, angemeldet mit Namen, Telefonnummer nie', { skip: OHNE_TESTPASSWORT }, async () => {
  const dora = await testkonto('dora.probe@beispiel.invalid');
  await helferprofil({ ...ort, radiusKm: 10, skills: ['nachbarschaftshilfe'], koordination: dora });
  const nachbarschaftshilfe = await kategorie('nachbarschaftshilfe');
  const daten = { kategorieId: nachbarschaftshilfe.id, ...ort, prioritaet: 1 };

  // Ohne Anmeldung: Begründung, Punktzahl und Distanz, aber kein Name
  const anonym = await vorschau(daten);
  assert.equal(anonym.status, 200);
  assert.equal(anonym.inhalt.angemeldet, false);
  assert.equal(anonym.inhalt.suchradiusKm, 10);
  const eintraege = anonym.inhalt.eintraege;
  assert.ok(eintraege.length > 0 && eintraege.length <= 3);
  assert.deepEqual(eintraege.map((e) => e.rang), eintraege.map((_, i) => i + 1));
  for (const e of eintraege) {
    assert.equal(e.name, null);
    assert.equal(e.istBetrieb, false);
    assert.match(e.begruendung, /Fähigkeit «Nachbarschaftshilfe»/);
    assert.equal(e.kriterien.length, 5);
    assert.equal(e.preis, null, 'kostenlose Kategorie ohne Preis');
    ohneKontaktdaten(e);
  }

  // Angemeldet: dieselbe Reihenfolge mit Namen, weiterhin ohne Telefonnummer
  const { token } = await neuesKonto();
  const angemeldet = await vorschau(daten, token);
  assert.equal(angemeldet.status, 200);
  assert.equal(angemeldet.inhalt.angemeldet, true);
  assert.deepEqual(angemeldet.inhalt.eintraege.map((e) => e.punktzahl), eintraege.map((e) => e.punktzahl));
  for (const e of angemeldet.inhalt.eintraege) {
    assert.equal(e.name, 'Test Person');
    ohneKontaktdaten(e);
  }
});

test('Kostenpflichtige Kategorie: Betriebe mit Preisangaben, Name des Betriebs nach der Anmeldung', { skip: OHNE_TESTPASSWORT }, async () => {
  const dora = await testkonto('dora.probe@beispiel.invalid');
  await helferprofil({
    ...ort, radiusKm: 10, skills: ['reinigung'], koordination: dora, typ: 1, betriebsname: 'Vorschau Reinigung GmbH',
    preise: { anfahrtChf: 40, stundenansatzChf: 80 },
  });
  const reinigung = await kategorie('reinigung');
  const daten = { kategorieId: reinigung.id, ...ort, prioritaet: 2 };

  const anonym = (await vorschau(daten)).inhalt;
  const betrieb = anonym.eintraege.find((e) => e.istBetrieb);
  assert.ok(betrieb, 'Betrieb in der Vorschau');
  assert.equal(betrieb.name, null);
  assert.equal(betrieb.preis.anfahrtChf, 40);
  assert.equal(betrieb.preis.stundenansatzChf, 80);
  assert.equal(typeof betrieb.preis.richtpreisErsteStunde, 'number');

  const { token } = await neuesKonto();
  const angemeldet = (await vorschau(daten, token)).inhalt;
  assert.ok(angemeldet.eintraege.some((e) => e.istBetrieb && e.name === 'Vorschau Reinigung GmbH'));
});

test('Ohne Anmeldung bestimmt die Postleitzahl die Region', async () => {
  // In Biel/Seeland gibt es in den Testdaten keine verifizierten Helfenden für Nachbarschaftshilfe
  const nachbarschaftshilfe = await kategorie('nachbarschaftshilfe');
  const biel = await vorschau({ kategorieId: nachbarschaftshilfe.id, plz: '2502', prioritaet: 2 });
  assert.equal(biel.status, 200);
  assert.deepEqual(biel.inhalt.eintraege, []);
});
