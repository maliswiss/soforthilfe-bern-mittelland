import { aktuelleSprache, t } from '../i18n';

const basis = '/api';

/// Jede Anfrage nennt die Sprache der Oberfläche; mit Token zusätzlich die Anmeldung.
function kopfzeilen(token) {
  const kopf = { 'Content-Type': 'application/json', 'Accept-Language': aktuelleSprache() };
  if (token) {
    kopf.Authorization = `Bearer ${token}`;
  }
  return kopf;
}

/// Ereignis, das die Oberfläche auslöst, sobald die Schnittstelle ein gesendetes Token ablehnt.
export const sitzungAbgelaufen = 'soforthilfe:sitzung-abgelaufen';

async function auswerten(antwort, mitToken = false) {
  // Ein abgelehntes Token bedeutet: die Sitzung ist abgelaufen oder das Konto nicht mehr gültig
  if (antwort.status === 401 && mitToken && typeof window !== 'undefined') {
    window.dispatchEvent(new CustomEvent(sitzungAbgelaufen));
  }

  if (antwort.status === 204) {
    return null;
  }

  const text = await antwort.text();
  let inhalt = null;
  try {
    inhalt = text ? JSON.parse(text) : null;
  } catch {
    inhalt = null;
  }

  // Die Schnittstelle liefert im Feld «fehler» einen Text für die Oberfläche, in «feld» das betroffene Feld
  if (!antwort.ok) {
    const fehler = new Error(inhalt?.fehler ?? t('allgemein.fehlerAnfrage'));
    fehler.status = antwort.status;
    fehler.feld = inhalt?.feld ?? null;
    fehler.daten = inhalt;
    throw fehler;
  }

  return inhalt;
}

/// Ohne Verbindung zur Schnittstelle eine verständliche Meldung statt eines technischen Fehlers.
async function abrufen(pfad, optionen) {
  try {
    return await fetch(basis + pfad, optionen);
  } catch {
    const fehler = new Error(t('allgemein.fehlerVerbindung'));
    fehler.status = 0;
    throw fehler;
  }
}

export async function holen(pfad, token) {
  const antwort = await abrufen(pfad, { headers: kopfzeilen(token) });
  return auswerten(antwort, Boolean(token));
}

export async function senden(pfad, rumpf, token, verfahren = 'POST') {
  const antwort = await abrufen(pfad, {
    method: verfahren,
    headers: kopfzeilen(token),
    body: rumpf === undefined ? undefined : JSON.stringify(rumpf)
  });
  return auswerten(antwort, Boolean(token));
}
