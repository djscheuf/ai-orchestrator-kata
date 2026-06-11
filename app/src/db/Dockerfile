FROM postgres:14-alpine

COPY app/src/db/schema.sql /docker-entrypoint-initdb.d/01-schema.sql

ENV POSTGRES_USER=postgres
ENV POSTGRES_PASSWORD=postgres
ENV POSTGRES_DB=financial_app

RUN apk add --no-cache postgresql-client

EXPOSE 5432
