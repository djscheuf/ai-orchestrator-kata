# Build stage
FROM node:20-alpine AS build
WORKDIR /app

# Copy package files
COPY app/src/ui/package*.json ./

# Install dependencies
RUN npm ci

# Copy source code
COPY app/src/ui/ .

# Build the application
RUN npm run build

# Runtime stage - serve static files
FROM node:20-alpine
WORKDIR /app

# Install a lightweight HTTP server
RUN npm install -g serve

# Copy built application from build stage
COPY --from=build /app/dist ./dist

# Copy config file
COPY app/src/ui/config.json ./dist/config.json

# Copy entrypoint script
COPY app/src/ui/docker-entrypoint.sh ./entrypoint.sh
RUN chmod +x ./entrypoint.sh

# Expose port 3000 (default Vite port)
EXPOSE 3000

# Set default API base URL (can be overridden at runtime)
ENV API_BASE_URL=http://localhost:5000

# Serve the static files
ENTRYPOINT ["./entrypoint.sh"]
