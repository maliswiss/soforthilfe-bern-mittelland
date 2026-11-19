<script setup>
/// Unverbindliche Vorschau ohne Anmeldung: wer in der Nähe helfen könnte, mit Begründung, Punktzahl und Preisangaben.
/// Gestaffelte Preisgabe: ohne Anmeldung keine Namen (die Schnittstelle liefert keine), angemeldet mit Namen;
/// Telefonnummern erst nach der Zusage auf eine Anfrage.
import { computed, onMounted, ref } from 'vue';
import { useRouter, RouterLink } from 'vue-router';
import { useI18n } from 'vue-i18n';
import { senden } from '../api/klient';
import { useKonto } from '../stores/konto';
import { useAnfrageEntwurf } from '../stores/anfrage';
import HinweisFeld from '../components/HinweisFeld.vue';
import OrtEingabe from '../components/OrtEingabe.vue';
import PreisAngaben from '../components/PreisAngaben.vue';
import { kategorieName } from '../api/datentexte';
import { zahl2 } from '../api/betrag';

const router = useRouter();
const { t } = useI18n();
const konto = useKonto();
const entwurf = useAnfrageEntwurf();

// Gewählte oder aus dem Freitext erkannte Kategorie
const kategorie = computed(() =>
  entwurf.kategorieId
    ? { id: entwurf.kategorieId, code: entwurf.kategorieCode, bezeichnung: entwurf.kategorieBezeichnung }
    : entwurf.einordnung?.kategorieId
      ? { id: entwurf.einordnung.kategorieId, code: entwurf.einordnung.code, bezeichnung: entwurf.einordnung.kategorie }
      : null
);

// Überschrift der Ergebnisse, zum Beispiel «Schlüsseldienst in der Nähe von Bern»
const ergebnisTitel = computed(() => {
  const name = kategorieName(ergebnis.value?.code, ergebnis.value?.kategorie);
  return ergebnis.value?.ort ? `${name} ${t('vorschau.inDerNaehe', { ort: ergebnis.value.ort })}` : name;
});

const plz = ref(entwurf.plz);
const ort = ref(entwurf.ort);
const ergebnis = ref(null);
const laeuft = ref(false);
const fehler = ref('');

onMounted(() => {
  if (!kategorie.value) {
    router.replace({ name: 'startseite' });
    return;
  }

  // Nach Anmeldung oder Registrierung gleich erneut suchen, damit die Namen sichtbar werden
  if (/^\d{4}$/.test(plz.value.trim())) {
    suchen();
  }
});

async function suchen() {
  fehler.value = '';

  if (!/^\d{4}$/.test(plz.value.trim())) {
    fehler.value = t('vorschau.fehlerPlz');
    return;
  }

  laeuft.value = true;
  try {
    ergebnis.value = await senden(
      '/anfragen/vorschau',
      {
        kategorieId: kategorie.value.id,
        plz: plz.value.trim(),
        ort: ort.value.trim() || null,
        prioritaet: entwurf.prioritaet
      },
      konto.token
    );

    // Angaben für das spätere Erfassen der Anfrage übernehmen
    entwurf.plz = plz.value.trim();
    entwurf.ort = ort.value.trim() || ergebnis.value.ort || '';
  } catch (problem) {
    ergebnis.value = null;
    fehler.value = problem.message;
  } finally {
    laeuft.value = false;
  }
}
</script>

