<script setup>
import { computed, onMounted, ref } from 'vue';
import Meldung from '../komponenten/Meldung.vue';
import Feld from '../komponenten/Feld.vue';
import { aufruf } from '../dienste/schnittstelle.js';
import { sitzung } from '../dienste/sitzung.js';
import { t } from '../texte/index.js';

const kategorien = ref([]);
const meine = ref([]);
const kategorieId = ref('');
const freitext = ref('');
const postleitzahl = ref('');
const laeuft = ref(false);
const fehler = ref(null);
const erfasst = ref(null);

const istHilfesuchend = computed(() => sitzung.konto?.rolle === 'Hilfesuchende');

// Kostenlose und kostenpflichtige Kategorien getrennt anzeigen (Anforderung A12).
const gruppen = computed(() => [
  { titel: t('anfrage.kostenlos'), hinweis: t('anfrage.kostenlosHinweis'), liste: kategorien.value.filter((k) => !k.kostenpflichtig) },
  { titel: t('anfrage.kostenpflichtig'), hinweis: t('anfrage.kostenpflichtigHinweis'), liste: kategorien.value.filter((k) => k.kostenpflichtig) },
]);

async function ladeMeine() {
  meine.value = await aufruf('GET', '/api/anfragen');
}

onMounted(async () => {
  if (!istHilfesuchend.value) {
    return;
  }
  try {
    kategorien.value = await aufruf('GET', '/api/kategorien');
    await ladeMeine();
  } catch (ausnahme) {
    fehler.value = ausnahme;
  }
});

// Dieselben Regeln wie die Schnittstelle, damit Fehler sofort sichtbar sind; massgebend bleibt die Schnittstelle.
function pruefe() {
  if (!kategorieId.value && !freitext.value.trim()) {
    return t('anfrage.inhaltFehlt');
  }
  if (!/^[1-9][0-9]{3}$/.test(postleitzahl.value.trim())) {
    return t('anfrage.postleitzahlFalsch');
  }
  return null;
}

async function absenden() {
  fehler.value = null;
  erfasst.value = null;
  const hinweis = pruefe();
  if (hinweis) {
    fehler.value = { titel: t('fehler.eingabe'), text: hinweis };
    return;
  }

  laeuft.value = true;
  try {
    erfasst.value = await aufruf('POST', '/api/anfragen', {
      kategorieId: kategorieId.value || null,
      freitext: freitext.value.trim() || null,
      postleitzahl: postleitzahl.value.trim(),
    });
    kategorieId.value = '';
    freitext.value = '';
    await ladeMeine();
  } catch (ausnahme) {
    fehler.value = ausnahme;
  } finally {
    laeuft.value = false;
  }
}

const datum = (wert) => new Date(wert).toLocaleString('de-CH', { dateStyle: 'medium', timeStyle: 'short' });
</script>

<template>
  <section>
    <h1 class="text-3xl font-bold text-tanne">{{ t('anfrage.titel') }}</h1>

    <Meldung v-if="!istHilfesuchend" art="hinweis" :titel="t('anfrage.nurHilfesuchende')" />

    <template v-else>
      <p class="mt-3 text-lg">{{ t('anfrage.einleitung') }}</p>

      <form novalidate @submit.prevent="absenden">
        <fieldset class="mt-6">
          <legend class="text-xl font-semibold text-tanne">{{ t('anfrage.kategorie') }}</legend>

          <div v-for="gruppe in gruppen" :key="gruppe.titel" class="mt-4">
            <p class="font-semibold">{{ gruppe.titel }}</p>
            <p class="text-sm text-gedaempft">{{ gruppe.hinweis }}</p>
            <div class="mt-2 grid gap-2 sm:grid-cols-2">
              <label
                v-for="kategorie in gruppe.liste"
                :key="kategorie.id"
                class="flex cursor-pointer gap-3 rounded-lg border-2 bg-karte px-3 py-3"
                :class="kategorieId === kategorie.id ? 'border-linie' : 'border-rand'"
              >
                <input v-model="kategorieId" type="radio" name="kategorie" :value="kategorie.id" class="mt-1 h-5 w-5 accent-linie" />
                <span>
                  <span class="block font-semibold">{{ kategorie.bezeichnung }}</span>
                  <span class="block text-sm text-gedaempft">{{ kategorie.skills.join(', ') }}</span>
                </span>
              </label>
            </div>
          </div>

          <label class="mt-4 flex cursor-pointer items-center gap-3 rounded-lg border-2 bg-karte px-3 py-3" :class="kategorieId === '' ? 'border-linie' : 'border-rand'">
            <input v-model="kategorieId" type="radio" name="kategorie" value="" class="h-5 w-5 accent-linie" />
            {{ t('anfrage.keineKategorie') }}
          </label>
        </fieldset>

        <div class="mt-6">
          <label for="freitext" class="block font-semibold text-tanne">{{ t('anfrage.freitext') }}</label>
          <textarea id="freitext" v-model="freitext" rows="4" maxlength="2000" aria-describedby="freitext-hinweis" class="mt-1 w-full rounded-lg border border-rand bg-white px-3 py-3 text-lg" />
          <p id="freitext-hinweis" class="mt-1 text-sm text-gedaempft">{{ t('anfrage.freitextHinweis') }}</p>
        </div>

        <Feld id="postleitzahl" v-model="postleitzahl" :beschriftung="t('anfrage.postleitzahl')" inputmode="numeric" maxlength="4" autocomplete="postal-code" class="max-w-40" />

        <Meldung v-if="fehler" :titel="fehler.titel ?? t('fehler.allgemein')" :text="fehler.text" />
        <Meldung v-if="erfasst" art="hinweis" :titel="t('anfrage.erfasst')" :text="t('anfrage.status', { status: t(`status.${erfasst.status}`) })" />

        <button type="submit" :disabled="laeuft" class="mt-6 rounded-lg bg-tanne px-6 py-3 text-lg font-semibold text-white disabled:opacity-60">
          {{ laeuft ? t('anfrage.laeuft') : t('anfrage.absenden') }}
        </button>
      </form>

      <h2 class="mt-10 text-2xl font-bold text-tanne">{{ t('anfrage.meine') }}</h2>
      <p v-if="meine.length === 0" class="mt-2 text-gedaempft">{{ t('anfrage.keine') }}</p>
      <ul class="mt-3 space-y-3">
        <li v-for="anfrage in meine" :key="anfrage.id" class="rounded-lg border border-rand bg-karte px-4 py-3">
          <p class="font-semibold">
            {{ anfrage.kategorie ?? t('anfrage.ohneKategorie') }} · {{ anfrage.postleitzahl }}
          </p>
          <p v-if="anfrage.freitext" class="mt-1">{{ anfrage.freitext }}</p>
          <p class="mt-1 text-sm text-gedaempft">
            {{ datum(anfrage.erstelltAm) }} · {{ t('anfrage.status', { status: t(`status.${anfrage.status}`) }) }}
          </p>
        </li>
      </ul>
    </template>
  </section>
</template>
