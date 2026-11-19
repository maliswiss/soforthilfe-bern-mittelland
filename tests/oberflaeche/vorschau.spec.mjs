// Oberflächentests: unverbindliche Vorschau ohne Anmeldung mit gestaffelter Preisgabe. Ohne Anmeldung sind Namen
// und Telefonnummer verdeckt, angemeldet erscheinen die Namen, die Telefonnummer erst nach der Zusage.

import { test, expect } from '@playwright/test';
import { konsoleBeobachten, erwarteKeineFehler, OHNE_TESTPASSWORT, TESTPASSWORT } from './hilfen.mjs';

test('Ohne Anmeldung: Begründung, Punktzahl und Preis, aber kein Name', { tag: '@testdaten' }, async ({ page }) => {
  test.skip(OHNE_TESTPASSWORT, 'Verifizierte Helfende kommen mit den Testdaten');
  const fehler = konsoleBeobachten(page);

  await page.goto('/');
  await page.getByRole('button', { name: /^Sanitär, Heizung, Elektro/ }).click();
  await page.getByRole('button', { name: 'Hilfe finden' }).click();
  await expect(page).toHaveURL(/\/vorschau$/);
  await expect(page.getByText('Anliegen: Sanitär, Heizung, Elektro')).toBeVisible();

  // Ohne Postleitzahl ein Hinweis, mit Postleitzahl ergänzt das Verzeichnis den Ort
  await page.getByRole('button', { name: 'Helfende in der Nähe suchen' }).click();
  await expect(page.getByRole('alert')).toHaveText('Bitte eine vierstellige Postleitzahl eingeben oder den Standort verwenden.');
  await page.getByLabel('Postleitzahl').fill('3011');
  await expect(page.getByLabel('Ort', { exact: true })).toHaveValue('Bern');
  await page.getByRole('button', { name: 'Helfende in der Nähe suchen' }).click();

  await expect(page.getByRole('heading', { level: 2, name: /Sanitär, Heizung, Elektro in der Nähe von Bern/ })).toBeVisible();
  await expect(page.getByText(/Suchradius 10 km · kostenpflichtige Leistung/)).toBeVisible();

  const treffer = page.locator('[data-test=vorschau-treffer]');
  await expect(treffer.first()).toContainText('Punktzahl');
  await expect(treffer.first()).toContainText('Fähigkeit «Sanitär, Heizung, Elektro»');
  await expect(treffer.first()).toContainText('Richtpreis');
  await expect(treffer.first()).toContainText('Name nach der Registrierung sichtbar');
  await expect(page.getByText('Bruno Muster')).toHaveCount(0);
  await expect(page.getByRole('link', { name: 'Konto erstellen und Angaben freischalten' })).toBeVisible();
  await erwarteKeineFehler(fehler);
});

test('Nach der Anmeldung erscheinen die Namen, die Telefonnummer erst nach der Zusage', { tag: '@testdaten' }, async ({ page }) => {
  test.skip(OHNE_TESTPASSWORT, 'Die Anmeldung braucht die Testdaten');

  await page.goto('/');
  await page.getByRole('button', { name: /^Sanitär, Heizung, Elektro/ }).click();
  await page.getByRole('button', { name: 'Hilfe finden' }).click();
  await page.getByLabel('Postleitzahl').fill('3011');
  await page.getByRole('button', { name: 'Helfende in der Nähe suchen' }).click();
  await page.locator('main').getByRole('link', { name: 'Anmelden', exact: true }).click();

  // Zurück in der Vorschau sucht die Seite gleich erneut, jetzt mit Namen
  await expect(page).toHaveURL(/\/anmeldung\?weiter=(%2F|\/)vorschau/);
  await page.getByLabel('E-Mail-Adresse').fill('anna.beispiel@beispiel.invalid');
  await page.getByLabel('Passwort').fill(TESTPASSWORT);
  await page.locator('form button[type=submit]').click();
  await expect(page).toHaveURL(/\/vorschau$/);
  const treffer = page.locator('[data-test=vorschau-treffer]');
  await expect(treffer.first()).toContainText('Die Telefonnummer wird nach der Zusage sichtbar.');
  await expect(treffer.first()).not.toContainText('Name nach der Registrierung sichtbar');
  await expect(treffer.first().getByRole('heading', { level: 3 })).toHaveText(/^1\. \S+/);

  await page.getByRole('link', { name: 'Anfrage erfassen' }).click();
  await expect(page.getByRole('heading', { name: 'Wo und wann?' })).toBeVisible();
  await expect(page.getByLabel('Postleitzahl')).toHaveValue('3011');
});
