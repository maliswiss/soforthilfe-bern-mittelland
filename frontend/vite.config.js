import { defineConfig } from 'vite';
import vue from '@vitejs/plugin-vue';
import tailwindcss from '@tailwindcss/vite';
import { VitePWA } from 'vite-plugin-pwa';

// Adresse der Schnittstelle für die Entwicklung; im Container übernimmt der Lastverteiler die Weiterleitung.
const schnittstelle = process.env.API_URL ?? 'http://localhost:5000';

export default defineConfig({
  plugins: [
    vue(),
    tailwindcss(),
    VitePWA({
      registerType: 'autoUpdate',
      includeAssets: ['icons/icon.svg'],
      manifest: {
        name: 'Soforthilfe Bern/Mittelland',
        short_name: 'Soforthilfe',
        description: 'Hyperlokale Vermittlung von zeitkritischer Alltagshilfe',
        lang: 'de-CH',
        start_url: '/',
        display: 'standalone',
        theme_color: '#133B33',
        background_color: '#F4F5F1',
        icons: [
          { src: 'icons/icon-192.png', sizes: '192x192', type: 'image/png' },
          { src: 'icons/icon-512.png', sizes: '512x512', type: 'image/png' },
          { src: 'icons/icon-512.png', sizes: '512x512', type: 'image/png', purpose: 'maskable' },
        ],
      },
      workbox: {
        // Nur die Oberfläche wird zwischengespeichert, nie Antworten der Schnittstelle (Personendaten).
        navigateFallbackDenylist: [/^\/api\//, /^\/health\//],
        globPatterns: ['**/*.{js,css,html,svg,png}'],
      },
    }),
  ],
  server: {
    port: 5173,
    // Gleicher Ursprung wie im Betrieb: Aufrufe an /api gehen an die Schnittstelle weiter.
    proxy: {
      '/api': schnittstelle,
      '/health': schnittstelle,
    },
  },
});
