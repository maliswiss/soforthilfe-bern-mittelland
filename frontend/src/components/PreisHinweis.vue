<script setup>
/// Hinweis zu den Preisen einer Kategorie, bevor jemand zusagt (Anforderungen A9 und A12):
/// bei bezahlpflichtigen Kategorien, welche Preisangaben vor der Zusage sichtbar sind,
/// bei kostenlosen nur der Hinweis, dass keine Kosten entstehen.
import { useI18n } from 'vue-i18n';

defineProps({
  bezahlpflichtig: { type: Boolean, required: true }
});

const { t } = useI18n();
</script>

<template>
  <div v-if="bezahlpflichtig" class="border-l-4 border-tanne bg-tanne-blass px-4 py-3 text-sm" data-test="preishinweis">
    <p class="font-medium">{{ t('preis.vorZusageTitel') }}</p>
    <p class="mt-1">{{ t('preis.vorZusageText') }}</p>
    <ul class="mt-1 list-disc pl-5">
      <li>{{ t('preis.vorZusagePunkt1') }}</li>
      <li>{{ t('preis.vorZusagePunkt2') }}</li>
      <li>{{ t('preis.vorZusagePunkt3') }}</li>
      <li>{{ t('preis.vorZusagePunkt4') }}</li>
    </ul>
    <p class="mt-1 font-medium">{{ t('preis.vorZusageVerbindlich') }}</p>
  </div>
  <p v-else class="text-sm text-grau" data-test="preishinweis">{{ t('preis.kostenlosHinweis') }}</p>
</template>
