<script setup>
import { useRouter } from 'vue-router';
import { abmelden, istAngemeldet, sitzung } from '../dienste/sitzung.js';
import { t } from '../texte/index.js';

const router = useRouter();

async function beenden() {
  abmelden();
  await router.push('/');
}
</script>

<template>
  <header class="bg-tanne text-white">
    <nav class="mx-auto flex max-w-3xl flex-wrap items-center justify-between gap-4 px-4 py-4" :aria-label="t('app.navigation')">
      <RouterLink to="/" class="flex items-center gap-3">
        <img src="/icons/icon.svg" alt="" class="h-10 w-10" />
        <span>
          <span class="block text-lg font-bold leading-tight">{{ t('app.name') }}</span>
          <span class="block text-sm text-hell">{{ t('app.untertitel') }}</span>
        </span>
      </RouterLink>

      <div class="flex items-center gap-4">
        <span v-if="istAngemeldet" class="text-sm text-hell">{{ sitzung.konto?.anzeigename }}</span>
        <button v-if="istAngemeldet" type="button" class="rounded-lg border border-hell px-3 py-2 font-semibold" @click="beenden">
          {{ t('navigation.abmelden') }}
        </button>
        <RouterLink v-else :to="{ name: 'anmelden' }" class="rounded-lg border border-hell px-3 py-2 font-semibold">
          {{ t('navigation.anmelden') }}
        </RouterLink>
      </div>
    </nav>
  </header>
</template>
