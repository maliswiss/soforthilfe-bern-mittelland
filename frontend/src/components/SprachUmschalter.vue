<script setup>
/// Umschalter der Sprache in der Kopfzeile. Zeigt die verfügbaren Sprachen als Kurzzeichen.
import { useI18n } from 'vue-i18n';
import { mitgeliefert, spracheSetzen } from '../i18n';

const { t, locale } = useI18n();
</script>

<template>
  <div class="flex items-center gap-1 text-xs" role="group" :aria-label="t('kopf.sprache')">
    <button
      v-for="eintrag in mitgeliefert"
      :key="eintrag.code"
      type="button"
      class="border px-1.5 py-0.5 uppercase"
      :class="eintrag.code === locale
        ? 'border-tanne bg-tanne text-white'
        : 'border-kiesel text-grau hover:border-tanne-hell'"
      :lang="eintrag.code"
      :title="eintrag.name"
      :aria-pressed="eintrag.code === locale"
      @click="spracheSetzen(eintrag.code)"
    >
      {{ eintrag.code }}
    </button>
  </div>
</template>
