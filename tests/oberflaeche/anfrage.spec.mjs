// Oberflächentests: Registrierung, Anmeldung und Anfrageformular.

import { test, expect } from '@playwright/test';
import {
  anliegenBeschreiben, anmelden, registrieren, konsoleBeobachten, erwarteKeineFehler,
  neueAdresse, TESTPASSWORT, OHNE_TESTPASSWORT
} from './hilfen.mjs';

test('Mehrwertnummern werden schon bei der Eingabe angezeigt und nicht gesendet', async ({ page }) => {
  await page.goto('/registrierung');
  for (const nummer of ['0900', '+41 901 123 456', '0041 906 12 34 56']) {
    await page.getByLabel('Telefonnummer').fill(nummer);
    await expect(page.getByRole('alert'), nummer).toContainText('kostenpflichtig');
  }
  await page.getByLabel('Telefonnummer').fill('031 123 45 67');
  await expect(page.getByRole('alert')).toHaveCount(0);
});

test('Anfrage ohne Konto beginnen, registrieren und erfassen', async ({ page }) => {
  const fehler = konsoleBeobachten(page);
  await anliegenBeschreiben(page, 'Der Wasserhahn in der Küche tropft seit heute Morgen stark.');

  // Ohne Anmeldung geht es zuerst zur Anmeldung, von dort zur Registrierung
  await expect(page).toHaveURL(/\/anmeldung/);
  await page.getByRole('link', { name: 'Jetzt registrieren' }).click();
  await registrieren(page);

  // Nach der Registrierung geht es mit dem Entwurf weiter
  await expect(page.getByRole('heading', { name: 'Wo und wann?' })).toBeVisible();
  await expect(page.getByText('Der Wasserhahn in der Küche tropft seit heute Morgen stark.')).toBeVisible();

  await page.getByRole('button', { name: 'Anfrage senden' }).click();
  await expect(page.getByRole('alert')).toHaveText('Bitte eine vierstellige Postleitzahl eingeben.');

  await page.getByLabel('Postleitzahl').fill('3011');
  await page.getByRole('button', { name: 'Anfrage senden' }).click();
  await expect(page.getByRole('alert')).toHaveText('Bitte den Ort eingeben.');

  await page.getByLabel('Ort', { exact: true }).fill('Bern');
  await page.getByLabel('Wann passt es Ihnen?').fill('heute zwischen 17 und 19 Uhr');
  await page.getByRole('button', { name: 'Anfrage senden' }).click();

  // Detailansicht der erfassten Anfrage
  await expect(page).toHaveURL(/\/anfragen\/[0-9a-f-]{36}$/);
  await expect(page.getByRole('status')).toHaveText('Ihre Anfrage ist erfasst und offen.');
  await expect(page.locator('main')).toContainText('3011 Bern');
  await expect(page.locator('main')).toContainText('Dringlichkeit mittel');

  // Eigene Anfragen
  await page.getByRole('link', { name: 'Meine Anfragen' }).click();
  await expect(page.getByRole('link', { name: 'Anfrage öffnen' })).toHaveCount(1);
  await erwarteKeineFehler(fehler);
});

test('Anfrage per Kategorie zeigt die gewählte Kategorie und ob sie kostenlos ist', async ({ page }) => {
  await page.goto('/registrierung');
  await registrieren(page);
  await expect(page).toHaveURL(/\/anfragen$/);

  await page.goto('/');
  await page.getByRole('button', { name: /Nachbarschaftshilfe/ }).click();
  await page.getByRole('button', { name: 'Hilfe finden' }).click();
  await expect(page.getByText('Anliegen: Nachbarschaftshilfe')).toBeVisible();
  await page.getByLabel('Postleitzahl').fill('3110');
  await page.getByLabel('Ort', { exact: true }).fill('Münsingen');
  await page.getByRole('button', { name: 'Anfrage senden' }).click();

  await expect(page.getByRole('heading', { level: 1 })).toHaveText('Nachbarschaftshilfe');
  await expect(page.locator('main')).toContainText('Kostenlose Kategorie');
});

test('Falsches Passwort zeigt eine Meldung, Abmelden beendet die Sitzung', async ({ page }) => {
  await page.goto('/registrierung');
  const adresse = await registrieren(page);
  await expect(page).toHaveURL(/\/anfragen$/);

  await page.getByRole('button', { name: 'Abmelden' }).click();
  await expect(page).toHaveURL(/\/$/);
  await expect(page.getByRole('link', { name: 'Anmelden' })).toBeVisible();

  await anmelden(page, adresse, 'falsches-passwort-123');
  await expect(page.getByRole('alert')).toContainText('E-Mail-Adresse oder Passwort ist falsch.');

  await page.getByLabel('Passwort').fill('oberflaeche-passwort-123');
  await page.locator('form button[type=submit]').click();
  await expect(page).toHaveURL(/\/anfragen$/);
});

test('Neue Adresse ohne Konto kann sich nicht anmelden', async ({ page }) => {
  await anmelden(page, neueAdresse(), 'irgendein-passwort-123');
  await expect(page.getByRole('alert')).toContainText('E-Mail-Adresse oder Passwort ist falsch.');
});

test('Testkonto sieht seine Anfrage, nur in der eigenen Region', { tag: '@testdaten' }, async ({ page }) => {
  test.skip(OHNE_TESTPASSWORT, 'TESTDATEN_PASSWORT ist nicht gesetzt');

  // Emil gehört zur Region Biel/Seeland: in Bern/Mittelland wird er abgewiesen
  await anmelden(page, 'emil.exempel@beispiel.invalid', TESTPASSWORT, 'bern-mittelland');
  await expect(page.getByRole('alert')).toContainText('E-Mail-Adresse oder Passwort ist falsch.');

  await anmelden(page, 'emil.exempel@beispiel.invalid', TESTPASSWORT, 'biel-seeland');
  await expect(page).toHaveURL(/\/anfragen$/);
  await expect(page.getByRole('banner')).toContainText('Biel/Seeland');
  await expect(page.locator('main')).toContainText('2502 Biel/Bienne');
  await page.getByRole('button', { name: 'Abmelden' }).click();
});
