import { defineStore } from 'pinia';

/// Hält die Eingaben der Startseite, bis die Anfrage verbindlich erfasst wird.
/// So kann die Person ohne Konto beginnen und sich erst vor dem Absenden anmelden.
export const useAnfrageEntwurf = defineStore('anfrageEntwurf', {
  state: () => ({
    kategorieId: null,
    kategorieBezeichnung: null,
    freitext: '',
    prioritaet: 1,
    plz: '',
    ort: '',
    // Erkanntes Wort beim Notfall-Stopp; bleibt nur im Speicher der Seite, nie in der Adresse
    notfallGrund: null
  }),

  getters: {
    vorhanden: (state) => Boolean(state.kategorieId || state.freitext)
  },

  actions: {
    leeren() {
      this.$reset();
    }
  }
});
