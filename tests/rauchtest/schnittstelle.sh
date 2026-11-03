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
erwarte "Regionen auflisten" 200 "$(status "$BASIS/api/regionen")"
erwarte "Region Bern in der Liste" true "$(jq 'map(.kennung) | index("bern") != null' "$ANTWORT")"
erwarte "Region aus dem Kopf X-Region" 200 "$(status -H 'X-Region: bern' "$BASIS/api/regionen/aktuell")"
erwarte "Kennung der aktuellen Region" bern "$(jq -r '.kennung' "$ANTWORT")"
erwarte "Unbekannte Region wird abgewiesen" 404 "$(status -H 'X-Region: gibt-es-nicht' "$BASIS/api/regionen/aktuell")"
erwarte "Ohne Region keine aktuelle Region" 400 "$(status "$BASIS/api/regionen/aktuell")"

echo "== Registrierung und Anmeldung"
# Zufällige Adresse und zufälliges Passwort je Lauf; die Domain «.invalid» kann es nicht geben.
adresse="rauchtest.$(date +%s%N)@beispiel.invalid"
kennwort="$(openssl rand -base64 24)"
registrierung="$(jq -n --arg e "$adresse" --arg p "$kennwort" \
  '{eMail: $e, passwort: $p, anzeigename: "Rauchtest Person", rolle: "Hilfesuchende"}')"
anmeldung="$(jq -n --arg e "$adresse" --arg p "$kennwort" '{eMail: $e, passwort: $p}')"
falsch="$(jq -n --arg e "$adresse" '{eMail: $e, passwort: "falsches-passwort-123"}')"
json=(-H 'Content-Type: application/json')

erwarte "Registrierung ohne Region" 400 "$(status "${json[@]}" -d "$registrierung" "$BASIS/api/konto/registrieren")"
erwarte "Registrierung in Bern" 201 "$(status "${json[@]}" -H 'X-Region: bern' -d "$registrierung" "$BASIS/api/konto/registrieren")"
erwarte "Antwort ohne Passwort-Hash" false "$(jq 'has("passwortHash")' "$ANTWORT")"
erwarte "Gleiche Adresse ein zweites Mal" 409 "$(status "${json[@]}" -H 'X-Region: bern' -d "$registrierung" "$BASIS/api/konto/registrieren")"

# Mehrwertnummern sind unabhängig von Landesvorwahl und Trennzeichen gesperrt.
for nummer in '+41 900 123 456' '0041-901-123-456' '0906/123456'; do
  adresse_nummer="nummer.$(date +%s%N)@beispiel.invalid"
  daten_nummer="$(jq -n --arg e "$adresse_nummer" --arg p "$kennwort" --arg t "$nummer" \
    '{eMail: $e, passwort: $p, anzeigename: "Rauchtest Nummer", rolle: "Hilfesuchende", telefon: $t}')"
  erwarte "Mehrwertnummer $nummer ablehnen" 400 "$(status "${json[@]}" -H 'X-Region: bern' -d "$daten_nummer" "$BASIS/api/konto/registrieren")"
done
erwarte "Anmeldung mit falschem Passwort" 401 "$(status "${json[@]}" -d "$falsch" "$BASIS/api/konto/anmelden")"
erwarte "Anmeldung" 200 "$(status "${json[@]}" -d "$anmeldung" "$BASIS/api/konto/anmelden")"
token="$(jq -r '.token' "$ANTWORT")"
erwarte "Eigenes Konto ohne Token" 401 "$(status "$BASIS/api/konto/ich")"
erwarte "Eigenes Konto mit Token" 200 "$(status -H "Authorization: Bearer $token" "$BASIS/api/konto/ich")"
erwarte "Region des Kontos" bern "$(jq -r '.region' "$ANTWORT")"
erwarte "Fremde Region im Kopf" 403 "$(status -H "Authorization: Bearer $token" -H 'X-Region: mittelland' "$BASIS/api/konto/ich")"
erwarte "Manipuliertes Token" 401 "$(status -H "Authorization: Bearer ${token}x" "$BASIS/api/konto/ich")"

# Kostenlose und kostenpflichtige Kategorien sind getrennt abrufbar.
erwarte "Kostenlose Kategorien" 200 "$(status "$BASIS/api/kategorien?kostenpflichtig=false")"
erwarte "Nur kostenlose Kategorien" true "$(jq 'length > 0 and all(.kostenpflichtig == false)' "$ANTWORT")"
erwarte "Kostenpflichtige Kategorien" 200 "$(status "$BASIS/api/kategorien?kostenpflichtig=true")"
erwarte "Nur kostenpflichtige Kategorien" true "$(jq 'length > 0 and all(.kostenpflichtig == true)' "$ANTWORT")"

