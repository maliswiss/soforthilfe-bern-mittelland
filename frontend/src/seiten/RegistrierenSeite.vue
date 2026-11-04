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
const formular = ref({ anzeigename: '', eMail: '', passwort: '', telefon: '', rolle: 'Hilfesuchende' });
const laeuft = ref(false);
const fehler = ref(null);

async function absenden() {
  laeuft.value = true;
  fehler.value = null;
  try {
    const { telefon, ...daten } = formular.value;
    // Das Konto entsteht in der gewählten Region (Kopf X-Region); danach direkt anmelden.
    await aufruf('POST', '/api/konto/registrieren', { ...daten, telefon: telefon.trim() || null });
    setzeAnmeldung(await aufruf('POST', '/api/konto/anmelden', { eMail: daten.eMail, passwort: daten.passwort }));
    const weiter = typeof route.query.weiter === 'string' && route.query.weiter.startsWith('/') ? route.query.weiter : null;
    await router.push(weiter ?? (daten.rolle === 'Hilfesuchende' ? { name: 'anfrage' } : '/'));
  } catch (ausnahme) {
    fehler.value = ausnahme;
  } finally {
    laeuft.value = false;
  }
}
</script>

<template>
  <section class="max-w-md">
    <h1 class="text-3xl font-bold text-tanne">{{ t('registrieren.titel') }}</h1>

    <div v-if="!sitzung.region">
      <Meldung art="hinweis" :titel="t('registrieren.regionFehlt')" />
      <RouterLink to="/" class="mt-4 inline-block font-semibold text-linie underline">{{ t('fehlerseite.zurueck') }}</RouterLink>
    </div>

    <form v-else novalidate @submit.prevent="absenden">
      <p class="mt-3 text-gedaempft">{{ t('registrieren.region', { name: sitzung.region.name }) }}</p>

      <fieldset class="mt-5">
        <legend class="font-semibold text-tanne">{{ t('registrieren.rolle') }}</legend>
        <div class="mt-2 grid grid-cols-2 gap-3">
          <label v-for="rolle in ['Hilfesuchende', 'Helfende']" :key="rolle" class="flex cursor-pointer items-center gap-2 rounded-lg border-2 bg-karte px-3 py-3" :class="formular.rolle === rolle ? 'border-linie' : 'border-rand'">
            <input v-model="formular.rolle" type="radio" name="rolle" :value="rolle" class="h-5 w-5 accent-linie" />
            {{ rolle === 'Hilfesuchende' ? t('registrieren.hilfesuchende') : t('registrieren.helfende') }}
          </label>
        </div>
      </fieldset>

      <Feld id="anzeigename" v-model="formular.anzeigename" :beschriftung="t('registrieren.anzeigename')" autocomplete="name" required minlength="2" maxlength="100" />
      <Feld id="email" v-model="formular.eMail" :beschriftung="t('registrieren.email')" type="email" autocomplete="email" required />
      <Feld id="passwort" v-model="formular.passwort" :beschriftung="t('registrieren.passwort')" type="password" autocomplete="new-password" required minlength="10" />
      <Feld id="telefon" v-model="formular.telefon" :beschriftung="t('registrieren.telefon')" :hinweis="t('registrieren.telefonHinweis')" type="tel" autocomplete="tel" />

      <Meldung v-if="fehler" :titel="fehler.titel ?? t('fehler.allgemein')" :text="fehler.text" />
      <button type="submit" :disabled="laeuft" class="mt-6 w-full rounded-lg bg-tanne px-6 py-3 text-lg font-semibold text-white disabled:opacity-60">
        {{ laeuft ? t('registrieren.laeuft') : t('registrieren.absenden') }}
      </button>
    </form>

    <p class="mt-6">
      {{ t('registrieren.vorhanden') }}
      <RouterLink :to="{ name: 'anmelden', query: route.query }" class="font-semibold text-linie underline">{{ t('registrieren.anmelden') }}</RouterLink>
    </p>
  </section>
</template>
