// Schnittstellentests: Begründung der Vorschläge mit Distanz, Fähigkeit, Bewertung, Verfügbarkeit und allen Teilpunkten.
// Die Anfrage liegt in 3150 Schwarzenburg, weit weg von den Profilen anderer Tests.

import { test } from 'node:test';
import assert from 'node:assert/strict';
import { aufruf, neuesKonto, testkonto, helferprofil, OHNE_TESTPASSWORT } from './hilfen.mjs';

const ort = { plz: '3150', ort: 'Schwarzenburg' };
const KRITERIEN = ['distanz', 'faehigkeit', 'bewertung', 'verfuegbarkeit', 'erfahrung'];
const GEWICHTE = [0.35, 0.25, 0.2, 0.1, 0.1];

test('Jeder Vorschlag nennt Distanz, Fähigkeit, Bewertung, Verfügbarkeit und alle Teilpunkte', { skip: OHNE_TESTPASSWORT }, async () => {
  const dora = await testkonto('dora.probe@beispiel.invalid');
  await helferprofil({ plz: '3152', ort: 'Mamishaus', radiusKm: 10, skills: ['nachbarschaftshilfe'], koordination: dora });

  const { token } = await neuesKonto();
  const kategorien = (await aufruf('GET', '/api/kategorien')).inhalt;
  const nachbarschaftshilfe = kategorien.find((k) => k.code === 'nachbarschaftshilfe');
  const erfasst = await aufruf('POST', '/api/anfragen', { token, daten: { kategorieId: nachbarschaftshilfe.id, prioritaet: 2, ...ort } });
  assert.equal(erfasst.status, 201);

  const liste = (await aufruf('GET', `/api/anfragen/${erfasst.inhalt.id}/vorschlaege`, { token })).inhalt;
  assert.ok(liste.length > 0);

  for (const v of liste) {
    // Text: Distanz, Fähigkeit mit Kategorie, Bewertung, Verifizierung, Verfügbarkeit, Einsätze
    const distanz = v.distanzMeter === 0 ? 'gleiche Postleitzahl'
      : v.distanzMeter < 1000 ? `${v.distanzMeter} m entfernt`
        : `${(Math.round(v.distanzMeter / 100) / 10).toFixed(1)} km entfernt`;
    assert.equal(v.begruendung,
      `${distanz} · Fähigkeit «${nachbarschaftshilfe.bezeichnung}» · noch keine Bewertungen · verifiziert · jetzt verfügbar · noch keine Einsätze`);

    // Alle Kriterien mit Teilpunkten, Gewicht und Beitrag; die Beiträge ergeben die Punktzahl
    assert.deepEqual(v.kriterien.map((k) => k.kriterium), KRITERIEN);
    v.kriterien.forEach((k, i) => {
      assert.ok(k.teilpunkte >= 0 && k.teilpunkte <= 1, `${k.kriterium} ${k.teilpunkte}`);
      assert.ok(Math.abs(k.gewicht - GEWICHTE[i]) < 1e-9, `${k.kriterium} Gewicht ${k.gewicht}`);
      assert.ok(Math.abs(k.beitrag - k.teilpunkte * k.gewicht) < 1e-4);
    });
    const summe = v.kriterien.reduce((s, k) => s + k.beitrag, 0);
    assert.ok(Math.abs(summe - v.punktzahl) < 0.001, `Summe ${summe}, Punktzahl ${v.punktzahl}`);

    const [distanzpunkte, faehigkeit, bewertung, verfuegbarkeit, erfahrung] = v.kriterien.map((k) => k.teilpunkte);
    assert.ok(Math.abs(distanzpunkte - (1 - v.distanzMeter / 15000)) < 0.001);
    assert.equal(faehigkeit, 1);
    assert.equal(bewertung, 0.6, 'ohne Bewertungen');
    assert.equal(verfuegbarkeit, 1);
    assert.equal(erfahrung, 0);
  }
});
