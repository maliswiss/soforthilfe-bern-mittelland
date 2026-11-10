import { senden } from './klient';
import { t } from '../i18n';

/// Maximale Wartezeit auf den Browser. Manche Browser antworten nie, auch nicht mit einem
/// Fehler; die eigene Zeitgrenze verhindert, dass die Oberfläche stehen bleibt.
const WARTEZEIT_MS = 12000;

const FEHLERSCHLUESSEL = {
  1: 'ort.fehlerVerweigert',
  2: 'ort.fehlerNichtVerfuegbar',
  3: 'ort.fehlerZeit'
};

function fehlertext(code) {
  return t(FEHLERSCHLUESSEL[code] ?? FEHLERSCHLUESSEL[2]);
}

/// Ermittelt den Standort über den Browser. Die Person gibt ihn dort ausdrücklich frei.
/// Liefert { breitengrad, laengengrad, genauigkeitMeter }.
export function standortErmitteln(wartezeitMs = WARTEZEIT_MS) {
  return new Promise((erfuellen, ablehnen) => {
    if (!window.isSecureContext) {
      ablehnen(new Error(t('ort.fehlerUnsicher')));
      return;
    }

    if (!('geolocation' in navigator)) {
      ablehnen(new Error(t('ort.fehlerBrowser')));
      return;
    }

    let erledigt = false;

    const sicherung = setTimeout(() => {
      if (!erledigt) {
        erledigt = true;
        ablehnen(new Error(fehlertext(3)));
      }
    }, wartezeitMs);

    navigator.geolocation.getCurrentPosition(
      (position) => {
        if (erledigt) {
          return;
        }
        erledigt = true;
        clearTimeout(sicherung);
        erfuellen({
          breitengrad: position.coords.latitude,
          laengengrad: position.coords.longitude,
          genauigkeitMeter: Math.round(position.coords.accuracy)
        });
      },
      (fehler) => {
        if (erledigt) {
          return;
        }
        erledigt = true;
        clearTimeout(sicherung);
        ablehnen(new Error(fehlertext(fehler.code)));
      },
      { enableHighAccuracy: false, timeout: wartezeitMs - 1000, maximumAge: 300000 }
    );
  });
}

/// Postleitzahl und Ort zum Standort, nur über die eigene Schnittstelle und ohne Drittanbieter.
/// Die Koordinaten stehen im Rumpf, nie in der Adresse, und werden dort nicht gespeichert.
export function ortZuStandort(standort) {
  return senden('/orte/umgebung', {
    breitengrad: standort.breitengrad,
    laengengrad: standort.laengengrad
  });
}
