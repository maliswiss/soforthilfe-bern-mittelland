// Schnittstellentests: Sprachimport ohne Neustart und ohne Deployment durch den Plattformbetrieb.

import { test } from 'node:test';
import assert from 'node:assert/strict';
import { aufruf, neuesKonto, testkonto, OHNE_TESTPASSWORT } from './hilfen.mjs';

const plattformbetrieb = () => testkonto('hans.betrieb@beispiel.invalid');
const importieren = (token, code, daten) => aufruf('PUT', `/api/sprachen/${code}`, { token, daten });

test('Aktive Sprachen und Texte ohne Anmeldung lesen, verwalten nur der Plattformbetrieb', async () => {
  const liste = await aufruf('GET', '/api/sprachen');
  assert.equal(liste.status, 200);
  assert.ok(Array.isArray(liste.inhalt));

  assert.equal((await aufruf('GET', '/api/sprachen/verwaltung')).status, 401);
  assert.equal((await importieren(undefined, 'xx', { name: 'Testsprache', aktiv: true, texte: { a: 'b' } })).status, 401);

  const { token } = await neuesKonto();
  assert.equal((await aufruf('GET', '/api/sprachen/verwaltung', { token })).status, 403);
  assert.equal((await importieren(token, 'xx', { name: 'Testsprache', aktiv: true, texte: { a: 'b' } })).status, 403);
  assert.equal((await aufruf('DELETE', '/api/sprachen/xx', { token })).status, 403);
});

test('Eine importierte Sprache gilt sofort, Änderungen ohne Neustart, abschalten und löschen', { skip: OHNE_TESTPASSWORT }, async () => {
  const token = await plattformbetrieb();
  await aufruf('DELETE', '/api/sprachen/xx', { token });

  const neu = await importieren(token, 'XX', { name: ' Testsprache ', aktiv: true, texte: { start: { titel1: 'Prova', titel2: 'Regione {region}' } } });
  assert.equal(neu.status, 200);
  assert.equal(neu.inhalt.code, 'xx');
  assert.equal(neu.inhalt.name, 'Testsprache');
  assert.equal(neu.inhalt.anzahlTexte, 2);

  assert.ok((await aufruf('GET', '/api/sprachen')).inhalt.some((s) => s.code === 'xx'));
  assert.deepEqual((await aufruf('GET', '/api/sprachen/xx/texte')).inhalt, { start: { titel1: 'Prova', titel2: 'Regione {region}' } });

  // Ein erneuter Import ersetzt die Texte und gilt beim nächsten Abruf
  await importieren(token, 'xx', { name: 'Testsprache', aktiv: true, texte: { start: { titel1: 'Nuovo' } } });
  assert.deepEqual((await aufruf('GET', '/api/sprachen/xx/texte')).inhalt, { start: { titel1: 'Nuovo' } });

  // Ausgeschaltet: nicht mehr im Umschalter und ohne Texte, in der Verwaltung weiterhin sichtbar
  const aus = await aufruf('PUT', '/api/sprachen/xx/aktiv', { token, daten: { aktiv: false } });
  assert.equal(aus.status, 200);
  assert.equal(aus.inhalt.aktiv, false);
  assert.ok(!(await aufruf('GET', '/api/sprachen')).inhalt.some((s) => s.code === 'xx'));
  assert.equal((await aufruf('GET', '/api/sprachen/xx/texte')).status, 404);
  assert.ok((await aufruf('GET', '/api/sprachen/verwaltung', { token })).inhalt.some((s) => s.code === 'xx' && !s.aktiv));

  const protokoll = (await aufruf('GET', '/api/protokoll?anzahl=500', { token })).inhalt.filter((e) => e.objektId === 'xx');
  assert.ok(protokoll.some((e) => e.aktion === 'Sprache.Importiert' && e.detail === '2 Texte, aktiv'));
  assert.ok(protokoll.some((e) => e.aktion === 'Sprache.Deaktiviert'));

  assert.equal((await aufruf('DELETE', '/api/sprachen/xx', { token })).status, 204);
  assert.equal((await aufruf('DELETE', '/api/sprachen/xx', { token })).status, 404);
  assert.equal((await aufruf('PUT', '/api/sprachen/xx/aktiv', { token, daten: { aktiv: true } })).status, 404);
});

