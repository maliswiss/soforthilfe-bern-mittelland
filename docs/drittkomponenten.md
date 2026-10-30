# Drittkomponenten

Verwendete oder geplante Fremdkomponenten mit Zweck, Lizenz und Quelle. Die genauen Versionen stehen in den Paketdateien. Die Liste wird mit dem Code ergänzt und bei der Endkontrolle jedes Releases geprüft.

## Backend

| Komponente | Zweck | Lizenz | Quelle |
|---|---|---|---|
| ASP.NET Core (inklusive SignalR) | Web-API und Echtzeit | MIT | https://github.com/dotnet/aspnetcore |
| .NET | Laufzeit und Bibliotheken | MIT | https://github.com/dotnet/runtime |
| Entity Framework Core | Datenzugriff | MIT | https://github.com/dotnet/efcore |
| Npgsql und Npgsql EF Core Provider | PostgreSQL-Treiber | PostgreSQL License | https://github.com/npgsql/efcore.pg |

## Frontend

| Komponente | Zweck | Lizenz | Quelle |
|---|---|---|---|
| Vue 3 | Oberfläche | MIT | https://github.com/vuejs/core |
| Vite | Build und Entwicklungsserver | MIT | https://github.com/vitejs/vite |
| vue-i18n | Mehrsprachigkeit | MIT | https://github.com/intlify/vue-i18n |
| Tailwind CSS | Gestaltung | MIT | https://github.com/tailwindlabs/tailwindcss |
| vite-plugin-pwa | Progressive Web App | MIT | https://github.com/vite-pwa/vite-plugin-pwa |

## Betrieb

| Komponente | Zweck | Lizenz | Quelle |
|---|---|---|---|
| PostgreSQL | Datenbank | PostgreSQL License | https://www.postgresql.org |
| PostGIS | Räumliche Abfragen | GPL-2.0-or-later | https://postgis.net |
| Traefik | Lastverteiler | MIT | https://github.com/traefik/traefik |
| Docker Compose | Containerplattform | Apache-2.0 | https://github.com/docker/compose |
| Prometheus | Kennzahlen | Apache-2.0 | https://github.com/prometheus/prometheus |
| Alertmanager | Alarmierung | Apache-2.0 | https://github.com/prometheus/alertmanager |
| Ollama | Sprachmodell als Rückfall | MIT | https://github.com/ollama/ollama |

PostGIS läuft als Erweiterung des Datenbankdienstes im eigenen Container und ist nicht in den Quelltext dieses Repositories eingebunden.

## Tests und Sicherheit

| Komponente | Zweck | Lizenz | Quelle |
|---|---|---|---|
| Node.js (node:test) | Schnittstellentests | MIT | https://github.com/nodejs/node |
| Playwright | Oberflächentests | Apache-2.0 | https://github.com/microsoft/playwright |
| Gitleaks | Suche nach Geheimnissen | MIT | https://github.com/gitleaks/gitleaks |
| OWASP ZAP | Sicherheitsscans | Apache-2.0 | https://github.com/zaproxy/zaproxy |

## Daten

| Quelle | Zweck | Bedingung | Link |
|---|---|---|---|
| Amtliches Ortschaftenverzeichnis, swisstopo | Orte und Postleitzahlen | frei nutzbar mit Quellenangabe «© swisstopo» | https://www.swisstopo.admin.ch |

## Dienste

| Dienst | Zweck |
|---|---|
| Telegram Bot API | Alarmierung des Betriebs |
| GitHub (Actions, Dependabot) | CI und Abhängigkeitsprüfung |