freie_kategorie="$(status "$BASIS/api/kategorien?kostenpflichtig=false" >/dev/null; jq -r '.[0].id' "$ANTWORT")"
anfrage="$(jq -n --arg k "$freie_kategorie" '{kategorieId: $k, postleitzahl: "3000"}')"
erwarte "Anfrage ohne Anmeldung" 401 "$(status "${json[@]}" -d "$anfrage" "$BASIS/api/anfragen")"
erwarte "Anfrage erfassen" 201 "$(status "${json[@]}" -H "Authorization: Bearer $token" -d "$anfrage" "$BASIS/api/anfragen")"
erwarte "Anfrage mit kostenloser Kategorie" false "$(jq -r '.kostenpflichtig' "$ANTWORT")"
erwarte "Erfasste Anfrage ist offen" Offen "$(jq -r '.status' "$ANTWORT")"
erwarte "Anfrage mit fremder Region ablehnen" 403 "$(status "${json[@]}" -H "Authorization: Bearer $token" -H 'X-Region: mittelland' -d "$anfrage" "$BASIS/api/anfragen")"
freitext_anfrage='{"freitext":"Unterstützung beim Einkauf benötigt","postleitzahl":"3000"}'
erwarte "Anfrage per Freitext" 201 "$(status "${json[@]}" -H "Authorization: Bearer $token" -d "$freitext_anfrage" "$BASIS/api/anfragen")"
ungueltig='{"freitext":"Text","kategorieId":"00000000-0000-0000-0000-000000000001","postleitzahl":"3000"}'
erwarte "Kategorie und Freitext zugleich ablehnen" 400 "$(status "${json[@]}" -H "Authorization: Bearer $token" -d "$ungueltig" "$BASIS/api/anfragen")"

echo "== Rollen und Berechtigungen"
if [ -z "${TESTDATEN_PASSWORT:-}" ]; then
  echo "übersprungen: TESTDATEN_PASSWORT ist nicht gesetzt"
else
  # Meldet ein fiktives Testkonto an und gibt das Token aus.
  anmelden() {
    status "${json[@]}" -d "$(jq -n --arg e "$1" --arg p "$TESTDATEN_PASSWORT" '{eMail: $e, passwort: $p}')" \
      "$BASIS/api/konto/anmelden" > /dev/null
    jq -r '.token' "$ANTWORT"
  }
  hilfesuchende="$token"
  betrieb="$(anmelden hans.betrieb@beispiel.invalid)"
  koordination_bern="$(anmelden dora.probe@beispiel.invalid)"
  erwarte "Anfrage als Koordination ablehnen" 403 "$(status "${json[@]}" -H "Authorization: Bearer $koordination_bern" -d "$anfrage" "$BASIS/api/anfragen")"
  admin_mittelland="$(anmelden greta.muster@beispiel.invalid)"

  neue_region="rauchtest-$(date +%s)"
  region_json="$(jq -n --arg k "$neue_region" '{kennung: $k, name: "Rauchtest"}')"
  erwarte "Region anlegen ohne Anmeldung" 401 "$(status "${json[@]}" -d "$region_json" "$BASIS/api/regionen")"
  erwarte "Region anlegen als Hilfesuchende" 403 "$(status "${json[@]}" -H "Authorization: Bearer $hilfesuchende" -d "$region_json" "$BASIS/api/regionen")"
  erwarte "Region anlegen als Plattformbetrieb" 201 "$(status "${json[@]}" -H "Authorization: Bearer $betrieb" -d "$region_json" "$BASIS/api/regionen")"
  erwarte "Gleiche Region ein zweites Mal" 409 "$(status "${json[@]}" -H "Authorization: Bearer $betrieb" -d "$region_json" "$BASIS/api/regionen")"
  erwarte "Neue Region ohne Neustart nutzbar" 200 "$(status -H "X-Region: $neue_region" "$BASIS/api/regionen/aktuell")"

  erwarte "Konten als Hilfesuchende" 403 "$(status -H "Authorization: Bearer $hilfesuchende" "$BASIS/api/konten")"
  erwarte "Konten als Koordination Bern" 200 "$(status -H "Authorization: Bearer $koordination_bern" "$BASIS/api/konten")"
  erwarte "Nur Konten aus Bern sichtbar" true "$(jq 'length > 0 and all(.region == "bern")' "$ANTWORT")"
  erwarte "Konten als Plattformbetrieb" 200 "$(status -H "Authorization: Bearer $betrieb" "$BASIS/api/konten")"
  erwarte "Plattformbetrieb sieht beide Regionen" true "$(jq 'map(.region) | (index("bern") != null) and (index("mittelland") != null)' "$ANTWORT")"
  bruno="$(jq -r '.[] | select(.eMail == "bruno.muster@beispiel.invalid") | .id' "$ANTWORT")"
  fritz="$(jq -r '.[] | select(.eMail == "fritz.platzhalter@beispiel.invalid") | .id' "$ANTWORT")"

  rolle='{"rolle": "Koordination"}'
  erwarte "Rolle setzen als Koordination" 403 "$(status -X PUT "${json[@]}" -H "Authorization: Bearer $koordination_bern" -d "$rolle" "$BASIS/api/konten/$bruno/rolle")"
  erwarte "Rolle in fremder Region setzen" 404 "$(status -X PUT "${json[@]}" -H "Authorization: Bearer $admin_mittelland" -d "$rolle" "$BASIS/api/konten/$bruno/rolle")"
  erwarte "Rolle in eigener Region setzen" 204 "$(status -X PUT "${json[@]}" -H "Authorization: Bearer $admin_mittelland" -d "$rolle" "$BASIS/api/konten/$fritz/rolle")"
  erwarte "Plattformbetrieb vergeben" 400 "$(status -X PUT "${json[@]}" -H "Authorization: Bearer $admin_mittelland" -d '{"rolle": "Plattformbetrieb"}' "$BASIS/api/konten/$fritz/rolle")"
fi

if [ "$fehler" -ne 0 ]; then
  echo "Rauchtest fehlgeschlagen."
  exit 1
fi
echo "Rauchtest bestanden."
