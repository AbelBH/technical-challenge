# ---------- Etapa 1: build de Angular (Node 24 LTS) ----------
FROM node:24-alpine AS build
WORKDIR /app

# Dependencias primero para aprovechar la caché de capas de Docker.
COPY package.json package-lock.json ./
RUN npm ci

COPY . .
RUN npx ng build --configuration production

# ---------- Etapa 2: servidor web nginx con los archivos estáticos ----------
FROM nginx:alpine AS runtime

COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist/currency-exchange-web/browser /usr/share/nginx/html

EXPOSE 80
