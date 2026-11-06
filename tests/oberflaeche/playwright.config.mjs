import { defineConfig, devices } from '@playwright/test';

// Oberflächentests gegen eine laufende Oberfläche (Vorschau oder Containerplattform).
// Adresse über OBERFLAECHE_URL, Standard ist die Vorschau von Vite.
export default defineConfig({
  testDir: '.',
  testMatch: '*.spec.mjs',
  timeout: 30_000,
  fullyParallel: false,
  workers: 1,
  retries: 0,
  reporter: [['list']],
  use: {
    baseURL: process.env.OBERFLAECHE_URL ?? 'http://localhost:4173',
    locale: 'de-CH',
    ignoreHTTPSErrors: true,
    trace: 'off',
    screenshot: 'off'
  },
  projects: [
    { name: 'desktop', use: { ...devices['Desktop Chrome'] }, testIgnore: 'mobil.spec.mjs' },
    { name: 'mobil', use: { ...devices['Pixel 7'] }, testMatch: 'mobil.spec.mjs' }
  ]
});
