<script setup>
import { computed, onBeforeUnmount, onMounted, watch } from 'vue';
import { useRoute, useRouter, RouterLink, RouterView } from 'vue-router';
import { useI18n } from 'vue-i18n';
import { useKonto } from './stores/konto';
import { sitzungAbgelaufen } from './api/klient';

const konto = useKonto();
const router = useRouter();
const route = useRoute();
const { t, locale } = useI18n();
konto.wiederherstellen();

const name = computed(() => konto.benutzer?.anzeigename ?? '');
const spracheName = computed(() => t(`sprache.${locale.value}`));

// Lehnt die Schnittstelle das Token ab, führt die Seite zur Anmeldung zurück und merkt sich das Ziel
function sitzungBeendet() {
  if (!konto.angemeldet) {
    return;
  }
  const ziel = route.fullPath;
  konto.abmelden();
  router.push({ name: 'anmeldung', query: { abgelaufen: '1', weiter: ziel } });
}

onMounted(() => {
  konto.regionenLaden();
  window.addEventListener(sitzungAbgelaufen, sitzungBeendet);
});

onBeforeUnmount(() => {
  window.removeEventListener(sitzungAbgelaufen, sitzungBeendet);
});

// Titel des Browserfensters folgt der Region
watch(
  () => konto.regionName,
  (region) => {
    document.title = `Soforthilfe ${region}`;
  },
  { immediate: true }
);

function abmelden() {
  konto.abmelden();
  router.push({ name: 'startseite' });
}
</script>

<template>
  <a href="#inhalt" class="sr-only focus:not-sr-only focus:block focus:bg-tanne focus:p-3 focus:text-white">
    {{ t('kopf.zumInhalt') }}
  </a>

  <div class="bg-notruf text-white print:hidden">
    <div class="mx-auto flex max-w-5xl flex-wrap items-center gap-x-3 gap-y-1 px-4 py-2 text-sm">
      <span class="font-semibold">{{ t('kopf.notfall') }}</span>
      <span>{{ t('kopf.notfallText') }}</span>
      <a href="tel:144" class="font-semibold underline underline-offset-2">{{ t('kopf.notfallAnrufen') }}</a>
    </div>
  </div>

  <header class="border-b border-kiesel bg-white print:hidden">
    <div class="mx-auto flex max-w-5xl flex-wrap items-center justify-between gap-3 px-4 py-4">
      <RouterLink :to="{ name: 'startseite' }" class="font-titel text-lg font-bold text-tanne">
        {{ t('kopf.marke') }} <span class="font-medium text-grau">{{ konto.regionName }}</span>
      </RouterLink>

      <nav class="flex flex-wrap items-center gap-4 text-sm">
        <RouterLink :to="{ name: 'ablauf' }" class="hover:text-tanne-hell">{{ t('kopf.ablauf') }}</RouterLink>

        <template v-if="konto.angemeldet">
          <RouterLink v-if="konto.istHilfesuchende" :to="{ name: 'meine-anfragen' }" class="hover:text-tanne-hell">
            {{ t('kopf.meineAnfragen') }}
          </RouterLink>
          <RouterLink v-if="konto.istHelfende" :to="{ name: 'helferprofil' }" class="hover:text-tanne-hell">
            {{ t('kopf.helferprofil') }}
          </RouterLink>
          <RouterLink v-if="konto.istKoordination" :to="{ name: 'koordination' }" class="hover:text-tanne-hell">
            {{ t('kopf.koordination') }}
          </RouterLink>
          <span class="text-grau">{{ name }}</span>
          <button type="button" class="underline underline-offset-2" @click="abmelden">{{ t('kopf.abmelden') }}</button>
        </template>

        <template v-else>
          <RouterLink :to="{ name: 'registrierung', query: { konto: 'helfende' } }" class="hover:text-tanne-hell">
            {{ t('kopf.fuerHelfende') }}
          </RouterLink>
          <RouterLink :to="{ name: 'anmeldung' }" class="hover:text-tanne-hell">{{ t('kopf.anmelden') }}</RouterLink>
        </template>
      </nav>
    </div>
  </header>

  <main id="inhalt" class="mx-auto max-w-5xl px-4 py-8">
    <RouterView />
  </main>

  <footer class="mt-12 border-t border-kiesel bg-white print:hidden">
    <div class="mx-auto grid max-w-5xl gap-6 px-4 py-8 text-sm text-grau sm:grid-cols-3">
      <div>
        <h2 class="mb-2 text-base">{{ t('fuss.problem') }}</h2>
        <p>support&#64;soforthilfe-bern.ch</p>
        <p>031 000 00 00</p>
        <p>{{ t('fuss.servicezeiten') }}</p>
      </div>
      <div>
        <h2 class="mb-2 text-base">{{ t('fuss.rechtliches') }}</h2>
        <p>{{ t('fuss.impressum') }}</p>
        <p>{{ t('fuss.datenschutz') }}</p>
        <p>{{ t('fuss.agb') }}</p>
      </div>
      <div>
        <h2 class="mb-2 text-base">{{ t('fuss.region') }}</h2>
        <p>{{ konto.regionName }}</p>
        <p>{{ t('fuss.sprache', { sprache: spracheName }) }}</p>
        <p class="mt-2 text-xs">{{ t('fuss.ortsdaten') }}</p>
      </div>
    </div>
  </footer>
</template>
