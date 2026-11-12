<script setup>
import { computed, ref } from 'vue';
import { useRoute, useRouter, RouterLink } from 'vue-router';
import { useI18n } from 'vue-i18n';
import { useKonto } from '../stores/konto';
import { istKostenpflichtig } from '../api/telefon';
import { internesZiel } from '../api/ziel';
import { regionZuPlz } from '../api/orte';
import HinweisFeld from '../components/HinweisFeld.vue';
import Warnhinweis from '../components/Warnhinweis.vue';
import OrtEingabe from '../components/OrtEingabe.vue';

const konto = useKonto();
const route = useRoute();
const router = useRouter();
const { t } = useI18n();

/// 0 = Hilfe suchen, 1 = Hilfe anbieten (entspricht der Kontoart der Schnittstelle)
const formular = ref({
  anzeigename: '',
  eMail: '',
  telefon: '',
  passwort: '',
  kontoart: route.query.konto === 'helfende' ? 1 : 0
});

// Die Postleitzahl bestimmt die Region des Kontos; gespeichert wird sie dabei nicht.
const wohnort = ref({ plz: '', ort: '' });

// Anforderung A10: kostenpflichtige Mehrwertnummern werden schon bei der Eingabe angezeigt
const telefonKostenpflichtig = computed(() => istKostenpflichtig(formular.value.telefon));
const fehler = ref('');
const laeuft = ref(false);

async function registrieren() {
  fehler.value = '';

  if (telefonKostenpflichtig.value) {
    document.getElementById('telefon')?.focus();
    return;
  }

  const plz = wohnort.value.plz.trim();
  if (!/^[1-9]\d{3}$/.test(plz)) {
    fehler.value = t('erfassen.fehlerPlz');
    return;
  }

  laeuft.value = true;
  try {
    const { bekannt, region } = await regionZuPlz(plz, wohnort.value.ort);
    if (!bekannt) {
      fehler.value = t('registrierung.plzUnbekannt', { plz });
      return;
    }
    if (!region) {
      fehler.value = t('registrierung.keineRegion', { plz });
      return;
    }
    konto.regionWaehlen(region);

    await konto.registrieren({
      ...formular.value,
      eMail: formular.value.eMail.trim(),
      telefon: formular.value.telefon.trim() || null
    });
    // Helfende richten als Nächstes ihr Profil ein; ohne Profil erhalten sie keine Anfragen
    const standardziel = formular.value.kontoart === 1 ? { name: 'helferprofil' } : { name: 'meine-anfragen' };
    router.push(internesZiel(route.query.weiter) ?? standardziel);
  } catch (problem) {
    fehler.value = problem.message;
  } finally {
    laeuft.value = false;
  }
}
</script>

<template>
  <div class="max-w-md">
    <h1 class="text-3xl">{{ t('registrierung.titel') }}</h1>
    <p class="mt-2 text-grau">{{ t('registrierung.einleitung') }}</p>

    <form class="karte mt-6 space-y-4" @submit.prevent="registrieren">
      <fieldset>
        <legend class="etikett">{{ t('registrierung.ichMoechte') }}</legend>
        <div class="grid gap-2 sm:grid-cols-2">
          <label
            class="flex cursor-pointer items-start gap-3 border px-3 py-3 text-sm"
            :class="formular.kontoart === 0 ? 'border-tanne bg-tanne-blass' : 'border-kiesel'"
          >
            <input v-model.number="formular.kontoart" type="radio" name="kontoart" :value="0" class="mt-1" />
            <span><span class="font-medium">{{ t('registrierung.hilfeSuchen') }}</span><br />{{ t('registrierung.hilfeSuchenText') }}</span>
          </label>
          <label
            class="flex cursor-pointer items-start gap-3 border px-3 py-3 text-sm"
            :class="formular.kontoart === 1 ? 'border-tanne bg-tanne-blass' : 'border-kiesel'"
          >
            <input v-model.number="formular.kontoart" type="radio" name="kontoart" :value="1" class="mt-1" />
            <span><span class="font-medium">{{ t('registrierung.hilfeAnbieten') }}</span><br />{{ t('registrierung.hilfeAnbietenText') }}</span>
          </label>
        </div>
      </fieldset>

      <div>
        <label for="name" class="etikett">{{ t('registrierung.name') }}</label>
        <input id="name" v-model="formular.anzeigename" class="feld" autocomplete="name" minlength="2" required />
      </div>

      <div>
        <label for="email" class="etikett">{{ t('anmeldung.email') }}</label>
        <input id="email" v-model="formular.eMail" type="email" class="feld" autocomplete="email" required />
      </div>

      <div>
        <label for="telefon" class="etikett">{{ t('registrierung.telefon') }}</label>
        <input
          id="telefon"
          v-model="formular.telefon"
          class="feld"
          :class="telefonKostenpflichtig ? 'border-notruf focus:border-notruf' : ''"
          type="tel"
          autocomplete="tel"
          :aria-invalid="telefonKostenpflichtig"
          :aria-describedby="telefonKostenpflichtig ? 'telefon-hinweis' : undefined"
        />
        <Warnhinweis v-if="telefonKostenpflichtig" kennung="telefon-hinweis" :meldung="t('telefon.kostenpflichtig')" />
        <p v-else class="mt-1 text-xs text-grau">{{ t('registrierung.telefonHinweis') }}</p>
      </div>

      <div>
        <OrtEingabe v-model:plz="wohnort.plz" v-model:ort="wohnort.ort" kennung="wohnort" />
        <p class="mt-1 text-xs text-grau">{{ t('registrierung.regionHinweis') }}</p>
      </div>

      <div>
        <label for="passwort" class="etikett">{{ t('registrierung.passwort') }}</label>
        <input
          id="passwort"
          v-model="formular.passwort"
          type="password"
          class="feld"
          autocomplete="new-password"
          minlength="10"
          required
        />
      </div>

      <HinweisFeld v-if="fehler" art="fehler" :meldung="fehler" />

      <button type="submit" class="knopf w-full" :disabled="laeuft">
        {{ laeuft ? t('registrierung.erstellen') : t('registrierung.titel') }}
      </button>
    </form>

    <p class="mt-4 text-sm text-grau">
      {{ t('registrierung.bereitsRegistriert') }}
      <RouterLink :to="{ name: 'anmeldung', query: route.query }" class="underline underline-offset-2">
        {{ t('kopf.anmelden') }}
      </RouterLink>
    </p>
  </div>
</template>
