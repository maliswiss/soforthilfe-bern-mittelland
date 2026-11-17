// Oberflächentests: Sprachwechsel zwischen Deutsch und Französisch, gespeicherte Wahl und Sprache des Browsers.

import { test, expect } from '@playwright/test';
import { konsoleBeobachten, erwarteKeineFehler } from './hilfen.mjs';

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

test.describe('Browser auf Französisch', () => {
  test.use({ locale: 'fr-CH' });

  test('Ohne gespeicherte Wahl gilt die Sprache des Browsers', async ({ page }) => {
    await page.goto('/');
    await expect(page.locator('html')).toHaveAttribute('lang', 'fr');
    await expect(page.getByRole('link', { name: 'Comment ça marche' })).toBeVisible();
  });
});
