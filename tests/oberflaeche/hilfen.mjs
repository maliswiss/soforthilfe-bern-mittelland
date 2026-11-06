// Gemeinsame Hilfen der Oberflächentests. Alle Personen sind fiktiv, Adressen auf «.invalid».

import { expect } from '@playwright/test';

export const TESTPASSWORT = process.env.TESTDATEN_PASSWORT ?? '';
export const OHNE_TESTPASSWORT = TESTPASSWORT === '';

export const neueAdresse = () => `oberflaeche.${Date.now()}.${Math.floor(Math.random() * 1e6)}@beispiel.invalid`;

/// Sammelt Fehler aus der Browserkonsole; erwartete Antworten 400 und 401 der Schnittstelle zählen nicht.
export function konsoleBeobachten(seite) {
  const fehler = [];
  seite.on('pageerror', (e) => fehler.push(e.message));
  seite.on('console', (meldung) => {
    if (meldung.type() === 'error' && !/status of 40[01]/.test(meldung.text())) {
      fehler.push(meldung.text());
    }
  });
  return fehler;
}

/// Startseite: Anliegen in eigenen Worten eingeben und «Hilfe finden» wählen.
export async function anliegenBeschreiben(seite, text) {
  await seite.goto('/');
  await seite.getByRole('tab', { name: 'Frei beschreiben' }).click();
  await seite.getByLabel('Beschreiben Sie Ihr Anliegen in eigenen Worten').fill(text);
  await seite.getByRole('button', { name: 'Hilfe finden' }).click();
}

/// Registrierung mit gültigen Angaben; danach ist die Person angemeldet.
export async function registrieren(seite, { adresse = neueAdresse(), telefon = '031 123 45 67' } = {}) {
  await seite.getByLabel('Name', { exact: true }).fill('Oberfläche Testperson');
  await seite.getByLabel('E-Mail-Adresse').fill(adresse);
  await seite.getByLabel('Telefonnummer').fill(telefon);
  await seite.getByLabel('Passwort, mindestens zehn Zeichen').fill('oberflaeche-passwort-123');
  await seite.getByRole('button', { name: 'Konto erstellen' }).click();
  return adresse;
}

/// Anmeldung über das Formular; die Region wird vorher gewählt, falls angegeben.
export async function anmelden(seite, adresse, passwort, region) {
  await seite.goto('/anmeldung');
  if (region) {
    await seite.locator('#region-anmeldung').selectOption(region);
  }
  await seite.getByLabel('E-Mail-Adresse').fill(adresse);
  await seite.getByLabel('Passwort').fill(passwort);
  await seite.locator('form button[type=submit]').click();
}

export async function erwarteKeineFehler(fehler) {
  expect(fehler, fehler.join('\n')).toEqual([]);
}
