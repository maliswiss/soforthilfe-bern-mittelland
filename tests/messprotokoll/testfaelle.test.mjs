// Prüft die Testfälle der Messung, bevor gemessen wird: Aufbau, Kategorien und Fähigkeiten aus den Stammdaten,
// Preisangaben bei kostenpflichtigen Kategorien, neue Texte und ob die erwarteten Helfenden erreichbar und die
// nächsten geeigneten sind. Läuft ohne Schnittstelle: node --test "tests/messprotokoll/*.test.mjs"

import { test } from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs';
import path from 'node:path';

const wurzel = path.resolve(import.meta.dirname, '../..');
const lesen = (...teile) => fs.readFileSync(path.join(wurzel, ...teile), 'utf8');
const daten = JSON.parse(lesen('tests/messprotokoll/testfaelle.json'));

// Kategorien und Bezahlpflicht aus den Stammdaten
const stammdaten = lesen('backend/src/Soforthilfe.Datenzugriff/Modell/Stammdaten.cs');
const kategorien = new Map(
  [...stammdaten.matchAll(/Id = Kategorie\w+, Kennung = "([a-z-]+)",[^\n]*Kostenpflichtig = (true|false)/g)]
    .map((m) => [m[1], m[2] === 'true'])
);

// Punkte der Postleitzahlen wie im Ortschaftenverzeichnis: gleicher Ortsname, sonst grösster Adressenanteil
const datenordner = path.join(wurzel, 'backend/src/Soforthilfe.Datenzugriff/Daten');
const csvDatei = fs.readdirSync(datenordner).find((d) => d.startsWith('amtliches_ortschaftenverzeichnis') && d.endsWith('.csv'));
const zeilen = fs.readFileSync(path.join(datenordner, csvDatei), 'utf8').replace(/^﻿/, '').split(/\r?\n/).filter(Boolean);
const kopf = zeilen[0].split(';').map((s) => s.trim());
const spalte = (w, name) => w[kopf.indexOf(name)].trim();
const ortschaften = zeilen.slice(1).map((z) => {
  const w = z.split(';');
  return { plz: spalte(w, 'PLZ4'), name: spalte(w, 'Ortschaftsname'), anteil: parseFloat(spalte(w, 'Adressenanteil')), lat: +spalte(w, 'N'), lon: +spalte(w, 'E') };
});
function punkt(plz, ort) {
  const alle = ortschaften.filter((o) => o.plz === plz);
  const gleich = alle.filter((o) => o.name.toLowerCase() === ort.toLowerCase());
  return (gleich.length ? gleich : alle).sort((a, b) => b.anteil - a.anteil)[0];
}
function meter(a, b) {
  const r = Math.PI / 180;
  const h = Math.sin((b.lat - a.lat) * r / 2) ** 2 + Math.cos(a.lat * r) * Math.cos(b.lat * r) * Math.sin((b.lon - a.lon) * r / 2) ** 2;
  return 6371008.8 * 2 * Math.asin(Math.sqrt(h));
}

// Verifizierte Helfende der Testdaten, die bei der Messung ebenfalls in Frage kommen
const testdaten = [{ name: 'Bruno Muster', plz: '3012', ort: 'Bern', radiusKm: 15, skills: ['sanitaer-elektro', 'kleine-reparatur'] }];
const helfende = [...daten.helfende, ...testdaten];

// Kleinster Suchradius bei hoher Dringlichkeit ist 15 km; mit Reserve für die Messung von PostGIS
const hoechstensMeter = 15000 * 0.98;

test('20 Fälle und 4 Notfälle mit vollständigen Angaben', () => {
  assert.equal(daten.region, 'bern-mittelland');
  assert.deepEqual(daten.faelle.map((f) => f.nr), Array.from({ length: 20 }, (_, i) => i + 1));
  assert.deepEqual(daten.notfaelle.map((n) => n.nr), ['N1', 'N2', 'N3', 'N4']);
  assert.ok(daten.faelle.filter((f) => f.sprache === 'fr').length >= 5, 'mindestens fünf französische Fälle');

  for (const f of daten.faelle) {
    assert.match(f.plz, /^[1-9]\d{3}$/, `Fall ${f.nr}`);
    assert.ok(f.ort, `Fall ${f.nr}`);
    assert.ok(['de', 'fr'].includes(f.sprache), `Fall ${f.nr}`);
    assert.ok(Boolean(f.freitext) !== Boolean(f.kategorie), `Fall ${f.nr}: Freitext oder Kategorie`);
    assert.ok(kategorien.has(f.erwartet), `Fall ${f.nr}: Kategorie ${f.erwartet}`);
    if (f.kategorie) {
      assert.equal(f.kategorie, f.erwartet, `Fall ${f.nr}`);
    }
    assert.ok(f.soll.length > 0, `Fall ${f.nr}`);
  }
  for (const n of daten.notfaelle) {
    assert.ok(n.freitext.length > 10 && ['de', 'fr'].includes(n.sprache), n.nr);
  }
});

test('Messdaten: eindeutige Namen, bekannte Fähigkeiten, Preise bei kostenpflichtigen Kategorien', () => {
  const namen = daten.helfende.map((h) => h.name);
  assert.equal(new Set(namen).size, namen.length);
  for (const h of daten.helfende) {
    assert.ok([0, 1].includes(h.typ), h.name);
    assert.ok(h.radiusKm >= 1 && h.radiusKm <= 50, h.name);
    assert.ok(punkt(h.plz, h.ort), `${h.name}: Postleitzahl ${h.plz}`);
    assert.ok(h.skills.every((s) => kategorien.has(s)), h.name);
    if (h.skills.some((s) => kategorien.get(s))) {
      assert.ok(h.preise?.anfahrtChf > 0 && h.preise?.stundenansatzChf > 0, `${h.name}: Preise`);
    }
  }
});

test('Die Texte stehen weder in den Trainingsdaten noch im Kalibrierungssatz', () => {
  const quellen = lesen('backend/src/Soforthilfe.Dienste/Verstaendnis/Trainingsdaten.cs')
    + lesen('backend/tests/Soforthilfe.Tests/TfIdfKlassifikatorTests.cs');
  for (const text of [...daten.faelle.map((f) => f.freitext).filter(Boolean), ...daten.notfaelle.map((n) => n.freitext)]) {
    assert.ok(!quellen.includes(text), text);
  }
});

test('Erwartete Helfende haben die Fähigkeit, sind erreichbar und die nächsten geeigneten', () => {
  for (const f of daten.faelle) {
    const ziel = punkt(f.plz, f.ort);
    assert.ok(ziel, `Fall ${f.nr}: Postleitzahl ${f.plz}`);

    const geeignet = helfende
      .filter((h) => h.skills.includes(f.erwartet))
      .map((h) => ({ name: h.name, distanz: meter(ziel, punkt(h.plz, h.ort)), radius: h.radiusKm * 1000 }))
      .filter((h) => h.distanz <= h.radius)
      .sort((a, b) => a.distanz - b.distanz);

    for (const name of f.soll) {
      const h = geeignet.find((g) => g.name === name);
      assert.ok(h, `Fall ${f.nr}: ${name} mit Fähigkeit und im eigenen Einsatzradius`);
      assert.ok(h.distanz <= hoechstensMeter && h.distanz <= h.radius * 0.98, `Fall ${f.nr}: ${name} sicher erreichbar`);
    }

    // Neue Helfende unterscheiden sich nur in der Distanz; der nächste geeignete gehört zu den erwarteten
    assert.ok(f.soll.includes(geeignet[0].name), `Fall ${f.nr}: nächster ist ${geeignet[0].name}`);
  }
});
