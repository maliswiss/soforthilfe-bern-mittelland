# Datenmodell

Die Daten liegen in PostgreSQL mit PostGIS. Das Schema entsteht ausschliesslich über die Migrationen von Entity Framework Core in [`backend/src/Soforthilfe.Datenzugriff/Migrations`](../backend/src/Soforthilfe.Datenzugriff/Migrations).

## Übersicht

```mermaid
erDiagram
  REGION |o--o{ KONTO : "gehört zu"
  REGION ||--o{ HELFERPROFIL : "gehört zu"
  REGION ||--o{ ANFRAGE : "gehört zu"
  REGION |o--o{ PROTOKOLLEINTRAG : "gehört zu"
  KONTO ||--o| HELFERPROFIL : hat
  KONTO ||--o{ ANFRAGE : stellt
  KATEGORIE ||--o{ SKILL : umfasst
  KATEGORIE |o--o{ ANFRAGE : ordnet
  HELFERPROFIL }o--o{ SKILL : bietet

  REGION {
    uuid Id PK
    text Kennung UK
    text Name
    bool Aktiv
    int[] Gemeinden
  }
  KONTO {
    uuid Id PK
    uuid RegionId FK
    text EMail UK
    text Anzeigename
    text Telefon
    text PasswortHash
    text Rolle
  }
  KATEGORIE {
    uuid Id PK
    text Kennung UK
    text Bezeichnung
    bool Kostenpflichtig
    bool NurOrientierung
    int Reihenfolge
  }
  SKILL {
    uuid Id PK
    uuid KategorieId FK
    text Kennung UK
    text Bezeichnung
  }
  HELFERPROFIL {
    uuid Id PK
    uuid KontoId FK
    uuid RegionId FK
    text Typ
    text Betriebsname
    text Beschreibung
    text Adresse
    text Postleitzahl
    text Ort
    geography Standort
    int EinsatzradiusKm
    bool Einsatzbereit
    bool Partner
    double Bewertungsschnitt
    int AnzahlBewertungen
    int AnzahlEinsaetze
  }
  ANFRAGE {
    uuid Id PK
    uuid RegionId FK
    uuid KontoId FK
    uuid KategorieId FK
    text Freitext
    text Postleitzahl
    text Ort
    text Adresse
    text Zeitfenster
    text Prioritaet
    text Status
  }
  ORTSCHAFT {
    int Id PK
    text Postleitzahl
    text Zusatzziffer
    text Name
    text Gemeinde
    int BfsNummer
    text Kanton
    double Adressenanteil
    text Sprache
    geography Standort
  }
  PROTOKOLLEINTRAG {
    bigint Id PK
    uuid RegionId FK
    text Aktion
    text Objekt
    text ObjektId
    text Detail
    timestamptz Zeitpunkt
  }
```

## Tabellen

| Tabelle | Inhalt | Region |
|---|---|---|
| `Regionen` | Mandanten der Plattform mit ihren Gemeinden (BFS-Nummern) | ist selbst die Region |
| `Konten` | Personen mit Rolle, E-Mail-Adresse, Passwort-Hash und freiwilliger Telefonnummer | genau eine; der Plattformbetrieb hat keine |
| `Kategorien` | Kategorien der Startseite, kostenlos oder kostenpflichtig | für alle Regionen gleich |
| `Skills` | Fähigkeiten je Kategorie | für alle Regionen gleich |
| `Helferprofile` | Privatperson oder Betrieb, Einsatzgebiet, Radius, Fähigkeiten und Verfügbarkeit helfender Personen ([helferprofil.md](helferprofil.md)) | genau eine |
| `HelferprofilSkills` | Zuordnung Profil zu Fähigkeit | über das Profil |
| `Anfragen` | Anliegen mit Kategorie und/oder Freitext, Ort, Zeitfenster, Dringlichkeit und Status | genau eine |
| `Protokoll` | Sicherheitsrelevante Vorgänge wie der Notfall-Stopp, ohne Personendaten | eine oder keine (ohne Anmeldung) |
| `Ortschaften` | Amtliches Ortschaftenverzeichnis von swisstopo: Postleitzahl, Ortschaft, Gemeinde mit Adressenanteil, Kanton und Punkt | für alle Regionen gleich |

