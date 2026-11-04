<script setup>
import { t } from '../texte/index.js';
</script>

<template>
  <section>
    <h1 class="text-3xl font-bold text-tanne">{{ t('fehlerseite.titel') }}</h1>
    <p class="mt-3">{{ t('fehlerseite.text') }}</p>
    <RouterLink to="/" class="mt-6 inline-block font-semibold text-linie underline">{{ t('fehlerseite.zurueck') }}</RouterLink>
  </section>
</template>
