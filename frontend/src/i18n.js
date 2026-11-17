import { ref } from 'vue';
import { createI18n } from 'vue-i18n';
import de from './sprachen/de.json';
import fr from './sprachen/fr.json';

/// Texte der Oberfläche über vue-i18n. Alle sichtbaren Texte stehen als Daten in sprachen/*.json,
/// nie direkt in den Komponenten. Deutsch ist die Grundsprache: fehlt ein Text, erscheint der deutsche.
/// Der Plattformbetrieb kann weitere Sprachen und korrigierte Texte importieren; sie kommen beim Start
/// von der Schnittstelle und gelten ohne neues Deployment der Oberfläche.

export const grundsprache = 'de';

const mitgelieferteTexte = { de, fr };

// Mitgelieferte Sprachen mit ihrem eigenen Namen
export const mitgeliefert = [
  { code: 'de', name: 'Deutsch' },
  { code: 'fr', name: 'Français' }
];

/// Sprachen im Umschalter: mitgelieferte und aktive importierte. «importiert» heisst, dass Texte
/// von der Schnittstelle kommen; bei Deutsch und Französisch ergänzen sie die mitgelieferten.
export const sprachen = ref(mitgeliefert.map((s) => ({ ...s, importiert: false })));

const speicherschluessel = 'soforthilfe.sprache';
const geladen = new Set();

export const i18n = createI18n({
  legacy: false,
  locale: grundsprache,
  fallbackLocale: grundsprache,
  messages: { de, fr },
  missingWarn: false,
  fallbackWarn: false
});

export function aktuelleSprache() {
  return i18n.global.locale.value;
}

/// Gebietsschema für Zahlen und Daten, z. B. «de-CH» oder «fr-CH».
export function kultur() {
  return `${aktuelleSprache()}-CH`;
}

/// Kurzform für Texte ausserhalb von Komponenten (Stores, Hilfsfunktionen).
export function t(schluessel, werte) {
  return i18n.global.t(schluessel, werte ?? {});
}

/// Gibt es den Text in der aktuellen Sprache oder in der Grundsprache?
export function textVorhanden(schluessel) {
  return i18n.global.te(schluessel) || i18n.global.te(schluessel, grundsprache);
}

/// Eigener Name einer Sprache, z. B. «Italiano».
export function spracheName(code) {
  return sprachen.value.find((s) => s.code === code)?.name ?? code;
}

function kopie(texte) {
  return JSON.parse(JSON.stringify(texte ?? {}));
}

function gespeicherteWahl() {
  try {
    return localStorage.getItem(speicherschluessel);
  } catch {
    return null;
  }
}

function browsersprache() {
  return (navigator.language ?? '').slice(0, 2).toLowerCase();
}

// Texte einer importierten Sprache holen und über die mitgelieferten legen; fehlende bleiben deutsch.
async function texteLaden(code) {
  if (geladen.has(code)) {
    return true;
  }
  try {
    const antwort = await fetch(`/api/sprachen/${code}/texte`, { cache: 'no-cache' });
    if (!antwort.ok) {
      return false;
    }
    const texte = await antwort.json();
    i18n.global.setLocaleMessage(code, kopie(mitgelieferteTexte[code]));
    i18n.global.mergeLocaleMessage(code, texte);
    geladen.add(code);
    return true;
  } catch {
    return false;
  }
}

/// Sprache wechseln. Eine Wahl im Umschalter gilt auch nach dem Neuladen (nur in diesem Browser);
/// die Sprache beim Start wird nicht gespeichert, damit später importierte Sprachen in Frage kommen.
/// Mitgelieferte Sprachen wechseln sofort, importierte nach dem Laden ihrer Texte.
export async function spracheSetzen(code, { merken = true } = {}) {
  const eintrag = sprachen.value.find((s) => s.code === code);
  let ziel = eintrag ? code : grundsprache;
  if (eintrag?.importiert && !(await texteLaden(code)) && !mitgelieferteTexte[code]) {
    ziel = grundsprache;
  }
  i18n.global.locale.value = ziel;
  document.documentElement.lang = ziel;
  if (merken) {
    try {
      localStorage.setItem(speicherschluessel, ziel);
    } catch {
      // Ohne Speicher gilt die Wahl nur bis zum Neuladen
    }
  }
  return ziel;
}

/// Gespeicherte Wahl, sonst die Sprache des Browsers, sonst Deutsch. Vor dem Laden der importierten
/// Sprachen kommen nur die mitgelieferten in Frage.
export function gewuenschteSprache() {
  const vorhanden = (code) => sprachen.value.some((s) => s.code === code);
  const gespeichert = gespeicherteWahl();
  if (gespeichert && vorhanden(gespeichert)) {
    return gespeichert;
  }
  const browser = browsersprache();
  return vorhanden(browser) ? browser : grundsprache;
}

/// Aktive importierte Sprachen von der Schnittstelle holen. Ohne Verbindung bleiben die mitgelieferten.
/// «neu» verwirft bereits geladene Texte, etwa nach einem Import durch den Plattformbetrieb.
export async function importierteSprachenLaden({ neu = false } = {}) {
  let liste = [];
  try {
    const antwort = await fetch('/api/sprachen', { cache: 'no-cache' });
    liste = antwort.ok ? await antwort.json() : [];
  } catch {
    liste = [];
  }

  if (neu) {
    for (const code of geladen) {
      if (mitgelieferteTexte[code]) {
        i18n.global.setLocaleMessage(code, kopie(mitgelieferteTexte[code]));
      }
    }
    geladen.clear();
  }

  const aktive = new Map((Array.isArray(liste) ? liste : []).map((s) => [s.code, s]));
  sprachen.value = [
    ...mitgeliefert.map((s) => ({ ...s, importiert: aktive.has(s.code) })),
    ...[...aktive.values()]
      .filter((s) => !mitgelieferteTexte[s.code])
      .map((s) => ({ code: s.code, name: s.name, importiert: true }))
  ];

  // Gewünschte Sprache erneut bestimmen: sie kann eine importierte sein oder ergänzte Texte haben
  const wunsch = gewuenschteSprache();
  const eintrag = sprachen.value.find((s) => s.code === wunsch);
  if (wunsch !== aktuelleSprache() || eintrag?.importiert || neu) {
    await spracheSetzen(wunsch, { merken: false });
  }
}
