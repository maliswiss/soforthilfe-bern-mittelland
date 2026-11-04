import de from './de.js';

// Rahmen für die Mehrsprachigkeit: alle Texte kommen über t() aus einer Sprachdatei.
const sprachen = { de };
let aktuelleSprache = 'de';

/**
 * Liefert den Text zu einem Schlüssel wie «start.titel».
 * Platzhalter in geschweiften Klammern werden ersetzt: t('gruss', { name: 'Anna' }).
 * Fehlt ein Text, erscheint der Schlüssel selbst, damit Lücken sofort auffallen.
 */
export function t(schluessel, werte = {}) {
  const text = schluessel
    .split('.')
    .reduce((teil, name) => (teil && typeof teil === 'object' ? teil[name] : undefined), sprachen[aktuelleSprache]);

  if (typeof text !== 'string') {
    return schluessel;
  }

  return text.replace(/\{(\w+)\}/g, (_, name) => String(werte[name] ?? `{${name}}`));
}

export function setzeSprache(sprache) {
  if (sprachen[sprache]) {
    aktuelleSprache = sprache;
    document.documentElement.lang = sprache === 'de' ? 'de-CH' : sprache;
  }
}
