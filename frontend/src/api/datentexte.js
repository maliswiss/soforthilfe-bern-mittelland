/// Namen von Kategorien und Fähigkeiten in der gewählten Sprache. Die Schnittstelle liefert sie auf
/// Deutsch; steht der Code in den Sprachdateien, gilt der Text der Sprachdatei.
import { t, textVorhanden } from '../i18n';

function name(bereich, code, ersatz) {
  const schluessel = `${bereich}.${code}`;
  return code && textVorhanden(schluessel) ? t(schluessel) : ersatz;
}

export function kategorieName(code, ersatz) {
  return name('kategorien', code, ersatz);
}

export function faehigkeitName(code, ersatz) {
  return name('faehigkeiten', code, ersatz);
}
