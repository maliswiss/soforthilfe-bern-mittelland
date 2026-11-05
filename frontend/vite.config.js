import { defineConfig } from 'vite';
import vue from '@vitejs/plugin-vue';
import tailwindcss from '@tailwindcss/vite';
import { VitePWA } from 'vite-plugin-pwa';
import { sicherheitskoepfe } from './csp.js';

// Adresse der Schnittstelle für die Entwicklung; im Container übernimmt der Lastverteiler die Weiterleitung.
const schnittstelle = process.env.API_URL ?? 'http://localhost:5000';

export default defineConfig({
  plugins: [
    vue(),
    tailwindcss(),
    VitePWA({
      registerType: 'autoUpdate',
      includeAssets: ['favicon.svg'],
      manifest: {
        name: 'Soforthilfe Bern/Mittelland',
        short_name: 'Soforthilfe',
        description: 'Schnelle Hilfe von verifizierten Helfenden in der Region Bern/Mittelland.',
        lang: 'de-CH',
        start_url: '/',
        display: 'standalone',
        background_color: '#F5F6F3',
        theme_color: '#133B33',
        icons: [
          { src: '/icon-192.png', sizes: '192x192', type: 'image/png' },
          { src: '/icon-512.png', sizes: '512x512', type: 'image/png' }
        ]
      },
      workbox: {
        // Nur die Oberfläche wird zwischengespeichert, nie Antworten der Schnittstelle (Personendaten).
        navigateFallbackDenylist: [/^\/api\//, /^\/health\//],
        globPatterns: ['**/*.{js,css,html,svg,png,woff2}']
      }
    })
  ],
  // Die Vorschau liefert dieselben Sicherheitsköpfe wie der Betrieb.
  preview: { headers: sicherheitskoepfe },
  server: {
    port: 5173,
    // Gleicher Ursprung wie im Betrieb: Aufrufe an /api gehen an die Schnittstelle weiter.
    proxy: {
      '/api': schnittstelle,
      '/health': schnittstelle
    }
  }
});
