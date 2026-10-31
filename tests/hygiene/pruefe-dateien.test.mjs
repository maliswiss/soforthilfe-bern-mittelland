// Tests für die Hygieneprüfung (node:test).
// Aufruf: node --test "tests/hygiene/*.test.mjs"
//
// Verbotene Namen und Geheimnisse werden in diesen Tests zur Laufzeit zusammengesetzt,
// damit sich das Suchmuster nicht im eigenen Quelltext findet.

import { test } from 'node:test';
import assert from 'node:assert/strict';
import { execFileSync, spawnSync } from 'node:child_process';
import { mkdtempSync, mkdirSync, readFileSync, rmSync, writeFileSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { dirname, join } from 'node:path';
import { fileURLToPath } from 'node:url';
import { pruefeDateien, pruefeInhalt, pruefePfad } from '../../scripts/hygiene/pruefe-dateien.mjs';

const SKRIPT = fileURLToPath(new URL('../../scripts/hygiene/pruefe-dateien.mjs', import.meta.url));
const DIESE_DATEI = fileURLToPath(import.meta.url);

const PW = ['Pass', 'word'].join('');
const UMGEBUNG = ['.', 'env'].join('');
const JWT_SCHLUESSEL = ['Jwt', 'Key'].join('_');
const BOT_SCHLUESSEL = ['telegram', 'bot', 'to' + 'ken'].join('_');
const CLIENT_SCHLUESSEL = 'Client' + 'Secret';
const TOKEN_SCHLUESSEL = 'to' + 'ken';
const API_SCHLUESSEL = 'Api' + 'Key';
const API_SCHLUESSEL_KLEIN = ['api', 'key'].join('_');

test('verbotene Pfade werden erkannt', () => {
  const verboten = [
    UMGEBUNG,
    `${UMGEBUNG}.local`,
    `deploy/${UMGEBUNG}`,
    'backend/src/Soforthilfe.Api/bin/Debug/app.dll',
    'backend/src/Soforthilfe.Api/obj/project.assets.json',
    'frontend/node_modules/vue/index.js',
    'frontend/dist/index.html',
    'tests/e2e/test-results/ergebnis.txt',
    'tests/e2e/playwright-report/index.html',
    'zap-berichte/bericht.html',
    'backend/src/Soforthilfe.Api/appsettings.Local.json',
    'backend/src/Soforthilfe.Api/appsettings.Development.Local.json',
    'secrets.json',
    'deploy/zertifikat.pfx',
    'deploy/schluessel.pem',
    'deploy/server.key',
    'deploy/server.crt',
    'sicherung/datenbank.dump',
    'sicherung/datenbank.sql.gz',
    'sicherung/alt.bak',
    'docs/bericht.pdf',
    'docs/plan.docx',
    'docs/zahlen.xlsx',
    'docs/folien.pptx',
    'protokoll.log',
    'backend/src/Soforthilfe.Api/.vs/Soforthilfe.Api/v18/.suo',
    'frontend/.vite/deps/vue.js',
    'sicherungen/soforthilfe-20261101.dump.json',
    'deploy/backup/stand.backup',
    'tests/sicherheit/ergebnisse/zap-baseline.html',
    'deploy/traefik/acme.json',
  ];
  for (const pfad of verboten) {
    assert.notDeepEqual(pruefePfad(pfad), [], `nicht erkannt: ${pfad}`);
  }
});

test('erlaubte Pfade werden nicht gemeldet', () => {
  const erlaubt = [
    `${UMGEBUNG}.example`,
    'README.md',
    'docs/architektur.md',
    'backend/src/Soforthilfe.Api/appsettings.json',
    'backend/src/Soforthilfe.Api/appsettings.Development.json',
    'backend/src/Robin/Datei.cs',
    'scripts/hygiene/pruefe-dateien.mjs',
    'tests/hygiene/pruefe-dateien.test.mjs',
    'frontend/src/distanz.js',
    'docs/messprotokolle/matching.md',
    'deploy/sicherung/sicherung.mjs',
    'tests/sicherheit/protokoll.mjs',
  ];
  for (const pfad of erlaubt) {
    assert.deepEqual(pruefePfad(pfad), [], `fälschlich gemeldet: ${pfad}`);
  }
});

test('Windows-Pfade werden gleich behandelt', () => {
  assert.notDeepEqual(pruefePfad('backend\\src\\Soforthilfe.Api\\bin\\app.dll'), []);
});

test('eingetragene Geheimnisse in Konfigurationen werden erkannt', () => {
  const funde = [
    `{ "${JWT_SCHLUESSEL}": "abcdefghijklmnopqrstuvwxyz123456" }`,
    `POSTGRES_${PW.toUpperCase()}: geheim`,
    `"ConnectionStrings": { "Datenbank": "Host=db;Username=app;${PW}=geheim" }`,
    `  ${BOT_SCHLUESSEL}: "123456:ABC"`,
    `"${CLIENT_SCHLUESSEL}": "wert"`,
  ];
  for (const text of funde) {
    assert.notDeepEqual(pruefeInhalt(text), [], `nicht erkannt: ${text}`);
  }
});

test('Platzhalter, leere Werte und Umgebungsverweise sind erlaubt', () => {
  const erlaubt = [
    `POSTGRES_${PW.toUpperCase()}: \${POSTGRES_${PW.toUpperCase()}}`,
    `"${JWT_SCHLUESSEL}": ""`,
    `JWT_KEY=<zufallswert-mit-mindestens-32-zeichen>`,
    `${TOKEN_SCHLUESSEL}: ${'$'}{{ secrets.GITHUB_TOKEN }}`,
    `"Datenbank": "Host=db;Username=app;${PW}=\${DB_KENNWORT}"`,
    `# ${PW}: nur ein Kommentar`,
    `"Logging": { "LogLevel": { "Default": "Information" } }`,
  ];
  for (const text of erlaubt) {
    assert.deepEqual(pruefeInhalt(text), [], `fälschlich gemeldet: ${text}`);
  }
});

test('Oberflächentexte und Sprachdateien werden nicht gemeldet', () => {
  const schluessel = 'pass' + 'wort';
  assert.deepEqual(pruefeInhalt(`"${schluessel}": "Zur Bestätigung Ihr Passwort"`), []);
  const befunde = pruefeDateien(['frontend/src/i18n/de.json'], () => `{ "start${schluessel}": "Startpasswort" }`);
  assert.deepEqual(befunde, []);
});

test('Befunde nennen nie den Wert des Geheimnisses', () => {
  const befunde = pruefeDateien(['appsettings.json'], () => `{ "${API_SCHLUESSEL}": "streng-vertraulich" }`);
  assert.equal(befunde.length, 1);
  assert.ok(!befunde[0].grund.includes('streng-vertraulich'));
});

test('das Suchmuster findet sich nicht selbst', () => {
  for (const datei of [SKRIPT, DIESE_DATEI]) {
    const relativ = datei.replaceAll('\\', '/').split('/').slice(-3).join('/');
    assert.deepEqual(pruefePfad(relativ), [], `Pfad gemeldet: ${relativ}`);
    assert.deepEqual(pruefeInhalt(readFileSync(datei, 'utf8')), [], `Inhalt gemeldet: ${relativ}`);
  }
});

function neuesRepository() {
  const verzeichnis = mkdtempSync(join(tmpdir(), 'hygiene-'));
  execFileSync('git', ['init', '-q'], { cwd: verzeichnis });
  execFileSync('git', ['config', 'core.autocrlf', 'false'], { cwd: verzeichnis });
  return verzeichnis;
}

function schreibe(verzeichnis, pfad, inhalt) {
  const ziel = join(verzeichnis, pfad);
  mkdirSync(dirname(ziel), { recursive: true });
  writeFileSync(ziel, inhalt);
}

function starteSkript(verzeichnis, ...argumente) {
  return spawnSync(process.execPath, [SKRIPT, ...argumente], { cwd: verzeichnis, encoding: 'utf8' });
}

test('gestagte verbotene Dateien lassen die Prüfung scheitern', (t) => {
  const repo = neuesRepository();
  t.after(() => rmSync(repo, { recursive: true, force: true }));

  schreibe(repo, 'README.md', '# Test\n');
  schreibe(repo, UMGEBUNG, 'WERT=1\n');
  schreibe(repo, 'backend/bin/app.dll', 'binär');
  execFileSync('git', ['add', '-f', '.'], { cwd: repo });

  const ergebnis = starteSkript(repo);
  assert.equal(ergebnis.status, 1, ergebnis.stdout + ergebnis.stderr);
  assert.match(ergebnis.stderr, /lokale Umgebungsdatei/);
  assert.match(ergebnis.stderr, /bin\//);
});

test('saubere gestagte Dateien bestehen die Prüfung', (t) => {
  const repo = neuesRepository();
  t.after(() => rmSync(repo, { recursive: true, force: true }));

  schreibe(repo, 'README.md', '# Test\n');
  schreibe(repo, `${UMGEBUNG}.example`, 'JWT_KEY=<platzhalter>\n');
  schreibe(repo, 'scripts/hygiene/pruefe-dateien.mjs', readFileSync(SKRIPT, 'utf8'));
  schreibe(repo, 'appsettings.json', '{ "Logging": { "LogLevel": { "Default": "Information" } } }\n');
  execFileSync('git', ['add', '.'], { cwd: repo });

  const ergebnis = starteSkript(repo);
  assert.equal(ergebnis.status, 0, ergebnis.stdout + ergebnis.stderr);
  assert.match(ergebnis.stdout, /bestanden/);
});

test('mit --alle werden alle versionierten Dateien geprüft', (t) => {
  const repo = neuesRepository();
  t.after(() => rmSync(repo, { recursive: true, force: true }));

  schreibe(repo, 'config.yml', `${API_SCHLUESSEL_KLEIN}: wert-im-klartext\n`);
  execFileSync('git', ['add', '.'], { cwd: repo });

  const ergebnis = starteSkript(repo, '--alle');
  assert.equal(ergebnis.status, 1, ergebnis.stdout + ergebnis.stderr);
  assert.match(ergebnis.stderr, /mögliches Geheimnis/);
  assert.ok(!ergebnis.stderr.includes('wert-im-klartext'));
});
