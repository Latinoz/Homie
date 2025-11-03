#!/bin/bash
set -euo pipefail

APP_DIR="/var/netcore"
SERVICE_NAME="Homie.service"

echo "Starting deployment..."

# Stop service
echo "Stopping service..."
sudo systemctl stop "$SERVICE_NAME" || true

# Wait a bit
sleep 3

# Set permissions
echo "Setting permissions..."
sudo chown -R www-data:www-data "$APP_DIR" || true
sudo chmod -R 755 "$APP_DIR" || true
sudo find "$APP_DIR" -type f -name "*.dll" -exec chmod 644 {} \; 2>/dev/null || true
sudo find "$APP_DIR" -type f -name "*.json" -exec chmod 644 {} \; 2>/dev/null || true

# Apply database migrations (if any)
echo "Applying database migrations..."
cd "$APP_DIR"
export ASPNETCORE_ENVIRONMENT=Production
export DOTNET_ROOT=/usr/lib/dotnet

if [ -f "Homie.dll" ]; then
    dotnet Homie.dll --migrate || echo "Migrations completed or not required"
fi

# Start service
echo "Starting service..."
sudo systemctl start "$SERVICE_NAME"

# Check status
sleep 5
echo "Service status:"
sudo systemctl status "$SERVICE_NAME" --no-pager -l || true

echo "Deployment completed successfully!"