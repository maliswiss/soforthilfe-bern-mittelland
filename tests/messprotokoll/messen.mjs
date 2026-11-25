// Messung des Matchings (Release 2) gegen eine laufende Schnittstelle. Legt die Messdaten aus testfaelle.json an,
// spielt die 20 Testfälle und die vier Notfälle durch, misst die Antwortzeiten und schreibt ein Messprotokoll als
// Markdown und CSV. Die Bewertung je Fall steht in auswertung.mjs, die Festlegungen in docs/messung-matching.md.
//
// Aufruf:   node tests/messprotokoll/messen.mjs
// Umgebung: API_URL (Standard http://localhost:5000), TESTDATEN_PASSWORT (Koordination der Testdaten, auch für die
//           Konten der Messdaten), OLLAMA_BASE_URL (nur zur Angabe im Protokoll, Standard http://localhost:11434),
//           MESSPROTOKOLL_ORDNER (Standard docs/messprotokolle)
// Mehrfach ausführbar: vorhandene Konten der Messdaten werden angemeldet und ihre Profile neu gespeichert.

import fs from 'node:fs';
import os from 'node:os';
import path from 'node:path';
import { execSync } from 'node:child_process';
import { performance } from 'node:perf_hooks';
import { aufruf, anmelden, neuesKonto, testkonto, BASIS, BERN, HELFENDE, TESTPASSWORT } from '../api/hilfen.mjs';
import {
  kriterienPruefen, geeignet, name, median, perzentil, mdZelle, csvZelle, stempel, schrittKennzahlen, SCHRITTE,
  notfallEintraegePruefen, zusatzBewerten, ZUSATZ_BEWERTUNG,
  ZIEL_GEEIGNET, ZIEL_ANTWORTZEIT_MS, SUCHRADIUS_KM, WEGE, DRINGLICHKEIT, KRITERIEN_TEXT,
} from './auswertung.mjs';

const wurzel = path.resolve(import.meta.dirname, '../..');
const { faelle, notfaelle, helfende } = JSON.parse(fs.readFileSync(path.join(import.meta.dirname, 'testfaelle.json'), 'utf8'));
const zusatz = JSON.parse(fs.readFileSync(path.join(import.meta.dirname, 'zusatzfaelle.json'), 'utf8')).faelle;
const K = Object.keys(KRITERIEN_TEXT);

if (!TESTPASSWORT) {
  console.error('TESTDATEN_PASSWORT ist nicht gesetzt; ohne Koordination lassen sich die Messdaten nicht freigeben.');
  process.exit(2);
}

async function gemessen(lauf) {
  const beginn = performance.now();
  const ergebnis = await lauf();
  return { ...ergebnis, ms: Math.round(performance.now() - beginn) };
}

function pruefen(antwort, erwartet, schritt) {
  if (antwort.status !== erwartet) {
    throw new Error(`${schritt}: Status ${antwort.status} statt ${erwartet}`);
  }
  return antwort.inhalt;
}

// Kurzname für die Adresse eines Kontos der Messdaten, z. B. «messung.wyss-sanitaer-und-heizung@beispiel.invalid»
const kurzname = (text) => text.toLowerCase().replace(/ä/g, 'ae').replace(/ö/g, 'oe').replace(/ü/g, 'ue')
  .normalize('NFD').replace(/[\u0300-\u036f]/g, '').replace(/[^a-z0-9]+/g, '-').replace(/^-|-$/g, '');

