<script setup>
import { onMounted, ref } from 'vue';
import { useRoute, RouterLink } from 'vue-router';
import { useI18n } from 'vue-i18n';
import { holen } from '../api/klient';
import { kultur } from '../i18n';
import { useKonto } from '../stores/konto';
import PreisHinweis from '../components/PreisHinweis.vue';
import HinweisFeld from '../components/HinweisFeld.vue';
import PrioritaetsMarke from '../components/PrioritaetsMarke.vue';
import StatusMarke from '../components/StatusMarke.vue';

const route = useRoute();
const { t } = useI18n();
const konto = useKonto();

const anfrage = ref(null);
const fehler = ref('');

onMounted(async () => {
  try {
    anfrage.value = await holen(`/anfragen/${route.params.id}`, konto.token);
  } catch (problem) {
    fehler.value = problem.message;
  }
});
</script>

<template>
  <div v-if="anfrage">
    <div class="flex flex-wrap items-center gap-3">
      <h1 class="text-3xl">{{ anfrage.kategorieBezeichnung ?? t('hinweis.anfrage') }}</h1>
      <StatusMarke :wert="anfrage.status" />
      <PrioritaetsMarke :wert="anfrage.prioritaet" />
    </div>

    <p class="mt-2 text-grau">
      {{ anfrage.plz }} {{ anfrage.ort }}
      <span v-if="anfrage.zeitfenster"> · {{ anfrage.zeitfenster }}</span>
      <span> · {{ t('anfrage.erfasstAm', { datum: new Date(anfrage.erstelltAm).toLocaleString(kultur()) }) }}</span>
    </p>

    <p v-if="anfrage.freitext" class="mt-4 max-w-lesen border-l-4 border-kiesel pl-4 text-grau">
      {{ anfrage.freitext }}
    </p>

    <p v-if="anfrage.status === 1" class="mt-6 max-w-2xl border-l-4 border-tanne bg-tanne-blass px-4 py-3" role="status">
      {{ t('anfrage.erfasst') }}
    </p>

    <dl class="karte mt-6 max-w-2xl space-y-3 text-sm">
      <div v-if="anfrage.adresse" class="flex gap-4">
        <dt class="w-32 shrink-0 text-grau">{{ t('anfrage.adresse') }}</dt>
        <dd>{{ anfrage.adresse }}</dd>
      </div>
      <div v-if="anfrage.zeitfenster" class="flex gap-4">
        <dt class="w-32 shrink-0 text-grau">{{ t('anfrage.zeitfenster') }}</dt>
        <dd>{{ anfrage.zeitfenster }}</dd>
      </div>
      <p class="text-grau">
        {{ anfrage.bezahlpflichtig ? t('anfrage.kostenpflichtig') : t('anfrage.kostenlos') }}
      </p>
    </dl>

    <PreisHinweis v-if="anfrage.bezahlpflichtig" :bezahlpflichtig="true" class="mt-4 max-w-2xl" />

    <RouterLink :to="{ name: 'meine-anfragen' }" class="knopf-leise mt-6">{{ t('anfrage.zurueck') }}</RouterLink>
  </div>

  <HinweisFeld v-if="fehler" art="fehler" :meldung="fehler" />
</template>
