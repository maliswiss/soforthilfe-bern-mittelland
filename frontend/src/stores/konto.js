import { defineStore } from 'pinia';
import { holen, senden } from '../api/klient';

const speicherschluessel = 'soforthilfe.sitzung';
const regionsschluessel = 'soforthilfe.region';
const standardregion = 'bern-mittelland';

/// Kurzname einer Region für Kopfzeile und Texte, z. B. «Bern/Mittelland».
export function regionKurzname(name) {
  return String(name ?? '').replace(/^Soforthilfe\s+/i, '') || 'Bern/Mittelland';
}

export const useKonto = defineStore('konto', {
  state: () => ({
    token: null,
    benutzer: null,
    geladen: false,
    // Regionen (Mandanten). Vor der Anmeldung folgt die Region aus der Postleitzahl, danach aus dem Konto.
    regionen: [],
    gewaehlteRegion: standardregion
  }),

  getters: {
    angemeldet: (state) => Boolean(state.token),
    istHelfende: (state) => state.benutzer?.rollen?.includes('Helfende') ?? false,
    istHilfesuchende: (state) => state.benutzer?.rollen?.includes('Hilfesuchende') ?? false,

    /// Schlüssel der wirksamen Region: die des Kontos oder vor der Anmeldung die aus der Postleitzahl bestimmte.
    region: (state) => state.benutzer?.mandantSchluessel ?? state.gewaehlteRegion,

    regionName(state) {
      if (state.benutzer?.mandantName) {
        return regionKurzname(state.benutzer.mandantName);
      }
      const gefunden = state.regionen.find((r) => r.schluessel === state.gewaehlteRegion);
      return regionKurzname(gefunden?.name);
    }
  },

  actions: {
    wiederherstellen() {
      if (this.geladen) {
        return;
      }
      try {
        const gespeichert = localStorage.getItem(speicherschluessel);
        if (gespeichert) {
          const daten = JSON.parse(gespeichert);
          // Abgelaufene Sitzungen werden nicht übernommen
          if (!daten.gueltigBis || new Date(daten.gueltigBis) > new Date()) {
            this.token = daten.token;
            this.benutzer = daten.benutzer;
          }
        }
        this.gewaehlteRegion = localStorage.getItem(regionsschluessel) || standardregion;
      } catch {
        // Ohne Speicher im Browser startet die Seite ohne Sitzung
      }
      this.geladen = true;
    },

    sichern(gueltigBis) {
      try {
        localStorage.setItem(
          speicherschluessel,
          JSON.stringify({ token: this.token, benutzer: this.benutzer, gueltigBis })
        );
      } catch {
        // Sitzung bleibt bis zum Schliessen der Seite erhalten
      }
    },

    async regionenLaden() {
      try {
        this.regionen = await holen('/mandanten');
      } catch {
        this.regionen = [];
      }
      // Eine nicht mehr aktive Region wird durch die erste aktive ersetzt
      if (this.regionen.length && !this.regionen.some((r) => r.schluessel === this.gewaehlteRegion)) {
        this.regionWaehlen(this.regionen[0].schluessel);
      }
    },

    regionWaehlen(schluessel) {
      this.gewaehlteRegion = schluessel;
      try {
        localStorage.setItem(regionsschluessel, schluessel);
      } catch {
        // Auswahl gilt nur für diese Seite
      }
    },

    uebernehmen(antwort) {
      this.token = antwort.token;
      this.benutzer = antwort.benutzer;
      this.sichern(antwort.gueltigBis);
    },

    /// Ohne Angabe einer Region meldet die Schnittstelle in der Region des Kontos an.
    async anmelden(eMail, passwort) {
      this.uebernehmen(await senden('/auth/anmeldung', { eMail, passwort }));
      if (this.benutzer?.mandantSchluessel) {
        this.regionWaehlen(this.benutzer.mandantSchluessel);
      }
    },

    async registrieren(daten) {
      this.uebernehmen(
        await senden('/auth/registrierung', { ...daten, mandant: this.gewaehlteRegion })
      );
    },

    abmelden() {
      // Die Region bleibt gewählt, damit die nächste Anmeldung in derselben Region beginnt
      if (this.benutzer?.mandantSchluessel) {
        this.regionWaehlen(this.benutzer.mandantSchluessel);
      }
      this.token = null;
      this.benutzer = null;
      try {
        localStorage.removeItem(speicherschluessel);
      } catch {
        // nichts zu tun
      }
    }
  }
});
