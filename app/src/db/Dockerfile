FROM mcr.microsoft.com/mssql/server:2022-latest

ENV ACCEPT_EULA=Y
ENV SA_PASSWORD=YourPassword123!
ENV MSSQL_PID=Developer

COPY app/src/db/schema.sql /docker-entrypoint-initdb.d/01-schema.sql
COPY app/src/db/seed_data.sql /docker-entrypoint-initdb.d/02-seed_data.sql

EXPOSE 1433

CMD ["/opt/mssql/bin/sqlservr"]
