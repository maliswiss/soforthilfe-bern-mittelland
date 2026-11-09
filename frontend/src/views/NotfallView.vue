<script setup>
/// Notfall-Stopp: Bei Hinweisen auf einen akuten gesundheitlichen Notfall vermittelt die Plattform nicht,
/// sondern verweist auf die Notrufnummer 144.
import { computed } from 'vue';
import { RouterLink } from 'vue-router';
import { useI18n } from 'vue-i18n';
import { useAnfrageEntwurf } from '../stores/anfrage';

const { t } = useI18n();
const entwurf = useAnfrageEntwurf();

// Erkanntes Wort aus der Prüfung, zum Beispiel «Erkanntes Schlüsselwort: bewusstlos».
// Es steht bewusst nicht in der Adresse, damit es weder im Verlauf noch in Protokollen landet.
const grund = computed(() => entwurf.notfallGrund ?? '');
</script>

<template>
  <div class="max-w-2xl border-2 border-notruf bg-white p-8" role="alert">
    <h1 class="text-3xl text-notruf">{{ t('notfall.titel') }}</h1>
    <p class="mt-4 text-lg">{{ t('notfall.text') }}</p>
    <p v-if="grund" class="mt-2 text-sm text-grau">{{ t('notfall.grund', { grund }) }}</p>

    <a href="tel:144" class="mt-6 inline-flex bg-notruf px-6 py-4 text-lg font-semibold text-white">
      {{ t('notfall.anrufen') }}
    </a>

    <p class="mt-6 text-grau">{{ t('notfall.abgrenzung') }}</p>

    <RouterLink :to="{ name: 'startseite' }" class="mt-6 inline-block underline underline-offset-2">
      {{ t('notfall.zurueck') }}
    </RouterLink>
  </div>
</template>
