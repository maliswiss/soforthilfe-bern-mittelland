// Schnittstellentests: Notfall-Stopp mit Hinweis «144 anrufen».

import { test } from 'node:test';
import assert from 'node:assert/strict';
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
