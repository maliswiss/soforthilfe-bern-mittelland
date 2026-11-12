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
  {
    path: '/helfen/profil',
    name: 'helferprofil',
    component: () => import('../views/HelferProfilView.vue'),
    meta: { anmeldung: true, rolle: 'Helfende' }
  },
  {
    path: '/koordination',
    name: 'koordination',
    component: () => import('../views/KoordinationView.vue'),
    meta: { anmeldung: true, rolle: 'Koordination' }
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

// Geschützte Seiten: ohne Anmeldung zur Anmeldung, danach zurück zum gewünschten Ziel.
// Seiten einer bestimmten Rolle führen ohne diese Rolle zur Startseite; die Schnittstelle prüft zusätzlich.
router.beforeEach((ziel) => {
  const konto = useKonto();
  konto.wiederherstellen();

  if (ziel.meta.anmeldung && !konto.angemeldet) {
    return { name: 'anmeldung', query: { weiter: ziel.fullPath } };
  }

  if (ziel.meta.rolle && !konto.benutzer?.rollen?.includes(ziel.meta.rolle)) {
    return { name: 'startseite' };
  }

  return true;
});

export default router;
