FROM node:24-alpine AS build
WORKDIR /app

COPY src/Web/orderflow-web/package*.json ./
RUN npm ci

COPY src/Web/orderflow-web/ ./
RUN npm run build

FROM nginx:alpine
COPY src/Web/orderflow-web/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html

EXPOSE 80
