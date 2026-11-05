/// Kostenpflichtige Mehrwertnummern der Schweiz (Anforderung A10). Gleiche Regel wie auf dem Server.
/// Der Hinweistext steht in sprachen/de.json unter «telefon.kostenpflichtig».

const kostenpflichtigeVorwahlen = ['0900', '0901', '0906'];

export function vereinheitlichen(nummer) {
  const ziffern = String(nummer ?? '').replace(/[^\d+]/g, '');
  if (ziffern.startsWith('+41')) {
    return `0${ziffern.slice(3)}`;
  }
  if (ziffern.startsWith('0041')) {
    return `0${ziffern.slice(4)}`;
  }
  return ziffern;
}

export function istKostenpflichtig(nummer) {
  const einheitlich = vereinheitlichen(nummer);
  return kostenpflichtigeVorwahlen.some((vorwahl) => einheitlich.startsWith(vorwahl));
}