## Stammdaten

Stammdaten kommen mit festen Kennungen über die Migrationen in jede Datenbank.

| Art | Inhalt |
|---|---|
| Region | `bern-mittelland` (Soforthilfe Bern/Mittelland) mit den 74 Gemeinden des Verwaltungskreises Bern-Mittelland |
| Kategorien, kostenpflichtig | Schlüsseldienst, Kleine Reparatur, Sanitär/Heizung/Elektro, Panne und Abschleppdienst, Transport- und Umzugshilfe, IT- und Handy-Soforthilfe, Reinigung nach Schäden, Lieferung Apotheke und Einkauf |
| Kategorien, kostenlos | Nachbarschaftshilfe, Begleitung und Orientierung |
| Fähigkeiten | je Kategorie eine Fähigkeit mit derselben Kennung |

Weitere Regionen entstehen im laufenden Betrieb als Daten (`POST /api/mandanten`), nicht als Migration. Die Testdaten legen auf diesem Weg die zweite Region `biel-seeland` an.

## Entscheide zum Modell

| Nr. | Entscheid | Begründung |
|---|---|---|
| 1 | Konten, Helferprofile und Anfragen tragen eine `RegionId` | Mandantentrennung auf Datenebene, Filter und Schreibprüfung im Datenbankkontext |
| 2 | Kategorien und Fähigkeiten gelten für alle Regionen | eine gemeinsame Auswahl, keine Doppelungen |
| 3 | `Adresse` getrennt von `Postleitzahl` und `Ort` | die genaue Adresse wird später erst nach der Zusage ausgeliefert |
| 4 | `Kostenpflichtig` je Kategorie | strikte Trennung kostenloser und kostenpflichtiger Angebote (Anforderung A12) |
| 5 | `NurOrientierung` je Kategorie | gesundheitliche Anliegen bleiben auf Orientierung, Transport und Begleitung begrenzt |
| 6 | Rolle, Status und Dringlichkeit als Text gespeichert | Daten bleiben ohne Code lesbar; an der Schnittstelle gelten feste Zahlenwerte |
| 7 | Eine E-Mail-Adresse gehört plattformweit zu genau einem Konto | eindeutige Anmeldung, keine Verwechslung zwischen Regionen |
| 8 | Nur der Passwort-Hash wird gespeichert | kein Klartext, auch nicht in Testdaten |
| 9 | `Protokoll` ohne Konto, IP-Adresse und Eingaben; das Detail nennt nur den Begriff aus der festen Liste | Nachvollziehbarkeit ohne Personendaten; Einträge ohne Region sieht nur der Plattformbetrieb |
| 10 | `Ortschaften` übernimmt jede Zeile des Verzeichnisses, auch mehrere Gemeinden je Ortschaft | der Adressenanteil zeigt, ob eine Postleitzahl mehrere Gemeinden berührt; Punkte als `geography` mit räumlichem Index |
| 11 | `Regionen.Gemeinden` als Liste von BFS-Gemeindenummern; eine Gemeinde gehört zu höchstens einer Region | die Postleitzahl bestimmt über ihre Gemeinden die Region; keine Regionswahl in der Oberfläche |
| 12 | `Helferprofile.Standort` ist der Punkt der Postleitzahl, nie ein genauer Standort | Entfernungen ohne Ortung der Person; ändert sich der Ort, wird der Punkt neu bestimmt |

## Testdaten

Mit `Testdaten__Laden=true` lädt die Schnittstelle beim Start fiktive Daten in eine Datenbank ohne Konten: acht Konten auf `beispiel.invalid` in beiden Regionen und im Plattformbetrieb, drei Helferprofile und zwei Anfragen. Die CI prüft die Anzahl der Zeilen je Tabelle.
