// Schnittstellentests: Einordnung einer Anfrage über die gewählte Kategorie, eindeutige Schlüsselwörter,
// maschinelles Lernen (TF-IDF) oder das lokale Sprachmodell. Ohne laufendes Sprachmodell (wie in der CI) bleiben
// unsichere Texte offen; mit Sprachmodell darf die letzte Stufe entscheiden.

import { test } from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs';
import { aufruf, neuesKonto } from './hilfen.mjs';

const KATEGORIEAUSWAHL = 0;
const REGELN = 1;
const MASCHINELLES_LERNEN = 2;
const SPRACHMODELL = 3;

// Hat das Sprachmodell entschieden, gelten Kategorie, Weg und Begründung der letzten Stufe.
function sprachmodellHatEntschieden(inhalt) {
  if (inhalt.erfassungsweg !== SPRACHMODELL) {
    return false;
  }
  assert.ok(inhalt.kategorieId);
  assert.equal(inhalt.konfidenz, null);
  assert.match(inhalt.begruendung, /lokale Sprachmodell/);
  assert.deepEqual(inhalt.alternativen, []);
  return true;
}

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
  if (sprachmodellHatEntschieden(inhalt)) {
    return;
  }
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
  if (sprachmodellHatEntschieden(inhalt)) {
    return;
  }
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
  // Ohne Sprachmodell bleibt die Kategorie leer; mit Sprachmodell darf es eine wählen
  const mitSprachmodell = (await verstehen({ freitext: 'Grüezi mitenand' })).inhalt.erfassungsweg === SPRACHMODELL;
  assert.ok(mitSprachmodell || offen.inhalt.kategorieId === null);
});

test('Die Texte der Messung werden richtig eingeordnet oder stehen mit der richtigen Kategorie zur Auswahl', async () => {
  const faelle = JSON.parse(fs.readFileSync(new URL('../messprotokoll/testfaelle.json', import.meta.url), 'utf8')).faelle
    .filter((f) => f.freitext);
  let richtig = 0;

  for (const { nr, freitext, erwartet } of faelle) {
    const { status, inhalt } = await verstehen({ freitext });
    assert.equal(status, 200, `Fall ${nr}`);
    assert.equal(inhalt.notfall, false, `Fall ${nr}`);
    if (sprachmodellHatEntschieden(inhalt)) {
      continue;
    }

    if (inhalt.kategorieId) {
      // Nie sicher falsch: eine eingeordnete Kategorie muss die erwartete sein
      assert.equal(inhalt.code, erwartet, `Fall ${nr}: «${freitext}»`);
      assert.ok([REGELN, MASCHINELLES_LERNEN].includes(inhalt.erfassungsweg), `Fall ${nr}`);
      assert.ok(inhalt.konfidenz > 0 && inhalt.konfidenz <= 1, `Fall ${nr}: Sicherheit ${inhalt.konfidenz}`);
      richtig += 1;
    } else {
      // Unsicher: die erwartete Kategorie steht unter den drei ähnlichsten
      assert.ok(inhalt.alternativen.some((a) => a.code === erwartet), `Fall ${nr}: ${erwartet} nicht zur Auswahl`);
    }
  }

  assert.ok(richtig >= 16, `nur ${richtig} von ${faelle.length} direkt eingeordnet`);
});

test('Alternativen: höchstens drei, ohne Doppelte, nach Ähnlichkeit geordnet', async () => {
  for (const freitext of ['Kann jemand etwas für mich erledigen?', 'Ich brauche dringend Unterstützung zu Hause.']) {
    const { inhalt } = await verstehen({ freitext });
    if (inhalt.kategorieId) {
      continue;
    }
    const liste = inhalt.alternativen;
    assert.ok(liste.length <= 3, freitext);
    assert.equal(new Set(liste.map((a) => a.code)).size, liste.length, freitext);
    assert.deepEqual(liste.map((a) => a.aehnlichkeit), [...liste.map((a) => a.aehnlichkeit)].sort((a, b) => b - a), freitext);
  }
});

test('Ein Freitext über 2000 Zeichen wird abgewiesen', async () => {
  const lang = 'Bitte helfen Sie mir beim Tragen. '.repeat(70);
  assert.ok(lang.length > 2000);
  assert.equal((await verstehen({ freitext: lang })).status, 400);

  const { token } = await neuesKonto();
  const erfasst = await aufruf('POST', '/api/anfragen', { token, daten: { freitext: lang, plz: '3011', ort: 'Bern' } });
  assert.equal(erfasst.status, 400);
});
