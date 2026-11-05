<script setup>
import { onMounted, ref } from 'vue';
import { RouterLink } from 'vue-router';
import { useI18n } from 'vue-i18n';
import { kultur } from '../i18n';
import { holen } from '../api/klient';
import { useKonto } from '../stores/konto';
import HinweisFeld from '../components/HinweisFeld.vue';
import StatusMarke from '../components/StatusMarke.vue';
import PrioritaetsMarke from '../components/PrioritaetsMarke.vue';

const konto = useKonto();
const { t } = useI18n();
const anfragen = ref([]);
const geladen = ref(false);
const fehler = ref('');

onMounted(async () => {
  try {
    anfragen.value = await holen('/anfragen/meine', konto.token);
  } catch (problem) {
    fehler.value = problem.message;
  } finally {
    geladen.value = true;
  }
});
</script>

<template>
  <div class="flex flex-wrap items-center justify-between gap-3">
    <h1 class="text-3xl">{{ t('meineAnfragen.titel') }}</h1>
    <RouterLink :to="{ name: 'startseite' }" class="knopf">{{ t('meineAnfragen.neu') }}</RouterLink>
  </div>

  <HinweisFeld v-if="fehler" art="fehler" :meldung="fehler" class="mt-6" />

  <p v-if="geladen && !fehler && anfragen.length === 0" class="karte mt-6 text-grau">
    {{ t('meineAnfragen.leer') }}
  </p>

  <ul class="mt-6 space-y-3">
    <li v-for="anfrage in anfragen" :key="anfrage.id" class="karte">
      <div class="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h2 class="text-xl">{{ anfrage.kategorieBezeichnung ?? t('meineAnfragen.ohneKategorie') }}</h2>
          <p class="text-sm text-grau">
            {{ anfrage.plz }} {{ anfrage.ort }} ·
            {{ new Date(anfrage.erstelltAm).toLocaleString(kultur()) }}
          </p>
        </div>
        <div class="flex flex-wrap items-center gap-2">
          <PrioritaetsMarke :wert="anfrage.prioritaet" />
          <StatusMarke :wert="anfrage.status" />
        </div>
      </div>

      <RouterLink
        :to="{ name: 'anfrage', params: { id: anfrage.id } }"
        class="mt-3 inline-block underline underline-offset-2"
      >
        {{ t('hinweis.anfrageOeffnen') }}
      </RouterLink>
    </li>
  </ul>
</template>
