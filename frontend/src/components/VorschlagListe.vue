<script setup>
/// Vorschläge des Smart-Matchings zu einer Anfrage: Rang, Name, Begründung, Entfernung, Verfügbarkeit, Punktzahl und
/// bei kostenpflichtigen Kategorien die Preisangaben (Anforderung A9). Wie die Reihenfolge zustande kommt, zeigen die
/// Teilpunkte je Kriterium und die Punktzahl, zugeklappt. Die Telefonnummer gibt es erst nach der Zusage.
import { useI18n } from 'vue-i18n';
import PreisAngaben from './PreisAngaben.vue';
import { zahl, zahl2 } from '../api/betrag';

defineProps({
  vorschlaege: { type: Array, required: true }
});

const { t } = useI18n();

function abstand(meter) {
  if (meter <= 0) {
    return t('anfrage.gleichePlz');
  }
  return meter < 1000
    ? t('anfrage.entfernungMeter', { meter: zahl(meter) })
    : t('anfrage.entfernungKm', { km: zahl(Math.round(meter / 100) / 10) });
}
</script>

<template>
  <section class="mt-8" aria-labelledby="vorschlaege-titel">
    <h2 id="vorschlaege-titel" class="text-2xl">{{ t('anfrage.vorschlaege') }}</h2>

    <p v-if="vorschlaege.length === 0" class="karte mt-4 text-grau">{{ t('anfrage.keineVorschlaege') }}</p>

    <ol v-else class="mt-4 space-y-3">
      <li v-for="vorschlag in vorschlaege" :key="vorschlag.id" class="karte" data-test="vorschlag">
        <div class="flex flex-wrap items-baseline justify-between gap-3">
          <h3 class="text-xl">{{ vorschlag.rang }}. {{ vorschlag.betriebsname ?? vorschlag.anzeigename }}</h3>
          <span class="text-sm text-grau">
            {{ vorschlag.typ === 1 ? t('vorschau.betrieb') : t('vorschau.privatperson') }} ·
            {{ t('vorschau.punktzahl', { punkte: zahl2(vorschlag.punktzahl) }) }}
          </span>
        </div>

        <p class="mt-2 text-grau">{{ vorschlag.begruendung }}</p>

        <p class="mt-2 text-sm text-grau">
          {{ abstand(vorschlag.distanzMeter) }} ·
          {{ vorschlag.jetztVerfuegbar ? t('anfrage.verfuegbar') : t('anfrage.nichtVerfuegbar') }}
        </p>

        <p v-if="vorschlag.partnerBevorzugt" class="mt-2 text-sm">{{ t('anfrage.partner') }}</p>

        <!-- Preisangaben schon vor der Zusage -->
        <PreisAngaben v-if="vorschlag.preis" :preis="vorschlag.preis" class="mt-3" />

        <p class="mt-4 border-t border-kiesel pt-4 text-sm text-grau">{{ t('anfrage.telefonNachZusage') }}</p>

        <!-- Zugeklappt; der Pfeil dreht sich beim Aufklappen -->
        <details class="group mt-3 text-sm">
          <summary class="flex cursor-pointer list-none items-center gap-1 text-tanne [&::-webkit-details-marker]:hidden">
            {{ t('anfrage.reihenfolge') }}
            <svg class="h-4 w-4 transition-transform group-open:rotate-180" viewBox="0 0 20 20" fill="currentColor" aria-hidden="true">
              <path d="M5.3 7.3a1 1 0 0 1 1.4 0L10 10.6l3.3-3.3a1 1 0 1 1 1.4 1.4l-4 4a1 1 0 0 1-1.4 0l-4-4a1 1 0 0 1 0-1.4Z" />
            </svg>
          </summary>
          <dl class="mt-2 max-w-xs divide-y divide-kiesel border-y border-kiesel" data-test="kriterien">
            <div v-for="k in vorschlag.kriterien" :key="k.kriterium" class="flex justify-between py-1">
              <dt class="text-grau">{{ t(`anfrage.teil.${k.kriterium}`) }}</dt>
              <dd>{{ zahl2(k.teilpunkte) }}</dd>
            </div>
            <div class="flex justify-between py-1 font-medium">
              <dt>{{ t('anfrage.summe') }}</dt>
              <dd>{{ zahl2(vorschlag.punktzahl) }}</dd>
            </div>
          </dl>
        </details>
      </li>
    </ol>
  </section>
</template>