/** Legt die Helfenden der Messung an oder aktualisiert sie; alle verifiziert und jetzt verfügbar. */
async function messdatenAnlegen(koordination) {
  const skills = pruefen(await aufruf('GET', '/api/skills'), 200, 'Fähigkeiten');
  for (const h of helfende) {
    const eMail = `messung.${kurzname(h.name)}@beispiel.invalid`;
    const registrierung = await aufruf('POST', '/api/auth/registrierung', {
      daten: { eMail, passwort: TESTPASSWORT, anzeigename: h.name, kontoart: HELFENDE, mandant: BERN },
    });
    const token = registrierung.status === 200 ? registrierung.inhalt.token : await anmelden(eMail, TESTPASSWORT);

    const profil = pruefen(await aufruf('PUT', '/api/helferprofil', {
      token,
      daten: {
        typ: h.typ, betriebsname: h.typ === 1 ? h.name : null, plz: h.plz, ort: h.ort, radiusKm: h.radiusKm,
        skillIds: h.skills.map((code) => skills.find((s) => s.code === code).id), ...h.preise,
      },
    }), 200, `Profil ${h.name}`);

    if (profil.verifizierung !== 2) {
      const nachweis = pruefen(await aufruf('POST', '/api/verifizierungen', { token, daten: { art: 3, nachweiswert: null } }), 200, `Nachweis ${h.name}`);
      pruefen(await aufruf('POST', `/api/verifizierungen/${nachweis.id}/entscheid`, {
        token: koordination, daten: { freigeben: true, bemerkung: null },
      }), 200, `Freigabe ${h.name}`);
    }
    pruefen(await aufruf('PUT', '/api/helferprofil/verfuegbarkeit', { token, daten: { jetztVerfuegbar: true } }), 200, `Verfügbarkeit ${h.name}`);
  }
}

// Einträge «Notfall.Stopp» der Region, sichtbar für die Koordination
async function notfallEintraege(koordination) {
  const liste = await aufruf('GET', '/api/protokoll?aktion=Notfall.Stopp&anzahl=500', { token: koordination });
  return Array.isArray(liste.inhalt) ? liste.inhalt : [];
}

async function sprachmodell() {
  const adresse = (process.env.OLLAMA_BASE_URL || 'http://localhost:11434').replace(/\/$/, '');
  try {
    const antwort = await fetch(`${adresse}/api/tags`, { signal: AbortSignal.timeout(2000) });
    const modelle = ((await antwort.json()).models ?? []).map((m) => m.name).join(', ');
    return `erreichbar unter ${adresse} (${modelle || 'kein Modell geladen'})`;
  } catch {
    return `nicht erreichbar unter ${adresse}; die Kette endet nach dem maschinellen Lernen`;
  }
}

function stand() {
  try {
    return execSync('git rev-parse --short HEAD', { cwd: wurzel, stdio: ['ignore', 'pipe', 'ignore'] }).toString().trim();
  } catch {
    return 'unbekannt';
  }
}

// ---------------------------------------------------------------------------------------------------------------
// Messung

const koordination = await testkonto('dora.probe@beispiel.invalid');
console.log(`Messung des Matchings gegen ${BASIS}: ${faelle.length} Fälle, ${notfaelle.length} Notfälle, ${zusatz.length} Zusatzfälle`);
await messdatenAnlegen(koordination);
console.log(`Messdaten: ${helfende.length} Helfende verifiziert und verfügbar`);

const hilfesuchend = await neuesKonto();
const { token } = hilfesuchend;
const kategorien = pruefen(await aufruf('GET', '/api/kategorien'), 200, 'Kategorien');
const ergebnisse = [];

