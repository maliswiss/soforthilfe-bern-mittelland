// Oberflächentests: Vorschlagsliste einer Anfrage mit Begründung, Entfernung, Punktzahl, Preisangaben und den
// zugeklappten Teilpunkten je Kriterium; die Telefonnummer erst nach der Zusage. Dazu der Sprachwechsel auf der
// Liste und der Hinweis, wenn niemand Passendes in Reichweite ist.

import { test, expect } from '@playwright/test';
import {
  anmelden, registrieren, konsoleBeobachten, erwarteKeineFehler, OHNE_TESTPASSWORT, TESTPASSWORT
} from './hilfen.mjs';

// Anfrage per Kategorie erfassen; die Seite der Anfrage zeigt danach die Vorschläge.
async function anfrageErfassen(page, kategorie, plz, ort) {
  await page.goto('/');
  await page.getByRole('button', { name: new RegExp(`^${kategorie}`) }).click();
  await page.getByRole('button', { name: 'Hilfe finden' }).click();
  await page.getByLabel('Postleitzahl').fill(plz);
  await expect(page.getByLabel('Ort', { exact: true })).toHaveValue(ort);
  await page.getByRole('button', { name: 'Anfrage senden' }).click();
}

test('Anfrage mit Vorschlägen: Begründung, Preis und Teilpunkte je Kriterium', { tag: '@testdaten' }, async ({ page }) => {
  test.skip(OHNE_TESTPASSWORT, 'Verifizierte Helfende kommen mit den Testdaten');
  const fehler = konsoleBeobachten(page);

  await anmelden(page, 'anna.beispiel@beispiel.invalid', TESTPASSWORT);
  await expect(page.getByRole('link', { name: 'Meine Anfragen' })).toBeVisible();
  await page.goto('/');
  await page.getByRole('button', { name: /^Sanitär, Heizung, Elektro/ }).click();
  await page.getByRole('button', { name: 'Hilfe finden' }).click();
  await page.getByLabel('Postleitzahl').fill('3011');
  await expect(page.getByLabel('Ort', { exact: true })).toHaveValue('Bern');
  await page.getByRole('button', { name: 'Anfrage senden' }).click();

  await expect(page.getByRole('status')).toHaveText('Ihre Anfrage ist erfasst. Passende Helfende sind gefunden.');
  await expect(page.getByRole('heading', { level: 2, name: 'Vorschläge' })).toBeVisible();

  const vorschlaege = page.locator('[data-test=vorschlag]');
  await expect(vorschlaege.first().getByRole('heading', { level: 3 })).toHaveText(/^1\. \S+/);
  expect(await vorschlaege.count()).toBeLessThanOrEqual(3);

  const erster = vorschlaege.first();
  await expect(erster).toContainText('Punktzahl');
  await expect(erster).toContainText('Fähigkeit «Sanitär, Heizung, Elektro»');
  await expect(erster).toContainText(/im gleichen Postleitzahlgebiet|m entfernt|km entfernt/);
  await expect(erster).toContainText('Richtpreis');
  await expect(erster).toContainText('Die Telefonnummer wird nach der Zusage sichtbar.');

  // Zugeklappt: Teilpunkte je Kriterium und darunter die Punktzahl
  const kriterien = erster.locator('[data-test=kriterien] > div');
  await expect(kriterien.first()).toBeHidden();
  await erster.getByText('Wie kommt diese Reihenfolge zustande?').click();
  await expect(kriterien).toHaveCount(6);
  await expect(kriterien.first()).toContainText(/Distanz\s*[01]\.\d\d/);
  await expect(kriterien.nth(1)).toContainText('Fähigkeit');
  await expect(kriterien.last()).toContainText(/Punktzahl\s*[01]\.\d\d/);
  await erwarteKeineFehler(fehler);
});

test('Sprachwechsel auf der Vorschlagsliste: Überschrift, Hinweise und Kriterien auf Französisch', { tag: '@testdaten' }, async ({ page }) => {
  test.skip(OHNE_TESTPASSWORT, 'Verifizierte Helfende kommen mit den Testdaten');
  const fehler = konsoleBeobachten(page);

  await anmelden(page, 'anna.beispiel@beispiel.invalid', TESTPASSWORT);
  await expect(page.getByRole('link', { name: 'Meine Anfragen' })).toBeVisible();
  await anfrageErfassen(page, 'Sanitär, Heizung, Elektro', '3011', 'Bern');
  await expect(page.getByRole('heading', { level: 2, name: 'Vorschläge' })).toBeVisible();
  const erster = page.locator('[data-test=vorschlag]').first();
  await expect(erster).toContainText('Die Telefonnummer wird nach der Zusage sichtbar.');

  // Ohne Neuladen: die Liste wechselt die Sprache, die Reihenfolge bleibt
  const namen = await page.locator('[data-test=vorschlag] h3').allTextContents();
  await page.getByRole('button', { name: 'fr', exact: true }).click();
  await expect(page.locator('html')).toHaveAttribute('lang', 'fr');
  await expect(page.getByRole('heading', { level: 2, name: 'Propositions' })).toBeVisible();
  expect(await page.locator('[data-test=vorschlag] h3').allTextContents()).toEqual(namen);
  await expect(erster).toContainText('Le numéro de téléphone devient visible après l’acceptation.');
  await expect(erster).toContainText(/dans le même NPA|à [\d'’ ]+ m|à [\d.,]+ km/);

  await erster.getByText('Comment cet ordre est-il établi ?').click();
  const kriterien = erster.locator('[data-test=kriterien] > div');
  await expect(kriterien).toHaveCount(6);
  await expect(kriterien.first()).toContainText(/Distance\s*[01][.,]\d\d/);
  await expect(kriterien.nth(1)).toContainText('Compétence');
  await expect(kriterien.last()).toContainText(/Score\s*[01][.,]\d\d/);

  // Zurück auf Deutsch
  await page.getByRole('button', { name: 'de', exact: true }).click();
  await expect(page.getByRole('heading', { level: 2, name: 'Vorschläge' })).toBeVisible();
  await expect(kriterien.first()).toContainText('Distanz');
  await erwarteKeineFehler(fehler);
});

test('Ohne passende Helfende in Reichweite steht ein Hinweis statt einer Liste', async ({ page }) => {
  const fehler = konsoleBeobachten(page);
  await page.goto('/registrierung');
  await registrieren(page);
  await expect(page).toHaveURL(/\/anfragen$/);

  // In 3532 Zäziwil gibt es in den Testdaten niemanden für Reinigung nach Schäden
  await anfrageErfassen(page, 'Reinigung nach Schäden', '3532', 'Zäziwil');
  await expect(page.getByRole('status')).toHaveText('Ihre Anfrage ist erfasst und offen.');
  await expect(page.getByRole('heading', { level: 2, name: 'Vorschläge' })).toBeVisible();
  await expect(page.getByText('Zurzeit ist niemand Passendes in Reichweite.')).toBeVisible();
  await expect(page.locator('[data-test=vorschlag]')).toHaveCount(0);
  await erwarteKeineFehler(fehler);
});
