# Drittkomponenten

Verwendete oder geplante Fremdkomponenten mit Zweck, Lizenz und Quelle. Die genauen Versionen stehen in den Paketdateien. Die Liste wird mit dem Code ergänzt und bei der Endkontrolle jedes Releases geprüft.

## Backend

| Komponente | Zweck | Lizenz | Quelle |
|---|---|---|---|
| ASP.NET Core (inklusive SignalR) | Web-API und Echtzeit | MIT | https://github.com/dotnet/aspnetcore |
| .NET | Laufzeit und Bibliotheken | MIT | https://github.com/dotnet/runtime |
| Entity Framework Core | Datenzugriff | MIT | https://github.com/dotnet/efcore |
| ASP.NET Core Authentication JwtBearer (mit Microsoft.IdentityModel) | Prüfung und Ausstellung der Tokens | MIT | https://github.com/dotnet/aspnetcore, https://github.com/AzureAD/azure-activedirectory-identitymodel-extensions-for-dotnet |
| Npgsql und Npgsql EF Core Provider (inklusive NetTopologySuite-Erweiterung) | PostgreSQL-Treiber und PostGIS-Typen | PostgreSQL License | https://github.com/npgsql/efcore.pg |
| NetTopologySuite | Geometrien für PostGIS | BSD-3-Clause | https://github.com/NetTopologySuite/NetTopologySuite |
| Entity Framework Core Tools (dotnet-ef) | Migrationen erstellen und anwenden | MIT | https://github.com/dotnet/efcore |

## Frontend

| Komponente | Zweck | Lizenz | Quelle |
|---|---|---|---|
| Vue 3 | Oberfläche | MIT | https://github.com/vuejs/core |
| Vue Router | Seiten und Navigation | MIT | https://github.com/vuejs/router |
| Vite | Build und Entwicklungsserver | MIT | https://github.com/vitejs/vite |
| vue-i18n | Mehrsprachigkeit | MIT | https://github.com/intlify/vue-i18n |
| Tailwind CSS | Gestaltung | MIT | https://github.com/tailwindlabs/tailwindcss |
| vite-plugin-pwa (mit Workbox) | Progressive Web App und Service Worker | MIT | https://github.com/vite-pwa/vite-plugin-pwa, https://github.com/GoogleChrome/workbox |

## Betrieb

| Komponente | Zweck | Lizenz | Quelle |
|---|---|---|---|
| PostgreSQL | Datenbank | PostgreSQL License | https://www.postgresql.org |
| PostGIS | Räumliche Abfragen | GPL-2.0-or-later | https://postgis.net |
| Container-Image postgis/postgis | Datenbank mit PostGIS als Container | MIT (Image-Definition) | https://github.com/postgis/docker-postgis |
| Traefik | Lastverteiler | MIT | https://github.com/traefik/traefik |
| nginx (Container-Image nginx-unprivileged) | Auslieferung der Oberfläche ohne Administratorrechte | BSD-2-Clause | https://github.com/nginxinc/docker-nginx-unprivileged |
| Container-Images .NET (sdk, aspnet) | Build und Laufzeit der Schnittstelle | MIT | https://github.com/dotnet/dotnet-docker |
| Container-Image Node.js (node:22-alpine) | Build der Oberfläche | MIT | https://github.com/nodejs/docker-node |
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
