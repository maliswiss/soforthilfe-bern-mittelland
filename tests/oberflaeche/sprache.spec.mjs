// Oberflächentests: Sprachwechsel zwischen Deutsch und Französisch, gespeicherte Wahl, Sprache des Browsers und
// Wechsel mitten in einem Formular.

import { test, expect } from '@playwright/test';
import { konsoleBeobachten, erwarteKeineFehler, neueAdresse } from './hilfen.mjs';

test('Wechsel auf Französisch: Texte, Kategorien und Sprache des Dokuments, auch nach dem Neuladen', async ({ page }) => {
  const fehler = konsoleBeobachten(page);
  await page.goto('/');
  await expect(page.getByRole('heading', { level: 1 })).toContainText('Nicht mehr suchen.');

  await page.getByRole('button', { name: 'fr', exact: true }).click();
  await expect(page.locator('html')).toHaveAttribute('lang', 'fr');
  await expect(page.getByRole('heading', { level: 1 })).toContainText('Ne cherchez plus.');
  await expect(page.getByRole('button', { name: /Serrurerie/ })).toBeVisible();
  await expect(page.getByText('Langue : Français')).toBeVisible();
  await expect(page.getByRole('button', { name: 'fr', exact: true })).toHaveAttribute('aria-pressed', 'true');

  await page.reload();
  await expect(page.getByRole('heading', { level: 1 })).toContainText('Ne cherchez plus.');

  await page.getByRole('button', { name: 'de', exact: true }).click();
  await expect(page.locator('html')).toHaveAttribute('lang', 'de');
  await expect(page.getByRole('button', { name: /Schlüsseldienst/ })).toBeVisible();
  await erwarteKeineFehler(fehler);
});

test('Wechsel mitten im Formular: Eingaben bleiben, Beschriftungen und Meldungen wechseln', async ({ page }) => {
  // Eine unbekannte Postleitzahl beantwortet die Schnittstelle mit 404; das ist erwartet
  const fehler = konsoleBeobachten(page, [400, 401, 404]);
  const adresse = neueAdresse();
  await page.goto('/registrierung');
  await page.getByLabel('Name', { exact: true }).fill('Oberfläche Testperson');
  await page.getByLabel('E-Mail-Adresse').fill(adresse);
  await page.getByLabel('Postleitzahl').fill('3011');
  await expect(page.getByLabel('Ort', { exact: true })).toHaveValue('Bern');

  await page.getByRole('button', { name: 'fr', exact: true }).click();
  await expect(page.getByRole('heading', { level: 1 })).toHaveText('Créer un compte');
  await expect(page.getByLabel('Nom', { exact: true })).toHaveValue('Oberfläche Testperson');
  await expect(page.getByLabel('NPA')).toHaveValue('3011');
  await expect(page.getByLabel('Localité', { exact: true })).toHaveValue('Bern');

  // Meldungen erscheinen in der gewählten Sprache
  await page.getByLabel('NPA').fill('9999');
  await page.getByLabel('Mot de passe, au moins dix caractères').fill('oberflaeche-passwort-123');
  await page.locator('form button[type=submit]').click();
  await expect(page.getByRole('alert')).toContainText('Le NPA 9999 ne figure pas dans le répertoire officiel des localités.');
  await expect(page).toHaveURL(/\/registrierung$/);

  await page.getByRole('button', { name: 'de', exact: true }).click();
  await expect(page.getByRole('heading', { level: 1 })).toHaveText('Konto erstellen');
  await expect(page.getByLabel('E-Mail-Adresse')).toHaveValue(adresse);
  await expect(page.getByLabel('Postleitzahl')).toHaveValue('9999');
  await erwarteKeineFehler(fehler);
});

test.describe('Browser auf Französisch', () => {
  test.use({ locale: 'fr-CH' });

  test('Ohne gespeicherte Wahl gilt die Sprache des Browsers', async ({ page }) => {
    await page.goto('/');
    await expect(page.locator('html')).toHaveAttribute('lang', 'fr');
    await expect(page.getByRole('link', { name: 'Comment ça marche' })).toBeVisible();
  });
});
