// Oberflächentests: Standortfreigabe nur auf Wunsch und nur über die eigene Schnittstelle.

import { test, expect } from '@playwright/test';

// Beobachtet alle Aufrufe der Seite: fremde Ziele und die Aufrufe der Standortübersetzung.
function aufrufeBeobachten(seite, basis) {
  const eigen = new URL(basis).origin;
  const beobachtet = { fremde: [], standort: [] };
  seite.on('request', (aufruf) => {
    const adresse = new URL(aufruf.url());
    if (['http:', 'https:'].includes(adresse.protocol) && adresse.origin !== eigen) {
      beobachtet.fremde.push(aufruf.url());
    }
    if (adresse.pathname === '/api/orte/umgebung') {
      beobachtet.standort.push(aufruf);
    }
  });
  return beobachtet;
}

test.describe('mit freigegebenem Standort in Münsingen', () => {
  test.use({ geolocation: { latitude: 46.873, longitude: 7.5615 }, permissions: ['geolocation'] });

  test('«Standort verwenden» füllt Postleitzahl und Ort, nur über die eigene Schnittstelle', async ({ page, baseURL }) => {
    const aufrufe = aufrufeBeobachten(page, baseURL);
    await page.goto('/registrierung');

    // Ohne Klick wird kein Standort abgefragt
    await expect(page.getByText('nur nach Ihrer Freigabe im Browser')).toBeVisible();
    expect(aufrufe.standort).toHaveLength(0);

    await page.getByRole('button', { name: 'Standort verwenden' }).click();
    await expect(page.getByLabel('Postleitzahl')).toHaveValue('3110');
    await expect(page.getByLabel('Ort', { exact: true })).toHaveValue('Münsingen');
    await expect(page.getByRole('status')).toContainText('Standort übernommen: 3110 Münsingen');

    // Ein Aufruf, als POST, die Koordinaten nicht in der Adresse, keine fremden Ziele
    expect(aufrufe.standort).toHaveLength(1);
    expect(aufrufe.standort[0].method()).toBe('POST');
    expect(aufrufe.standort[0].url()).not.toMatch(/46\.87|7\.56/);
    expect(aufrufe.fremde).toEqual([]);

    // Nichts gespeichert: weder im Speicher des Browsers noch in der Adresse der Seite
    const speicher = await page.evaluate(() => JSON.stringify({ ...localStorage, ...sessionStorage }));
    expect(speicher).not.toMatch(/46\.87|7\.56/);
    await expect(page).toHaveURL(/\/registrierung$/);
  });
});

test.describe('mit freigegebenem Standort im Ausland', () => {
  test.use({ geolocation: { latitude: 48.8566, longitude: 2.3522 }, permissions: ['geolocation'] });

  test('Ohne Schweizer Postleitzahl erscheint ein Hinweis, die Eingabe bleibt von Hand möglich', async ({ page }) => {
    await page.goto('/registrierung');
    await page.getByRole('button', { name: 'Standort verwenden' }).click();
    await expect(page.getByRole('alert')).toContainText('keine Schweizer Postleitzahl');
    await expect(page.getByLabel('Postleitzahl')).toHaveValue('');
  });
});

test('Ohne Freigabe wird der Zugriff verweigert und erklärt', async ({ page, context }) => {
  await context.clearPermissions();
  await page.goto('/registrierung');
  await page.getByRole('button', { name: 'Standort verwenden' }).click();
  // Je nach Browser folgt die Ablehnung sofort oder nach der eigenen Wartezeit von zwölf Sekunden
  await expect(page.getByRole('alert')).toContainText('Postleitzahl und Ort eintragen.', { timeout: 15_000 });
  await expect(page.getByLabel('Postleitzahl')).toHaveValue('');
});
