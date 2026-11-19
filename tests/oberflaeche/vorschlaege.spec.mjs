// Oberflächentests: Vorschlagsliste einer Anfrage mit Begründung, Entfernung, Punktzahl, Preisangaben und den
// zugeklappten Teilpunkten je Kriterium; die Telefonnummer erst nach der Zusage.

import { test, expect } from '@playwright/test';
import { anmelden, konsoleBeobachten, erwarteKeineFehler, OHNE_TESTPASSWORT, TESTPASSWORT } from './hilfen.mjs';

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
