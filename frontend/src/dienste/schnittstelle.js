import { abmelden, aktuelleRegion, sitzung } from './sitzung.js';
import { t } from '../texte/index.js';

/**
 * Fehler der Schnittstelle mit Status und verständlicher Meldung (aus dem Problem-Dokument).
 */
export class SchnittstellenFehler extends Error {
  constructor(status, titel, text) {
    super(text || titel);
    this.status = status;
    this.titel = titel;
    this.text = text;
  }
}

/**
 * Ruft die Schnittstelle auf. Token und Region werden automatisch mitgesendet.
 * @param {string} methode HTTP-Methode
 * @param {string} pfad Pfad ab /api
 * @param {unknown} [daten] Inhalt als JSON
 */
export async function aufruf(methode, pfad, daten) {
  const koepfe = { Accept: 'application/json' };
  if (sitzung.token) {
    koepfe.Authorization = `Bearer ${sitzung.token}`;
  } else if (aktuelleRegion.value) {
    // Ohne Anmeldung bestimmt der Kopf X-Region die Region (Mandant).
    koepfe['X-Region'] = aktuelleRegion.value;
  }
  if (daten !== undefined) {
    koepfe['Content-Type'] = 'application/json';
  }

  let antwort;
  try {
    antwort = await fetch(pfad, {
      method: methode,
      headers: koepfe,
      body: daten === undefined ? undefined : JSON.stringify(daten),
    });
  } catch {
    throw new SchnittstellenFehler(0, t('fehler.verbindung'), t('fehler.verbindungText'));
  }

  const inhalt = await liesInhalt(antwort);
  if (antwort.ok) {
    return inhalt;
  }

  // Abgelaufenes oder ungültiges Token: Sitzung beenden, damit die Person sich neu anmeldet.
  if (antwort.status === 401 && sitzung.token) {
    abmelden();
  }

  // Prüffehler der Felder kommen ohne eigene Meldung; dann ein allgemeiner Hinweis.
  if (inhalt?.errors) {
    throw new SchnittstellenFehler(antwort.status, t('fehler.eingabe'), t('fehler.eingabeText'));
  }
  throw new SchnittstellenFehler(antwort.status, inhalt?.title ?? t('fehler.allgemein'), inhalt?.detail ?? '');
}

async function liesInhalt(antwort) {
  const text = await antwort.text();
  if (!text) {
    return null;
  }
  try {
    return JSON.parse(text);
  } catch {
    return null;
  }
}
