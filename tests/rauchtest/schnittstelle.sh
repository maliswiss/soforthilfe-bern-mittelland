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

json=(-H 'Content-Type: application/json')

echo "== Orte und Postleitzahlen"
erwarte "Ortschaften zur Postleitzahl 3011" 200 "$(status "$BASIS/api/orte?plz=3011")"
erwarte "Ort zur Postleitzahl" Bern "$(jq -r '.[0].ort' "$ANTWORT")"
erwarte "Region zur Postleitzahl" bern-mittelland "$(jq -r '.[0].region' "$ANTWORT")"
erwarte "Hinweis auf mehrere Gemeinden" true "$(status "$BASIS/api/orte?plz=3174" > /dev/null; jq '.[0].plzUeberMehrereGemeinden' "$ANTWORT")"
erwarte "Postleitzahl ausserhalb der Regionen" null "$(status "$BASIS/api/orte?plz=8001" > /dev/null; jq -r '.[0].region' "$ANTWORT")"
erwarte "Unbekannte Postleitzahl" 0 "$(status "$BASIS/api/orte?plz=3000" > /dev/null; jq 'length' "$ANTWORT")"
erwarte "Ungültige Postleitzahl" 400 "$(status "$BASIS/api/orte?plz=abcd")"
standort='{"breitengrad":46.9466,"laengengrad":7.444}'
erwarte "Postleitzahl zum freigegebenen Standort" 200 "$(status "${json[@]}" -d "$standort" "$BASIS/api/orte/umgebung")"
erwarte "Standort Bundesplatz ergibt 3011" 3011 "$(jq -r '.plz' "$ANTWORT")"
erwarte "Standort im Ausland" 404 "$(status "${json[@]}" -d '{"breitengrad":48.8566,"laengengrad":2.3522}' "$BASIS/api/orte/umgebung")"
erwarte "Koordinaten nicht in der Adresse" 405 "$(status "$BASIS/api/orte/umgebung?breitengrad=46.9466&laengengrad=7.444")"

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
anfrage_ausserhalb='{"freitext":"Unterstützung beim Einkauf benötigt","plz":"8001","ort":"Zürich"}'
erwarte "Anfrage ausserhalb der Region" 400 "$(status "${json[@]}" -H "Authorization: Bearer $token" -d "$anfrage_ausserhalb" "$BASIS/api/anfragen")"
erwarte "Anfrage erfassen" 201 "$(status "${json[@]}" -H "Authorization: Bearer $token" -d "$anfrage" "$BASIS/api/anfragen")"
erwarte "Erfasste Anfrage ist offen" 1 "$(jq -r '.status' "$ANTWORT")"
erwarte "Eigene Anfragen auflisten" 200 "$(status -H "Authorization: Bearer $token" "$BASIS/api/anfragen/meine")"
erwarte "Eine eigene Anfrage" 1 "$(jq 'length' "$ANTWORT")"

echo "== Helferprofil"
helfende="$(jq -n --arg e "rauchtest.helfende.$(date +%s%N)@beispiel.invalid" --arg p "$kennwort" \
  '{eMail: $e, passwort: $p, anzeigename: "Rauchtest Helfende", kontoart: 1, mandant: "bern-mittelland"}')"
