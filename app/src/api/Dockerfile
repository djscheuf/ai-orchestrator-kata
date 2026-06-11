# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project file
COPY ["app/src/api/FinancialApi.csproj", "app/src/api/"]

# Restore dependencies
RUN dotnet restore "app/src/api/FinancialApi.csproj"

# Copy source code
COPY . .

# Publish the application
RUN dotnet publish "app/src/api/FinancialApi.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

# Expose port
EXPOSE 5000

# Set default environment (can be overridden at runtime)
ENV ASPNETCORE_ENVIRONMENT=Development
ENV ASPNETCORE_URLS=http://+:5000

ENTRYPOINT ["dotnet", "FinancialApi.dll"]
