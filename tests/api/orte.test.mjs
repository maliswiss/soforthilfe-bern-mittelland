// Schnittstellentests: Postleitzahl-Zuordnung über das amtliche Ortschaftenverzeichnis, Region aus der Postleitzahl
// und Standortfreigabe nur über die eigene Schnittstelle.

import { test } from 'node:test';
import assert from 'node:assert/strict';
import { aufruf, neuesKonto, BERN, BIEL, OHNE_TESTPASSWORT } from './hilfen.mjs';

const orte = (plz) => aufruf('GET', `/api/orte?plz=${encodeURIComponent(plz)}`);

test('Postleitzahl ergibt Ort, Gemeinde, Kanton und Region, ohne Anmeldung', async () => {
  const { status, inhalt } = await orte('3011');
  assert.equal(status, 200);
  assert.deepEqual(inhalt, [
    { plz: '3011', ort: 'Bern', gemeinde: 'Bern', kanton: 'BE', plzUeberMehrereGemeinden: false, region: BERN },
  ]);
});

test('Mehrere Ortschaften einer Postleitzahl werden alle genannt', async () => {
  const { inhalt } = await orte('3086');
  assert.deepEqual(inhalt.map((o) => o.ort), ['Zimmerwald', 'Englisberg']);
  assert.ok(inhalt.every((o) => o.gemeinde === 'Wald (BE)' && o.region === BERN));
});

test('Hinweis, wenn eine Postleitzahl mehrere Gemeinden umfasst', async () => {
  // Thörishaus liegt zu 59 % in Köniz und zu 41 % in Neuenegg.
  const [thoerishaus] = (await orte('3174')).inhalt;
  assert.equal(thoerishaus.ort, 'Thörishaus');
  assert.equal(thoerishaus.gemeinde, 'Köniz');
  assert.equal(thoerishaus.plzUeberMehrereGemeinden, true);

  // Splitter unter 3 % lösen den Hinweis nicht aus: Worb mit Anteilen in Muri und Stettlen unter einem Prozent,
  // aber 3,6 % in Vechigen zählt.
  assert.equal((await orte('3076')).inhalt[0].plzUeberMehrereGemeinden, true);
  assert.equal((await orte('3110')).inhalt[0].plzUeberMehrereGemeinden, false);
});

test('Ausserhalb der Regionen bleibt die Region leer, unbekannte Postleitzahlen liefern eine leere Liste', async () => {
  const [zuerich] = (await orte('8001')).inhalt;
  assert.equal(zuerich.ort, 'Zürich');
  assert.equal(zuerich.kanton, 'ZH');
  assert.equal(zuerich.region, null);

  const unbekannt = await orte('3000');
  assert.equal(unbekannt.status, 200);
  assert.deepEqual(unbekannt.inhalt, []);
});

test('Ungültige Postleitzahlen werden mit Hinweis auf das Feld abgewiesen', async () => {
  for (const plz of ['', '301', '30111', '0123', 'abcd', '30 11']) {
    const { status, inhalt } = await orte(plz);
    assert.equal(status, 400, plz);
    assert.equal(inhalt.feld, 'plz', plz);
  }
});

test('Die Region Biel/Seeland wird über ihre Gemeinden erkannt', { skip: OHNE_TESTPASSWORT }, async () => {
  const [biel] = (await orte('2502')).inhalt;
  assert.equal(biel.ort, 'Biel/Bienne');
  assert.equal(biel.region, BIEL);

  // 3053 berührt beide Regionen: Münchenbuchsee gehört zu Bern/Mittelland, Lätti (Rapperswil) zu Biel/Seeland.
  const liste = (await orte('3053')).inhalt;
  assert.equal(liste.find((o) => o.ort === 'Münchenbuchsee').region, BERN);
  assert.equal(liste.find((o) => o.ort === 'Lätti').region, BIEL);
});

test('Eine Anfrage braucht eine bekannte Postleitzahl in der Region des Kontos', async () => {
  const { token } = await neuesKonto(BERN);
  const anfrage = (plz, ort) => aufruf('POST', '/api/anfragen', { token, daten: { freitext: 'Hilfe beim Tragen von Einkäufen', plz, ort } });

  const unbekannt = await anfrage('3000', 'Bern');
  assert.equal(unbekannt.status, 400);
  assert.equal(unbekannt.inhalt.feld, 'plz');

  const ausserhalb = await anfrage('8001', 'Zürich');
  assert.equal(ausserhalb.status, 400);
  assert.equal(ausserhalb.inhalt.feld, 'plz');
  assert.match(ausserhalb.inhalt.fehler, /ausserhalb der Region/);

  // Grenzfall: eine Postleitzahl, die auch eine Gemeinde der Region berührt, ist erlaubt.
  assert.equal((await anfrage('3053', 'Münchenbuchsee')).status, 201);
  assert.equal((await anfrage('3174', 'Thörishaus')).status, 201);
});

const umgebung = (daten) => aufruf('POST', '/api/orte/umgebung', { daten });

test('Freigegebener Standort ergibt Postleitzahl, Ort und Region, ohne Anmeldung', async () => {
  const bundesplatz = await umgebung({ breitengrad: 46.9466, laengengrad: 7.444 });
  assert.equal(bundesplatz.status, 200);
  assert.equal(bundesplatz.inhalt.plz, '3011');
  assert.equal(bundesplatz.inhalt.ort, 'Bern');
  assert.equal(bundesplatz.inhalt.region, BERN);

  const muensingen = await umgebung({ breitengrad: 46.873, laengengrad: 7.5615 });
  assert.equal(muensingen.inhalt.plz, '3110');
  assert.equal(muensingen.inhalt.ort, 'Münsingen');

  // Die Antwort enthält keine Koordinaten.
  assert.deepEqual(Object.keys(bundesplatz.inhalt).sort(), ['gemeinde', 'kanton', 'ort', 'plz', 'plzUeberMehrereGemeinden', 'region']);
});

test('Standort ausserhalb der Schweiz liefert 404 mit Hinweis', async () => {
  const paris = await umgebung({ breitengrad: 48.8566, laengengrad: 2.3522 });
  assert.equal(paris.status, 404);
  assert.match(paris.inhalt.fehler, /keine Schweizer Postleitzahl/);
});

test('Ungültige oder fehlende Koordinaten werden abgewiesen', async () => {
  for (const daten of [
    { breitengrad: 91, laengengrad: 7.4 },
    { breitengrad: 46.9, laengengrad: -181 },
    { breitengrad: 46.9 },
    { laengengrad: 7.4 },
    {},
  ]) {
    assert.equal((await umgebung(daten)).status, 400, JSON.stringify(daten));
  }
});

test('Koordinaten werden nur im Rumpf angenommen, nie in der Adresse', async () => {
  const { status } = await aufruf('GET', '/api/orte/umgebung?breitengrad=46.9466&laengengrad=7.444');
  assert.equal(status, 405);
});
