<script setup>
import { onMounted, ref } from 'vue';
import { useRouter } from 'vue-router';
import { useI18n } from 'vue-i18n';
import { senden } from '../api/klient';
import { useKonto } from '../stores/konto';
import { useAnfrageEntwurf } from '../stores/anfrage';
import HinweisFeld from '../components/HinweisFeld.vue';
import OrtEingabe from '../components/OrtEingabe.vue';

const router = useRouter();
const { t } = useI18n();
const konto = useKonto();
const entwurf = useAnfrageEntwurf();

const formular = ref({
  plz: entwurf.plz,
  ort: entwurf.ort,
  adresse: '',
  zeitfenster: ''
});

const fehler = ref('');
const laeuft = ref(false);

// Ohne Kategorie und ohne Beschreibung beginnt die Anfrage auf der Startseite
onMounted(() => {
  if (!entwurf.vorhanden) {
    router.replace({ name: 'startseite' });
  }
});

async function absenden() {
  fehler.value = '';

  if (!/^\d{4}$/.test(formular.value.plz.trim())) {
    fehler.value = t('erfassen.fehlerPlz');
    return;
  }

  if (!formular.value.ort.trim()) {
    fehler.value = t('erfassen.fehlerOrt');
    return;
  }

  laeuft.value = true;

  try {
    const anfrage = await senden(
      '/anfragen',
      {
        kategorieId: entwurf.kategorieId,
        freitext: entwurf.freitext || null,
        plz: formular.value.plz.trim(),
        ort: formular.value.ort.trim(),
        adresse: formular.value.adresse.trim() || null,
        zeitfenster: formular.value.zeitfenster.trim() || null,
        prioritaet: entwurf.prioritaet
      },
      konto.token
    );

    entwurf.leeren();
    router.push({ name: 'anfrage', params: { id: anfrage.id } });
  } catch (problem) {
    // Gesundheitlicher Notfall: nicht vermitteln, sondern auf 144 verweisen
    if (problem.status === 451) {
      entwurf.leeren();
      entwurf.notfallGrund = problem.daten?.grund ?? null;
      router.push({ name: 'notfall' });
      return;
    }
    fehler.value = problem.message;
  } finally {
    laeuft.value = false;
  }
}
</script>

<template>
  <h1 class="text-3xl">{{ t('erfassen.titel') }}</h1>
  <p class="mt-2 max-w-lesen text-grau">{{ t('erfassen.einleitung') }}</p>

  <form class="karte mt-6 max-w-2xl space-y-4" novalidate @submit.prevent="absenden">
    <p v-if="entwurf.kategorieBezeichnung" class="border-l-4 border-tanne bg-tanne-blass px-4 py-3 text-sm">
      {{ t('erfassen.anliegen', { kategorie: entwurf.kategorieBezeichnung }) }}
    </p>
    <div v-if="entwurf.freitext" class="border-l-4 border-tanne bg-tanne-blass px-4 py-3 text-sm">
      <p class="font-medium">{{ t('erfassen.anliegenFreitext') }}</p>
      <p class="mt-1">{{ entwurf.freitext }}</p>
    </div>

    <OrtEingabe v-model:plz="formular.plz" v-model:ort="formular.ort" kennung="anfrage" />

    <div>
      <label for="adresse" class="etikett">{{ t('erfassen.adresse') }}</label>
      <input id="adresse" v-model="formular.adresse" class="feld" maxlength="250" autocomplete="street-address" />
    </div>

    <div>
      <label for="zeitfenster" class="etikett">{{ t('erfassen.zeitfenster') }}</label>
      <input
        id="zeitfenster"
        v-model="formular.zeitfenster"
        class="feld"
        maxlength="120"
        :placeholder="t('erfassen.zeitfensterBeispiel')"
      />
    </div>

    <HinweisFeld v-if="fehler" art="fehler" :meldung="fehler" />

    <button type="submit" class="knopf w-full" :disabled="laeuft">
      {{ laeuft ? t('erfassen.sendet') : t('erfassen.senden') }}
    </button>
  </form>
</template>
