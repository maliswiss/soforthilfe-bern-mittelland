<script setup>
import { onMounted, ref } from 'vue';
import Meldung from '../komponenten/Meldung.vue';
import { aufruf } from '../dienste/schnittstelle.js';
import { istAngemeldet, sitzung, waehleRegion } from '../dienste/sitzung.js';
import { t } from '../texte/index.js';

const regionen = ref([]);
const laedt = ref(true);
const fehler = ref(null);

// Die Liste der Regionen ist öffentlich; ohne Anmeldung wählt die Person ihre Region selbst.
onMounted(async () => {
  try {
    regionen.value = await aufruf('GET', '/api/regionen');
  } catch (ausnahme) {
    fehler.value = ausnahme;
  } finally {
    laedt.value = false;
  }
});

const regionName = (kennung) => regionen.value.find((r) => r.kennung === kennung)?.name ?? kennung;
</script>

<template>
  <section>
    <h1 class="text-3xl font-bold text-tanne">{{ t('start.titel') }}</h1>
    <p class="mt-3 text-lg">{{ t('start.einleitung') }}</p>

    <!-- Angemeldet gilt die Region des Kontos und kann hier nicht gewechselt werden. -->
    <p v-if="istAngemeldet" class="mt-8 text-lg font-semibold text-tanne">
      {{ t('start.regionKonto', { name: regionName(sitzung.konto?.region) }) }}
    </p>

    <fieldset v-else class="mt-8">
      <legend class="text-xl font-semibold text-tanne">{{ t('start.regionTitel') }}</legend>
      <p v-if="laedt" class="mt-3 text-gedaempft">{{ t('start.regionenLaden') }}</p>
      <Meldung v-if="fehler" :titel="fehler.titel ?? t('fehler.allgemein')" :text="fehler.text" />

      <div class="mt-3 grid gap-3 sm:grid-cols-2">
        <label
          v-for="region in regionen"
          :key="region.kennung"
          class="flex cursor-pointer items-center gap-3 rounded-lg border-2 bg-karte px-4 py-4 text-lg"
          :class="sitzung.region?.kennung === region.kennung ? 'border-linie' : 'border-rand'"
        >
          <input
            type="radio"
            name="region"
            class="h-5 w-5 accent-linie"
            :value="region.kennung"
            :checked="sitzung.region?.kennung === region.kennung"
            @change="waehleRegion(region)"
          />
          <span>{{ region.name }}</span>
        </label>
      </div>
      <p v-if="sitzung.region" class="mt-3 text-gedaempft">{{ t('start.regionGewaehlt', { name: sitzung.region.name }) }}</p>
    </fieldset>

    <div class="mt-8 flex flex-wrap items-center gap-4">
      <RouterLink
        v-if="istAngemeldet || sitzung.region"
        :to="{ name: 'anfrage' }"
        class="rounded-lg bg-tanne px-6 py-3 text-lg font-semibold text-white"
      >
        {{ t('start.weiter') }}
      </RouterLink>
      <p v-if="!istAngemeldet">
        {{ t('start.konto') }}
        <RouterLink :to="{ name: 'registrieren' }" class="font-semibold text-linie underline">{{ t('start.registrieren') }}</RouterLink>
      </p>
    </div>
  </section>
</template>
