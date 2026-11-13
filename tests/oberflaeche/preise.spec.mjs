// Oberflächentests: Preisangaben vor der Zusage bei bezahlpflichtigen Kategorien, bei kostenlosen nicht;
// die Kategorie kommt aus der Wahl der Person oder aus der Verständniskette.

import { test, expect } from '@playwright/test';
import { anliegenBeschreiben, registrieren, konsoleBeobachten, erwarteKeineFehler } from './hilfen.mjs';

test('Erkannte bezahlpflichtige Kategorie: Preisangaben vor der Zusage beim Erfassen und in der Anfrage', async ({ page }) => {
  const fehler = konsoleBeobachten(page);
  await anliegenBeschreiben(page, 'Die Heizung ist ausgefallen und die Wohnung wird kalt.');
  await page.getByRole('link', { name: 'Jetzt registrieren' }).click();
  await registrieren(page);

  // Vorschlag der Verständniskette mit Begründung, änderbar
  const einordnung = page.locator('[data-test=einordnung]');
  await expect(einordnung).toContainText('Erkannte Kategorie: Sanitär, Heizung, Elektro');
  await expect(einordnung).toContainText('Erkannt über das Schlüsselwort «Heizung».');
  await expect(page.getByLabel('Kategorie', { exact: true })).toHaveValue(/.+/);

  const hinweis = page.locator('[data-test=preishinweis]');
  await expect(hinweis).toContainText('Preise vor der Zusage');
  await expect(hinweis).toContainText('Richtpreis für die erste Stunde inklusive Anfahrt');
  await expect(hinweis).toContainText('Verbindlich wird erst Ihre Zusage.');

  await page.getByLabel('Postleitzahl').fill('3011');
  await expect(page.getByLabel('Ort', { exact: true })).toHaveValue('Bern');
  await page.getByRole('button', { name: 'Anfrage senden' }).click();

  await expect(page.getByRole('heading', { level: 1 })).toHaveText('Sanitär, Heizung, Elektro');
  await expect(page.locator('[data-test=preishinweis]')).toContainText('Preise vor der Zusage');
  await erwarteKeineFehler(fehler);
});

test('Kostenlose Kategorie: keine Preisangaben, nur der Hinweis', async ({ page }) => {
  await page.goto('/registrierung');
  await registrieren(page);
  await expect(page).toHaveURL(/\/anfragen$/);

  await page.goto('/');
  await page.getByRole('button', { name: /Nachbarschaftshilfe/ }).click();
  await page.getByRole('button', { name: 'Hilfe finden' }).click();

  await expect(page.locator('[data-test=preishinweis]')).toHaveText('Diese Hilfe ist kostenlos.');
  await expect(page.getByText('Preise vor der Zusage')).toHaveCount(0);
  await expect(page.locator('[data-test=einordnung]')).toHaveCount(0);

  await page.getByLabel('Postleitzahl').fill('3110');
  await expect(page.getByLabel('Ort', { exact: true })).toHaveValue('Münsingen');
  await page.getByRole('button', { name: 'Anfrage senden' }).click();
  await expect(page.getByRole('heading', { level: 1 })).toHaveText('Nachbarschaftshilfe');
  await expect(page.getByText('Preise vor der Zusage')).toHaveCount(0);
});

test('Unsichere Einordnung: die ähnlichsten Kategorien stehen zur Auswahl, die Wahl bestimmt die Preisangaben', async ({ page }) => {
  await page.goto('/registrierung');
  await registrieren(page);
  await expect(page).toHaveURL(/\/anfragen$/);

  await anliegenBeschreiben(page, 'Ich brauche jemanden mit einem Bus für einen Tisch');
  await expect(page.locator('[data-test=einordnung]')).toContainText('Bitte wählen Sie die passende Kategorie.');
  const auswahl = page.getByLabel('Kategorie', { exact: true });
  await expect(auswahl).toHaveValue('');
  await expect(auswahl.locator('optgroup[label="Am ähnlichsten"] option')).toHaveCount(3);
  await expect(page.locator('[data-test=preishinweis]')).toHaveCount(0);

  await auswahl.selectOption({ label: 'Transport- und Umzugshilfe' });
  await expect(page.locator('[data-test=preishinweis]')).toContainText('Preise vor der Zusage');

  await page.getByLabel('Postleitzahl').fill('3011');
  await expect(page.getByLabel('Ort', { exact: true })).toHaveValue('Bern');
  await page.getByRole('button', { name: 'Anfrage senden' }).click();
  await expect(page.getByRole('heading', { level: 1 })).toHaveText('Transport- und Umzugshilfe');
});
