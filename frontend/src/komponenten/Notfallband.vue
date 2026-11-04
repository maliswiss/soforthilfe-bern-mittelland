<script setup>
import { t } from '../texte/index.js';
</script>

<template>
  <!-- Rotes Band mit direktem Anruf; bewusst nur die Sanitätsnotrufnummer 144. -->
  <div class="bg-notfall-hell px-4 py-2 text-center text-notfall" role="note">
    <span class="font-semibold">{{ t('notfall.text') }}</span>
    <a href="tel:144" class="ml-2 font-bold underline underline-offset-2">{{ t('notfall.anrufen') }}</a>
  </div>
</template>
