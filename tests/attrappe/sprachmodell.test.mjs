// Prüft die Attrappe des Sprachmodells ohne Schnittstelle: Antworten im Format von Ollama, Merkwörter und das
// Zählen der Anfragen. Aufruf: node --test "tests/attrappe/*.test.mjs"

import { test } from 'node:test';
import assert from 'node:assert/strict';
import { attrappe, anfrageText, antwortFuer, MERKWOERTER, OHNE_KATEGORIE } from './sprachmodell.mjs';

// Gleicher Aufbau wie die Anweisung der Schnittstelle (OllamaSprachmodell.Anweisung)
const anweisung = (text) => `Du ordnest Hilfeanfragen aus dem Alltag einer Kategorie zu.
Erlaubte Kategorien:
- nachbarschaftshilfe: Nachbarschaftshilfe
Antworte nur mit JSON in der Form {"kategorie":"<kennung>"}.
Passt keine Kategorie, antworte {"kategorie":null}.

Anfrage: ${text}`;

async function mitServer(lauf) {
  const server = attrappe();
  await new Promise((fertig) => server.listen(0, '127.0.0.1', fertig));
  const basis = `http://127.0.0.1:${server.address().port}`;
  try {
    await lauf(basis);
  } finally {
    server.close();
  }
}

const erzeugen = (basis, rumpf) => fetch(`${basis}/api/generate`, {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify(rumpf),
});

test('Der Text der Anfrage steht nach dem letzten «Anfrage:», auch wenn er selbst das Wort enthält', () => {
  assert.equal(anfrageText(anweisung('Grüezi mitenand')), 'Grüezi mitenand');
  assert.equal(anfrageText(anweisung('Anfrage: Plimbo')), 'Plimbo');
  assert.equal(anfrageText('ohne Anfrage'), '');
});

test('Merkwörter bestimmen die Antwort, alle anderen Texte ergeben keine Kategorie', () => {
  assert.equal(antwortFuer('Grüezi mitenand, PLIMBO'), '{"kategorie":"nachbarschaftshilfe"}');
  assert.equal(antwortFuer('Quarzel'), '{"kategorie":"zauberei"}');
  assert.throws(() => JSON.parse(antwortFuer('wumpel')));
  assert.equal(antwortFuer('Die Heizung ist ausgefallen.'), OHNE_KATEGORIE);
  assert.equal(new Set(MERKWOERTER.map((m) => m.wort)).size, MERKWOERTER.length);
});

test('Antwort im Format von Ollama: «response» enthält das JSON des Modells', async () => {
  await mitServer(async (basis) => {
    const antwort = await erzeugen(basis, { model: 'llama3.2:3b', prompt: anweisung('Grüezi, Plimbo'), stream: false, format: 'json' });
    assert.equal(antwort.status, 200);
    const inhalt = await antwort.json();
    assert.equal(inhalt.model, 'llama3.2:3b');
    assert.equal(inhalt.done, true);
    assert.deepEqual(JSON.parse(inhalt.response), { kategorie: 'nachbarschaftshilfe' });
  });
});

test('Vorladen ohne Anweisung, Liste der Modelle und unbekannte Adressen', async () => {
  await mitServer(async (basis) => {
    const laden = await (await erzeugen(basis, { model: 'llama3.2:3b', keep_alive: '30m' })).json();
    assert.equal(laden.done_reason, 'load');
    assert.equal(laden.response, '');

    const modelle = await (await fetch(`${basis}/api/tags`)).json();
    assert.deepEqual(modelle.models.map((m) => m.name), ['attrappe']);
    assert.equal((await fetch(`${basis}/api/chat`, { method: 'POST' })).status, 404);
    assert.equal((await fetch(`${basis}/api/generate`, { method: 'POST', body: '{kein json' })).status, 400);
  });
});

test('Gezählt werden nur Anfragen mit Text, je enthaltenem Teil', async () => {
  await mitServer(async (basis) => {
    await erzeugen(basis, { model: 'x' });
    await erzeugen(basis, { model: 'x', prompt: anweisung('Plimbo 111') });
    await erzeugen(basis, { model: 'x', prompt: anweisung('Plimbo 111') });
    await erzeugen(basis, { model: 'x', prompt: anweisung('Grüezi 222') });

    const anzahl = async (teil) => (await (await fetch(`${basis}/attrappe/aufrufe?enthaelt=${encodeURIComponent(teil)}`)).json()).anzahl;
    assert.equal(await anzahl('111'), 2);
    assert.equal(await anzahl('Grüezi'), 1);
    assert.equal(await anzahl('333'), 0);
  });
});
