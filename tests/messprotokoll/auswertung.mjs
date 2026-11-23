// Auswertung der Messung des Matchings: Kriterien je Fall, Kennzahlen der Antwortzeit und Zellen für Markdown und
// CSV. Ohne Zugriff auf Netz oder Dateien, damit die Regeln in der CI geprüft werden (auswertung.test.mjs).

export const ZIEL_GEEIGNET = 16;
// Zielbereich der Antwortzeit je Fall, vom Absenden bis zu den Vorschlägen (mit Sprachmodell auf dem Arbeitsgerät)
export const ZIEL_ANTWORTZEIT_MS = 7000;
// Suchradius je Dringlichkeit (0 tief, 1 mittel, 2 hoch), wie in docs/matching.md
export const SUCHRADIUS_KM = { 0: 5, 1: 10, 2: 15 };
export const KRITERIEN = ['distanz', 'faehigkeit', 'bewertung', 'verfuegbarkeit', 'erfahrung'];
export const WEGE = { 0: 'Kategorieauswahl', 1: 'Regeln', 2: 'Maschinelles Lernen', 3: 'Sprachmodell' };
export const DRINGLICHKEIT = { 0: 'tief', 1: 'mittel', 2: 'hoch' };

export const KRITERIEN_TEXT = {
  K1: 'Kategorie wie erwartet erkannt oder gewählt',
  K2: 'ein bis drei Vorschläge',
  K3: 'alle Vorschläge innerhalb des Suchradius der Anfrage',
  K4: 'jeder Vorschlag mit Begründung und allen fünf Teilpunkten; die Beiträge ergeben die Punktzahl',
  K5: 'Rang 1 gehört zu den vor der Messung festgelegten Helfenden (Soll)',
  K6: 'Preisangaben bei kostenpflichtiger Kategorie, keine bei kostenloser',
};

/** Name eines Vorschlags, wie ihn die Liste zeigt: bei Betrieben der Betriebsname. */
export function name(vorschlag) {
  return vorschlag?.betriebsname || vorschlag?.anzeigename || '';
}

/** Gehört der angezeigte Name zum Soll? Ein gekürzter Nachname («Nina H.») zählt ebenfalls. */
export function gehoertZuSoll(anzeige, soll) {
  return soll.some((s) => s === anzeige || (anzeige.endsWith('.') && s.startsWith(anzeige.slice(0, -1))));
}

/**
 * Kriterien K1 bis K6 eines Falls.
 * @param {{ erwartet: string, soll: string[] }} fall Testfall aus testfaelle.json
 * @param {{ code?: string|null, bezahlpflichtig?: boolean }|null} einordnung Ergebnis des Verständnis-Schritts
 * @param {number|null} prioritaet Dringlichkeit der erfassten Anfrage
 * @param {any[]} vorschlaege Vorschläge der Anfrage
 */
export function kriterienPruefen(fall, einordnung, prioritaet, vorschlaege) {
  const radiusMeter = (SUCHRADIUS_KM[prioritaet] ?? 0) * 1000;
  const vorhanden = vorschlaege.length > 0;
  return {
    K1: einordnung?.code === fall.erwartet,
    K2: vorhanden && vorschlaege.length <= 3,
    K3: vorhanden && vorschlaege.every((v) => v.distanzMeter <= radiusMeter),
    K4: vorhanden && vorschlaege.every((v) => begruendet(v)),
    K5: vorhanden && gehoertZuSoll(name(vorschlaege[0]), fall.soll),
    K6: vorhanden && (einordnung?.bezahlpflichtig
      ? vorschlaege.every((v) => v.preis?.richtpreisErsteStunde > 0)
      : vorschlaege.every((v) => v.preis === null || v.preis === undefined)),
  };
}

function begruendet(vorschlag) {
  const kriterien = vorschlag.kriterien ?? [];
  const summe = kriterien.reduce((s, k) => s + k.beitrag, 0);
  return typeof vorschlag.begruendung === 'string' && vorschlag.begruendung.trim().length > 0
    && KRITERIEN.every((name) => kriterien.some((k) => k.kriterium === name && typeof k.teilpunkte === 'number'))
    && Math.abs(summe - vorschlag.punktzahl) < 0.002;
}

export const geeignet = (kriterien) => Object.values(kriterien).every(Boolean);

export function median(werte) {
  if (!werte.length) {
    return null;
  }
  const sortiert = [...werte].sort((a, b) => a - b);
  const mitte = Math.floor(sortiert.length / 2);
  return sortiert.length % 2 ? sortiert[mitte] : Math.round((sortiert[mitte - 1] + sortiert[mitte]) / 2);
}

/** Perzentil nach der Rangmethode: der kleinste Wert, unter dem mindestens p Prozent liegen. */
export function perzentil(werte, p) {
  if (!werte.length) {
    return null;
  }
  const sortiert = [...werte].sort((a, b) => a - b);
  return sortiert[Math.min(sortiert.length - 1, Math.max(0, Math.ceil((p / 100) * sortiert.length) - 1))];
}

/** Zelle einer Markdown-Tabelle: ohne senkrechten Strich und Zeilenumbruch. */
export function mdZelle(wert) {
  return String(wert ?? '').replace(/\|/g, '/').replace(/\r?\n/g, ' ');
}

/** Zelle einer CSV-Datei mit Semikolon, wie sie Tabellenkalkulationen in der Schweiz erwarten. */
export function csvZelle(wert) {
  const text = String(wert ?? '');
  return /[;"\r\n]/.test(text) ? `"${text.replace(/"/g, '""')}"` : text;
}

/** Zeitstempel für Dateinamen in Schweizer Zeit, z. B. «2026-11-24-09-15». */
export function stempel(zeitpunkt) {
  const teile = Object.fromEntries(new Intl.DateTimeFormat('en-CA', {
    timeZone: 'Europe/Zurich', year: 'numeric', month: '2-digit', day: '2-digit', hour: '2-digit', minute: '2-digit', hourCycle: 'h23',
  }).formatToParts(zeitpunkt).map((t) => [t.type, t.value]));
  return `${teile.year}-${teile.month}-${teile.day}-${teile.hour}-${teile.minute}`;
}
