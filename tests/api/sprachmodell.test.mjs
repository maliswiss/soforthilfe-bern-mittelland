// Schnittstellentests der letzten Stufe der Verständniskette mit der Attrappe des Sprachmodells, wie in der CI.
// Sie laufen nur, wenn SPRACHMODELL_ATTRAPPE auf die laufende Attrappe zeigt (tests/attrappe/sprachmodell.mjs) und
// die Schnittstelle sie über OLLAMA_BASE_URL nutzt. Mit dem echten Modell oder ohne Modell werden sie übersprungen.
// Jeder Text erhält eine eigene Nummer, damit er nicht aus dem Zwischenspeicher eines früheren Laufs kommt.

import { test } from 'node:test';
import assert from 'node:assert/strict';
import { randomInt } from 'node:crypto';
import { aufruf, neuesKonto } from './hilfen.mjs';

const ATTRAPPE = process.env.SPRACHMODELL_ATTRAPPE?.replace(/\/$/, '');
const OHNE_ATTRAPPE = ATTRAPPE ? false : 'SPRACHMODELL_ATTRAPPE ist nicht gesetzt';

const REGELN = 1;
const SPRACHMODELL = 3;
const BEGRUENDUNG = 'Eingeordnet durch das lokale Sprachmodell, weil Regeln und maschinelles Lernen nicht sicher entschieden haben.';

const nummer = () => String(Date.now()) + String(randomInt(1000, 9999));
const verstehen = (freitext) => aufruf('POST', '/api/anfragen/verstehen', { daten: { freitext } });

/** Wie oft die Attrappe einen Text mit diesem Teil erhalten hat. */
async function aufrufe(teil) {
  const antwort = await fetch(`${ATTRAPPE}/attrappe/aufrufe?enthaelt=${encodeURIComponent(teil)}`);
  return (await antwort.json()).anzahl;
}

test('Bleiben Regeln und maschinelles Lernen offen, gilt die Kategorie des Sprachmodells', { skip: OHNE_ATTRAPPE }, async () => {
  const n = nummer();
  const { status, inhalt } = await verstehen(`Grüezi mitenand, Plimbo ${n}`);
  assert.equal(status, 200);
  assert.equal(inhalt.notfall, false);
  assert.equal(inhalt.code, 'nachbarschaftshilfe');
  assert.equal(inhalt.kategorie, 'Nachbarschaftshilfe');
  assert.equal(inhalt.erfassungsweg, SPRACHMODELL);
  assert.equal(inhalt.konfidenz, null);
  assert.equal(inhalt.begruendung, BEGRUENDUNG);
  assert.deepEqual(inhalt.alternativen, []);
  assert.equal(await aufrufe(n), 1);
});

test('Eine Anfrage nur mit Freitext speichert die Kategorie und den Weg über das Sprachmodell', { skip: OHNE_ATTRAPPE }, async () => {
  const { token } = await neuesKonto();
  const erfasst = await aufruf('POST', '/api/anfragen', {
    token, daten: { freitext: `Grüezi mitenand, Plimbo ${nummer()}`, plz: '3011', ort: 'Bern' },
  });
  assert.equal(erfasst.status, 201);
  assert.equal(erfasst.inhalt.kategorieCode, 'nachbarschaftshilfe');
  assert.equal(erfasst.inhalt.erfassungsweg, SPRACHMODELL);
});

test('Unbekannte Kennung oder unlesbare Antwort: keine Kategorie, die ähnlichsten stehen zur Auswahl', { skip: OHNE_ATTRAPPE }, async () => {
  for (const wort of ['Quarzel', 'Wumpel']) {
    const n = nummer();
    const { inhalt } = await verstehen(`Grüezi mitenand, ${wort} ${n}`);
    assert.equal(inhalt.kategorieId, null, wort);
    assert.equal(inhalt.erfassungsweg, null, wort);
    assert.match(inhalt.begruendung, /Bitte wählen Sie eine Kategorie/, wort);
    assert.equal(await aufrufe(n), 1, `${wort}: gefragt`);
  }

  // Nach einer unlesbaren Antwort entscheidet das Sprachmodell beim nächsten Text wieder, ohne Pause
  const weiter = await verstehen(`Grüezi mitenand, Plimbo ${nummer()}`);
  assert.equal(weiter.inhalt.erfassungsweg, SPRACHMODELL);
});

test('Entscheiden die Regeln, wird das Sprachmodell nicht gefragt', { skip: OHNE_ATTRAPPE }, async () => {
  const n = nummer();
  const { inhalt } = await verstehen(`Die Heizung ist ausgefallen. Plimbo ${n}`);
  assert.equal(inhalt.code, 'sanitaer-elektro');
  assert.equal(inhalt.erfassungsweg, REGELN);
  assert.equal(await aufrufe(n), 0);
});

test('Gleiche Texte kommen aus dem Zwischenspeicher, ohne neue Frage an das Modell', { skip: OHNE_ATTRAPPE }, async () => {
  const n = nummer();
  const text = `Grüezi mitenand, Plimbo ${n}`;
  const erste = await verstehen(text);
  const zweite = await verstehen(text);
  assert.equal(erste.inhalt.code, 'nachbarschaftshilfe');
  assert.deepEqual(zweite.inhalt, erste.inhalt);
  assert.equal(await aufrufe(n), 1);
});

test('Bei einem Notfallhinweis wird das Sprachmodell nicht gefragt', { skip: OHNE_ATTRAPPE }, async () => {
  const n = nummer();
  const { inhalt } = await verstehen(`Plimbo ${n}: mein Nachbar ist bewusstlos.`);
  assert.equal(inhalt.notfall, true);
  assert.equal(await aufrufe(n), 0);
});