status "${json[@]}" -d "$helfende" "$BASIS/api/auth/registrierung" > /dev/null
token_helfende="$(jq -r '.token' "$ANTWORT")"
status "$BASIS/api/skills" > /dev/null
skill_frei="$(jq -r 'map(select(.code == "nachbarschaftshilfe")) | .[0].id' "$ANTWORT")"
profil="$(jq -n --arg s "$skill_frei" '{typ: 0, plz: "3011", ort: "Bern", radiusKm: 5, skillIds: [$s]}')"
erwarte "Helferprofil vor dem Anlegen" 404 "$(status -H "Authorization: Bearer $token_helfende" "$BASIS/api/helferprofil")"
erwarte "Helferprofil als Hilfesuchende" 403 "$(status -H "Authorization: Bearer $token" "$BASIS/api/helferprofil")"
erwarte "Helferprofil anlegen" 200 "$(status -X PUT "${json[@]}" -H "Authorization: Bearer $token_helfende" -d "$profil" "$BASIS/api/helferprofil")"
erwarte "Fähigkeit im Profil" nachbarschaftshilfe "$(jq -r '.skills[0].code' "$ANTWORT")"
richtpreis='{"anfahrtChf":40,"stundenansatzChf":95,"nachtzuschlagProzent":50,"wochenendzuschlagProzent":25,"mwstPflichtig":true,"zeitpunkte":["2026-11-11T22:00:00+01:00"]}'
erwarte "Richtpreis berechnen" 200 "$(status "${json[@]}" -d "$richtpreis" "$BASIS/api/preise/richtpreis")"
erwarte "Richtpreis nachts mit MWST in Rappen" 19730 "$(jq '.[0].richtpreisErsteStunde * 100 | round' "$ANTWORT")"
skill_bezahlt="$(status "$BASIS/api/skills" > /dev/null; jq -r 'map(select(.code == "sanitaer-elektro")) | .[0].id' "$ANTWORT")"
erwarte "Bezahlpflichtige Fähigkeit ohne Preise" 400 "$(status -X PUT "${json[@]}" -H "Authorization: Bearer $token_helfende" -d "$(jq -n --arg s "$skill_bezahlt" '{typ: 0, plz: "3011", ort: "Bern", radiusKm: 5, skillIds: [$s]}')" "$BASIS/api/helferprofil")"
erwarte "Verfügbarkeit ausschalten" false "$(status -X PUT "${json[@]}" -H "Authorization: Bearer $token_helfende" -d '{"jetztVerfuegbar":false}' "$BASIS/api/helferprofil/verfuegbarkeit" > /dev/null; jq '.jetztVerfuegbar' "$ANTWORT")"
erwarte "UID-Nummer einreichen" 200 "$(status "${json[@]}" -H "Authorization: Bearer $token_helfende" -d '{"art":0,"nachweiswert":"CHE 123 456 788"}' "$BASIS/api/verifizierungen")"
erwarte "UID normalisiert" CHE-123.456.788 "$(jq -r '.nachweiswert' "$ANTWORT")"
erwarte "Nachweis in Prüfung" 1 "$(jq '.status' "$ANTWORT")"
erwarte "Falsche Prüfziffer" 400 "$(status "${json[@]}" -H "Authorization: Bearer $token_helfende" -d '{"art":0,"nachweiswert":"CHE-123.456.789"}' "$BASIS/api/verifizierungen")"
erwarte "Kein Upload von Dokumenten" 415 "$(status -H "Authorization: Bearer $token_helfende" -F art=0 -F "dokument=@/dev/null;filename=ausweis.pdf" "$BASIS/api/verifizierungen")"

echo "== Notfall-Stopp"
notfall_text='{"freitext":"Mein Mann ist bewusstlos auf dem Boden."}'
erwarte "Notfallprüfung ohne Anmeldung" 200 "$(status "${json[@]}" -d "$notfall_text" "$BASIS/api/anfragen/verstehen")"
erwarte "Notfall erkannt" true "$(jq '.notfall' "$ANTWORT")"
notfall_anfrage='{"freitext":"Mein Mann ist bewusstlos auf dem Boden.","plz":"3011","ort":"Bern"}'
erwarte "Anfrage mit Notfall wird gestoppt" 451 "$(status "${json[@]}" -H "Authorization: Bearer $token" -d "$notfall_anfrage" "$BASIS/api/anfragen")"
erwarte "Weiterhin eine einzige eigene Anfrage" 1 "$(status -H "Authorization: Bearer $token" "$BASIS/api/anfragen/meine" > /dev/null; jq 'length' "$ANTWORT")"
erwarte "Protokoll ohne Anmeldung" 401 "$(status "$BASIS/api/protokoll")"
erwarte "Protokoll als Hilfesuchende" 403 "$(status -H "Authorization: Bearer $token" "$BASIS/api/protokoll")"

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

  erwarte "Protokoll als Koordination Bern/Mittelland" 200 "$(status -H "Authorization: Bearer $koordination_bern" "$BASIS/api/protokoll?aktion=Notfall.Stopp")"
  erwarte "Notfall-Stopp der Anfrage protokolliert" true "$(jq 'any(.objekt == "Anfrage" and .detail == "Erkanntes Schlüsselwort: bewusstlos")' "$ANTWORT")"
  erwarte "Nur Einträge aus Bern/Mittelland" true "$(jq 'length > 0 and all(.region == "bern-mittelland")' "$ANTWORT")"
  erwarte "Protokoll ohne Personendaten" true "$(jq 'all(keys == ["aktion", "detail", "id", "objekt", "objektId", "region", "zeitpunkt"])' "$ANTWORT")"
  erwarte "Protokoll als Mandantenadministration Biel/Seeland" 200 "$(status -H "Authorization: Bearer $admin_biel" "$BASIS/api/protokoll")"
  erwarte "Keine Einträge aus Bern/Mittelland" false "$(jq 'any(.region == "bern-mittelland")' "$ANTWORT")"
  erwarte "Protokoll als Plattformbetrieb" 200 "$(status -H "Authorization: Bearer $betrieb" "$BASIS/api/protokoll")"
  erwarte "Notfall-Stopp ohne Region nur im Plattformbetrieb" true "$(jq 'any(.region == null and .objekt == "Verstaendnis")' "$ANTWORT")"
fi

if [ "$fehler" -ne 0 ]; then
  echo "Rauchtest fehlgeschlagen."
  exit 1
fi
echo "Rauchtest bestanden."
