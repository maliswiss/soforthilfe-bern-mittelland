<script setup>
/// Auswahl der Region (Mandant) vor Anmeldung und Registrierung.
/// Nach der Anmeldung bestimmt das Konto die Region; die Auswahl ist dann gesperrt.
import { computed, onMounted } from 'vue';
import { useI18n } from 'vue-i18n';
import { useKonto, regionKurzname } from '../stores/konto';

const props = defineProps({
  kennung: { type: String, default: 'region' },
  kompakt: { type: Boolean, default: false }
});

const konto = useKonto();
const { t } = useI18n();

onMounted(() => {
  if (!konto.regionen.length) {
    konto.regionenLaden();
  }
});

const auswahl = computed({
  get: () => konto.region,
  set: (wert) => konto.regionWaehlen(wert)
});
</script>

<template>
  <div v-if="konto.mehrereRegionen" :class="props.kompakt ? 'flex items-center gap-2' : ''">
    <label :for="props.kennung" :class="props.kompakt ? 'text-xs text-grau' : 'etikett'">{{ t('region.titel') }}</label>
    <select
      :id="props.kennung"
      v-model="auswahl"
      :class="props.kompakt ? 'border border-kiesel bg-white px-2 py-1 text-sm' : 'feld'"
      :disabled="konto.angemeldet"
    >
      <option v-for="region in konto.regionen" :key="region.schluessel" :value="region.schluessel">
        {{ regionKurzname(region.name) }}
      </option>
    </select>
    <p v-if="!props.kompakt" class="mt-1 text-xs text-grau">
      {{ t('region.hinweis') }}
    </p>
  </div>
</template>
