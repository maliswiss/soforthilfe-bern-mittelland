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

if [ "$fehler" -ne 0 ]; then
  echo "Rauchtest fehlgeschlagen."
  exit 1
fi
echo "Rauchtest bestanden."
