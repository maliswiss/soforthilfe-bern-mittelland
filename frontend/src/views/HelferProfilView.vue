<script setup>
import { computed, onMounted, ref, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import { kultur } from '../i18n';
import { holen, senden } from '../api/klient';
import { betrag, betragLesen, prozentLesen } from '../api/betrag';
import { istKostenpflichtig } from '../api/telefon';
import { useKonto } from '../stores/konto';
import HinweisFeld from '../components/HinweisFeld.vue';
import OrtEingabe from '../components/OrtEingabe.vue';
import Warnhinweis from '../components/Warnhinweis.vue';

const konto = useKonto();
const { t } = useI18n();

const skills = ref([]);
const formular = ref({
  typ: 0,
  betriebsname: '',
  beschreibung: '',
  plz: '',
  ort: '',
  radiusKm: 10,
  skillIds: [],
  adresse: ''
});

// Anforderung A9: Preisangaben, vor der Zusage für Anfragende sichtbar
const preise = ref({
  anfahrt: '',
  stundenansatz: '',
  nachtzuschlag: '',
  wochenendzuschlag: '',
  mwstPflichtig: false
});

const profilVorhanden = ref(false);
const verifizierung = ref(null);
const nachweise = ref([]);
const nachweis = ref({ art: 0, nachweiswert: '' });
const meldung = ref('');
const fehler = ref('');

const nachweisarten = [0, 1, 2, 3, 4];
const TELEFON = 2;
const EMAIL = 3;

// Anforderung A10: eine bestätigte Telefonnummer darf keine kostenpflichtige Mehrwertnummer sein
const nachweisKostenpflichtig = computed(
  () => nachweis.value.art === TELEFON && istKostenpflichtig(nachweis.value.nachweiswert)
);

// Preise sind Pflicht, sobald eine bezahlpflichtige Fähigkeit gewählt ist
const preiseErforderlich = computed(() =>
  skills.value.some((s) => s.bezahlpflichtig && formular.value.skillIds.includes(s.id))
);

const platzhalter = computed(() => ({ 0: 'CHE-123.456.789', 1: 'CHE-123.456.789 HR', 2: '031 123 45 67', 4: 'Gemeinde Muri, Frau Muster' })[nachweis.value.art] ?? '');

function alsText(wert) {
  return wert === null || wert === undefined ? '' : String(wert);
}

function zeit(wert) {
  return new Date(wert).toLocaleString(kultur(), { dateStyle: 'medium', timeStyle: 'short' });
}

async function nachweiseLaden() {
  try {
    nachweise.value = await holen('/verifizierungen/meine', konto.token);
  } catch {
    nachweise.value = [];
  }
}

function uebernehmen(profil) {
  formular.value = {
    typ: profil.typ,
    betriebsname: profil.betriebsname ?? '',
    beschreibung: profil.beschreibung ?? '',
    plz: profil.plz,
    ort: profil.ort,
    radiusKm: profil.radiusKm,
    skillIds: profil.skills.map((s) => s.id),
    adresse: profil.adresse ?? ''
  };
  preise.value = {
    anfahrt: alsText(profil.anfahrtChf),
    stundenansatz: alsText(profil.stundenansatzChf),
    nachtzuschlag: alsText(profil.nachtzuschlagProzent),
    wochenendzuschlag: alsText(profil.wochenendzuschlagProzent),
    mwstPflichtig: profil.mwstPflichtig
  };
  verifizierung.value = profil.verifizierung;
  profilVorhanden.value = true;
}

onMounted(async () => {
  try {
    skills.value = await holen('/skills');
  } catch (problem) {
    fehler.value = problem.message;
  }

  // Ohne Profil liefert die Schnittstelle 404; dann bleibt das Formular leer
  try {
    uebernehmen(await holen('/helferprofil', konto.token));
    await nachweiseLaden();
  } catch (problem) {
    if (problem.status !== 404) {
      fehler.value = problem.message;
    }
  }
});

function skillUmschalten(id) {
  const liste = formular.value.skillIds;
  formular.value.skillIds = liste.includes(id) ? liste.filter((s) => s !== id) : [...liste, id];
}

// Vorschau des Richtpreises, wie Anfragende ihn sehen. Beispielwoche im Winter (Schweizer Zeit);
// gerechnet wird auf dem Server, damit Profil, Vorschau und Vorschläge dieselbe Regel nutzen.
const beispielzeiten = [
  { schluessel: 'werktagTag', zeitpunkt: '2027-01-13T10:00:00+01:00' },
  { schluessel: 'werktagNacht', zeitpunkt: '2027-01-13T22:00:00+01:00' },
  { schluessel: 'wochenendeTag', zeitpunkt: '2027-01-16T10:00:00+01:00' },
  { schluessel: 'wochenendeNacht', zeitpunkt: '2027-01-16T22:00:00+01:00' }
];
const vorschau = ref([]);
let vorschauZeitgeber = null;

watch(
  preise,
  () => {
    clearTimeout(vorschauZeitgeber);
    vorschauZeitgeber = setTimeout(vorschauBerechnen, 300);
  },
  { deep: true }
);

async function vorschauBerechnen() {
  const anfahrt = betragLesen(preise.value.anfahrt);
  const stundenansatz = betragLesen(preise.value.stundenansatz);
  const nacht = prozentLesen(preise.value.nachtzuschlag);
  const wochenende = prozentLesen(preise.value.wochenendzuschlag);
  if (anfahrt === null || stundenansatz === null || Number.isNaN(nacht) || Number.isNaN(wochenende)) {
    vorschau.value = [];
    return;
  }
  try {
    const ergebnis = await senden('/preise/richtpreis', {
      anfahrtChf: anfahrt,
      stundenansatzChf: stundenansatz,
      nachtzuschlagProzent: nacht,
      wochenendzuschlagProzent: wochenende,
      mwstPflichtig: preise.value.mwstPflichtig,
      zeitpunkte: beispielzeiten.map((b) => b.zeitpunkt)
    });
    vorschau.value = beispielzeiten.map((b, i) => ({ ...b, preis: ergebnis[i] }));
  } catch {
    vorschau.value = [];
  }
}

async function speichern() {
  fehler.value = '';
  meldung.value = '';

  if (formular.value.skillIds.length === 0) {
    fehler.value = t('profil.fehlerFaehigkeit');
    return;
  }

  const anfahrt = preise.value.anfahrt === '' ? null : betragLesen(preise.value.anfahrt);
  const stundenansatz = preise.value.stundenansatz === '' ? null : betragLesen(preise.value.stundenansatz);
  const nacht = prozentLesen(preise.value.nachtzuschlag);
  const wochenende = prozentLesen(preise.value.wochenendzuschlag);

  if ((preise.value.anfahrt !== '' && anfahrt === null)
      || (preise.value.stundenansatz !== '' && stundenansatz === null)
      || Number.isNaN(nacht) || Number.isNaN(wochenende)) {
    fehler.value = t('profil.fehlerPreisformat');
    return;
  }

  if (preiseErforderlich.value && (anfahrt === null || stundenansatz === null)) {
    fehler.value = t('profil.fehlerPreise');
    document.getElementById('anfahrt')?.focus();
    return;
  }

  try {
    const profil = await senden(
      '/helferprofil',
      {
        ...formular.value,
        betriebsname: formular.value.typ === 1 ? formular.value.betriebsname : null,
        anfahrtChf: anfahrt,
        stundenansatzChf: stundenansatz,
        nachtzuschlagProzent: nacht,
        wochenendzuschlagProzent: wochenende,
        mwstPflichtig: preise.value.mwstPflichtig
      },
      konto.token,
      'PUT'
    );
    uebernehmen(profil);
    // Nach prüfrelevanten Änderungen liegen freigegebene Nachweise wieder zur Prüfung vor
    await nachweiseLaden();
    meldung.value = t('profil.gespeichert');
  } catch (problem) {
    fehler.value = problem.message;
  }
}

async function nachweisEinreichen() {
  fehler.value = '';
  meldung.value = '';
  if (nachweisKostenpflichtig.value) {
    document.getElementById('wert')?.focus();
    return;
  }
  try {
    // Bei der E-Mail-Adresse gilt immer die des Kontos; eine leere Telefonnummer steht für die des Kontos
    const wert = nachweis.value.art === EMAIL ? null : nachweis.value.nachweiswert.trim() || null;
    await senden('/verifizierungen', { art: nachweis.value.art, nachweiswert: wert }, konto.token);
    verifizierung.value = 1;
    nachweis.value.nachweiswert = '';
    await nachweiseLaden();
    meldung.value = t('profil.nachweisEingereicht');
  } catch (problem) {
    fehler.value = problem.message;
  }
}
</script>

<template>
  <h1 class="text-3xl">{{ t('profil.titel') }}</h1>
  <p class="mt-2 max-w-lesen text-grau">{{ t('profil.einleitung') }}</p>

  <HinweisFeld v-if="meldung" :meldung="meldung" class="mt-4" />
  <HinweisFeld v-if="fehler" art="fehler" :meldung="fehler" class="mt-4" />

  <form class="karte mt-6 max-w-2xl space-y-5" novalidate @submit.prevent="speichern">
    <div>
      <label for="typ" class="etikett">{{ t('profil.typ') }}</label>
      <select id="typ" v-model.number="formular.typ" class="feld">
        <option :value="0">{{ t('profil.privatperson') }}</option>
        <option :value="1">{{ t('profil.betrieb') }}</option>
      </select>
    </div>

    <div v-if="formular.typ === 1">
      <label for="betrieb" class="etikett">{{ t('profil.betriebsname') }}</label>
      <input id="betrieb" v-model="formular.betriebsname" class="feld" autocomplete="organization" />
    </div>

    <div>
      <label for="beschreibung" class="etikett">{{ t('profil.beschreibung') }}</label>
      <textarea id="beschreibung" v-model="formular.beschreibung" rows="3" class="feld"></textarea>
    </div>

    <OrtEingabe v-model:plz="formular.plz" v-model:ort="formular.ort" kennung="profil" />
    <p class="-mt-2 text-xs text-grau">{{ t('profil.plzMittelpunkt') }}</p>

    <div>
      <label for="profil-adresse" class="etikett">{{ t('profil.adresse') }}</label>
      <input id="profil-adresse" v-model="formular.adresse" class="feld" autocomplete="street-address" />
      <p class="mt-1 text-xs text-grau">{{ t('profil.adresseHinweis') }}</p>
    </div>

    <div class="max-w-[200px]">
      <label for="radius" class="etikett">{{ t('profil.radius') }}</label>
      <input id="radius" v-model.number="formular.radiusKm" type="number" min="1" max="100" class="feld" />
    </div>

    <div>
      <span class="etikett">{{ t('profil.faehigkeiten') }}</span>
      <div class="grid gap-2 sm:grid-cols-2">
        <button
          v-for="skill in skills"
          :key="skill.id"
          type="button"
          class="border px-3 py-2 text-left text-sm"
          :class="formular.skillIds.includes(skill.id)
            ? 'border-tanne bg-tanne-blass'
            : 'border-kiesel hover:border-tanne-hell'"
          :aria-pressed="formular.skillIds.includes(skill.id)"
          @click="skillUmschalten(skill.id)"
        >
          {{ skill.bezeichnung }}
          <span v-if="!skill.bezahlpflichtig" class="block text-xs text-grau">{{ t('allgemein.kostenlos') }}</span>
        </button>
      </div>
    </div>

    <fieldset class="space-y-4 border-t border-kiesel pt-4">
      <legend class="text-lg">{{ t('preis.titel') }}</legend>
      <p class="text-sm text-grau">
        {{ preiseErforderlich ? t('profil.preiseHinweisPflicht') : t('profil.preiseHinweisFrei') }}
      </p>

      <div class="grid gap-4 sm:grid-cols-2">
        <div>
          <label for="anfahrt" class="etikett">{{ t('profil.anfahrt') }}</label>
          <input id="anfahrt" v-model="preise.anfahrt" class="feld" inputmode="decimal" placeholder="40.00" :required="preiseErforderlich" />
        </div>
        <div>
          <label for="stundenansatz" class="etikett">{{ t('profil.stundenansatz') }}</label>
          <input id="stundenansatz" v-model="preise.stundenansatz" class="feld" inputmode="decimal" placeholder="95.00" :required="preiseErforderlich" />
        </div>
        <div>
          <label for="nachtzuschlag" class="etikett">{{ t('profil.nachtzuschlag') }}</label>
          <input id="nachtzuschlag" v-model="preise.nachtzuschlag" class="feld" inputmode="numeric" placeholder="50" />
        </div>
        <div>
          <label for="wochenendzuschlag" class="etikett">{{ t('profil.wochenendzuschlag') }}</label>
          <input id="wochenendzuschlag" v-model="preise.wochenendzuschlag" class="feld" inputmode="numeric" placeholder="25" />
        </div>
      </div>

      <label class="flex items-start gap-3 text-sm">
        <input v-model="preise.mwstPflichtig" type="checkbox" class="mt-1" />
        <span>{{ t('profil.mwstPflichtig') }}</span>
      </label>
      <p class="text-xs text-grau">{{ t('profil.zuschlagRegel') }}</p>

      <div v-if="vorschau.length" class="bg-tanne-blass px-4 py-3 text-sm" data-test="preisvorschau">
        <p class="font-medium">{{ t('profil.vorschauTitel') }}</p>
        <dl class="mt-1 grid grid-cols-[auto_1fr] gap-x-4">
          <template v-for="eintrag in vorschau" :key="eintrag.schluessel">
            <dt class="text-grau">{{ t(`profil.vorschau.${eintrag.schluessel}`) }}</dt>
            <dd>
              {{ t('preis.richtpreisWert', { betrag: betrag(eintrag.preis.richtpreisErsteStunde) }) }}
              <span v-if="eintrag.preis.angewandterZuschlagProzent" class="text-grau">
                (+{{ eintrag.preis.angewandterZuschlagProzent }} %)
              </span>
            </dd>
          </template>
        </dl>
      </div>
    </fieldset>

    <button type="submit" class="knopf w-full">{{ t('profil.speichern') }}</button>
  </form>

  <section class="mt-8 max-w-2xl" aria-labelledby="verifizierung-titel">
    <h2 id="verifizierung-titel" class="text-2xl">{{ t('profil.verifizierung') }}</h2>
    <p class="mt-2 text-grau">{{ t('profil.verifizierungText') }}</p>
    <p v-if="verifizierung !== null" class="mt-1 text-sm" data-test="verifizierungsstand">
      {{ t('profil.verifizierungStand', { stand: t(`verifizierung.${verifizierung}`) }) }}
    </p>

    <p v-if="!profilVorhanden" class="karte mt-4 text-grau">{{ t('profil.zuerstProfil') }}</p>

    <template v-else>
      <form class="karte mt-4 space-y-4" novalidate @submit.prevent="nachweisEinreichen">
        <div>
          <label for="art" class="etikett">{{ t('profil.nachweisart') }}</label>
          <select id="art" v-model.number="nachweis.art" class="feld">
            <option v-for="wert in nachweisarten" :key="wert" :value="wert">
              {{ t(`profil.nachweis.${wert}`) }}
            </option>
          </select>
        </div>

        <div>
          <label for="wert" class="etikett">{{ t('profil.angabe') }}</label>
          <input
            v-if="nachweis.art === EMAIL"
            id="wert"
            :value="konto.benutzer?.eMail ?? ''"
            class="feld bg-papier"
            readonly
            aria-describedby="wert-erklaerung"
          />
          <input
            v-else
            id="wert"
            v-model="nachweis.nachweiswert"
            class="feld"
            :class="nachweisKostenpflichtig ? 'border-notruf focus:border-notruf' : ''"
            :placeholder="platzhalter"
            :inputmode="nachweis.art === TELEFON ? 'tel' : undefined"
            :aria-invalid="nachweisKostenpflichtig"
            :aria-describedby="nachweisKostenpflichtig ? 'wert-hinweis' : 'wert-erklaerung'"
          />
          <Warnhinweis v-if="nachweisKostenpflichtig" kennung="wert-hinweis" :meldung="t('telefon.kostenpflichtig')" />
          <p id="wert-erklaerung" class="mt-1 text-xs text-grau">{{ t(`profil.angabeHinweis.${nachweis.art}`) }}</p>
        </div>

        <button type="submit" class="knopf">{{ t('profil.nachweisEinreichen') }}</button>
      </form>

      <h3 class="mt-6 text-xl">{{ t('profil.meineNachweise') }}</h3>
      <p v-if="nachweise.length === 0" class="mt-2 text-sm text-grau">{{ t('profil.keineNachweise') }}</p>
      <ul v-else class="mt-2 space-y-2" data-test="nachweise">
        <li v-for="eintrag in nachweise" :key="eintrag.id" class="karte text-sm">
          <div class="flex flex-wrap items-baseline justify-between gap-2">
            <span class="font-medium">{{ t(`profil.nachweis.${eintrag.art}`) }}</span>
            <span class="border border-tanne px-2 py-0.5 text-xs font-medium text-tanne">
              {{ t(`verifizierung.${eintrag.status}`) }}
            </span>
          </div>
          <p class="mt-1 text-grau">
            {{ eintrag.nachweiswert }} · {{ t('profil.eingereicht', { zeit: zeit(eintrag.eingereichtAm) }) }}
            <template v-if="eintrag.geprueftAm"> · {{ t('profil.geprueft', { zeit: zeit(eintrag.geprueftAm) }) }}</template>
          </p>
          <p v-if="eintrag.bemerkung" class="mt-1">{{ t('profil.bemerkung', { text: eintrag.bemerkung }) }}</p>
        </li>
      </ul>
    </template>
  </section>
</template>
