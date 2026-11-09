import { createRouter, createWebHistory } from 'vue-router';
import { useKonto } from '../stores/konto';

const routen = [
  { path: '/', name: 'startseite', component: () => import('../views/StartseiteView.vue') },
  { path: '/anmeldung', name: 'anmeldung', component: () => import('../views/AnmeldungView.vue') },
  { path: '/registrierung', name: 'registrierung', component: () => import('../views/RegistrierungView.vue') },
  {
    path: '/anfrage/erfassen',
    name: 'anfrage-erfassen',
    component: () => import('../views/AnfrageErfassenView.vue'),
    meta: { anmeldung: true }
  },
  {
    path: '/anfragen',
    name: 'meine-anfragen',
    component: () => import('../views/MeineAnfragenView.vue'),
    meta: { anmeldung: true }
  },
  {
    path: '/anfragen/:id',
    name: 'anfrage',
    component: () => import('../views/AnfrageView.vue'),
    meta: { anmeldung: true }
  },
  { path: '/so-funktioniert-es', name: 'ablauf', component: () => import('../views/AblaufView.vue') },
  { path: '/notfall', name: 'notfall', component: () => import('../views/NotfallView.vue') },
  // Unbekannte Pfade führen zur Startseite
  { path: '/:pfad(.*)*', redirect: { name: 'startseite' } }
];

const router = createRouter({
  history: createWebHistory(),
  routes: routen,
  scrollBehavior: () => ({ top: 0 })
});

// Geschützte Seiten: ohne Anmeldung zur Anmeldung, danach zurück zum gewünschten Ziel
router.beforeEach((ziel) => {
  const konto = useKonto();
  konto.wiederherstellen();

  if (ziel.meta.anmeldung && !konto.angemeldet) {
    return { name: 'anmeldung', query: { weiter: ziel.fullPath } };
  }

  return true;
});

export default router;
