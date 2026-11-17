// Oberflächentests: Sprachverwaltung des Plattformbetriebs. Eine importierte Sprache erscheint ohne neues Deployment
// im Umschalter, fehlende Texte bleiben deutsch.

import { test, expect } from '@playwright/test';
import { anmelden, konsoleBeobachten, erwarteKeineFehler, OHNE_TESTPASSWORT, TESTPASSWORT } from './hilfen.mjs';

const betrieb = 'hans.betrieb@beispiel.invalid';
const datei = (name, inhalt) => ({ name, mimeType: 'application/json', buffer: Buffer.from(JSON.stringify(inhalt)) });

test('Ohne Anmeldung führt die Sprachverwaltung zur Anmeldung', async ({ page }) => {
  await page.goto('/sprachen');
  await expect(page).toHaveURL(/\/anmeldung\?weiter=(%2F|\/)sprachen/);
});

test('Plattformbetrieb importiert, schaltet und löscht eine Sprache', { skip: OHNE_TESTPASSWORT }, async ({ page, browser, request }) => {
  // Reste früherer Läufe entfernen
  const { token } = await (await request.post('/api/auth/anmeldung', { data: { eMail: betrieb, passwort: TESTPASSWORT } })).json();
  await request.delete('/api/sprachen/xz', { headers: { Authorization: `Bearer ${token}` } });

  const fehler = konsoleBeobachten(page);
  await anmelden(page, betrieb, TESTPASSWORT);
  await page.getByRole('link', { name: 'Sprachen', exact: true }).click();
  await expect(page.getByRole('heading', { level: 1 })).toHaveText('Sprachen der Oberfläche');

  await page.getByLabel('Code').fill('xz');
  await page.getByLabel('Name', { exact: true }).fill('Testlingua');

  // Prüfung im Browser vor dem Import
  await page.getByLabel('Sprachdatei (JSON)').setInputFiles(datei('falsch.json', { kopf: { ablauf: 'Come <b>funziona</b>' } }));
  await expect(page.locator('[data-test=pruefung]')).toContainText('nicht erlaubt. Betroffen: «kopf.ablauf»');

  await page.getByLabel('Sprachdatei (JSON)').setInputFiles(
    datei('xz.json', { kopf: { ablauf: 'Come funziona', sprachen: 'Lingue' }, extra: { titel: 'Mai visto' } })
  );
  await expect(page.locator('[data-test=pruefung]')).toContainText('xz.json: 3 Texte geprüft.');
  await expect(page.locator('[data-test=pruefung]')).toContainText('erscheinen auf Deutsch');
  await expect(page.locator('[data-test=pruefung]')).toContainText('extra.titel');

  await page.getByRole('button', { name: 'Importieren' }).click();
  await expect(page.getByText('Testlingua ist importiert (3 Texte).')).toBeVisible();
  await expect(page.locator('[data-test=sprache]', { hasText: 'Testlingua' })).toContainText('aktiv');

  // Gilt sofort im Umschalter, ohne Neuladen; fehlende Texte bleiben deutsch
  await page.getByRole('button', { name: 'xz', exact: true }).click();
  await expect(page.locator('html')).toHaveAttribute('lang', 'xz');
  await expect(page.getByRole('link', { name: 'Come funziona' })).toBeVisible();
  await expect(page.getByRole('heading', { level: 1 })).toHaveText('Sprachen der Oberfläche');
  await expect(page.getByText('Sprache: Testlingua')).toBeVisible();

  // Auch Besucherinnen ohne Anmeldung sehen die neue Sprache
  const besuch = await browser.newPage();
  await besuch.goto('/');
  await expect(besuch.getByRole('button', { name: 'xz', exact: true })).toBeVisible();
  await besuch.close();

  // Ausschalten: verschwindet aus dem Umschalter, die Oberfläche fällt auf Deutsch zurück
  await page.getByRole('button', { name: 'Ausschalten' }).click();
  await expect(page.getByText('Testlingua ist ausgeschaltet.')).toBeVisible();
  await expect(page.getByRole('button', { name: 'xz', exact: true })).toHaveCount(0);
  await expect(page.locator('html')).toHaveAttribute('lang', 'de');

  await page.getByRole('button', { name: 'Löschen', exact: true }).click();
  await page.getByRole('button', { name: 'Wirklich löschen' }).click();
  await expect(page.getByText('Testlingua ist gelöscht.')).toBeVisible();
  await expect(page.locator('[data-test=sprache]', { hasText: 'Testlingua' })).toHaveCount(0);
  await erwarteKeineFehler(fehler);
});

test('Ohne Rolle Plattformbetrieb kein Zugang zur Sprachverwaltung', { skip: OHNE_TESTPASSWORT }, async ({ page }) => {
  await anmelden(page, 'emil.exempel@beispiel.invalid', TESTPASSWORT);
  await expect(page.getByRole('link', { name: 'Meine Anfragen' })).toBeVisible();
  await expect(page.getByRole('link', { name: 'Sprachen', exact: true })).toHaveCount(0);
  await page.goto('/sprachen');
  await expect(page).toHaveURL(/\/$/);
});
