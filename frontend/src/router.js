import { createRouter, createWebHistory } from 'vue-router';
import StartSeite from './seiten/StartSeite.vue';
import { istAngemeldet } from './dienste/sitzung.js';

// Weitere Seiten werden erst beim Aufruf geladen, damit der Start schnell bleibt.
const routen = [
  { path: '/', name: 'start', component: StartSeite },
  { path: '/anmelden', name: 'anmelden', component: () => import('./seiten/AnmeldenSeite.vue') },
  { path: '/registrieren', name: 'registrieren', component: () => import('./seiten/RegistrierenSeite.vue') },
  { path: '/anfrage', name: 'anfrage', component: () => import('./seiten/AnfrageSeite.vue'), meta: { anmeldung: true } },
  { path: '/:pfad(.*)*', name: 'nicht-gefunden', component: () => import('./seiten/NichtGefundenSeite.vue') },
];

const router = createRouter({
  history: createWebHistory(),
  routes: routen,
  scrollBehavior: () => ({ top: 0 }),
});

// Seiten mit Anmeldung: ohne Token zur Anmeldung und danach zurück.
router.beforeEach((ziel) => {
  if (ziel.meta.anmeldung && !istAngemeldet.value) {
    return { name: 'anmelden', query: { weiter: ziel.fullPath } };
  }
  return true;
});

export default router;
