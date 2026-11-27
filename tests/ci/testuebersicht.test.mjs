// Tests für die Testübersicht der CI (node:test).
// Aufruf: node --test "tests/ci/*.test.mjs"

import { test } from 'node:test';
import assert from 'node:assert/strict';
import { spawnSync } from 'node:child_process';
import { mkdtempSync, readFileSync, rmSync, writeFileSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { join } from 'node:path';
import { fileURLToPath } from 'node:url';
import { auswerten, tabelle } from '../../scripts/ci/testuebersicht.mjs';

const SKRIPT = fileURLToPath(new URL('../../scripts/ci/testuebersicht.mjs', import.meta.url));

test('node:test im Format TAP', () => {
  const ausgabe = ['ok 132 - Vorschläge', '1..132', '# tests 132', '# suites 0', '# pass 126', '# fail 0',
    '# cancelled 0', '# skipped 6', '# todo 0', '# duration_ms 37417.45'].join('\n');
  assert.deepEqual(auswerten(ausgabe), { tests: 132, bestanden: 126, fehlgeschlagen: 0, uebersprungen: 6 });
});

test('node:test mit dem Reporter spec', () => {
  const ausgabe = ['✔ Anmeldung (12ms)', 'ℹ tests 13', 'ℹ suites 0', 'ℹ pass 12', 'ℹ fail 1', 'ℹ cancelled 0',
    'ℹ skipped 0', 'ℹ todo 0'].join('\n');
  assert.deepEqual(auswerten(ausgabe), { tests: 13, bestanden: 12, fehlgeschlagen: 1, uebersprungen: 0 });
});

test('dotnet test mit Microsoft.Testing.Platform', () => {
  const ausgabe = ['Test run summary: Passed! - Soforthilfe.Tests.dll (net10.0|x64)', '  total: 273', '  failed: 0',
    '  succeeded: 273', '  skipped: 0', '  duration: 4s 590ms'].join('\n');
  assert.deepEqual(auswerten(ausgabe), { tests: 273, bestanden: 273, fehlgeschlagen: 0, uebersprungen: 0 });
});

test('Playwright mit dem Reporter list', () => {
  const ausgabe = ['  ✓  38 [mobil] › mobil.spec.mjs:5:1 › Startseite (1.2s)', '', '  1 flaky', '  37 passed (1.4m)'].join('\n');
  assert.deepEqual(auswerten(ausgabe), { tests: 38, bestanden: 38, fehlgeschlagen: 0, uebersprungen: 0 });
});

test('Playwright mit Fehlern', () => {
  const ausgabe = ['  2 failed', '    [desktop] › anmeldung.spec.mjs:3:1 › Anmeldung', '  1 skipped', '  35 passed (1.3m)'].join('\n');
  assert.deepEqual(auswerten(ausgabe), { tests: 38, bestanden: 35, fehlgeschlagen: 2, uebersprungen: 1 });
});

test('Rauchtest zählt jede Prüfung', () => {
  const ausgabe = ['== Regionen (Mandanten)', 'ok      Regionen auflisten (200)', 'ok      Region in der Liste (true)',
    'FEHLER  Kategorien: erwartet 200, erhalten 500', 'Rauchtest fehlgeschlagen.'].join('\n');
  assert.deepEqual(auswerten(ausgabe), { tests: 3, bestanden: 2, fehlgeschlagen: 1, uebersprungen: 0 });
});

test('unbekannte Ausgabe wird nicht gezählt', () => {
  assert.equal(auswerten('Build succeeded.\n0 Warning(s)'), null);
  assert.equal(auswerten(''), null);
});

test('Tabelle mit Ergebnis, fehlender und unbekannter Ausgabe', () => {
  const text = tabelle([
    { name: 'Einheitentests', vorhanden: true, ergebnis: { tests: 273, bestanden: 273, fehlgeschlagen: 0, uebersprungen: 0 } },
    { name: 'Rauchtest', vorhanden: true, ergebnis: { tests: 3, bestanden: 2, fehlgeschlagen: 1, uebersprungen: 0 } },
    { name: 'Oberflächentests', vorhanden: false, ergebnis: null },
    { name: 'Sonstiges', vorhanden: true, ergebnis: null },
  ]);
  const zeilen = text.split('\n');
  assert.equal(zeilen[0], '| Testart | Tests | bestanden | fehlgeschlagen | übersprungen | Ergebnis |');
  assert.equal(zeilen[2], '| Einheitentests | 273 | 273 | 0 | 0 | bestanden |');
  assert.equal(zeilen[3], '| Rauchtest | 3 | 2 | 1 | 0 | fehlgeschlagen |');
  assert.equal(zeilen[4], '| Oberflächentests | – | – | – | – | nicht gelaufen |');
  assert.equal(zeilen[5], '| Sonstiges | – | – | – | – | Ausgabe nicht erkannt |');
});

test('Aufruf schreibt in die Zusammenfassung und endet ohne Fehler', () => {
  const ordner = mkdtempSync(join(tmpdir(), 'testuebersicht-'));
  try {
    const ausgabe = join(ordner, 'api.log');
    const zusammenfassung = join(ordner, 'zusammenfassung.md');
    writeFileSync(ausgabe, '# tests 2\n# pass 2\n# fail 0\n# skipped 0\n');
    const lauf = spawnSync(process.execPath, [SKRIPT, `Schnittstellentests=${ausgabe}`, `Oberflächentests=${join(ordner, 'fehlt.log')}`],
      { encoding: 'utf8', env: { ...process.env, GITHUB_STEP_SUMMARY: zusammenfassung } });

    assert.equal(lauf.status, 0);
    const text = readFileSync(zusammenfassung, 'utf8');
    assert.match(text, /^### Testübersicht/);
    assert.match(text, /\| Schnittstellentests \| 2 \| 2 \| 0 \| 0 \| bestanden \|/);
    assert.match(text, /\| Oberflächentests \| – \| – \| – \| – \| nicht gelaufen \|/);
  } finally {
    rmSync(ordner, { recursive: true, force: true });
  }
});