test('Ungültige Sprachdateien werden mit Hinweis auf das Feld abgewiesen und nicht gespeichert', { skip: OHNE_TESTPASSWORT }, async () => {
  const token = await plattformbetrieb();
  const fall = async (code, daten, feld, meldung) => {
    const { status, inhalt } = await importieren(token, code, daten);
    assert.equal(status, 400, JSON.stringify(daten));
    assert.equal(inhalt.feld?.toLowerCase(), feld, JSON.stringify(daten));
    if (meldung) {
      assert.match(inhalt.fehler, meldung);
    }
  };

  const gut = { name: 'Testsprache', aktiv: true, texte: { start: { titel1: 'Prova' } } };
  await fall('ita', gut, 'code');
  await fall('xy', { ...gut, name: 'X' }, 'name');
  await fall('xy', { ...gut, texte: { start: { titel1: '<script>alert(1)</script>' } } }, 'texte', /start\.titel1/);
  await fall('xy', { ...gut, texte: { start: { titel1: 'Regione {region' } } }, 'texte', /Platzhalter/);
  await fall('xy', { ...gut, texte: [] }, 'texte');
  await fall('xy', { ...gut, texte: {} }, 'texte');

  assert.equal((await aufruf('GET', '/api/sprachen/xy/texte')).status, 404);
});

test('Grenzen der Sprachdatei: Anzahl, Länge, Tiefe, Schlüssel und Grösse', { skip: OHNE_TESTPASSWORT }, async () => {
  const token = await plattformbetrieb();
  const fall = async (texte, meldung) => {
    const { status, inhalt } = await importieren(token, 'xv', { name: 'Testsprache', aktiv: true, texte });
    assert.equal(status, 400, meldung.source);
    assert.equal(inhalt.feld, 'texte');
    assert.match(inhalt.fehler, meldung);
  };

  const viele = (anzahl) => ({ g: Object.fromEntries(Array.from({ length: anzahl }, (_, i) => [`t${i}`, 'x'])) });
  await fall(viele(3001), /Höchstens 3000 Texte/);
  await fall({ start: { titel1: 'a'.repeat(1001) } }, /höchstens 1000 Zeichen\. Betroffen: «start\.titel1»/);
  await fall({ a: { b: { c: { d: { e: { f: 'tief' } } } } } }, /Höchstens 5 Ebenen/);
  await fall({ 'titel eins': 'Prova' }, /Schlüssel bestehen aus/);
  await fall({ start: { anzahl: 3 } }, /entweder ein Text oder eine Gruppe von Texten\. Betroffen: «start\.anzahl»/);
  await fall({ start: { titel1: 'Mail an info@beispiel.invalid' } }, /< > @ \|/);
  assert.equal((await aufruf('GET', '/api/sprachen/xv/texte')).status, 404);

  // Genau 3000 Texte sind erlaubt
  const grenze = await importieren(token, 'xv', { name: 'Testsprache', aktiv: false, texte: viele(3000) });
  assert.equal(grenze.status, 200);
  assert.equal(grenze.inhalt.anzahlTexte, 3000);
  assert.equal((await aufruf('DELETE', '/api/sprachen/xv', { token })).status, 204);

  // Über 1 MB nimmt die Schnittstelle nicht an: Status 413, kein Serverfehler
  const gross = { g: Object.fromEntries(Array.from({ length: 1200 }, (_, i) => [`t${i}`, 'x'.repeat(900)])) };
  const zuGross = await importieren(token, 'xv', { name: 'Testsprache', aktiv: true, texte: gross });
  assert.equal(zuGross.status, 413);
  assert.match(zuGross.inhalt.fehler, /zu gross/);
  assert.equal((await aufruf('GET', '/api/sprachen/verwaltung', { token })).inhalt.some((s) => s.code === 'xv'), false);
});

test('Ein ungültiger erneuter Import lässt die bisherigen Texte unverändert', { skip: OHNE_TESTPASSWORT }, async () => {
  const token = await plattformbetrieb();
  await aufruf('DELETE', '/api/sprachen/xw', { token });
  const texte = { start: { titel1: 'Prova' } };
  assert.equal((await importieren(token, 'xw', { name: 'Testsprache', aktiv: true, texte })).status, 200);

  const falsch = await importieren(token, 'xw', { name: 'Testsprache', aktiv: true, texte: { start: { titel1: 'Regione {region' } } });
  assert.equal(falsch.status, 400);
  assert.deepEqual((await aufruf('GET', '/api/sprachen/xw/texte')).inhalt, texte);
  assert.equal((await aufruf('GET', '/api/sprachen/verwaltung', { token })).inhalt.find((s) => s.code === 'xw')?.anzahlTexte, 1);

  assert.equal((await aufruf('DELETE', '/api/sprachen/xw', { token })).status, 204);
});
