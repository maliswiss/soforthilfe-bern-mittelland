#!/usr/bin/env bash
# Rauchtest der Schnittstelle: prüft nach jedem Build die Grundfunktionen gegen eine laufende Instanz.
# Aufruf: API_URL=http://localhost:5000 bash tests/rauchtest/schnittstelle.sh
# Voraussetzungen: curl und jq.

set -uo pipefail

BASIS="${API_URL:-http://localhost:5000}"
ANTWORT="$(mktemp)"
trap 'rm -f "$ANTWORT"' EXIT
fehler=0

# Ruft die Schnittstelle auf, legt den Inhalt in $ANTWORT ab und gibt den Statuscode aus.
status() {
  curl -s -o "$ANTWORT" -w '%{http_code}' "$@"
}

# Vergleicht erwarteten und tatsächlichen Wert und merkt sich Abweichungen.
erwarte() {
  local name="$1" erwartet="$2" erhalten="$3"
  if [ "$erwartet" = "$erhalten" ]; then
    echo "ok      $name ($erhalten)"
  else
    echo "FEHLER  $name: erwartet $erwartet, erhalten $erhalten"
    fehler=1
  fi
}

echo "== Regionen (Mandanten)"
erwarte "Regionen auflisten" 200 "$(status "$BASIS/api/mandanten")"
erwarte "Region Bern/Mittelland in der Liste" true "$(jq 'map(.schluessel) | index("bern-mittelland") != null' "$ANTWORT")"
erwarte "Unbekannte Region im Kopf X-Region" 404 "$(status -H 'X-Region: gibt-es-nicht' "$BASIS/api/kategorien")"

echo "== Registrierung und Anmeldung"
# Zufällige Adresse und zufälliges Passwort je Lauf; die Domain «.invalid» kann es nicht geben.
adresse="rauchtest.$(date +%s%N)@beispiel.invalid"
kennwort="$(openssl rand -base64 24)"
registrierung="$(jq -n --arg e "$adresse" --arg p "$kennwort" \
  '{eMail: $e, passwort: $p, anzeigename: "Rauchtest Person", kontoart: 0, mandant: "bern-mittelland"}')"
fremde_region="$(jq -n --arg e "rauchtest.$(date +%s%N)@beispiel.invalid" --arg p "$kennwort" \
  '{eMail: $e, passwort: $p, anzeigename: "Rauchtest Person", mandant: "gibt-es-nicht"}')"
anmeldung="$(jq -n --arg e "$adresse" --arg p "$kennwort" '{eMail: $e, passwort: $p, mandant: "bern-mittelland"}')"
falsch="$(jq -n --arg e "$adresse" '{eMail: $e, passwort: "falsches-passwort-123", mandant: "bern-mittelland"}')"
json=(-H 'Content-Type: application/json')

erwarte "Registrierung in unbekannter Region" 400 "$(status "${json[@]}" -d "$fremde_region" "$BASIS/api/auth/registrierung")"
erwarte "Feld der Fehlermeldung" mandant "$(jq -r '.feld' "$ANTWORT")"
erwarte "Registrierung in Bern/Mittelland" 200 "$(status "${json[@]}" -d "$registrierung" "$BASIS/api/auth/registrierung")"
erwarte "Antwort ohne Passwort-Hash" false "$(jq 'tostring | contains("passwortHash")' "$ANTWORT")"
erwarte "Gleiche Adresse ein zweites Mal" 409 "$(status "${json[@]}" -d "$registrierung" "$BASIS/api/auth/registrierung")"

erwarte "Anmeldung mit falschem Passwort" 401 "$(status "${json[@]}" -d "$falsch" "$BASIS/api/auth/anmeldung")"
erwarte "Text für die Oberfläche" true "$(jq '.fehler | length > 0' "$ANTWORT")"
erwarte "Anmeldung" 200 "$(status "${json[@]}" -d "$anmeldung" "$BASIS/api/auth/anmeldung")"
token="$(jq -r '.token' "$ANTWORT")"
erwarte "Eigenes Konto ohne Token" 401 "$(status "$BASIS/api/auth/ich")"
erwarte "Eigenes Konto mit Token" 200 "$(status -H "Authorization: Bearer $token" "$BASIS/api/auth/ich")"
erwarte "Region des Kontos" bern-mittelland "$(jq -r '.mandantSchluessel' "$ANTWORT")"
erwarte "Fremde Region im Kopf" 403 "$(status -H "Authorization: Bearer $token" -H 'X-Region: biel-seeland' "$BASIS/api/auth/ich")"
erwarte "Manipuliertes Token" 401 "$(status -H "Authorization: Bearer ${token}x" "$BASIS/api/auth/ich")"

