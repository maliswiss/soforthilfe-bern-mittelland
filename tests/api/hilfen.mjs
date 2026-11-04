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

/**
 * Registriert ein neues Konto und meldet es an.
 * @returns {Promise<{ adresse: string, passwort: string, token: string, konto: any }>}
 */
export async function neuesKonto(region = 'bern', rolle = 'Hilfesuchende', zusatz = {}) {
  const adresse = neueAdresse();
  const passwort = neuesPasswort();
  const registrierung = await aufruf('POST', '/api/konto/registrieren', {
    region,
    daten: { eMail: adresse, passwort, anzeigename: 'Test Person', rolle, ...zusatz },
  });
  if (registrierung.status !== 201) {
    throw new Error(`Registrierung fehlgeschlagen: ${registrierung.status}`);
  }
  const token = await anmelden(adresse, passwort);
  return { adresse, passwort, token, konto: registrierung.inhalt };
}

/** Meldet ein Konto an und liefert das Token. */
export async function anmelden(adresse, passwort) {
  const antwort = await aufruf('POST', '/api/konto/anmelden', { daten: { eMail: adresse, passwort } });
  if (antwort.status !== 200) {
    throw new Error(`Anmeldung fehlgeschlagen: ${antwort.status}`);
  }
  return antwort.inhalt.token;
}

/** Meldet ein fiktives Testkonto aus den Testdaten an. */
export const testkonto = (adresse) => anmelden(adresse, TESTPASSWORT);

/** Liest den Inhalt (Payload) eines Tokens, ohne die Signatur zu prüfen. */
export function tokenInhalt(token) {
  return JSON.parse(Buffer.from(token.split('.')[1], 'base64url').toString('utf8'));
}
