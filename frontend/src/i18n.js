import { createI18n } from 'vue-i18n';
import de from './sprachen/de.json';

/// Texte der Oberfläche über vue-i18n. Alle sichtbaren Texte stehen in sprachen/de.json,
/// nie direkt in den Komponenten. Deutsch ist die Grundsprache.

export const grundsprache = 'de';

export const i18n = createI18n({
  legacy: false,
  locale: grundsprache,
  fallbackLocale: grundsprache,
  messages: { de },
  missingWarn: false,
  fallbackWarn: false
});

export function aktuelleSprache() {
  return i18n.global.locale.value;
}

/// Gebietsschema für Zahlen und Daten, z. B. «de-CH».
export function kultur() {
  return `${aktuelleSprache()}-CH`;
}

/// Kurzform für Texte ausserhalb von Komponenten (Stores, Hilfsfunktionen).
export function t(schluessel, werte) {
  return i18n.global.t(schluessel, werte ?? {});
}
