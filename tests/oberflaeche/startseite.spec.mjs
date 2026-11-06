// Oberflächentests: Startseite, Notfallband und Regionswahl.

import { test, expect } from '@playwright/test';
import { konsoleBeobachten, erwarteKeineFehler } from './hilfen.mjs';

test('Notfallband mit 144 steht auf jeder Seite', async ({ page }) => {
  for (const pfad of ['/', '/anmeldung', '/registrierung', '/so-funktioniert-es']) {
    await page.goto(pfad);
    const anruf = page.getByRole('link', { name: 'Jetzt 144 anrufen' });
    await expect(anruf, pfad).toBeVisible();
    await expect(anruf, pfad).toHaveAttribute('href', 'tel:144');
  }
});

test('Startseite zeigt zehn Kategorien, kostenlose sind gekennzeichnet', async ({ page }) => {
  const fehler = konsoleBeobachten(page);
  await page.goto('/');

  await expect(page.getByRole('heading', { level: 1 })).toContainText('Passende Hilfe finden.');
  const kacheln = page.locator('[aria-pressed]');
  await expect(kacheln).toHaveCount(10);
  await expect(page.getByRole('button', { name: /Nachbarschaftshilfe/ })).toContainText('kostenlos');
  await expect(page.getByRole('button', { name: /^Schlüsseldienst$/ })).not.toContainText('kostenlos');
  await erwarteKeineFehler(fehler);
});

test('Ohne Auswahl oder mit zu kurzem Text erscheint ein Hinweis', async ({ page }) => {
  await page.goto('/');
  await page.getByRole('button', { name: 'Hilfe finden' }).click();
  await expect(page.getByRole('alert')).toHaveText('Bitte wählen Sie eine Kategorie.');

  await page.getByRole('tab', { name: 'Frei beschreiben' }).click();
  await page.getByLabel('Beschreiben Sie Ihr Anliegen in eigenen Worten').fill('Hilfe');
  await page.getByRole('button', { name: 'Hilfe finden' }).click();
  await expect(page.getByRole('alert')).toHaveText('Bitte beschreiben Sie Ihr Anliegen in einem kurzen Satz.');
});

test('Regionswahl bietet die aktiven Regionen an und merkt sich die Auswahl', async ({ page }) => {
  await page.goto('/');
  const auswahl = page.locator('#region-kopf');
  await expect(auswahl.locator('option[value="bern-mittelland"]')).toHaveText('Bern/Mittelland');
  await expect(auswahl.locator('option[value="biel-seeland"]')).toHaveText('Biel/Seeland');

  await auswahl.selectOption('biel-seeland');
  await expect(page.getByRole('link', { name: /Soforthilfe/ }).first()).toContainText('Biel/Seeland');
  await page.reload();
  await expect(page.locator('#region-kopf')).toHaveValue('biel-seeland');
  await page.locator('#region-kopf').selectOption('bern-mittelland');
});

test('Geschützte Seiten führen zur Anmeldung und merken sich das Ziel', async ({ page }) => {
  await page.goto('/anfragen');
  await expect(page).toHaveURL(/\/anmeldung\?weiter=%2Fanfragen|\/anmeldung\?weiter=\/anfragen/);
  await expect(page.getByRole('heading', { name: 'Anmelden' })).toBeVisible();
});
