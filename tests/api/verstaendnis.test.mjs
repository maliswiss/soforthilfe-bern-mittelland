// Schnittstellentests: Einordnung einer Anfrage über die gewählte Kategorie, eindeutige Schlüsselwörter oder
// maschinelles Lernen (TF-IDF).

import { test } from 'node:test';
import assert from 'node:assert/strict';
import { aufruf, neuesKonto } from './hilfen.mjs';

const KATEGORIEAUSWAHL = 0;
const REGELN = 1;
const MASCHINELLES_LERNEN = 2;

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

test('Ohne eindeutiges Schlüsselwort ordnet das maschinelle Lernen ein und nennt Ähnlichkeit und Abstand', async () => {
  const { status, inhalt } = await verstehen({ freitext: 'Bitte Milch, Brot und Früchte vorbeibringen' });
  assert.equal(status, 200);
  assert.equal(inhalt.code, 'lieferdienst');
  assert.equal(inhalt.erfassungsweg, MASCHINELLES_LERNEN);
  assert.ok(inhalt.konfidenz >= 0.12 && inhalt.konfidenz < 1);
  assert.match(inhalt.begruendung,
    /^Klassifikation über maschinelles Lernen: Ähnlichkeit 0\.\d\d zur Kategorie «Lieferung Apotheke und Einkauf», Abstand zur zweitbesten Kategorie 0\.\d\d\.$/);
  assert.deepEqual(inhalt.alternativen, []);
});

test('Bei knappem Ergebnis bleibt die Kategorie offen, die drei ähnlichsten stehen zur Auswahl', async () => {
  const { inhalt } = await verstehen({ freitext: 'Ich brauche jemanden mit einem Bus für einen Tisch' });
  assert.equal(inhalt.kategorieId, null);
  assert.equal(inhalt.erfassungsweg, null);
  assert.match(inhalt.begruendung, /Bitte wählen Sie eine Kategorie/);
  assert.equal(inhalt.alternativen.length, 3);
  assert.equal(inhalt.alternativen[0].code, 'transporthilfe');
  assert.ok(inhalt.alternativen.every((a) => a.kategorieId && a.kategorie && a.aehnlichkeit > 0));
  const werte = inhalt.alternativen.map((a) => a.aehnlichkeit);
  assert.deepEqual(werte, [...werte].sort((a, b) => b - a));
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

  const ueberLernen = await aufruf('POST', '/api/anfragen', { token, daten: { freitext: 'Bitte Milch, Brot und Früchte vorbeibringen', ...ort } });
  assert.equal(ueberLernen.inhalt.kategorieBezeichnung, 'Lieferung Apotheke und Einkauf');

  const offen = await aufruf('POST', '/api/anfragen', { token, daten: { freitext: 'Grüezi mitenand', ...ort } });
  assert.equal(offen.status, 201);
  assert.equal(offen.inhalt.kategorieId, null);
});
