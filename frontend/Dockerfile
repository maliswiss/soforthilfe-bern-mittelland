# Oberfläche als Container-Image.
# Build: Vue-Anwendung mit Vite erzeugen, Abhängigkeiten genau nach Sperrdatei.
FROM node:22-alpine AS build
WORKDIR /quelle
COPY package.json package-lock.json ./
RUN npm ci
COPY . .
RUN npm run build

# Auslieferung: statische Dateien über nginx, ohne Administratorrechte (Port 8080).
FROM nginxinc/nginx-unprivileged:1.31-alpine AS laufzeit
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY nginx-sicherheit.conf /etc/nginx/sicherheit.conf
COPY --from=build /quelle/dist /usr/share/nginx/html
EXPOSE 8080
