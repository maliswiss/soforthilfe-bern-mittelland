<script setup>
import Kopfzeile from './komponenten/Kopfzeile.vue';
import Notfallband from './komponenten/Notfallband.vue';
import { t } from './texte/index.js';
</script>

<template>
  <div class="flex min-h-screen flex-col">
    <!-- Das Notfallband steht auf jeder Seite ganz oben. -->
    <Notfallband />
    <Kopfzeile />

    <main class="mx-auto w-full max-w-3xl flex-1 px-4 py-8">
      <RouterView />
    </main>

    <footer class="border-t border-rand bg-karte px-4 py-4 text-center text-sm text-gedaempft">
      {{ t('fusszeile.hinweis') }}
    </footer>
  </div>
</template>
