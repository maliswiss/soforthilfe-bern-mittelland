<script setup>
import { useI18n } from 'vue-i18n';

const { t } = useI18n();
const schritte = [1, 2, 3, 4];
</script>

<template>
  <h1 class="text-3xl">{{ t('ablauf.titel') }}</h1>

  <ol class="mt-6 max-w-lesen space-y-6">
    <li v-for="nummer in schritte" :key="nummer">
      <h2 class="text-xl">{{ t(`ablauf.schritt${nummer}`) }}</h2>
      <p class="mt-1 text-grau">{{ t(`ablauf.schritt${nummer}Text`) }}</p>
    </li>
  </ol>

  <section class="mt-10 max-w-lesen">
    <h2 class="text-2xl">{{ t('ablauf.abgrenzungTitel') }}</h2>
    <p class="mt-2 text-grau">{{ t('ablauf.abgrenzungText') }}</p>
  </section>
</template>
