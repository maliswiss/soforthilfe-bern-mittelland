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
erwarte "Anmeldung mit falschem Passwort" 401 "$(status "${json[@]}" -d "$falsch" "$BASIS/api/konto/anmelden")"
erwarte "Anmeldung" 200 "$(status "${json[@]}" -d "$anmeldung" "$BASIS/api/konto/anmelden")"
token="$(jq -r '.token' "$ANTWORT")"
erwarte "Eigenes Konto ohne Token" 401 "$(status "$BASIS/api/konto/ich")"
erwarte "Eigenes Konto mit Token" 200 "$(status -H "Authorization: Bearer $token" "$BASIS/api/konto/ich")"
erwarte "Region des Kontos" bern "$(jq -r '.region' "$ANTWORT")"
erwarte "Fremde Region im Kopf" 403 "$(status -H "Authorization: Bearer $token" -H 'X-Region: mittelland' "$BASIS/api/konto/ich")"
erwarte "Manipuliertes Token" 401 "$(status -H "Authorization: Bearer ${token}x" "$BASIS/api/konto/ich")"

if [ "$fehler" -ne 0 ]; then
  echo "Rauchtest fehlgeschlagen."
  exit 1
fi
echo "Rauchtest bestanden."
