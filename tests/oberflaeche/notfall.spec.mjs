// Oberflächentests: Notfall-Stopp mit Hinweis «144 anrufen».

import { test, expect } from '@playwright/test';
import { anliegenBeschreiben } from './hilfen.mjs';

test('Beschreibung mit Notfallhinweis führt zur Notfallseite statt zur Anfrage', async ({ page }) => {
  await anliegenBeschreiben(page, 'Mein Mann ist bewusstlos auf dem Boden.');

  await expect(page).toHaveURL(/\/notfall$/);
  await expect(page.getByRole('heading', { name: 'Bitte sofort 144 anrufen' })).toBeVisible();
  await expect(page.locator('main')).toContainText('Erkanntes Schlüsselwort: bewusstlos');
  await expect(page.locator('main').getByRole('link', { name: '144 anrufen' })).toHaveAttribute('href', 'tel:144');
  await expect(page.locator('main')).toContainText('vermittelt keine medizinischen Leistungen');

  // Das erkannte Wort steht nicht in der Adresse
  expect(page.url()).not.toContain('bewusstlos');

  await page.getByRole('link', { name: 'Zurück zur Startseite' }).click();
  await expect(page).toHaveURL(/\/$/);
});

test('Alltägliche Beschreibung führt weiter zur Anmeldung', async ({ page }) => {
  await anliegenBeschreiben(page, 'Mein Schlüssel ist im Schloss abgebrochen.');
  await expect(page).toHaveURL(/\/anmeldung/);
});
