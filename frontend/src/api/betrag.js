/// Beträge in Franken anzeigen und einlesen.
import { kultur } from '../i18n';

/// Betrag ohne Währung, z. B. «1’200.50» (de-CH).
export function betrag(wert) {
  const format = new Intl.NumberFormat(kultur(), { minimumFractionDigits: 2, maximumFractionDigits: 2 });
  return format.format(Number(wert ?? 0));
}

/// Betrag mit Währung, z. B. «CHF 95.00».
export function franken(wert) {
  return `CHF ${betrag(wert)}`;
}

/// Zahl nach den Regeln der Sprache, z. B. 8.1.
export function zahl(wert) {
  return new Intl.NumberFormat(kultur()).format(Number(wert ?? 0));
}

/// Liest Eingaben wie «215.50», «215,50» oder «1'200» ein. Ungültige Eingaben ergeben null.
export function betragLesen(text) {
  const bereinigt = String(text ?? '')
    .trim()
    .replace(/[’'\s]/g, '')
    .replace(',', '.');
  if (bereinigt === '' || !/^\d+(\.\d{1,2})?$/.test(bereinigt)) {
    return null;
  }
  return Number(bereinigt);
}

/// Zuschlag in ganzen Prozent. Leer ergibt null, ungültige Eingaben NaN.
export function prozentLesen(text) {
  const wert = String(text ?? '').trim();
  if (wert === '') {
    return null;
  }
  return /^\d{1,3}$/.test(wert) ? Number(wert) : NaN;
}