echo "== Kategorien und Anfrage"
erwarte "Kategorien auflisten" 200 "$(status "$BASIS/api/kategorien")"
erwarte "Zehn Kategorien" 10 "$(jq 'length' "$ANTWORT")"
erwarte "Fähigkeiten auflisten" 200 "$(status "$BASIS/api/skills")"
anfrage='{"freitext":"Unterstützung beim Einkauf benötigt","plz":"3011","ort":"Bern"}'
erwarte "Anfrage ohne Anmeldung" 401 "$(status "${json[@]}" -d "$anfrage" "$BASIS/api/anfragen")"
erwarte "Anfrage erfassen" 201 "$(status "${json[@]}" -H "Authorization: Bearer $token" -d "$anfrage" "$BASIS/api/anfragen")"
erwarte "Erfasste Anfrage ist offen" 1 "$(jq -r '.status' "$ANTWORT")"
erwarte "Eigene Anfragen auflisten" 200 "$(status -H "Authorization: Bearer $token" "$BASIS/api/anfragen/meine")"
erwarte "Eine eigene Anfrage" 1 "$(jq 'length' "$ANTWORT")"

echo "== Rollen und Berechtigungen"
if [ -z "${TESTDATEN_PASSWORT:-}" ]; then
  echo "übersprungen: TESTDATEN_PASSWORT ist nicht gesetzt"
else
  # Meldet ein fiktives Testkonto in seiner Region an und gibt das Token aus.
  anmelden() {
    status "${json[@]}" -d "$(jq -n --arg e "$1" --arg p "$TESTDATEN_PASSWORT" --arg m "$2" '{eMail: $e, passwort: $p, mandant: $m}')" \
      "$BASIS/api/auth/anmeldung" > /dev/null
    jq -r '.token' "$ANTWORT"
  }
  hilfesuchende="$token"
  betrieb="$(anmelden hans.betrieb@beispiel.invalid bern-mittelland)"
  koordination_bern="$(anmelden dora.probe@beispiel.invalid bern-mittelland)"
  erwarte "Anfrage als Koordination ablehnen" 403 "$(status "${json[@]}" -H "Authorization: Bearer $koordination_bern" -d "$anfrage" "$BASIS/api/anfragen")"
  admin_biel="$(anmelden greta.muster@beispiel.invalid biel-seeland)"

  neue_region="rauchtest-$(date +%s)"
  region_json="$(jq -n --arg k "$neue_region" '{schluessel: $k, name: "Soforthilfe Rauchtest"}')"
  erwarte "Region anlegen ohne Anmeldung" 401 "$(status "${json[@]}" -d "$region_json" "$BASIS/api/mandanten")"
  erwarte "Region anlegen als Hilfesuchende" 403 "$(status "${json[@]}" -H "Authorization: Bearer $hilfesuchende" -d "$region_json" "$BASIS/api/mandanten")"
  erwarte "Region anlegen als Plattformbetrieb" 201 "$(status "${json[@]}" -H "Authorization: Bearer $betrieb" -d "$region_json" "$BASIS/api/mandanten")"
  erwarte "Gleiche Region ein zweites Mal" 409 "$(status "${json[@]}" -H "Authorization: Bearer $betrieb" -d "$region_json" "$BASIS/api/mandanten")"
  erwarte "Neue Region ohne Neustart in der Liste" true "$(status "$BASIS/api/mandanten" > /dev/null; jq --arg k "$neue_region" 'map(.schluessel) | index($k) != null' "$ANTWORT")"

  erwarte "Konten als Hilfesuchende" 403 "$(status -H "Authorization: Bearer $hilfesuchende" "$BASIS/api/konten")"
  erwarte "Konten als Koordination Bern/Mittelland" 200 "$(status -H "Authorization: Bearer $koordination_bern" "$BASIS/api/konten")"
  erwarte "Nur Konten aus Bern/Mittelland sichtbar" true "$(jq 'length > 0 and all(.region == "bern-mittelland")' "$ANTWORT")"
  erwarte "Konten als Plattformbetrieb" 200 "$(status -H "Authorization: Bearer $betrieb" "$BASIS/api/konten")"
  erwarte "Plattformbetrieb sieht beide Regionen" true "$(jq 'map(.region) | (index("bern-mittelland") != null) and (index("biel-seeland") != null)' "$ANTWORT")"
  bruno="$(jq -r '.[] | select(.eMail == "bruno.muster@beispiel.invalid") | .id' "$ANTWORT")"
  fritz="$(jq -r '.[] | select(.eMail == "fritz.platzhalter@beispiel.invalid") | .id' "$ANTWORT")"

  rolle='{"rolle": "Koordination"}'
  erwarte "Rolle setzen als Koordination" 403 "$(status -X PUT "${json[@]}" -H "Authorization: Bearer $koordination_bern" -d "$rolle" "$BASIS/api/konten/$bruno/rolle")"
  erwarte "Rolle in fremder Region setzen" 404 "$(status -X PUT "${json[@]}" -H "Authorization: Bearer $admin_biel" -d "$rolle" "$BASIS/api/konten/$bruno/rolle")"
  erwarte "Rolle in eigener Region setzen" 204 "$(status -X PUT "${json[@]}" -H "Authorization: Bearer $admin_biel" -d "$rolle" "$BASIS/api/konten/$fritz/rolle")"
  erwarte "Plattformbetrieb vergeben" 400 "$(status -X PUT "${json[@]}" -H "Authorization: Bearer $admin_biel" -d '{"rolle": "Plattformbetrieb"}' "$BASIS/api/konten/$fritz/rolle")"
fi

if [ "$fehler" -ne 0 ]; then
  echo "Rauchtest fehlgeschlagen."
  exit 1
fi
echo "Rauchtest bestanden."