for (const fall of faelle) {
  const gewaehlt = fall.kategorie ? kategorien.find((k) => k.code === fall.kategorie) : null;
  const zeiten = {};
  const beginn = performance.now();

  // 1. Verständnis, 2. Anfrage erfassen (das Matching erzeugt die Vorschläge), 3. Vorschläge laden
  const verstanden = await gemessen(() => aufruf('POST', '/api/anfragen/verstehen', {
    token, daten: { kategorieId: gewaehlt?.id ?? null, freitext: fall.freitext ?? null },
  }));
  zeiten.verstehen = verstanden.ms;
  const v = verstanden.inhalt ?? {};
  let anfrage = null;
  let vorschlaege = [];
  let fehler = null;

  if (verstanden.status !== 200) {
    fehler = `Verständnis: Status ${verstanden.status}`;
  } else if (!v.notfall && v.kategorieId) {
    const erfasst = await gemessen(() => aufruf('POST', '/api/anfragen', {
      token, daten: { kategorieId: v.kategorieId, freitext: fall.freitext ?? null, plz: fall.plz, ort: fall.ort },
    }));
    zeiten.erfassen = erfasst.ms;
    if (erfasst.status === 201) {
      anfrage = erfasst.inhalt;
      const geladen = await gemessen(() => aufruf('GET', `/api/anfragen/${anfrage.id}/vorschlaege`, { token }));
      zeiten.vorschlaege = geladen.ms;
      vorschlaege = Array.isArray(geladen.inhalt) ? geladen.inhalt : [];
    } else {
      fehler = `Erfassen: Status ${erfasst.status}`;
    }
  } else {
    fehler = v.notfall ? 'als Notfall gestoppt' : 'keine Kategorie erkannt';
  }
  zeiten.gesamt = Math.round(performance.now() - beginn);

  const kriterien = kriterienPruefen(fall, v, anfrage?.prioritaet ?? null, vorschlaege);
  const ergebnis = { fall, v, anfrage, vorschlaege, zeiten, kriterien, fehler, geeignet: geeignet(kriterien) };
  ergebnis.imZiel = zeiten.gesamt <= ZIEL_ANTWORTZEIT_MS;
  ergebnisse.push(ergebnis);
  console.log(`  Fall ${String(fall.nr).padStart(2)}: ${ergebnis.geeignet ? 'geeignet      ' : 'nicht geeignet'} ${String(zeiten.gesamt).padStart(6)} ms  ${WEGE[v.erfassungsweg] ?? '–'}  ${v.code ?? 'keine Kategorie'} → ${name(vorschlaege[0]) || '–'}`);
}

// Notfälle: gestoppt im Verständnis-Schritt und beim Erfassen (451), jeder Stopp im Protokoll
const notfallErgebnisse = [];
for (const fall of notfaelle) {
  const vorher = new Set((await notfallEintraege(koordination)).map((e) => e.id));
  const verstanden = await gemessen(() => aufruf('POST', '/api/anfragen/verstehen', { token, daten: { freitext: fall.freitext } }));
  const erfasst = await aufruf('POST', '/api/anfragen', { token, daten: { freitext: fall.freitext, plz: '3011', ort: 'Bern' } });
  const neu = (await notfallEintraege(koordination)).filter((e) => !vorher.has(e.id));
  const protokoll = notfallEintraegePruefen(neu, fall.freitext, { id: hilfesuchend.konto?.id, adresse: hilfesuchend.adresse, anzeigename: hilfesuchend.konto?.anzeigename });
  const gestoppt = verstanden.inhalt?.notfall === true && erfasst.status === 451;
  notfallErgebnisse.push({ fall, gestoppt, eintraege: neu.length, protokoll, grund: verstanden.inhalt?.notfallGrund, ms: verstanden.ms, status: erfasst.status });
  console.log(`  Notfall ${fall.nr}: ${gestoppt ? 'gestoppt' : 'NICHT gestoppt'}, ${neu.length} Protokolleinträge ${protokoll.erfuellt ? 'ohne Personendaten' : 'NICHT wie erwartet'} (${verstanden.ms} ms)`);
}

// Zusatzmessung: schwierige Texte nur im Verständnis-Schritt, ohne Anfrage; misst auch die letzte Stufe
const zusatzErgebnisse = [];
for (const z of zusatz) {
  const verstanden = await gemessen(() => aufruf('POST', '/api/anfragen/verstehen', { token, daten: { freitext: z.freitext } }));
  const antwort = verstanden.inhalt ?? {};
  const bewertung = verstanden.status === 200 ? zusatzBewerten(z, antwort) : 'verfehlt';
  zusatzErgebnisse.push({ z, v: antwort, ms: verstanden.ms, bewertung });
  console.log(`  Zusatz ${z.nr.padEnd(3)}: ${ZUSATZ_BEWERTUNG[bewertung].padEnd(46)} ${String(verstanden.ms).padStart(6)} ms  ${antwort.notfall ? 'Notfall' : (WEGE[antwort.erfassungsweg] ?? 'offen')}`);
}

