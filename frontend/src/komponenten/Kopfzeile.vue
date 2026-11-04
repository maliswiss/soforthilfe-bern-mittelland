<script setup>
import { t } from '../texte/index.js';
</script>

<template>
  <header class="bg-tanne text-white">
    <nav class="mx-auto flex max-w-3xl items-center justify-between gap-4 px-4 py-4" :aria-label="t('app.navigation')">
      <RouterLink to="/" class="flex items-center gap-3">
        <img src="/icons/icon.svg" alt="" class="h-10 w-10" />
        <span>
          <span class="block text-lg font-bold leading-tight">{{ t('app.name') }}</span>
          <span class="block text-sm text-hell">{{ t('app.untertitel') }}</span>
        </span>
      </RouterLink>
    </nav>
  </header>
</template>
