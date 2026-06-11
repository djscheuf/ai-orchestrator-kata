FROM postgres:14-alpine

COPY schema.sql /docker-entrypoint-initdb.d/01-schema.sql

ENV POSTGRES_USER=postgres
ENV POSTGRES_PASSWORD=postgres
ENV POSTGRES_DB=financial_app

EXPOSE 5432