<template>
  <h1 class="text-3xl">{{ t('vorschau.titel') }}</h1>
  <p class="mt-2 max-w-lesen text-grau">{{ t('vorschau.einleitung') }}</p>

  <form v-if="kategorie" class="karte mt-6 max-w-2xl space-y-4" novalidate @submit.prevent="suchen">
    <p class="border-l-4 border-tanne bg-tanne-blass px-4 py-3 text-sm">
      {{ t('vorschau.anliegen', { kategorie: kategorieName(kategorie.code, kategorie.bezeichnung) }) }}
    </p>

    <OrtEingabe v-model:plz="plz" v-model:ort="ort" kennung="vorschau" />

    <button type="submit" class="knopf w-full" :disabled="laeuft">
      {{ laeuft ? t('vorschau.suchen') : t('vorschau.suchenKnopf') }}
    </button>
  </form>

  <HinweisFeld v-if="fehler" art="fehler" :meldung="fehler" class="mt-4 max-w-2xl" />

  <section v-if="ergebnis" class="mt-8" aria-live="polite">
    <h2 class="text-2xl">{{ ergebnisTitel }}</h2>
    <p class="mt-1 text-sm text-grau">
      {{ t('vorschau.suchradius', { km: ergebnis.suchradiusKm }) }} ·
      {{ ergebnis.bezahlpflichtig ? t('vorschau.kostenpflichtig') : t('vorschau.kostenlos') }}
    </p>

    <p v-if="!ergebnis.eintraege.length" class="karte mt-4 text-grau">{{ t('vorschau.keineTreffer') }}</p>

    <template v-else>
      <p class="mt-3 text-sm text-grau">
        {{ ergebnis.angemeldet ? t('vorschau.hinweisAngemeldet') : t('vorschau.hinweisAnonym') }}
      </p>

      <ol class="mt-4 space-y-3">
        <li v-for="eintrag in ergebnis.eintraege" :key="eintrag.rang" class="karte" data-test="vorschau-treffer">
          <div class="flex flex-wrap items-baseline justify-between gap-3">
            <h3 class="text-xl">
              {{ eintrag.rang }}.
              <span v-if="eintrag.name">{{ eintrag.name }}</span>
              <template v-else>
                <!-- Platzhalter: die Schnittstelle liefert ohne Anmeldung keine Namen -->
                <span class="select-none blur-[6px]" aria-hidden="true">
                  {{ eintrag.istBetrieb ? t('vorschau.platzhalterBetrieb') : t('vorschau.platzhalterPerson') }}
                </span>
                <span class="sr-only">{{ t('vorschau.nameNachRegistrierung') }}</span>
              </template>
            </h3>
            <span class="text-sm text-grau">
              {{ eintrag.istBetrieb ? t('vorschau.betrieb') : t('vorschau.privatperson') }} ·
              {{ t('vorschau.punktzahl', { punkte: zahl2(eintrag.punktzahl) }) }}
            </span>
          </div>

          <p class="mt-2 text-grau">{{ eintrag.begruendung }}</p>

          <PreisAngaben v-if="ergebnis.bezahlpflichtig" :preis="eintrag.preis" class="mt-3" />

          <div class="mt-4 flex flex-wrap items-center gap-3 border-t border-kiesel pt-4">
            <span v-if="ergebnis.angemeldet" class="text-sm text-grau">{{ t('vorschau.telefonNachZusage') }}</span>

            <template v-else>
              <span class="select-none border border-kiesel bg-papier px-4 py-2 font-medium text-grau blur-[4px]" aria-hidden="true">
                031 000 00 00
              </span>
              <RouterLink :to="{ name: 'registrierung', query: { weiter: '/vorschau' } }" class="text-sm underline underline-offset-2">
                {{ t('vorschau.registrierenUmZuSehen') }}
              </RouterLink>
            </template>
          </div>
        </li>
      </ol>
    </template>

    <div class="karte mt-6 max-w-2xl">
      <h2 class="text-xl">{{ t('vorschau.weiterTitel') }}</h2>
      <p class="mt-2 text-grau">{{ t('vorschau.weiterText') }}</p>
      <div class="mt-4 flex flex-wrap gap-3">
        <!-- Massgebend ist die Antwort der Schnittstelle, auch bei abgelaufener Sitzung -->
        <template v-if="!ergebnis.angemeldet">
          <RouterLink :to="{ name: 'registrierung', query: { weiter: '/vorschau' } }" class="knopf">
            {{ t('vorschau.kontoErstellen') }}
          </RouterLink>
          <RouterLink :to="{ name: 'anmeldung', query: { weiter: '/vorschau' } }" class="knopf-leise">
            {{ t('kopf.anmelden') }}
          </RouterLink>
        </template>
        <RouterLink v-else :to="{ name: 'anfrage-erfassen' }" class="knopf">{{ t('vorschau.anfrageErfassen') }}</RouterLink>
      </div>
    </div>
  </section>
</template>
