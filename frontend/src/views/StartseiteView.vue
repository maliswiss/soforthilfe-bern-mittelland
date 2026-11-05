<script setup>
import { onMounted, ref } from 'vue';
import { useRouter, RouterLink } from 'vue-router';
import { useI18n } from 'vue-i18n';
import { holen } from '../api/klient';
import { useAnfrageEntwurf } from '../stores/anfrage';
import { useKonto } from '../stores/konto';
import HinweisFeld from '../components/HinweisFeld.vue';

const router = useRouter();
const { t } = useI18n();
const entwurf = useAnfrageEntwurf();
const konto = useKonto();

const kategorien = ref([]);
const eingabeart = ref(entwurf.freitext && !entwurf.kategorieId ? 'freitext' : 'kategorie');
const gewaehlteKategorie = ref(entwurf.kategorieId);
const freitext = ref(entwurf.freitext);
const fehler = ref('');

onMounted(async () => {
  try {
    kategorien.value = await holen('/kategorien');
  } catch {
    fehler.value = t('start.fehlerKategorien');
  }
});

// Die Eingabe bleibt im Entwurf; angemeldet geht es direkt weiter, sonst zuerst zur Anmeldung.
function hilfeFinden() {
  fehler.value = '';

  if (eingabeart.value === 'kategorie' && !gewaehlteKategorie.value) {
    fehler.value = t('start.fehlerKategorie');
    return;
  }

  if (eingabeart.value === 'freitext' && freitext.value.trim().length < 10) {
    fehler.value = t('start.fehlerFreitext');
    return;
  }

  const kategorie = kategorien.value.find((k) => k.id === gewaehlteKategorie.value);
  entwurf.kategorieId = eingabeart.value === 'kategorie' ? gewaehlteKategorie.value : null;
  entwurf.kategorieBezeichnung = eingabeart.value === 'kategorie' ? kategorie?.bezeichnung ?? null : null;
  entwurf.freitext = eingabeart.value === 'freitext' ? freitext.value.trim() : '';

  router.push({ name: 'anfrage-erfassen' });
}
</script>

<template>
  <section class="grid gap-10 lg:grid-cols-[1.1fr_1fr] lg:items-start">
    <div>
      <h1 class="text-4xl leading-tight sm:text-5xl">{{ t('start.titel1') }}<br />{{ t('start.titel2') }}</h1>
      <p class="mt-4 max-w-lesen text-lg text-grau">
        {{ t('start.einleitung', { region: konto.regionName }) }}
      </p>

      <dl class="mt-8 divide-y divide-kiesel border-y border-kiesel">
        <div class="flex gap-4 py-3">
          <dt class="w-40 shrink-0 text-grau">{{ t('start.schrittAnfrage') }}</dt>
          <dd>{{ t('start.schrittAnfrageText') }}</dd>
        </div>
        <div class="flex gap-4 py-3">
          <dt class="w-40 shrink-0 text-grau">{{ t('start.schrittVerstaendnis') }}</dt>
          <dd>{{ t('start.schrittVerstaendnisText') }}</dd>
        </div>
        <div class="flex gap-4 py-3">
          <dt class="w-40 shrink-0 text-grau">{{ t('start.schrittVorschlaege') }}</dt>
          <dd>{{ t('start.schrittVorschlaegeText') }}</dd>
        </div>
        <div class="flex gap-4 py-3">
          <dt class="w-40 shrink-0 text-grau">{{ t('start.schrittAbschluss') }}</dt>
          <dd>{{ t('start.schrittAbschlussText') }}</dd>
        </div>
      </dl>
    </div>

    <div class="border-2 border-tanne bg-white p-6">
      <h2 class="text-2xl">{{ t('start.frage') }}</h2>

      <div class="mt-4 flex border border-kiesel" role="tablist">
        <button
          type="button"
          role="tab"
          class="flex-1 px-4 py-2 text-sm font-medium"
          :class="eingabeart === 'kategorie' ? 'bg-tanne text-white' : 'bg-white text-tinte'"
          :aria-selected="eingabeart === 'kategorie'"
          @click="eingabeart = 'kategorie'"
        >
          {{ t('start.kategorieWaehlen') }}
        </button>
        <button
          type="button"
          role="tab"
          class="flex-1 px-4 py-2 text-sm font-medium"
          :class="eingabeart === 'freitext' ? 'bg-tanne text-white' : 'bg-white text-tinte'"
          :aria-selected="eingabeart === 'freitext'"
          @click="eingabeart = 'freitext'"
        >
          {{ t('start.freiBeschreiben') }}
        </button>
      </div>

      <div v-if="eingabeart === 'kategorie'" class="mt-4 grid gap-2 sm:grid-cols-2">
        <button
          v-for="kategorie in kategorien"
          :key="kategorie.id"
          type="button"
          class="border px-3 py-3 text-left text-sm"
          :class="gewaehlteKategorie === kategorie.id
            ? 'border-tanne bg-tanne-blass'
            : 'border-kiesel hover:border-tanne-hell'"
          :aria-pressed="gewaehlteKategorie === kategorie.id"
          @click="gewaehlteKategorie = kategorie.id"
        >
          {{ kategorie.bezeichnung }}
          <span v-if="!kategorie.bezahlpflichtig" class="mt-1 block text-xs text-grau">{{ t('allgemein.kostenlos') }}</span>
        </button>
      </div>

      <div v-else class="mt-4">
        <label for="freitext" class="etikett">{{ t('start.freitextEtikett') }}</label>
        <textarea
          id="freitext"
          v-model="freitext"
          rows="5"
          maxlength="2000"
          class="feld"
          :placeholder="t('start.freitextBeispiel')"
        ></textarea>
      </div>

      <HinweisFeld v-if="fehler" art="fehler" :meldung="fehler" class="mt-4" />

      <button type="button" class="knopf mt-5 w-full" @click="hilfeFinden">
        {{ t('start.hilfeFinden') }}
      </button>

      <p class="mt-3 text-xs text-grau">
        {{ t('start.ohneKonto') }}
      </p>
    </div>
  </section>

  <section class="mt-14 grid gap-6 border-t border-kiesel pt-10 sm:grid-cols-2">
    <div>
      <h2 class="text-2xl">{{ t('start.helfenTitel') }}</h2>
      <p class="mt-2 max-w-lesen text-grau">
        {{ t('start.helfenText') }}
      </p>
      <RouterLink
        v-if="!konto.angemeldet"
        :to="{ name: 'registrierung', query: { konto: 'helfende' } }"
        class="knopf-leise mt-4"
      >
        {{ t('start.alsHelfendeRegistrieren') }}
      </RouterLink>
    </div>

    <div>
      <h2 class="text-2xl">{{ t('start.vertrauenTitel') }}</h2>
      <ul class="mt-2 max-w-lesen space-y-2 text-grau">
        <li>{{ t('start.vertrauen1') }}</li>
        <li>{{ t('start.vertrauen2') }}</li>
        <li>{{ t('start.vertrauen3') }}</li>
        <li>{{ t('start.vertrauen4') }}</li>
      </ul>
    </div>
  </section>
</template>
