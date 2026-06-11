FROM postgres:14-alpine

COPY app/src/db/schema.sql /docker-entrypoint-initdb.d/01-schema.sql
COPY app/src/db/seed_data.sql /docker-entrypoint-initdb.d/02-seed_data.sql

ENV POSTGRES_USER=postgres
ENV POSTGRES_PASSWORD=postgres
ENV POSTGRES_DB=financial_app

RUN apk add --no-cache postgresql-client

EXPOSE 5432
