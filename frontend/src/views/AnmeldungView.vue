<script setup>
import { ref } from 'vue';
import { useRoute, useRouter, RouterLink } from 'vue-router';
import { useI18n } from 'vue-i18n';
import { useKonto } from '../stores/konto';
import { internesZiel } from '../api/ziel';
import HinweisFeld from '../components/HinweisFeld.vue';
import RegionAuswahl from '../components/RegionAuswahl.vue';

const konto = useKonto();
const route = useRoute();
const router = useRouter();
const { t } = useI18n();

const eMail = ref('');
const passwort = ref('');
const fehler = ref('');
const laeuft = ref(false);

async function anmelden() {
  fehler.value = '';
  laeuft.value = true;
  try {
    await konto.anmelden(eMail.value.trim(), passwort.value);
    // Hilfesuchende landen bei ihren Anfragen, alle anderen auf der Startseite
    const standardziel = konto.istHilfesuchende ? { name: 'meine-anfragen' } : { name: 'startseite' };
    router.push(internesZiel(route.query.weiter) ?? standardziel);
  } catch (problem) {
    fehler.value =
      problem.status === 401 && konto.mehrereRegionen
        ? `${problem.message} ${t('anmeldung.regionPruefen')}`
        : problem.message;
  } finally {
    laeuft.value = false;
  }
}
</script>

<template>
  <div class="max-w-md">
    <h1 class="text-3xl">{{ t('anmeldung.titel') }}</h1>

    <form class="karte mt-6 space-y-4" @submit.prevent="anmelden">
      <RegionAuswahl kennung="region-anmeldung" />

      <div>
        <label for="email" class="etikett">{{ t('anmeldung.email') }}</label>
        <input id="email" v-model="eMail" type="email" class="feld" autocomplete="email" required />
      </div>

      <div>
        <label for="passwort" class="etikett">{{ t('anmeldung.passwort') }}</label>
        <input
          id="passwort"
          v-model="passwort"
          type="password"
          class="feld"
          autocomplete="current-password"
          required
        />
      </div>

      <HinweisFeld v-if="route.query.abgelaufen && !fehler" art="hinweis" :meldung="t('anmeldung.sitzungAbgelaufen')" />
      <HinweisFeld v-if="fehler" art="fehler" :meldung="fehler" />

      <button type="submit" class="knopf w-full" :disabled="laeuft">
        {{ laeuft ? t('anmeldung.pruefen') : t('anmeldung.knopf') }}
      </button>
    </form>

    <p class="mt-4 text-sm text-grau">
      {{ t('anmeldung.keinKonto') }}
      <RouterLink :to="{ name: 'registrierung', query: route.query }" class="underline underline-offset-2">
        {{ t('anmeldung.registrieren') }}
      </RouterLink>
    </p>
  </div>
</template>