// ---------------------------------------------------------------------------------------------------------------
// Auswertung und Protokoll

const anzahlGeeignet = ergebnisse.filter((e) => e.geeignet).length;
const zeiten = ergebnisse.map((e) => e.zeiten.gesamt);
const imZiel = ergebnisse.filter((e) => e.imZiel).length;
const notfallErfuellt = notfallErgebnisse.every((n) => n.gestoppt && n.protokoll.erfuellt);
const zielErfuellt = anzahlGeeignet >= ZIEL_GEEIGNET;
const wege = {};
for (const e of ergebnisse) {
  const weg = WEGE[e.v.erfassungsweg] ?? 'ohne Kategorie';
  wege[weg] = (wege[weg] ?? 0) + 1;
}
const gewichte = ergebnisse.find((e) => e.vorschlaege.length)?.vorschlaege[0].kriterien
  .map((k) => `${k.kriterium} ${k.gewicht}`).join(', ') ?? 'keine Vorschläge';
const info = (await aufruf('GET', '/api/info')).inhalt;
const jetzt = new Date();
const ja = (wert) => (wert ? '✓' : '✗');

const md = [];
md.push('# Messprotokoll Matching (Release 2)', '');
md.push(`Erstellt am ${jetzt.toLocaleString('de-CH', { timeZone: 'Europe/Zurich' })} mit \`tests/messprotokoll/messen.mjs\`; Testfälle und Messdaten aus \`tests/messprotokoll/testfaelle.json\`, festgelegt vor der Messung.`, '');

md.push('## Ergebnis', '');
md.push('| Messgrösse | Ziel | Ergebnis | Bewertung |', '|---|---|---|---|');
md.push(`| Geeigneter, begründeter Vorschlag | mindestens ${ZIEL_GEEIGNET} von ${faelle.length} | **${anzahlGeeignet} von ${faelle.length}** | ${zielErfuellt ? 'erfüllt' : 'nicht erfüllt'} |`);
md.push(`| Antwortzeit je Fall | höchstens ${ZIEL_ANTWORTZEIT_MS / 1000} s | Median ${median(zeiten)} ms, 90 % bis ${perzentil(zeiten, 90)} ms, höchstens ${Math.max(...zeiten)} ms | ${imZiel} von ${faelle.length} im Zielbereich |`);
md.push(`| Notfälle | alle gestoppt und protokolliert | ${notfallErgebnisse.filter((n) => n.gestoppt).length} von ${notfaelle.length} gestoppt | ${notfallErfuellt ? 'erfüllt' : 'nicht erfüllt'} |`);
md.push('', `Weg der Einordnung: ${Object.entries(wege).map(([w, n]) => `${w} ${n}`).join(', ')}.`, '');

// Antwortzeit je Schritt; der erste Fall enthält die ersten Abfragen nach dem Start
md.push('## Antwortzeit je Schritt', '');
md.push('| Schritt | Fälle | Median ms | 90 % bis ms | höchstens ms |', '|---|---|---|---|---|');
for (const [schritt, k] of Object.entries(schrittKennzahlen(ergebnisse.map((e) => e.zeiten)))) {
  md.push(`| ${SCHRITTE[schritt]} | ${k.anzahl} | ${k.median ?? '–'} | ${k.p90 ?? '–'} | ${k.max ?? '–'} |`);
}
md.push('', `Zielbereich: höchstens ${ZIEL_ANTWORTZEIT_MS / 1000} s je Fall. Der erste Fall enthält die ersten Abfragen und den Aufbau der Verbindungen nach dem Start der Schnittstelle.`, '');

