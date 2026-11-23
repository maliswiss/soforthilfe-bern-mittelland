// Attrappe des lokalen Sprachmodells für die CI. Sie beantwortet die Schnittstelle von Ollama
// (POST /api/generate, GET /api/tags) sofort und immer gleich, ohne Modell. Die Antwort folgt festen Merkwörtern
// im Text der Anfrage, damit Tests jeden Ausgang der letzten Stufe gezielt auslösen; alle anderen Texte ergeben
// keine Kategorie, so wie ein Modell, das nicht sicher ist.
//
// Start:     node tests/attrappe/sprachmodell.mjs   (Port ATTRAPPE_PORT, Standard 11434, nur auf diesem Rechner)
// Anbindung: die Schnittstelle mit OLLAMA_BASE_URL=http://localhost:11434 starten
// Prüfung:   GET /attrappe/aufrufe?enthaelt=<text> zählt die Anfragen, deren Text <text> enthält

import http from 'node:http';
import { pathToFileURL } from 'node:url';

export const MODELL = 'attrappe';

// Merkwort im Text (Gross- und Kleinschreibung egal) und die Antwort, die das Modell liefern würde
export const MERKWOERTER = [
  { wort: 'plimbo', antwort: '{"kategorie":"nachbarschaftshilfe"}', bedeutung: 'erlaubte Kennung' },
  { wort: 'quarzel', antwort: '{"kategorie":"zauberei"}', bedeutung: 'unbekannte Kennung' },
  { wort: 'wumpel', antwort: 'Dazu kann ich nichts sagen.', bedeutung: 'kein JSON' },
];
export const OHNE_KATEGORIE = '{"kategorie":null}';

const MAX_RUMPF = 1_000_000;
const MAX_TEXTE = 10_000;

/** Text der Anfrage aus der Anweisung der Schnittstelle: alles nach dem letzten «Anfrage:». */
export function anfrageText(anweisung) {
  const stelle = String(anweisung ?? '').lastIndexOf('Anfrage:');
  return stelle < 0 ? '' : anweisung.slice(stelle + 'Anfrage:'.length).trim();
}

/** Antwort des Modells für einen Text: die des ersten Merkworts, sonst keine Kategorie. */
export function antwortFuer(text) {
  const klein = String(text ?? '').toLowerCase();
  return MERKWOERTER.find((m) => klein.includes(m.wort))?.antwort ?? OHNE_KATEGORIE;
}

/** Erstellt den Server; texte merkt sich die beantworteten Texte für /attrappe/aufrufe. */
export function attrappe() {
  const texte = [];

  const senden = (antwort, status, inhalt) => {
    antwort.writeHead(status, { 'Content-Type': 'application/json' });
    antwort.end(JSON.stringify(inhalt));
  };

  return http.createServer((anfrage, antwort) => {
    const adresse = new URL(anfrage.url, 'http://localhost');

    if (anfrage.method === 'GET' && adresse.pathname === '/api/tags') {
      return senden(antwort, 200, { models: [{ name: MODELL, model: MODELL }] });
    }
    if (anfrage.method === 'GET' && adresse.pathname === '/attrappe/aufrufe') {
      const teil = adresse.searchParams.get('enthaelt') ?? '';
      return senden(antwort, 200, { anzahl: teil ? texte.filter((t) => t.includes(teil)).length : texte.length });
    }
    if (anfrage.method !== 'POST' || adresse.pathname !== '/api/generate') {
      return senden(antwort, 404, { error: 'nicht vorhanden' });
    }

    let rumpf = '';
    anfrage.setEncoding('utf8');
    anfrage.on('data', (stueck) => {
      rumpf += stueck;
      if (rumpf.length > MAX_RUMPF) {
        anfrage.destroy();
      }
    });
    anfrage.on('end', () => {
      let daten;
      try {
        daten = JSON.parse(rumpf || '{}');
      } catch {
        return senden(antwort, 400, { error: 'kein JSON' });
      }
      const grundlage = { model: daten.model ?? MODELL, created_at: new Date().toISOString(), done: true };

      // Ohne Anweisung lädt Ollama nur das Modell (Vorladen beim Start der Schnittstelle)
      if (!daten.prompt) {
        return senden(antwort, 200, { ...grundlage, response: '', done_reason: 'load' });
      }

      const text = anfrageText(daten.prompt);
      if (texte.length >= MAX_TEXTE) {
        texte.length = 0;
      }
      texte.push(text);
      return senden(antwort, 200, { ...grundlage, response: antwortFuer(text), done_reason: 'stop' });
    });
  });
}

if (import.meta.url === pathToFileURL(process.argv[1] ?? '').href) {
  const port = Number(process.env.ATTRAPPE_PORT ?? 11434);
  attrappe().listen(port, '127.0.0.1', () => {
    console.log(`Attrappe des Sprachmodells läuft auf http://localhost:${port}`);
  });
}
