<script setup>
/// Dringlichkeit einer Anfrage; «hoch» erscheint in der Farbe für dringende Fälle.
import { useI18n } from 'vue-i18n';

defineProps({
  wert: { type: Number, required: true }
});

const { t } = useI18n();
</script>

<template>
  <span
    class="border px-2 py-0.5 text-xs font-medium"
    :class="wert === 2
      ? 'border-dringend text-dringend'
      : 'border-kiesel text-grau'"
  >
    {{ t('prioritaet.marke', { stufe: t(`prioritaet.${wert}`) }) }}
  </span>
</template>
