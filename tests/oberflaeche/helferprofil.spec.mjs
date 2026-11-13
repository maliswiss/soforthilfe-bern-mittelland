// Oberflächentests: Helferprofil mit Preisangaben, Nachweise ohne Ausweisdokumente und Freigabe durch die Koordination.

import { test, expect } from '@playwright/test';
import {
  anmelden, registrieren, konsoleBeobachten, erwarteKeineFehler, neueAdresse, TESTPASSWORT, OHNE_TESTPASSWORT
} from './hilfen.mjs';

test('Helfende richten nach der Registrierung ihr Profil ein und reichen einen Nachweis ein', async ({ page }) => {
  // Vor dem ersten Speichern meldet die Schnittstelle 404 für das Profil; das ist erwartet
  const fehler = konsoleBeobachten(page, [400, 401, 404]);
  await page.goto('/registrierung?konto=helfende');
  await registrieren(page);

  await expect(page).toHaveURL(/\/helfen\/profil$/);
  await expect(page.getByRole('heading', { level: 1 })).toHaveText('Helferprofil');
  await expect(page.getByText('Speichern Sie zuerst Ihr Profil')).toBeVisible();

  await page.getByLabel('Postleitzahl').fill('3011');
  await expect(page.getByLabel('Ort', { exact: true })).toHaveValue('Bern');
  await page.getByRole('button', { name: /Sanitär, Heizung, Elektro/ }).click();

  // Bezahlpflichtige Fähigkeit ohne Preise wird nicht gespeichert
  await page.getByRole('button', { name: 'Profil speichern' }).click();
  await expect(page.getByRole('alert')).toHaveText('Für bezahlpflichtige Fähigkeiten sind Anfahrt und Stundenansatz anzugeben.');

  await page.getByLabel('Anfahrt (CHF pauschal)').fill('40');
  await page.getByLabel('Stundenansatz (CHF)').fill('95');
  await page.getByLabel('Nachtzuschlag 20–7 Uhr (%)').fill('50');
  // Die Vorschau zeigt die Preise so, wie Anfragende sie vor der Zusage sehen
  const vorschau = page.locator('[data-test=preisvorschau]');
  await expect(vorschau).toContainText('Richtpreis erste Stunde: ca. CHF 135.00 (inklusive Anfahrt)');
  await expect(vorschau).toContainText('Richtpreis erste Stunde: ca. CHF 182.50 (inklusive Anfahrt, Nachtzuschlag 50 %)');
  await expect(vorschau).toContainText('CHF 95.00 pro Stunde');
  await expect(vorschau).toContainText('nicht mehrwertsteuerpflichtig');

  await page.getByRole('button', { name: 'Profil speichern' }).click();
  await expect(page.getByRole('status')).toHaveText('Das Profil ist gespeichert.');
  await expect(page.locator('[data-test=verifizierungsstand]')).toHaveText('Aktueller Stand: nicht verifiziert');

  // Mehrwertnummern zeigt die Oberfläche schon bei der Eingabe an und sendet sie nicht
  await page.getByLabel('Art des Nachweises').selectOption({ label: 'Bestätigte Telefonnummer' });
  await page.getByLabel('Angabe', { exact: true }).fill('0900 123 456');
  await expect(page.getByRole('alert')).toContainText('kostenpflichtig');
  await page.getByRole('button', { name: 'Nachweis einreichen' }).click();
  await expect(page.getByText('Noch keine Nachweise eingereicht.')).toBeVisible();

  // Ohne Angabe gilt die Telefonnummer des Kontos
  await page.getByLabel('Angabe', { exact: true }).fill('');
  await page.getByRole('button', { name: 'Nachweis einreichen' }).click();
  await expect(page.getByRole('status')).toHaveText('Der Nachweis liegt bei der Koordination zur Prüfung.');
  await expect(page.locator('[data-test=verifizierungsstand]')).toHaveText('Aktueller Stand: in Prüfung');
  const liste = page.locator('[data-test=nachweise] li');
  await expect(liste).toHaveCount(1);
  await expect(liste.first()).toContainText('Bestätigte Telefonnummer');
  await expect(liste.first()).toContainText('031 123 45 67');
  await expect(liste.first()).toContainText('in Prüfung');

  // Nach dem Neuladen steht alles wieder da
  await page.reload();
  await expect(page.getByLabel('Stundenansatz (CHF)')).toHaveValue('95');
  await expect(page.locator('[data-test=nachweise] li')).toHaveCount(1);
  await erwarteKeineFehler(fehler);
});