md.push('## Messumgebung', '');
md.push('| Angabe | Wert |', '|---|---|');
md.push(`| Gerät | ${mdZelle(os.cpus()[0]?.model ?? 'unbekannt')}, ${os.cpus().length} logische Kerne, ${Math.round(os.totalmem() / 1024 ** 3)} GB Arbeitsspeicher |`);
md.push(`| Betriebssystem | ${os.type()} ${os.release()} (${os.arch()}) |`);
md.push(`| Node.js | ${process.version} |`);
md.push(`| Schnittstelle | ${BASIS}, ${mdZelle(info?.name ?? '')} ${mdZelle(info?.version ?? '')} |`);
md.push(`| Stand des Codes | ${stand()} |`);
md.push(`| Sprachmodell | ${mdZelle(await sprachmodell())} |`);
md.push(`| Gewichte des Matchings | ${gewichte} |`);
md.push(`| Suchradius | tief ${SUCHRADIUS_KM[0]} km, mittel ${SUCHRADIUS_KM[1]} km, hoch ${SUCHRADIUS_KM[2]} km |`, '');

md.push('## Kriterien', '');
md.push('Ein Fall gilt als geeignet und begründet, wenn alle Kriterien erfüllt sind:', '');
for (const k of K) {
  md.push(`- **${k}** ${KRITERIEN_TEXT[k]}`);
}
md.push('', 'Die Antwortzeit reicht vom Absenden der Beschreibung bis zum Eintreffen der Vorschläge (Verständnis, Erfassen mit Matching, Vorschläge laden), gemessen beim Aufrufer.', '');

md.push('## Testfälle', '');
md.push(`| Nr. | Sprache | Ort | Weg | Sicherheit | Dringlichkeit | Kategorie | ${K.join(' | ')} | Rang 1 | Distanz | Zeit ms | Ergebnis |`);
md.push(`|---|---|---|---|---|---|---|${K.map(() => '---').join('|')}|---|---|---|---|`);
for (const e of ergebnisse) {
  const r1 = e.vorschlaege[0];
  md.push(`| ${e.fall.nr} | ${e.fall.sprache} | ${e.fall.plz} ${mdZelle(e.fall.ort)} | ${WEGE[e.v.erfassungsweg] ?? '–'} | ${e.v.konfidenz ?? '–'} | ${DRINGLICHKEIT[e.anfrage?.prioritaet] ?? '–'} | ${mdZelle(e.v.kategorie ?? '–')} | ${K.map((k) => ja(e.kriterien[k])).join(' | ')} | ${r1 ? mdZelle(name(r1)) : '–'} | ${r1 ? `${r1.distanzMeter} m` : '–'} | ${e.zeiten.gesamt} | ${e.geeignet ? '**geeignet**' : 'nicht geeignet'} |`);
}
md.push('');

md.push('## Einzelheiten', '');
for (const e of ergebnisse) {
  md.push(`### Fall ${e.fall.nr}`, '');
  md.push(e.fall.freitext ? `> ${mdZelle(e.fall.freitext)}` : `> Kategorie gewählt: ${mdZelle(e.v.kategorie)}`, '');
  md.push(`- Verständnis: ${mdZelle(e.v.begruendung ?? '–')}`);
  md.push(`- Erwartet: ${e.fall.erwartet}; Soll auf Rang 1: ${e.fall.soll.join(', ')}`);
  md.push(`- Zeiten: Verständnis ${e.zeiten.verstehen} ms, Erfassen ${e.zeiten.erfassen ?? '–'} ms, Vorschläge ${e.zeiten.vorschlaege ?? '–'} ms`);
  if (e.fehler) {
    md.push(`- Abbruch: ${e.fehler}`);
  }
  for (const x of e.vorschlaege) {
    const preis = x.preis?.richtpreisErsteStunde ? `, Richtpreis CHF ${Number(x.preis.richtpreisErsteStunde).toFixed(2)}` : '';
    md.push(`  ${x.rang}. **${mdZelle(name(x))}** (Punktzahl ${x.punktzahl.toFixed(2)}${preis}): ${mdZelle(x.begruendung)}`);
  }
  if (!e.vorschlaege.length) {
    md.push('  Keine Vorschläge.');
  }
  md.push('');
}

