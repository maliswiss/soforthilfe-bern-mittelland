#!/usr/bin/env node
// Hygieneprüfung für das Repository.
// Verhindert, dass Build-Ausgaben, Editor-Dateien, lokale Umgebungsdateien, Schlüssel,
// Zertifikate, Datenbank-Abzüge, Sicherungen, vollständige Sicherheitsberichte sowie
// Office- und PDF-Dateien versioniert werden, und sucht in Konfigurationsdateien nach eingetragenen Geheimnissen.
//
// Aufruf:
//   node scripts/hygiene/pruefe-dateien.mjs          prüft die gestagten Dateien
//   node scripts/hygiene/pruefe-dateien.mjs --alle   prüft alle versionierten Dateien
//
// Rückgabewert: 0 = bestanden, 1 = Befunde, 2 = Fehler beim Aufruf von Git.

import { execFileSync } from 'node:child_process';
import { pathToFileURL } from 'node:url';

const VERBOTENE_VERZEICHNISSE = new Set([
  'bin',
  'obj',
  'node_modules',
  'dist',
  'out',
  'publish',
  'coverage',
  'test-results',
  'playwright-report',
  'zap-berichte',
  'sicherungen',
  'backup',
  '.vite',
  '.vs',
  '.vscode',
  '.idea',
]);

// Verzeichnisse, die nur an dieser Stelle verboten sind
const VERBOTENE_PFADE = ['tests/sicherheit/ergebnisse/'];

const VERBOTENE_ENDUNGEN = [
  '.pfx', '.pem', '.key', '.crt', '.p12',
  '.dump', '.dump.json', '.bak', '.backup', '.sql.gz',
  '.log',
  '.doc', '.docx', '.ppt', '.pptx', '.xls', '.xlsx', '.pdf',
];

const LOKALE_KONFIGURATION = /^appsettings(\..+)?\.local\.json$/;

// Nur diese Dateitypen werden auf eingetragene Geheimnisse untersucht.
const KONFIGURATIONS_ENDUNGEN = ['.json', '.yml', '.yaml', '.toml', '.config', '.xml', '.props'];

// Sprachdateien enthalten Oberflächentexte wie «Passwort» und werden nicht inhaltlich geprüft.
const SPRACH_VERZEICHNISSE = new Set(['sprachen', 'locales', 'i18n']);

// Die Suchbegriffe werden zusammengesetzt, damit dieses Skript sich bei einer
// Inhaltsprüfung nicht selbst als Fund meldet.
const PW = 'pass' + 'word';
const SCHLUESSEL_ENDUNGEN = [PW, 'pass' + 'wort', 'pwd', 'secret', 'token', 'apikey', 'api_key', 'privatekey', 'private_key', 'jwt_key', 'jwtkey'];

