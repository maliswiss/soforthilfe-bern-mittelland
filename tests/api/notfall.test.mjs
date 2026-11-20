// Schnittstellentests: Notfall-Stopp mit Hinweis «144 anrufen».

import { test } from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs';
import { aufruf, neuesKonto } from './hilfen.mjs';

const verstehen = (daten) => aufruf('POST', '/api/anfragen/verstehen', { daten });

test('Akute gesundheitliche Hinweise stoppen die Vermittlung, auch ohne Anmeldung', async () => {
  const faelle = [
    ['Mein Mann ist bewusstlos auf dem Boden.', 'bewusstlos'],
    ['Ich glaube, mein Vater hat einen Herzinfarkt!', 'Herzinfarkt'],
    ['Mon voisin est inconscient dans la cuisine.', 'inconscient'],
    ['My father is having a heart attack', 'heart attack'],
  ];
  for (const [freitext, wort] of faelle) {
    const { status, inhalt } = await verstehen({ freitext });
    assert.equal(status, 200, freitext);
    assert.equal(inhalt.notfall, true, freitext);
    assert.equal(inhalt.notfallGrund, `Erkanntes Schlüsselwort: ${wort}`, freitext);
    assert.match(inhalt.notfallHinweis, /144/, freitext);
  }
});

test('Alltägliche Anliegen, Brand und Einbruch lösen keinen Notfall-Stopp aus', async () => {
  for (const freitext of [
    'Mein Schlüssel ist im Schloss abgebrochen.',
    'Notfall: Wasserrohrbruch im Keller.',
    'In der Küche brennt es.',
    'Bei uns wurde eingebrochen.',
  ]) {
    const { status, inhalt } = await verstehen({ freitext });
    assert.equal(status, 200, freitext);
    assert.equal(inhalt.notfall, false, freitext);
    assert.equal(inhalt.notfallGrund, null, freitext);
    assert.equal(inhalt.notfallHinweis, null, freitext);
  }
});

test('Verständnis braucht eine Kategorie oder einen Freitext', async () => {
  const { status, inhalt } = await verstehen({ freitext: '   ' });
  assert.equal(status, 400);
  assert.equal(inhalt.feld, 'freitext');

  const { inhalt: kategorien } = await aufruf('GET', '/api/kategorien');
  const nurKategorie = await verstehen({ kategorieId: kategorien[0].id });
  assert.equal(nurKategorie.status, 200);
  assert.equal(nurKategorie.inhalt.notfall, false);
});

test('Eine Anfrage mit Notfallhinweis wird nicht gespeichert, sondern mit 451 und Hinweis auf 144 beantwortet', async () => {
  const { token } = await neuesKonto();
  const { status, inhalt } = await aufruf('POST', '/api/anfragen', {
    token,
    daten: { freitext: 'Meine Nachbarin bekommt keine Luft mehr.', plz: '3011', ort: 'Bern' },
  });

  assert.equal(status, 451);
  assert.equal(inhalt.notfall, true);
  assert.match(inhalt.hinweis, /144/);
  assert.equal(inhalt.grund, 'Erkanntes Schlüsselwort: bekommt keine Luft');
  assert.match(inhalt.fehler, /144/);

  const meine = await aufruf('GET', '/api/anfragen/meine', { token });
  assert.equal(meine.inhalt.length, 0, 'keine Anfrage gespeichert');
});

test('Die Notfälle der Messung werden im Verständnis-Schritt und beim Erfassen gestoppt', async () => {
  const faelle = JSON.parse(fs.readFileSync(new URL('../messprotokoll/testfaelle.json', import.meta.url), 'utf8')).notfaelle;
  assert.equal(faelle.length, 4);
  const { token } = await neuesKonto();

  for (const { nr, freitext } of faelle) {
    const verstanden = await verstehen({ freitext });
    assert.equal(verstanden.inhalt.notfall, true, nr);
    assert.match(verstanden.inhalt.notfallHinweis, /144/, nr);
    assert.equal(verstanden.inhalt.kategorieId ?? null, null, `${nr}: keine Einordnung`);

    const erfasst = await aufruf('POST', '/api/anfragen', { token, daten: { freitext, plz: '3011', ort: 'Bern' } });
    assert.equal(erfasst.status, 451, nr);
  }

  const meine = await aufruf('GET', '/api/anfragen/meine', { token });
  assert.equal(meine.inhalt.length, 0, 'keine Anfrage gespeichert');
});

test('Ein Notfallhinweis stoppt auch bei gewählter Kategorie', async () => {
  const { inhalt: kategorien } = await aufruf('GET', '/api/kategorien');
  const begleitung = kategorien.find((k) => k.code === 'begleitung');
  const freitext = 'Bitte Begleitung, meine Mutter hat seit einer Stunde Atemnot.';

  const verstanden = await verstehen({ kategorieId: begleitung.id, freitext });
  assert.equal(verstanden.status, 200);
  assert.equal(verstanden.inhalt.notfall, true);
  assert.equal(verstanden.inhalt.notfallGrund, 'Erkanntes Schlüsselwort: Atemnot');

  const { token } = await neuesKonto();
  const erfasst = await aufruf('POST', '/api/anfragen', {
    token,
    daten: { kategorieId: begleitung.id, freitext, plz: '3011', ort: 'Bern', prioritaet: 0 },
  });
  assert.equal(erfasst.status, 451);
  assert.equal((await aufruf('GET', '/api/anfragen/meine', { token })).inhalt.length, 0);
});

test('Schreibweisen: Gross- und Kleinschreibung, Umlaute, Akzente und zusammengesetzte Wörter', async () => {
  for (const [freitext, fundstelle] of [
    ['MEIN VATER IST BEWUSSTLOS!!!', 'BEWUSSTLOS'],
    ['Sie ist ohnmaechtig geworden.', 'ohnmaechtig'],
    ['Er ist plötzlich ohnmächtig.', 'ohnmächtig'],
    ['Verdacht auf Herzinfarktsymptome bei meinem Onkel', 'Herzinfarktsymptome'],
    ['Mon père est ÉVANOUI dans le salon.', 'ÉVANOUI'],
    ['Mia madre non respira bene.', 'non respira'],
  ]) {
    const { inhalt } = await verstehen({ freitext });
    assert.equal(inhalt.notfall, true, freitext);
    assert.equal(inhalt.notfallGrund, `Erkanntes Schlüsselwort: ${fundstelle}`, freitext);
  }
});

test('Kurze Begriffe zählen nur als ganzes Wort', async () => {
  // «wehen» und «avc» haben höchstens fünf Buchstaben: in anderen Wörtern kein Stopp, als ganzes Wort ein Stopp
  for (const freitext of ['Schneewehen versperren die Garageneinfahrt, wer kann schaufeln?', 'Mein Navcom-Gerät im Auto ist kaputt.']) {
    assert.equal((await verstehen({ freitext })).inhalt.notfall, false, freitext);
  }
  for (const freitext of ['Bei meiner Frau haben die Wehen eingesetzt.', 'Elle a fait un AVC hier soir.']) {
    assert.equal((await verstehen({ freitext })).inhalt.notfall, true, freitext);
  }
});
