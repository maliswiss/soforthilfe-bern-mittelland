<script setup>
import { ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import Feld from '../komponenten/Feld.vue';
import Meldung from '../komponenten/Meldung.vue';
import { aufruf } from '../dienste/schnittstelle.js';
import { setzeAnmeldung, sitzung } from '../dienste/sitzung.js';
import { t } from '../texte/index.js';

const route = useRoute();
const router = useRouter();
const eMail = ref('');
const passwort = ref('');
const laeuft = ref(false);
const fehler = ref(null);

async function absenden() {
  laeuft.value = true;
  fehler.value = null;
  try {
    setzeAnmeldung(await aufruf('POST', '/api/auth/anmeldung', { eMail: eMail.value, passwort: passwort.value, mandant: sitzung.region?.kennung ?? 'bern-mittelland' }));
    // Zurück zur Seite, die die Anmeldung verlangt hat; nur interne Pfade sind erlaubt.
    const weiter = typeof route.query.weiter === 'string' && route.query.weiter.startsWith('/') ? route.query.weiter : '/';
    await router.push(weiter);
  } catch (ausnahme) {
    fehler.value = ausnahme;
  } finally {
    laeuft.value = false;
  }
}
</script>

<template>
  <section class="max-w-md">
    <h1 class="text-3xl font-bold text-tanne">{{ t('anmelden.titel') }}</h1>
    <form novalidate @submit.prevent="absenden">
      <Feld id="email" v-model="eMail" :beschriftung="t('anmelden.email')" type="email" autocomplete="username" required />
      <Feld id="passwort" v-model="passwort" :beschriftung="t('anmelden.passwort')" type="password" autocomplete="current-password" required />
      <Meldung v-if="fehler" :titel="fehler.titel ?? t('fehler.allgemein')" :text="fehler.text" />
      <button type="submit" :disabled="laeuft" class="mt-6 w-full rounded-lg bg-tanne px-6 py-3 text-lg font-semibold text-white disabled:opacity-60">
        {{ laeuft ? t('anmelden.laeuft') : t('anmelden.absenden') }}
      </button>
    </form>
    <p class="mt-6">
      {{ t('anmelden.neu') }}
      <RouterLink :to="{ name: 'registrieren', query: route.query }" class="font-semibold text-linie underline">{{ t('anmelden.registrieren') }}</RouterLink>
    </p>
  </section>
</template>
