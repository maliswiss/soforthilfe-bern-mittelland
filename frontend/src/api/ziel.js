/// Ziel nach Anmeldung oder Registrierung aus «?weiter=…». Erlaubt sind nur Pfade innerhalb
/// der Anwendung, damit kein Link auf eine fremde Seite weiterleiten kann.
export function internesZiel(weiter) {
  const ziel = Array.isArray(weiter) ? weiter[0] : weiter;
  return typeof ziel === 'string' && ziel.startsWith('/') && !ziel.startsWith('//') && !ziel.startsWith('/\\')
    ? ziel
    : null;
}
