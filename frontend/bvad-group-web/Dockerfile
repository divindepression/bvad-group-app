# ==================== STAGE 1 : Build ====================
FROM node:20-alpine AS build
WORKDIR /app

# Timeouts pour connexions lentes
RUN npm config set fetch-timeout 600000 && \
    npm config set fetch-retry-mintimeout 20000 && \
    npm config set fetch-retry-maxtimeout 120000 && \
    npm config set fetch-retries 5

COPY package*.json ./
RUN npm install --no-audit --no-fund --loglevel=error

COPY . .
RUN npx ng build --configuration production

# ==================== STAGE 2 : Nginx ====================
FROM nginx:alpine AS runtime
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist/bvad-group-web/browser /usr/share/nginx/html

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]