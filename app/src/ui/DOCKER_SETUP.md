# UI Docker Setup

## Overview
The UI is containerized as a production-ready static build served with a lightweight HTTP server. This setup is designed for end-to-end testing in a Docker Compose network.

## Files

- **Dockerfile**: Multi-stage build that compiles the React/TypeScript/Vite application and serves it statically
- **docker-entrypoint.sh**: Startup script that injects environment variables into the config at runtime
- **config.json**: Configuration file containing the API base URL
- **.dockerignore**: Optimizes the Docker build context

## Building the Image

```bash
docker build -f app/src/ui/Dockerfile -t financial-app-ui:latest .
```

## Running the Container

### Default (localhost backend)
```bash
docker run -p 3000:3000 financial-app-ui:latest
```

### With custom backend URL
```bash
docker run -p 3000:3000 -e API_BASE_URL=http://api:5000 financial-app-ui:latest
```

## Environment Variables

- **API_BASE_URL**: Base URL for the backend API (default: `http://localhost:5000`)
  - The `/api` suffix is automatically appended by the client
  - Example: `API_BASE_URL=http://api:5000` results in API calls to `http://api:5000/api`

## Configuration Flow

1. At build time, the Vite bundle is created with static assets
2. At runtime, the entrypoint script substitutes the `API_BASE_URL` environment variable into `config.json`
3. The React app loads `config.json` on startup and configures the API client with the correct backend URL
4. All API requests use the configured URL

## Docker Compose Integration

For end-to-end testing with Docker Compose:

```yaml
services:
  ui:
    build:
      context: .
      dockerfile: app/src/ui/Dockerfile
    ports:
      - "3000:3000"
    environment:
      - API_BASE_URL=http://api:5000
    depends_on:
      - api

  api:
    build:
      context: .
      dockerfile: app/src/api/Dockerfile
    ports:
      - "5000:80"
```

## Port

The UI is served on port **3000** (default Vite development port).
