<script setup>
import { onMounted, ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { kultur } from '../i18n';
import { holen, senden } from '../api/klient';
import { useKonto } from '../stores/konto';
import HinweisFeld from '../components/HinweisFeld.vue';

const konto = useKonto();
const { t } = useI18n();
const offene = ref([]);
const protokoll = ref([]);
const bemerkungen = ref({});
const geladen = ref(false);
const meldung = ref('');
const fehler = ref('');

function zeit(wert) {
  return new Date(wert).toLocaleString(kultur(), { dateStyle: 'medium', timeStyle: 'short' });
}

// Offene Nachweise und Protokoll der eigenen Region; die Region ergibt sich aus der Anmeldung
async function laden() {
  try {
    offene.value = await holen('/verifizierungen/offen', konto.token);
    protokoll.value = await holen('/protokoll?anzahl=25', konto.token);
  } catch (problem) {
    fehler.value = problem.message;
  } finally {
    geladen.value = true;
  }
}

onMounted(laden);

async function entscheiden(id, freigeben) {
  fehler.value = '';
  meldung.value = '';
  try {
    await senden(
      `/verifizierungen/${id}/entscheid`,
      { freigeben, bemerkung: bemerkungen.value[id]?.trim() || null },
      konto.token
    );
    meldung.value = freigeben ? t('koordination.freigegeben') : t('koordination.abgelehnt');
    delete bemerkungen.value[id];
    await laden();
  } catch (problem) {
    fehler.value = problem.message;
  }
}
</script>

<template>
  <h1 class="text-3xl">{{ t('koordination.titel', { region: konto.regionName }) }}</h1>

  <HinweisFeld v-if="meldung" :meldung="meldung" class="mt-4" />
  <HinweisFeld v-if="fehler" art="fehler" :meldung="fehler" class="mt-4" />

  <section class="mt-6" aria-labelledby="offen-titel">
    <h2 id="offen-titel" class="text-2xl">{{ t('koordination.offen') }}</h2>

    <p v-if="!geladen" class="mt-3 text-grau">{{ t('allgemein.laden') }}</p>
    <p v-else-if="offene.length === 0" class="karte mt-3 text-grau">{{ t('koordination.nichtsZuPruefen') }}</p>

    <ul class="mt-3 space-y-3">
      <li v-for="eintrag in offene" :key="eintrag.id" class="karte" data-test="pruefung">
        <h3 class="text-xl">
          {{ eintrag.anzeigename }}<template v-if="eintrag.betriebsname"> · {{ eintrag.betriebsname }}</template>
        </h3>
        <p class="mt-1 text-sm text-grau">
          {{ t('koordination.profil', {
            typ: eintrag.typ === 1 ? t('profil.betrieb') : t('profil.privatperson'),
            plz: eintrag.plz,
            ort: eintrag.ort,
            stand: t(`verifizierung.${eintrag.profilstatus}`)
          }) }}
        </p>
        <p class="mt-1 text-sm">
          {{ t(`profil.nachweis.${eintrag.art}`) }} · {{ eintrag.nachweiswert || t('koordination.ohneAngabe') }} ·
          <span class="text-grau">{{ t('koordination.eingereicht', { zeit: zeit(eintrag.eingereichtAm) }) }}</span>
        </p>

        <label :for="`bemerkung-${eintrag.id}`" class="etikett mt-3">{{ t('koordination.bemerkung') }}</label>
        <input :id="`bemerkung-${eintrag.id}`" v-model="bemerkungen[eintrag.id]" class="feld" maxlength="500" />

        <div class="mt-4 flex flex-wrap gap-3">
          <button type="button" class="knopf" @click="entscheiden(eintrag.id, true)">{{ t('koordination.freigeben') }}</button>
          <button type="button" class="knopf-leise" @click="entscheiden(eintrag.id, false)">{{ t('koordination.ablehnen') }}</button>
        </div>
      </li>
    </ul>
  </section>

  <section class="mt-8" aria-labelledby="protokoll-titel">
    <h2 id="protokoll-titel" class="text-2xl">{{ t('koordination.protokoll') }}</h2>
    <p class="mt-1 text-xs text-grau">{{ t('koordination.protokollHinweis') }}</p>
    <div class="mt-3 overflow-x-auto">
      <table class="w-full border-collapse text-sm">
        <thead>
          <tr class="border-b border-kiesel text-left text-grau">
            <th class="py-2 pr-4 font-medium">{{ t('koordination.zeitpunkt') }}</th>
            <th class="py-2 pr-4 font-medium">{{ t('koordination.aktion') }}</th>
            <th class="py-2 font-medium">{{ t('koordination.detail') }}</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="eintrag in protokoll" :key="eintrag.id" class="border-b border-kiesel">
            <td class="py-2 pr-4 text-grau">{{ zeit(eintrag.zeitpunkt) }}</td>
            <td class="py-2 pr-4">{{ eintrag.aktion }}</td>
            <td class="py-2 text-grau">{{ eintrag.detail || '' }}</td>
          </tr>
        </tbody>
      </table>
    </div>
  </section>
</template>