test('Die Koordination gibt einen Nachweis frei, die helfende Person sieht den neuen Stand', { skip: OHNE_TESTPASSWORT }, async ({ page }) => {
  // Helfende Person mit Betrieb und UID-Nummer, über die Schnittstelle vorbereitet
  const adresse = neueAdresse();
  const passwort = 'oberflaeche-passwort-123';
  const betriebsname = `Oberfläche Begleitdienst ${Date.now()}`;
  const registrierung = await page.request.post('/api/auth/registrierung', {
    data: { eMail: adresse, passwort, anzeigename: 'Oberfläche Helfende', kontoart: 1, mandant: 'bern-mittelland' }
  });
  expect(registrierung.status()).toBe(200);
  const kopf = { Authorization: `Bearer ${(await registrierung.json()).token}` };
  const skills = await (await page.request.get('/api/skills')).json();
  const begleitung = skills.find((s) => s.code === 'begleitung');
  expect((await page.request.put('/api/helferprofil', {
    headers: kopf, data: { typ: 1, betriebsname, plz: '3011', ort: 'Bern', radiusKm: 10, skillIds: [begleitung.id] }
  })).status()).toBe(200);
  expect((await page.request.post('/api/verifizierungen', {
    headers: kopf, data: { art: 0, nachweiswert: 'CHE-123.456.788' }
  })).status()).toBe(200);

  // Die Koordination landet nach der Anmeldung bei den offenen Verifizierungen
  await anmelden(page, 'dora.probe@beispiel.invalid', TESTPASSWORT);
  await expect(page).toHaveURL(/\/koordination$/);
  await expect(page.getByRole('heading', { level: 1 })).toHaveText('Koordination Bern/Mittelland');

  const karte = page.locator('[data-test=pruefung]').filter({ hasText: betriebsname });
  await expect(karte).toContainText('Betrieb · 3011 Bern · Profil in Prüfung');
  await expect(karte).toContainText('UID-Nummer des Betriebs · CHE-123.456.788');
  await karte.getByLabel('Bemerkung zur Prüfung').fill('Eintrag im UID-Register geprüft');
  await karte.getByRole('button', { name: 'Freigeben' }).click();
  await expect(page.getByRole('status')).toHaveText('Der Nachweis ist freigegeben.');
  await expect(karte).toHaveCount(0);
  await expect(page.locator('table')).toContainText('Verifizierung.Freigegeben');

  // Die helfende Person sieht den Stand und die Bemerkung
  await page.getByRole('button', { name: 'Abmelden' }).click();
  await anmelden(page, adresse, passwort);
  await expect(page).toHaveURL(/\/helfen\/profil$/);
  await expect(page.locator('[data-test=verifizierungsstand]')).toHaveText('Aktueller Stand: verifiziert');
  await expect(page.locator('[data-test=nachweise] li').first()).toContainText('Bemerkung der Koordination: Eintrag im UID-Register geprüft');
});

test('Helferprofil und Koordination nur mit der passenden Rolle', async ({ page }) => {
  await page.goto('/helfen/profil');
  await expect(page).toHaveURL(/\/anmeldung\?weiter=/);

  await page.goto('/registrierung');
  await registrieren(page);
  await expect(page).toHaveURL(/\/anfragen$/);
  await expect(page.getByRole('link', { name: 'Helferprofil' })).toHaveCount(0);
  await expect(page.getByRole('link', { name: 'Koordination' })).toHaveCount(0);

  await page.goto('/koordination');
  await expect(page).toHaveURL(/\/$/);
  await page.goto('/helfen/profil');
  await expect(page).toHaveURL(/\/$/);
});
