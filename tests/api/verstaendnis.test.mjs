// Schnittstellentests: Einordnung einer Anfrage über die gewählte Kategorie oder eindeutige Schlüsselwörter.

import { test } from 'node:test';
import assert from 'node:assert/strict';
import { aufruf, neuesKonto } from './hilfen.mjs';

const KATEGORIEAUSWAHL = 0;
const REGELN = 1;

const verstehen = (daten) => aufruf('POST', '/api/anfragen/verstehen', { daten });

async function kategorien() {
  const { inhalt } = await aufruf('GET', '/api/kategorien');
  return Object.fromEntries(inhalt.map((k) => [k.code, k]));
}

test('Die gewählte Kategorie gilt, auch wenn der Freitext etwas anderes nahelegt', async () => {
  const { begleitung } = await kategorien();
  const { status, inhalt } = await verstehen({ kategorieId: begleitung.id, freitext: 'Die Heizung ist ausgefallen.' });

  assert.equal(status, 200);
  assert.equal(inhalt.notfall, false);
  assert.equal(inhalt.kategorieId, begleitung.id);
  assert.equal(inhalt.kategorie, 'Begleitung und Orientierung');
  assert.equal(inhalt.code, 'begleitung');
  assert.equal(inhalt.bezahlpflichtig, false);
  assert.equal(inhalt.erfassungsweg, KATEGORIEAUSWAHL);
  assert.equal(inhalt.konfidenz, 1);
  assert.equal(inhalt.begruendung, 'Kategorie durch die anfragende Person gewählt: Begleitung und Orientierung.');
});

test('Eindeutige Schlüsselwörter ordnen den Freitext ein, auch auf Französisch, Italienisch und Englisch', async () => {
  const faelle = [
    ['Die Heizung ist ausgefallen.', 'sanitaer-elektro', 'Heizung'],
    ['Ich habe mich ausgesperrt.', 'schluesseldienst', 'ausgesperrt'],
    ['Mein Auto springt nicht an.', 'pannenhilfe', 'Auto'],
    ['Kann jemand meine Katze füttern?', 'nachbarschaftshilfe', 'Katze'],
    ['Le robinet de la cuisine coule.', 'sanitaer-elektro', 'robinet'],
    ['Mi serve aiuto con la stampante.', 'it-soforthilfe', 'stampante'],
    ['I am locked out of my flat.', 'schluesseldienst', 'locked out'],
  ];
  for (const [freitext, kennung, wort] of faelle) {
    const { status, inhalt } = await verstehen({ freitext });
    assert.equal(status, 200, freitext);
    assert.equal(inhalt.code, kennung, freitext);
    assert.equal(inhalt.erfassungsweg, REGELN, freitext);
    assert.ok(inhalt.konfidenz >= 0.7 && inhalt.konfidenz <= 0.95, freitext);
    assert.equal(inhalt.begruendung, `Erkannt über das Schlüsselwort «${wort}».`, freitext);
  }

  const kostenpflichtig = await verstehen({ freitext: 'Mein Auto springt nicht an.' });
  assert.equal(kostenpflichtig.inhalt.bezahlpflichtig, true);
  const kostenlos = await verstehen({ freitext: 'Kann jemand meine Katze füttern?' });
  assert.equal(kostenlos.inhalt.bezahlpflichtig, false);
});

test('Ohne erkennbaren Inhalt bleibt die Kategorie offen und die Begründung bittet um eine Auswahl', async () => {
  const { status, inhalt } = await verstehen({ freitext: 'Grüezi mitenand' });
  assert.equal(status, 200);
  assert.equal(inhalt.kategorieId, null);
  assert.equal(inhalt.kategorie, null);
  assert.match(inhalt.begruendung, /Bitte wählen Sie eine Kategorie/);
});

test('Bei einem Notfallhinweis wird nicht eingeordnet', async () => {
  const { inhalt } = await verstehen({ freitext: 'Mein Nachbar ist bewusstlos.' });
  assert.equal(inhalt.notfall, true);
  assert.equal(inhalt.kategorieId, null);
  assert.equal(inhalt.erfassungsweg, null);
  assert.deepEqual(inhalt.alternativen, []);
});

test('Eine Anfrage nur mit Freitext erhält die erkannte Kategorie', async () => {
  const { token } = await neuesKonto();
  const ort = { plz: '3011', ort: 'Bern' };

  const erkannt = await aufruf('POST', '/api/anfragen', { token, daten: { freitext: 'Mein Schlüssel ist im Schloss abgebrochen', ...ort } });
  assert.equal(erkannt.status, 201);
  assert.equal(erkannt.inhalt.kategorieBezeichnung, 'Schlüsseldienst');

  const offen = await aufruf('POST', '/api/anfragen', { token, daten: { freitext: 'Grüezi mitenand', ...ort } });
  assert.equal(offen.status, 201);
  assert.equal(offen.inhalt.kategorieId, null);
});
