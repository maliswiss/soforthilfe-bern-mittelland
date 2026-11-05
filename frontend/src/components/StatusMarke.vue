<script setup>
/// Status einer Anfrage als kleine Marke (Zahlenwert der Schnittstelle, Text aus de.json).
import { useI18n } from 'vue-i18n';

defineProps({
  wert: { type: Number, required: true }
});

const { t, te } = useI18n();
</script>

<template>
  <span class="border border-tanne px-2 py-0.5 text-xs font-medium text-tanne">
    {{ te(`status.${wert}`) ? t(`status.${wert}`) : t('allgemein.unbekannt') }}
  </span>
</template>