const SCHLUESSEL_WERT = /["']?([A-Za-z0-9_.-]+)["']?\s*[:=]\s*(?:"([^"]*)"|'([^']*)'|([^"'\s,;}#]*))/g;
const VERBINDUNG_MIT_KENNWORT = new RegExp('(?:^|[;"\'\\s])(?:' + PW + '|pwd)\\s*=\\s*([^;"\'\\s]*)', 'gi');

// Leere Werte, Platzhalter und Verweise auf Umgebungsvariablen sind erlaubt.
const ERLAUBTER_WERT = /^(|\$\{.*|<.*|__.*|%.*|true|false|null|\d+)$/i;

/**
 * Prüft einen Dateipfad gegen die verbotenen Verzeichnisse, Dateinamen und Endungen.
 * @param {string} pfad relativer Pfad im Repository
 * @returns {string[]} Gründe; leer, wenn der Pfad erlaubt ist
 */
export function pruefePfad(pfad) {
  const teile = pfad.replaceAll('\\', '/').split('/').filter(Boolean);
  const name = (teile.at(-1) ?? '').toLowerCase();
  const verzeichnisse = teile.slice(0, -1).map((teil) => teil.toLowerCase());
  const gruende = [];

  const verzeichnis = verzeichnisse.find((teil) => VERBOTENE_VERZEICHNISSE.has(teil));
  if (verzeichnis) {
    gruende.push(`Build- oder Berichtsverzeichnis «${verzeichnis}/»`);
  }
  if (name === '.env' || (name.startsWith('.env.') && name !== '.env.example')) {
    gruende.push('lokale Umgebungsdatei');
  }
  const normalisiert = teile.join('/').toLowerCase();
  const pfadRegel = VERBOTENE_PFADE.find((p) => normalisiert.startsWith(p));
  if (pfadRegel) {
    gruende.push(`Ergebnisverzeichnis «${pfadRegel}»`);
  }
  if (LOKALE_KONFIGURATION.test(name) || name === 'secrets.json' || name === 'acme.json') {
    gruende.push('lokale Konfiguration mit Zugangsdaten');
  }
  const endung = VERBOTENE_ENDUNGEN.find((e) => name.endsWith(e));
  if (endung) {
    gruende.push(`verbotener Dateityp «${endung}»`);
  }
  return gruende;
}

/**
 * Sucht im Text nach Schlüsseln wie Kennwort, Token oder Secret mit eingetragenem Wert.
 * Der gefundene Wert wird nie ausgegeben, nur Zeile und Schlüssel.
 * @param {string} text Dateiinhalt
 * @returns {string[]} Gründe; leer, wenn nichts gefunden wurde
 */
export function pruefeInhalt(text) {
  const gruende = [];
  const zeilen = text.split(/\r?\n/);

  zeilen.forEach((zeile, index) => {
    const getrimmt = zeile.trim();
    if (getrimmt.startsWith('#') || getrimmt.startsWith('//')) {
      return;
    }
    const nummer = index + 1;

    for (const treffer of zeile.matchAll(SCHLUESSEL_WERT)) {
      const schluessel = treffer[1];
      const wert = treffer[2] ?? treffer[3] ?? treffer[4] ?? '';
      const klein = schluessel.toLowerCase();
      // Ein Wert mit Leerzeichen ist ein Text und kein Geheimnis.
      const istText = /\s/.test(wert);
      if (SCHLUESSEL_ENDUNGEN.some((e) => klein.endsWith(e)) && !istText && !ERLAUBTER_WERT.test(wert)) {
        gruende.push(`mögliches Geheimnis in Zeile ${nummer} (Schlüssel «${schluessel}»)`);
      }
    }

    for (const treffer of zeile.matchAll(VERBINDUNG_MIT_KENNWORT)) {
      if (!ERLAUBTER_WERT.test(treffer[1])) {
        gruende.push(`mögliches Kennwort in einer Verbindungsangabe in Zeile ${nummer}`);
      }
    }
  });

  return gruende;
}

/**
 * Prüft eine Liste von Dateien.
 * @param {string[]} dateien relative Pfade
 * @param {(pfad: string) => string} leseInhalt liefert den zu prüfenden Inhalt
 * @returns {{ datei: string, grund: string }[]}
 */
export function pruefeDateien(dateien, leseInhalt) {
  const befunde = [];
  for (const datei of dateien) {
    for (const grund of pruefePfad(datei)) {
      befunde.push({ datei, grund });
    }
    const klein = datei.replaceAll('\\', '/').toLowerCase();
    const istSprachdatei = klein.split('/').slice(0, -1).some((teil) => SPRACH_VERZEICHNISSE.has(teil));
    if (!istSprachdatei && KONFIGURATIONS_ENDUNGEN.some((e) => klein.endsWith(e))) {
      for (const grund of pruefeInhalt(leseInhalt(datei))) {
        befunde.push({ datei, grund });
      }
    }
  }
  return befunde;
}

function git(argumente) {
  return execFileSync('git', argumente, { encoding: 'utf8', maxBuffer: 64 * 1024 * 1024 });
}

function main() {
  const alle = process.argv.includes('--alle');
  const argumente = alle
    ? ['ls-files', '-z']
    : ['diff', '--cached', '--name-only', '--diff-filter=ACMR', '-z'];

  let dateien;
  try {
    dateien = git(argumente).split('\0').filter(Boolean);
  } catch {
    console.error('Hygieneprüfung: Git konnte nicht aufgerufen werden. Bitte im Repository ausführen.');
    process.exit(2);
  }

  // Geprüft wird der Inhalt im Index, also genau das, was committet würde.
  const befunde = pruefeDateien(dateien, (pfad) => git(['show', `:${pfad}`]));

  if (befunde.length === 0) {
    const anzahl = dateien.length === 1 ? '1 Datei' : `${dateien.length} Dateien`;
    console.log(`Hygieneprüfung bestanden (${anzahl} geprüft).`);
    return;
  }

  console.error('Hygieneprüfung fehlgeschlagen:');
  for (const { datei, grund } of befunde) {
    console.error(`  ${datei}: ${grund}`);
  }
  if (!alle) {
    console.error('Datei aus dem Index entfernen: git restore --staged <datei>');
  }
  process.exitCode = 1;
}

if (process.argv[1] && import.meta.url === pathToFileURL(process.argv[1]).href) {
  main();
}
