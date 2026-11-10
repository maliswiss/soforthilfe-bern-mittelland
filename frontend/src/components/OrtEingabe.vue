<script setup>
import { computed, ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { orteZuPlz } from '../api/orte';
import { ortZuStandort, standortErmitteln } from '../api/standort';
import HinweisFeld from './HinweisFeld.vue';

/// Postleitzahl und Ort. Zur Postleitzahl ergänzt das amtliche Ortschaftenverzeichnis den Ort
/// und weist darauf hin, wenn die Postleitzahl mehrere Gemeinden umfasst. Auf Wunsch übersetzt
/// die eigene Schnittstelle den freigegebenen Standort in Postleitzahl und Ort; die Koordinaten
/// bleiben nur für diesen einen Aufruf im Speicher und werden nirgends abgelegt.
const plz = defineModel('plz', { type: String, default: '' });
const ort = defineModel('ort', { type: String, default: '' });

defineProps({
  kennung: { type: String, default: 'ort' },
  pflicht: { type: Boolean, default: true }
});

const { t } = useI18n();
const ortschaften = ref([]);
const geladenePlz = ref('');
const laeuft = ref(false);
const meldung = ref('');
const fehler = ref('');

// Mit übernommenem Standort ist der Hinweis auf mehrere Gemeinden nicht nötig.
const mehrereGemeinden = computed(() => !meldung.value && ortschaften.value.some((o) => o.plzUeberMehrereGemeinden));

async function ortschaftenLaden(wert, ortErsetzen) {
  if (!/^[1-9]\d{3}$/.test(wert)) {
    ortschaften.value = [];
    geladenePlz.value = '';
    return;
  }

  if (geladenePlz.value !== wert) {
    try {
      ortschaften.value = await orteZuPlz(wert);
      geladenePlz.value = wert;
    } catch {
      // Die Ergänzung ist eine Hilfe; ohne sie bleibt die Eingabe von Hand möglich
      ortschaften.value = [];
      return;
    }
  }

  const passt = ortschaften.value.some((o) => o.ort === ort.value.trim());
  if (ortErsetzen && ortschaften.value.length > 0 && !passt) {
    ort.value = ortschaften.value[0].ort;
  }
}

/// Nur Eingaben der Person lösen die Ergänzung aus, nicht programmatisch gesetzte Werte.
/// Der Wert wird aus dem Ereignis gelesen, da das Modell erst nach dem nächsten Rendern nachzieht.
function plzEingegeben(ereignis) {
  // Von Hand geändert: die Meldung zum übernommenen Standort gilt nicht mehr
  meldung.value = '';
  ortschaftenLaden(ereignis.target.value.trim(), true);
}

function ortFokussiert() {
  ortschaftenLaden(plz.value.trim(), false);
}

/// Standort nur auf Klick: der Browser fragt die Person um Freigabe.
async function standortVerwenden() {
  fehler.value = '';
  meldung.value = '';
  laeuft.value = true;

  try {
    const position = await standortErmitteln();
    const treffer = await ortZuStandort(position);

    plz.value = treffer.plz;
    ort.value = treffer.ort;
    await ortschaftenLaden(treffer.plz, false);

    meldung.value =
      position.genauigkeitMeter > 1000
        ? t('ort.uebernommenUngenau', {
            plz: treffer.plz,
            ort: treffer.ort,
            km: Math.round(position.genauigkeitMeter / 1000)
          })
        : t('ort.uebernommen', { plz: treffer.plz, ort: treffer.ort });
  } catch (problem) {
    fehler.value = problem.message;
  } finally {
    laeuft.value = false;
  }
}
</script>

<template>
  <div class="space-y-3">
    <div class="grid gap-4 sm:grid-cols-[140px_1fr]">
      <div>
        <label :for="`${kennung}-plz`" class="etikett">{{ t('ort.plz') }}</label>
        <input
          :id="`${kennung}-plz`"
          v-model="plz"
          class="feld"
          inputmode="numeric"
          maxlength="4"
          pattern="[0-9]{4}"
          :title="t('ort.plzTitel')"
          autocomplete="postal-code"
          :required="pflicht"
          @input="plzEingegeben"
        />
      </div>
      <div>
        <label :for="`${kennung}-ort`" class="etikett">{{ t('ort.ort') }}</label>
        <input
          :id="`${kennung}-ort`"
          v-model="ort"
          class="feld"
          maxlength="120"
          :list="`${kennung}-ortschaften`"
          autocomplete="address-level2"
          :required="pflicht"
          @focus="ortFokussiert"
        />
        <datalist :id="`${kennung}-ortschaften`">
          <option v-for="eintrag in ortschaften" :key="eintrag.ort" :value="eintrag.ort" />
        </datalist>
      </div>
    </div>

    <div class="flex flex-wrap items-center gap-3">
      <button type="button" class="knopf-leise" :disabled="laeuft" @click="standortVerwenden">
        {{ laeuft ? t('ort.standortLaeuft') : t('ort.standortVerwenden') }}
      </button>
      <span v-if="meldung" class="text-sm text-grau" role="status">{{ meldung }}</span>
    </div>
    <p class="text-xs text-grau">{{ t('ort.standortHinweis') }}</p>

    <p v-if="mehrereGemeinden" class="text-sm text-grau" data-test="plz-mehrere-gemeinden">
      {{ t('ort.mehrereGemeinden') }}
    </p>

    <HinweisFeld v-if="fehler" art="fehler" :meldung="fehler" />
  </div>
</template>
