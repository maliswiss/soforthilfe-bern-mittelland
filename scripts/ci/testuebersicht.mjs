// Testübersicht für die CI: liest die Ausgaben der Testläufe und schreibt eine Tabelle mit Anzahl,
// bestandenen, fehlgeschlagenen und übersprungenen Tests in die Zusammenfassung des Laufs.
//
// Aufruf: node scripts/ci/testuebersicht.mjs "Schnittstellentests=testlauf/api.log" …
// Ohne GITHUB_STEP_SUMMARY erscheint die Tabelle in der Konsole. Das Skript bricht nie ab:
// Ob ein Lauf fehlschlägt, entscheidet der jeweilige Testschritt, nicht die Übersicht.

import { appendFileSync, existsSync, readFileSync } from 'node:fs';
import { pathToFileURL } from 'node:url';

// node:test als TAP («# pass 12») oder mit dem Reporter spec («ℹ pass 12»).
function nodeTest(text) {
  const wert = (name) => {
    const treffer = [...text.matchAll(new RegExp(`^(?:#|ℹ) ${name} (\\d+)\\s*$`, 'gm'))];
    return treffer.length ? Number(treffer.at(-1)[1]) : null;
  };
  const tests = wert('tests');
  if (tests === null) {
    return null;
  }
  return { tests, bestanden: wert('pass') ?? 0, fehlgeschlagen: (wert('fail') ?? 0) + (wert('cancelled') ?? 0), uebersprungen: (wert('skipped') ?? 0) + (wert('todo') ?? 0) };
}

// dotnet test mit Microsoft.Testing.Platform: «total: 273», «failed: 0», «succeeded: 273», «skipped: 0».
function dotnetTest(text) {
  const wert = (name) => {
    const treffer = [...text.matchAll(new RegExp(`^\\s*${name}: (\\d+)\\s*$`, 'gm'))];
    return treffer.length ? Number(treffer.at(-1)[1]) : null;
  };
  const tests = wert('total');
  if (tests === null) {
    return null;
  }
  return { tests, bestanden: wert('succeeded') ?? 0, fehlgeschlagen: wert('failed') ?? 0, uebersprungen: wert('skipped') ?? 0 };
}

// Playwright: «38 passed (1.2m)», «1 failed», «2 skipped», «1 flaky».
function playwright(text) {
  const wert = (name) => {
    const treffer = [...text.matchAll(new RegExp(`^\\s*(\\d+) ${name}\\b`, 'gm'))];
    return treffer.length ? Number(treffer.at(-1)[1]) : 0;
  };
  const bestanden = wert('passed');
  const fehlgeschlagen = wert('failed');
  const uebersprungen = wert('skipped');
  const unsicher = wert('flaky');
  const tests = bestanden + fehlgeschlagen + uebersprungen + unsicher;
  if (tests === 0) {
    return null;
  }
  return { tests, bestanden: bestanden + unsicher, fehlgeschlagen, uebersprungen };
}

// Rauchtest: je Prüfung eine Zeile «ok      …» oder «FEHLER  …».
function rauchtest(text) {
  const bestanden = (text.match(/^ok {2,}/gm) ?? []).length;
  const fehlgeschlagen = (text.match(/^FEHLER {2}/gm) ?? []).length;
  if (bestanden + fehlgeschlagen === 0) {
    return null;
  }
  return { tests: bestanden + fehlgeschlagen, bestanden, fehlgeschlagen, uebersprungen: 0 };
}

/** Erkennt das Format der Ausgabe und zählt die Tests; ohne erkennbares Format null. */
export function auswerten(text) {
  return nodeTest(text) ?? dotnetTest(text) ?? playwright(text) ?? rauchtest(text);
}

/** Tabelle in Markdown; fehlende Ausgaben gelten als «nicht gelaufen». */
export function tabelle(eintraege) {
  const zeilen = [
    '| Testart | Tests | bestanden | fehlgeschlagen | übersprungen | Ergebnis |',
    '|---|---|---|---|---|---|',
  ];
  for (const { name, ergebnis, vorhanden } of eintraege) {
    if (!vorhanden) {
      zeilen.push(`| ${name} | – | – | – | – | nicht gelaufen |`);
    } else if (!ergebnis) {
      zeilen.push(`| ${name} | – | – | – | – | Ausgabe nicht erkannt |`);
    } else {
      const { tests, bestanden, fehlgeschlagen, uebersprungen } = ergebnis;
      const urteil = fehlgeschlagen > 0 ? 'fehlgeschlagen' : 'bestanden';
      zeilen.push(`| ${name} | ${tests} | ${bestanden} | ${fehlgeschlagen} | ${uebersprungen} | ${urteil} |`);
    }
  }
  return zeilen.join('\n');
}

function hauptprogramm(argumente) {
  const eintraege = argumente.map((argument) => {
    const trenner = argument.indexOf('=');
    const name = argument.slice(0, trenner);
    const datei = argument.slice(trenner + 1);
    const vorhanden = trenner > 0 && existsSync(datei);
    return { name, vorhanden, ergebnis: vorhanden ? auswerten(readFileSync(datei, 'utf8')) : null };
  });

  const text = `### Testübersicht\n\n${tabelle(eintraege)}\n`;
  if (process.env.GITHUB_STEP_SUMMARY) {
    appendFileSync(process.env.GITHUB_STEP_SUMMARY, text);
  }
  console.log(text);
}

if (process.argv[1] && import.meta.url === pathToFileURL(process.argv[1]).href) {
  hauptprogramm(process.argv.slice(2));
}
