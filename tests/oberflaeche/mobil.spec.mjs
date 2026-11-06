// Oberflächentests: Darstellung auf dem Mobiltelefon.

import { test, expect } from '@playwright/test';

test('Startseite und Anmeldung passen ohne seitliches Scrollen auf das Mobiltelefon', async ({ page }) => {
  for (const pfad of ['/', '/anmeldung', '/registrierung']) {
    await page.goto(pfad);
    await expect(page.getByRole('link', { name: 'Jetzt 144 anrufen' })).toBeVisible();
    const breite = await page.evaluate(() => ({
      inhalt: document.documentElement.scrollWidth,
      fenster: window.innerWidth
    }));
    expect(breite.inhalt, pfad).toBeLessThanOrEqual(breite.fenster);
  }
});
