#!/bin/sh

# Substitute environment variables in config.json
if [ -n "$API_BASE_URL" ]; then
  sed -i "s|http://localhost:5000|$API_BASE_URL|g" /app/dist/config.json
fi

# Start the server
exec serve -s dist -l 3000
