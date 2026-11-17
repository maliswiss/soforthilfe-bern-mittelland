<script setup>
/// Sprachverwaltung für den Plattformbetrieb: importierte Sprachen ansehen, ein- und ausschalten, löschen und
/// neue Sprachdateien importieren. Änderungen gelten sofort, ohne neues Deployment.
import { computed, onMounted, ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { importierteSprachenLaden, kultur } from '../i18n';
import { holen, senden } from '../api/klient';
import { sprachdateiPruefen } from '../api/sprachpruefung';
import { useKonto } from '../stores/konto';
import HinweisFeld from '../components/HinweisFeld.vue';
import deutsch from '../sprachen/de.json';

const konto = useKonto();
const { t } = useI18n();

const liste = ref([]);
const geladen = ref(false);
const meldung = ref('');
const fehler = ref('');
const loeschenFrage = ref(null);

const code = ref('');
const name = ref('');
const aktiv = ref(true);
const dateiname = ref('');
const pruefung = ref(null);
const feldfehler = ref({});
const sendet = ref(false);
const dateifeld = ref(null);

const zeigeListe = (eintraege, anzahl = 8) =>
  eintraege.slice(0, anzahl).join(', ') + (eintraege.length > anzahl ? ' …' : '');

function zeit(wert) {
  return new Date(wert).toLocaleString(kultur(), { dateStyle: 'medium', timeStyle: 'short' });
}

async function laden() {
  try {
    liste.value = await holen('/sprachen/verwaltung', konto.token);
  } catch (problem) {
    fehler.value = problem.message;
  } finally {
    geladen.value = true;
  }
}

onMounted(laden);

// Nach jeder Änderung Liste und Umschalter neu laden, damit die Wirkung gleich sichtbar ist
async function nachAenderung(text) {
  meldung.value = text;
  await laden();
  await importierteSprachenLaden({ neu: true });
}

function zuruecksetzen() {
  meldung.value = '';
  fehler.value = '';
  feldfehler.value = {};
}

async function dateiGewaehlt(ereignis) {
  zuruecksetzen();
  const datei = ereignis.target.files?.[0];
  dateiname.value = datei?.name ?? '';
  pruefung.value = datei ? sprachdateiPruefen(await datei.text(), deutsch) : null;
}

const pruefungsfehler = computed(() =>
  pruefung.value?.fehler ? t(`sprachverwaltung.fehler.${pruefung.value.fehler}`, { pfad: pruefung.value.pfad ?? '' }) : ''
);

function vorlageHerunterladen() {
  const inhalt = new Blob([JSON.stringify(deutsch, null, 2)], { type: 'application/json' });
  const verweis = document.createElement('a');
  verweis.href = URL.createObjectURL(inhalt);
  verweis.download = 'sprachvorlage-de.json';
  verweis.click();
  URL.revokeObjectURL(verweis.href);
}

// Vorhandene Sprache ersetzen: Code und Name übernehmen, danach die Datei wählen
function ersetzen(eintrag) {
  zuruecksetzen();
  code.value = eintrag.code;
  name.value = eintrag.name;
  aktiv.value = eintrag.aktiv;
  dateifeld.value?.focus();
}

async function importieren() {
  zuruecksetzen();
  const wert = code.value.trim().toLowerCase();
  const fehlerJeFeld = {};
  if (!/^[a-z]{2}$/.test(wert)) {
    fehlerJeFeld.code = t('sprachverwaltung.codeFehler');
  }
  if (name.value.trim().length < 2 || name.value.trim().length > 40) {
    fehlerJeFeld.name = t('sprachverwaltung.nameFehler');
  }
  if (!pruefung.value) {
    fehlerJeFeld.datei = t('sprachverwaltung.dateiFehlt');
  } else if (pruefung.value.fehler) {
    fehlerJeFeld.datei = pruefungsfehler.value;
  }
  if (Object.keys(fehlerJeFeld).length) {
    feldfehler.value = fehlerJeFeld;
    return;
  }

  sendet.value = true;
  try {
    const ergebnis = await senden(
      `/sprachen/${wert}`,
      { name: name.value.trim(), aktiv: aktiv.value, texte: pruefung.value.texte },
      konto.token,
      'PUT'
    );
    code.value = '';
    name.value = '';
    aktiv.value = true;
    pruefung.value = null;
    dateiname.value = '';
    if (dateifeld.value) {
      dateifeld.value.value = '';
    }
    await nachAenderung(t('sprachverwaltung.importiert', { name: ergebnis.name, anzahl: ergebnis.anzahlTexte }));
  } catch (problem) {
    if (problem.feld) {
      feldfehler.value = { [problem.feld === 'texte' ? 'datei' : problem.feld]: problem.message };
    } else {
      fehler.value = problem.message;
    }
  } finally {
    sendet.value = false;
  }
}

async function schalten(eintrag) {
  zuruecksetzen();
  try {
    const neu = await senden(`/sprachen/${eintrag.code}/aktiv`, { aktiv: !eintrag.aktiv }, konto.token, 'PUT');
    await nachAenderung(t(neu.aktiv ? 'sprachverwaltung.eingeschaltet' : 'sprachverwaltung.ausgeschaltet', { name: neu.name }));
  } catch (problem) {
    fehler.value = problem.message;
  }
}

async function loeschen(eintrag) {
  zuruecksetzen();
  loeschenFrage.value = null;
  try {
    await senden(`/sprachen/${eintrag.code}`, undefined, konto.token, 'DELETE');
    await nachAenderung(t('sprachverwaltung.geloescht', { name: eintrag.name }));
  } catch (problem) {
    fehler.value = problem.message;
  }
}
</script>

<template>
  <h1 class="text-3xl">{{ t('sprachverwaltung.titel') }}</h1>
  <p class="mt-2 max-w-3xl text-grau">{{ t('sprachverwaltung.einleitung') }}</p>

  <HinweisFeld v-if="meldung" :meldung="meldung" class="mt-4" />
  <HinweisFeld v-if="fehler" art="fehler" :meldung="fehler" class="mt-4" />

  <section class="mt-6" aria-labelledby="liste-titel">
    <h2 id="liste-titel" class="text-2xl">{{ t('sprachverwaltung.importierte') }}</h2>

    <p v-if="!geladen" class="mt-3 text-grau">{{ t('allgemein.laden') }}</p>
    <p v-else-if="liste.length === 0" class="karte mt-3 text-grau">{{ t('sprachverwaltung.keine') }}</p>

    <div v-else class="mt-3 overflow-x-auto">
      <table class="w-full border-collapse text-sm">
        <thead>
          <tr class="border-b border-kiesel text-left text-grau">
            <th class="py-2 pr-4 font-medium">{{ t('sprachverwaltung.code') }}</th>
            <th class="py-2 pr-4 font-medium">{{ t('sprachverwaltung.name') }}</th>
            <th class="py-2 pr-4 font-medium">{{ t('sprachverwaltung.texte') }}</th>
            <th class="py-2 pr-4 font-medium">{{ t('sprachverwaltung.stand') }}</th>
            <th class="py-2 pr-4 font-medium">{{ t('sprachverwaltung.geaendert') }}</th>
            <th class="py-2 font-medium"><span class="sr-only">{{ t('sprachverwaltung.aktionen') }}</span></th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="eintrag in liste" :key="eintrag.code" class="border-b border-kiesel" data-test="sprache">
            <td class="py-2 pr-4 font-semibold uppercase">{{ eintrag.code }}</td>
            <td class="py-2 pr-4" :lang="eintrag.code">{{ eintrag.name }}</td>
            <td class="py-2 pr-4">{{ eintrag.anzahlTexte }}</td>
            <td class="py-2 pr-4">
              {{ eintrag.aktiv ? t('sprachverwaltung.aktiv') : t('sprachverwaltung.inaktiv') }}
            </td>
            <td class="py-2 pr-4 text-grau">{{ zeit(eintrag.geaendertAm) }}</td>
            <td class="py-2">
              <div class="flex flex-wrap gap-2">
                <button type="button" class="knopf-leise" @click="schalten(eintrag)">
                  {{ eintrag.aktiv ? t('sprachverwaltung.ausschalten') : t('sprachverwaltung.einschalten') }}
                </button>
                <button type="button" class="knopf-leise" @click="ersetzen(eintrag)">
                  {{ t('sprachverwaltung.ersetzen') }}
                </button>
                <template v-if="loeschenFrage === eintrag.code">
                  <button type="button" class="knopf" @click="loeschen(eintrag)">
                    {{ t('sprachverwaltung.loeschenBestaetigen') }}
                  </button>
                  <button type="button" class="knopf-leise" @click="loeschenFrage = null">
                    {{ t('sprachverwaltung.abbrechen') }}
                  </button>
                </template>
                <button v-else type="button" class="knopf-leise" @click="loeschenFrage = eintrag.code">
                  {{ t('sprachverwaltung.loeschen') }}
                </button>
              </div>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </section>

  <section class="karte mt-8 max-w-3xl" aria-labelledby="import-titel">
    <h2 id="import-titel" class="text-2xl">{{ t('sprachverwaltung.importTitel') }}</h2>
    <p class="mt-1 text-sm text-grau">{{ t('sprachverwaltung.importHinweis') }}</p>
    <button type="button" class="mt-2 text-sm underline underline-offset-2" @click="vorlageHerunterladen">
      {{ t('sprachverwaltung.vorlage') }}
    </button>

    <form class="mt-4 grid gap-4 sm:grid-cols-2" novalidate @submit.prevent="importieren">
      <div>
        <label for="sprache-code" class="etikett">{{ t('sprachverwaltung.code') }}</label>
        <input
          id="sprache-code"
          v-model="code"
          class="feld"
          maxlength="2"
          autocomplete="off"
          :aria-invalid="Boolean(feldfehler.code)"
          aria-describedby="sprache-code-hinweis"
        />
        <p id="sprache-code-hinweis" class="mt-1 text-xs" :class="feldfehler.code ? 'text-notruf' : 'text-grau'">
          {{ feldfehler.code || t('sprachverwaltung.codeHinweis') }}
        </p>
      </div>

      <div>
        <label for="sprache-name" class="etikett">{{ t('sprachverwaltung.name') }}</label>
        <input
          id="sprache-name"
          v-model="name"
          class="feld"
          maxlength="40"
          autocomplete="off"
          :aria-invalid="Boolean(feldfehler.name)"
          aria-describedby="sprache-name-hinweis"
        />
        <p id="sprache-name-hinweis" class="mt-1 text-xs" :class="feldfehler.name ? 'text-notruf' : 'text-grau'">
          {{ feldfehler.name || t('sprachverwaltung.nameHinweis') }}
        </p>
      </div>

      <div class="sm:col-span-2">
        <label for="sprache-datei" class="etikett">{{ t('sprachverwaltung.datei') }}</label>
        <input
          id="sprache-datei"
          ref="dateifeld"
          type="file"
          accept=".json,application/json"
          class="feld"
          :aria-invalid="Boolean(feldfehler.datei)"
          aria-describedby="sprache-datei-hinweis"
          @change="dateiGewaehlt"
        />
        <div id="sprache-datei-hinweis" class="mt-1 space-y-1 text-xs" data-test="pruefung">
          <p v-if="feldfehler.datei" class="text-notruf">{{ feldfehler.datei }}</p>
          <p v-else-if="pruefungsfehler" class="text-notruf">{{ pruefungsfehler }}</p>
          <template v-else-if="pruefung">
            <p>{{ t('sprachverwaltung.geprueft', { anzahl: pruefung.anzahl, datei: dateiname }) }}</p>
            <p v-if="pruefung.fehlend.length === 0">{{ t('sprachverwaltung.vollstaendig') }}</p>
            <p v-else class="text-grau">
              {{ t('sprachverwaltung.fehlend', { anzahl: pruefung.fehlend.length, liste: zeigeListe(pruefung.fehlend) }) }}
            </p>
            <p v-if="pruefung.unbekannt.length" class="text-notruf">
              {{ t('sprachverwaltung.unbekannt', { anzahl: pruefung.unbekannt.length, liste: zeigeListe(pruefung.unbekannt) }) }}
            </p>
            <p v-if="pruefung.andererPlatzhalter.length" class="text-notruf">
              {{ t('sprachverwaltung.platzhalter', { liste: zeigeListe(pruefung.andererPlatzhalter) }) }}
            </p>
          </template>
          <p v-else class="text-grau">{{ t('sprachverwaltung.dateiHinweis') }}</p>
        </div>
      </div>

      <label class="flex items-center gap-2 text-sm sm:col-span-2">
        <input v-model="aktiv" type="checkbox" />
        {{ t('sprachverwaltung.sofortAktiv') }}
      </label>

      <div class="sm:col-span-2">
        <button type="submit" class="knopf" :disabled="sendet">
          {{ sendet ? t('sprachverwaltung.importiertGerade') : t('sprachverwaltung.importieren') }}
        </button>
      </div>
    </form>
  </section>
</template>
