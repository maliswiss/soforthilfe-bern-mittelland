<script setup>
/// Preisangaben einer helfenden Person oder eines Betriebs (Anforderung A9), vor der Zusage sichtbar.
/// Der Richtpreis gilt für die erste Stunde inklusive Anfahrt; nachts oder am Wochenende
/// mit dem höheren der beiden Zuschläge, bei MWST-Pflicht inklusive MWST.
import { computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { betrag, franken, zahl } from '../api/betrag';

const props = defineProps({
  preis: { type: Object, default: null },
  kompakt: { type: Boolean, default: false }
});

const { t } = useI18n();

const zusatz = computed(() => {
  const p = props.preis;
  if (!p) {
    return '';
  }
  const zuschlag =
    p.angewandterZuschlag === 'nacht'
      ? t('preis.mitNachtzuschlag', { prozent: p.angewandterZuschlagProzent })
      : p.angewandterZuschlag === 'wochenende'
        ? t('preis.mitWochenendzuschlag', { prozent: p.angewandterZuschlagProzent })
        : '';
  return t('preis.richtpreisHinweis', { zuschlag, mwst: p.mwstPflichtig ? t('preis.mitMwst') : '' });
});
</script>

<template>
  <div v-if="preis" class="text-sm">
    <p v-if="preis.richtpreisErsteStunde !== null" class="font-medium text-tinte">
      {{ t('preis.richtpreisZeile', { betrag: t('preis.richtpreisWert', { betrag: betrag(preis.richtpreisErsteStunde) }) }) }}
      <span class="font-normal text-grau">({{ zusatz }})</span>
    </p>
    <p v-else class="text-grau">{{ t('preis.keineAngaben') }}</p>

    <dl v-if="!kompakt && preis.richtpreisErsteStunde !== null" class="mt-1 grid grid-cols-[auto_1fr] gap-x-3 text-grau">
      <dt>{{ t('preis.anfahrt') }}</dt>
      <dd>{{ franken(preis.anfahrtChf) }}</dd>
      <dt>{{ t('preis.stundenansatz') }}</dt>
      <dd>{{ t('preis.proStunde', { betrag: betrag(preis.stundenansatzChf) }) }}</dd>
      <dt>{{ t('preis.nachtzuschlag') }}</dt>
      <dd>{{ preis.nachtzuschlagProzent ? t('preis.prozent', { prozent: preis.nachtzuschlagProzent }) : t('preis.zuschlagKeiner') }}</dd>
      <dt>{{ t('preis.wochenendzuschlag') }}</dt>
      <dd>{{ preis.wochenendzuschlagProzent ? t('preis.prozent', { prozent: preis.wochenendzuschlagProzent }) : t('preis.zuschlagKeiner') }}</dd>
    </dl>
    <p v-if="!kompakt && preis.richtpreisErsteStunde !== null" class="mt-1 text-xs text-grau">
      {{ preis.mwstPflichtig ? t('preis.mwstZzgl', { satz: zahl(preis.mwstSatzProzent) }) : t('preis.mwstKeine') }}
    </p>
  </div>
  <p v-else class="text-sm text-grau">{{ t('preis.kostenlosHinweis') }}</p>
</template>
