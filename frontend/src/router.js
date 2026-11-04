import { createRouter, createWebHistory } from 'vue-router';
import StartSeite from './seiten/StartSeite.vue';

// Weitere Seiten werden erst beim Aufruf geladen, damit der Start schnell bleibt.
const routen = [
  { path: '/', name: 'start', component: StartSeite },
  { path: '/:pfad(.*)*', name: 'nicht-gefunden', component: () => import('./seiten/NichtGefundenSeite.vue') },
];

export default createRouter({
  history: createWebHistory(),
  routes: routen,
  scrollBehavior: () => ({ top: 0 }),
});
