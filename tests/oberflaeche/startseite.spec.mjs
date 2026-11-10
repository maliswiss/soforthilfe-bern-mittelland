// Oberflächentests: Startseite, Notfallband, Fusszeile und Region aus der Postleitzahl.

import { test, expect } from '@playwright/test';
import { konsoleBeobachten, erwarteKeineFehler, registrieren, OHNE_TESTPASSWORT } from './hilfen.mjs';

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

test('Keine Regionswahl: die Region folgt aus der Postleitzahl der Registrierung', { tag: '@testdaten' }, async ({ page }) => {
  test.skip(OHNE_TESTPASSWORT, 'Die Region Biel/Seeland kommt mit den Testdaten');
  await page.goto('/');
  await expect(page.getByRole('combobox')).toHaveCount(0);

  await page.goto('/registrierung');
  await registrieren(page, { plz: '2502', ort: 'Biel/Bienne' });
  await expect(page).toHaveURL(/\/anfragen$/);
  await expect(page.getByRole('banner')).toContainText('Biel/Seeland');
  await page.getByRole('button', { name: 'Abmelden' }).click();
});

test('Postleitzahl ausserhalb der Regionen wird bei der Registrierung erklärt', async ({ page }) => {
  await page.goto('/registrierung');
  await registrieren(page, { plz: '8001', ort: 'Zürich' });
  await expect(page.getByRole('alert')).toHaveText('Für die Postleitzahl 8001 gibt es noch keine Region auf der Plattform.');
  await expect(page).toHaveURL(/\/registrierung/);
});

test('Fusszeile nennt die Quelle der Ortsdaten', async ({ page }) => {
  await page.goto('/');
  await expect(page.getByRole('contentinfo')).toContainText('Amtliches Ortschaftenverzeichnis © swisstopo');
});

test('Geschützte Seiten führen zur Anmeldung und merken sich das Ziel', async ({ page }) => {
  await page.goto('/anfragen');
  await expect(page).toHaveURL(/\/anmeldung\?weiter=%2Fanfragen|\/anmeldung\?weiter=\/anfragen/);
  await expect(page.getByRole('heading', { name: 'Anmelden' })).toBeVisible();
});