md.push('## Notfälle', '');
md.push('| Nr. | Sprache | Text | Gestoppt | Grund | Protokolleinträge | Ohne Anfrage | Nur Begriff | Ohne Freitext und Konto | Zeit ms | Erfassen |', '|---|---|---|---|---|---|---|---|---|---|---|');
for (const n of notfallErgebnisse) {
  const p = n.protokoll;
  md.push(`| ${n.fall.nr} | ${n.fall.sprache} | ${mdZelle(n.fall.freitext)} | ${ja(n.gestoppt)} | ${mdZelle(n.grund ?? '–')} | ${n.eintraege}${p.beideSchritte ? ' (Verständnis, Erfassen)' : ''} | ${ja(p.ohneAnfrage)} | ${ja(p.nurBegriff)} | ${ja(p.ohneFreitext && p.ohneKonto)} | ${n.ms} | ${n.status === 451 ? 'gestoppt (451)' : n.status} |`);
}
md.push('', 'Geprüft wird je Notfall: Stopp im Verständnis-Schritt und beim Erfassen, je ein Eintrag «Notfall.Stopp» für beide Schritte, ohne Bezug auf eine Anfrage, im Detail nur der erkannte Begriff, weder Wortlaut des Freitexts noch Kennung, Adresse oder Name des Kontos.', '');

// Zusatzmessung im Protokoll
const zaehlen = (b) => zusatzErgebnisse.filter((e) => e.bewertung === b).length;
const fehler = zusatzErgebnisse.filter((e) => ['falsch', 'verpasst', 'fehlalarm'].includes(e.bewertung));
const mitSprachmodell = zusatzErgebnisse.filter((e) => e.v.erfassungsweg === 3).map((e) => e.ms);
md.push('## Zusatzmessung mit schwierigen Texten', '');
md.push('Getrennt von den 20 Fällen und ohne Ziel: Mundart, Tippfehler, Umschreibungen, andere Sprachen, ein langer Text und Notfälle in ungewohnter Form aus `tests/messprotokoll/zusatzfaelle.json`, nur der Verständnis-Schritt.', '');
md.push('| Bewertung | Fälle |', '|---|---|');
for (const [b, text] of Object.entries(ZUSATZ_BEWERTUNG)) {
  md.push(`| ${text} | ${zaehlen(b)} |`);
}
md.push('', `Fehler (falsch, Notfall nicht erkannt, Fehlalarm): ${fehler.length ? fehler.map((e) => e.z.nr).join(', ') : 'keine'}. Entscheidungen des Sprachmodells: ${mitSprachmodell.length}${mitSprachmodell.length ? `, Antwortzeit Median ${median(mitSprachmodell)} ms, höchstens ${Math.max(...mitSprachmodell)} ms` : ''}.`, '');
md.push('| Nr. | Gruppe | Sprache | Text | Erwartet | Ergebnis | Weg | Alternativen | Bewertung | Zeit ms |', '|---|---|---|---|---|---|---|---|---|---|');
for (const e of zusatzErgebnisse) {
  const ergebnis = e.v.notfall ? `Notfall (${mdZelle(e.v.notfallGrund ?? '')})` : (e.v.code ?? 'keine Kategorie');
  const alternativen = (e.v.alternativen ?? []).map((a) => a.code).join(', ') || '–';
  md.push(`| ${e.z.nr} | ${e.z.gruppe} | ${e.z.sprache} | ${mdZelle(e.z.freitext)} | ${e.z.erwartet} | ${ergebnis} | ${e.v.notfall ? '–' : (WEGE[e.v.erfassungsweg] ?? '–')} | ${alternativen} | ${ZUSATZ_BEWERTUNG[e.bewertung]} | ${e.ms} |`);
}
md.push('');

