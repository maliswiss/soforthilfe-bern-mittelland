import { createI18n } from 'vue-i18n';
import de from './sprachen/de.json';
import fr from './sprachen/fr.json';

/// Texte der Oberfläche über vue-i18n. Alle sichtbaren Texte stehen als Daten in sprachen/*.json,
/// nie direkt in den Komponenten. Deutsch ist die Grundsprache: fehlt ein Text, erscheint der deutsche.

export const grundsprache = 'de';

// Mitgelieferte Sprachen mit ihrem eigenen Namen
export const mitgeliefert = [
  { code: 'de', name: 'Deutsch' },
  { code: 'fr', name: 'Français' }
];

const speicherschluessel = 'soforthilfe.sprache';

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

/// Sprache wechseln; die Wahl gilt auch nach dem Neuladen (nur in diesem Browser).
export function spracheSetzen(code) {
  const ziel = i18n.global.availableLocales.includes(code) ? code : grundsprache;
  i18n.global.locale.value = ziel;
  document.documentElement.lang = ziel;
  try {
    localStorage.setItem(speicherschluessel, ziel);
  } catch {
    // Ohne Speicher gilt die Wahl nur bis zum Neuladen
  }
  return ziel;
}

/// Gespeicherte Wahl, sonst die Sprache des Browsers, sonst Deutsch.
export function gewuenschteSprache() {
  try {
    const gespeichert = localStorage.getItem(speicherschluessel);
    if (gespeichert && i18n.global.availableLocales.includes(gespeichert)) {
      return gespeichert;
    }
  } catch {
    // Speicher nicht verfügbar
  }
  const browser = (navigator.language ?? '').slice(0, 2).toLowerCase();
  return i18n.global.availableLocales.includes(browser) ? browser : grundsprache;
}
