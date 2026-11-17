/// Prüfung einer Sprachdatei vor dem Import, mit denselben Regeln wie die Schnittstelle. Zusätzlich vergleicht sie
/// mit den deutschen Texten: fehlende Schlüssel erscheinen später auf Deutsch, unbekannte zeigt die Oberfläche nie.

const schluesselMuster = /^[A-Za-z0-9_-]{1,60}$/;
const unzulaessig = /[<>@|]/;
const platzhalterMuster = /\{([A-Za-z0-9_]+)\}/g;
const maxTiefe = 5;
const maxTexte = 3000;
const maxLaenge = 1000;

/// Alle Texte als flache Liste «pfad → text».
export function flach(objekt, pfad = '', ziel = new Map()) {
  for (const [schluessel, wert] of Object.entries(objekt ?? {})) {
    const voll = pfad ? `${pfad}.${schluessel}` : schluessel;
    if (wert !== null && typeof wert === 'object' && !Array.isArray(wert)) {
      flach(wert, voll, ziel);
    } else {
      ziel.set(voll, wert);
    }
  }
  return ziel;
}

function platzhalter(text) {
  return [...String(text).matchAll(platzhalterMuster)].map((m) => m[1]).sort().join(',');
}

// Erster Verstoss gegen die Regeln der Schnittstelle als { fehler, pfad }, sonst null.
function regelverstoss(objekt, pfad, tiefe, zaehler) {
  if (tiefe > maxTiefe) {
    return { fehler: 'zuTief', pfad };
  }
  for (const [schluessel, wert] of Object.entries(objekt)) {
    const voll = pfad ? `${pfad}.${schluessel}` : schluessel;
    if (!schluesselMuster.test(schluessel)) {
      return { fehler: 'schluessel', pfad: voll };
    }
    if (wert !== null && typeof wert === 'object' && !Array.isArray(wert)) {
      const innen = regelverstoss(wert, voll, tiefe + 1, zaehler);
      if (innen) {
        return innen;
      }
      continue;
    }
    if (typeof wert !== 'string') {
      return { fehler: 'keinText', pfad: voll };
    }
    if (wert.length > maxLaenge) {
      return { fehler: 'zuLang', pfad: voll };
    }
    if (unzulaessig.test(wert)) {
      return { fehler: 'zeichen', pfad: voll };
    }
    if (/[{}]/.test(wert.replace(platzhalterMuster, ''))) {
      return { fehler: 'klammern', pfad: voll };
    }
    zaehler.anzahl += 1;
    if (zaehler.anzahl > maxTexte) {
      return { fehler: 'zuViele', pfad: null };
    }
  }
  return null;
}

/// Prüft den Inhalt einer Datei. Ergebnis: texte (Objekt oder null), fehler mit pfad, anzahl und der
/// Vergleich mit den deutschen Texten (fehlend, unbekannt, andere Platzhalter).
export function sprachdateiPruefen(inhalt, grundlage) {
  let texte;
  try {
    texte = JSON.parse(inhalt);
  } catch {
    return { texte: null, fehler: 'json', pfad: null };
  }
  if (texte === null || typeof texte !== 'object' || Array.isArray(texte)) {
    return { texte: null, fehler: 'keinObjekt', pfad: null };
  }

  const zaehler = { anzahl: 0 };
  const verstoss = regelverstoss(texte, '', 1, zaehler);
  if (verstoss) {
    return { texte: null, ...verstoss };
  }
  if (zaehler.anzahl === 0) {
    return { texte: null, fehler: 'leer', pfad: null };
  }

  const neu = flach(texte);
  const basis = flach(grundlage);
  const fehlend = [...basis.keys()].filter((k) => !neu.has(k));
  const unbekannt = [...neu.keys()].filter((k) => !basis.has(k));
  const andererPlatzhalter = [...neu.keys()].filter(
    (k) => basis.has(k) && platzhalter(basis.get(k)) !== platzhalter(neu.get(k))
  );

  return { texte, fehler: null, pfad: null, anzahl: zaehler.anzahl, fehlend, unbekannt, andererPlatzhalter };
}
