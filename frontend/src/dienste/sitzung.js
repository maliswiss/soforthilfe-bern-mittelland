import { computed, reactive } from 'vue';

// Sitzung der Person im Browser: Token und Konto nur für diese Registerkarte (sessionStorage),
// die gewählte Region dauerhaft als Komfort (localStorage). Speicher kann fehlen, darum immer mit try/catch.
const TOKEN_SCHLUESSEL = 'soforthilfe.sitzung';
const REGION_SCHLUESSEL = 'soforthilfe.region';

function lies(speicher, schluessel) {
  try {
    const wert = speicher.getItem(schluessel);
    return wert ? JSON.parse(wert) : null;
  } catch {
    return null;
  }
}

function schreibe(speicher, schluessel, wert) {
  try {
    if (wert === null) {
      speicher.removeItem(schluessel);
    } else {
      speicher.setItem(schluessel, JSON.stringify(wert));
    }
  } catch {
    // Ohne Speicher gilt die Sitzung nur bis zum Neuladen der Seite.
  }
}

const gespeichert = lies(sessionStorage, TOKEN_SCHLUESSEL);
const nochGueltig = gespeichert && new Date(gespeichert.ablauf) > new Date();

export const sitzung = reactive({
  token: nochGueltig ? gespeichert.token : null,
  ablauf: nochGueltig ? gespeichert.ablauf : null,
  konto: nochGueltig ? gespeichert.konto : null,
  region: lies(localStorage, REGION_SCHLUESSEL),
});

export const istAngemeldet = computed(() => sitzung.token !== null);

// Angemeldet gilt immer die Region des Kontos, sonst die gewählte.
export const aktuelleRegion = computed(() => sitzung.konto?.region ?? sitzung.region?.kennung ?? null);

export function setzeAnmeldung(antwort) {
  // Antwort der Schnittstelle: Token, Ablauf und Benutzer mit Rollen und Region (Mandant).
  const benutzer = antwort.benutzer;
  const gesichert = {
    token: antwort.token,
    ablauf: antwort.gueltigBis,
    konto: { ...benutzer, rolle: benutzer.rollen[0], region: benutzer.mandantSchluessel },
  };
  sitzung.token = gesichert.token;
  sitzung.ablauf = gesichert.ablauf;
  sitzung.konto = gesichert.konto;
  schreibe(sessionStorage, TOKEN_SCHLUESSEL, gesichert);
}

export function abmelden() {
  sitzung.token = null;
  sitzung.ablauf = null;
  sitzung.konto = null;
  schreibe(sessionStorage, TOKEN_SCHLUESSEL, null);
}

export function waehleRegion(region) {
  sitzung.region = region;
  schreibe(localStorage, REGION_SCHLUESSEL, region);
}