// CSV für die Auswertung in einer Tabellenkalkulation
const csv = [['Nr', 'Sprache', 'PLZ', 'Ort', 'Weg', 'Sicherheit', 'Dringlichkeit', 'Kategorie', 'Erwartet', ...K, 'Geeignet',
  'Rang1', 'DistanzRang1', 'AnzahlVorschlaege', 'ZeitVerstehenMs', 'ZeitErfassenMs', 'ZeitVorschlaegeMs', 'ZeitGesamtMs', 'ImZielbereich']];
for (const e of ergebnisse) {
  csv.push([e.fall.nr, e.fall.sprache, e.fall.plz, e.fall.ort, WEGE[e.v.erfassungsweg] ?? '', e.v.konfidenz ?? '',
    DRINGLICHKEIT[e.anfrage?.prioritaet] ?? '', e.v.code ?? '', e.fall.erwartet, ...K.map((k) => (e.kriterien[k] ? 1 : 0)),
    e.geeignet ? 1 : 0, name(e.vorschlaege[0]), e.vorschlaege[0]?.distanzMeter ?? '', e.vorschlaege.length,
    e.zeiten.verstehen, e.zeiten.erfassen ?? '', e.zeiten.vorschlaege ?? '', e.zeiten.gesamt, e.imZiel ? 1 : 0]);
}

const ordner = path.resolve(process.env.MESSPROTOKOLL_ORDNER ?? path.join(wurzel, 'docs', 'messprotokolle'));
fs.mkdirSync(ordner, { recursive: true });
const basis = path.join(ordner, `r2-matching-${stempel(jetzt)}`);
fs.writeFileSync(`${basis}.md`, md.join('\n') + '\n', 'utf8');
// Mit BOM, damit Tabellenkalkulationen die Umlaute erkennen; Zeilenenden wie im Repository (LF)
fs.writeFileSync(`${basis}.csv`, '\uFEFF' + csv.map((z) => z.map(csvZelle).join(';')).join('\n') + '\n', 'utf8');

const zusatzCsv = [['Nr', 'Gruppe', 'Sprache', 'Erwartet', 'Notfall', 'Kategorie', 'Weg', 'Alternativen', 'Bewertung', 'ZeitMs']];
for (const e of zusatzErgebnisse) {
  zusatzCsv.push([e.z.nr, e.z.gruppe, e.z.sprache, e.z.erwartet, e.v.notfall ? 1 : 0, e.v.code ?? '', WEGE[e.v.erfassungsweg] ?? '',
    (e.v.alternativen ?? []).map((a) => a.code).join(' '), e.bewertung, e.ms]);
}
fs.writeFileSync(`${basis}-zusatz.csv`, '\uFEFF' + zusatzCsv.map((z) => z.map(csvZelle).join(';')).join('\n') + '\n', 'utf8');

console.log('');
console.log(`Geeignet: ${anzahlGeeignet} von ${faelle.length} (Ziel ${ZIEL_GEEIGNET}): ${zielErfuellt ? 'erfüllt' : 'nicht erfüllt'}`);
console.log(`Antwortzeit: Median ${median(zeiten)} ms, höchstens ${Math.max(...zeiten)} ms, ${imZiel} von ${faelle.length} im Zielbereich`);
console.log(`Notfälle: ${notfallErfuellt ? 'alle gestoppt und protokolliert' : 'nicht alle gestoppt oder protokolliert'}`);
console.log(`Protokoll: ${basis}.md`);
console.log(`Tabelle:   ${basis}.csv`);
console.log(`Zusatz:    ${basis}-zusatz.csv (Fehler: ${fehler.length ? fehler.map((e) => e.z.nr).join(', ') : 'keine'})`);
process.exitCode = zielErfuellt && notfallErfuellt ? 0 : 1;
