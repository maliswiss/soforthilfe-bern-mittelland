import { holen } from './klient';

/// Ortschaften zu einer vierstelligen Postleitzahl aus dem amtlichen Ortschaftenverzeichnis,
/// je Ortschaft mit Gemeinde, Kanton, Region und dem Hinweis auf mehrere Gemeinden.
export function orteZuPlz(plz) {
  return holen(`/orte?plz=${encodeURIComponent(plz)}`);
}

/// Region zu Postleitzahl und Ort: die Ortschaft mit passendem Namen, sonst die erste.
/// Liefert { bekannt, region }; region ist null, wenn die Plattform dort noch keine Region hat.
export async function regionZuPlz(plz, ort) {
  const ortschaften = await orteZuPlz(plz);
  if (!ortschaften.length) {
    return { bekannt: false, region: null };
  }
  const gesucht = String(ort ?? '').trim().toLowerCase();
  const passend = ortschaften.find((o) => o.ort.toLowerCase() === gesucht) ?? ortschaften[0];
  return { bekannt: true, region: passend.region };
}
