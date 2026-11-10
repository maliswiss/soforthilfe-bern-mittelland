// Gemeinsame Hilfen für die Schnittstellentests.
// Die Tests laufen gegen eine laufende Instanz: API_URL (Standard http://localhost:5000).
// Testkonten aus den Testdaten brauchen TESTDATEN_PASSWORT; ohne diesen Wert werden die betroffenen Tests übersprungen.

import { randomBytes, randomUUID } from 'node:crypto';

export const BASIS = process.env.API_URL ?? 'http://localhost:5000';
export const TESTPASSWORT = process.env.TESTDATEN_PASSWORT;
export const OHNE_TESTPASSWORT = TESTPASSWORT ? false : 'TESTDATEN_PASSWORT ist nicht gesetzt';

/**
 * Ruft die Schnittstelle auf und liefert Status und (falls vorhanden) den JSON-Inhalt.
 * @param {string} methode HTTP-Methode
 * @param {string} pfad Pfad ab /api
 * @param {{ token?: string, region?: string, daten?: unknown }} [optionen]
 */
export async function aufruf(methode, pfad, { token, region, daten } = {}) {
  const koepfe = { Accept: 'application/json' };
  if (token) koepfe.Authorization = `Bearer ${token}`;
  if (region) koepfe['X-Region'] = region;
  if (daten !== undefined) koepfe['Content-Type'] = 'application/json';

  const antwort = await fetch(`${BASIS}${pfad}`, {
    method: methode,
    headers: koepfe,
    body: daten === undefined ? undefined : JSON.stringify(daten),
  });

  const text = await antwort.text();
  let inhalt = null;
  if (text) {
    try {
      inhalt = JSON.parse(text);
    } catch {
      inhalt = text;
    }
  }
  return { status: antwort.status, inhalt };
}

// Zufällige, sicher fiktive Adresse und zufälliges Passwort je Test.
export const neueAdresse = (vorsilbe = 'test') => `${vorsilbe}.${randomUUID()}@beispiel.invalid`;
export const neuesPasswort = () => randomBytes(18).toString('base64url');

// Regionen der Testdaten: die Pilotregion aus den Stammdaten und eine zweite Region für die Datentrennung.
export const BERN = 'bern-mittelland';
export const BIEL = 'biel-seeland';

// Kontoart bei der Registrierung: 0 Hilfe suchen, 1 Hilfe anbieten.
export const HILFESUCHENDE = 0;
export const HELFENDE = 1;

/**
 * Registriert ein neues Konto; die Antwort meldet es gleich an.
 * @returns {Promise<{ adresse: string, passwort: string, token: string, konto: any }>}
 */
export async function neuesKonto(mandant = BERN, kontoart = HILFESUCHENDE, zusatz = {}) {
  const adresse = neueAdresse();
  const passwort = neuesPasswort();
  const registrierung = await aufruf('POST', '/api/auth/registrierung', {
    daten: { eMail: adresse, passwort, anzeigename: 'Test Person', kontoart, mandant, ...zusatz },
  });
  if (registrierung.status !== 200) {
    throw new Error(`Registrierung fehlgeschlagen: ${registrierung.status}`);
  }
  return { adresse, passwort, token: registrierung.inhalt.token, konto: registrierung.inhalt.benutzer };
}

/** Meldet ein Konto in der angegebenen Region an und liefert das Token. */
export async function anmelden(adresse, passwort, mandant = BERN) {
  const antwort = await aufruf('POST', '/api/auth/anmeldung', { daten: { eMail: adresse, passwort, mandant } });
  if (antwort.status !== 200) {
    throw new Error(`Anmeldung fehlgeschlagen: ${antwort.status}`);
  }
  return antwort.inhalt.token;
}

// Testkonten der Region Biel/Seeland; alle übrigen gehören zu Bern/Mittelland oder zum Plattformbetrieb.
const KONTEN_BIEL = ['emil.exempel', 'fritz.platzhalter', 'greta.muster'];

/** Meldet ein fiktives Testkonto aus den Testdaten in seiner Region an. */
export const testkonto = (adresse) =>
  anmelden(adresse, TESTPASSWORT, KONTEN_BIEL.some((name) => adresse.startsWith(`${name}@`)) ? BIEL : BERN);

/** Liest den Inhalt (Payload) eines Tokens, ohne die Signatur zu prüfen. */
export function tokenInhalt(token) {
  return JSON.parse(Buffer.from(token.split('.')[1], 'base64url').toString('utf8'));
}
