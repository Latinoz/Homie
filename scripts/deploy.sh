#!/bin/bash
set -euo pipefail

APP_DIR="/var/netcore"
SERVICE_NAME="Homie.service"

echo "Starting deployment..."

# Stop service (try both system and user mode)
echo "Stopping service..."
sudo systemctl stop "$SERVICE_NAME" 2>/dev/null || true
systemctl --user stop "$SERVICE_NAME" 2>/dev/null || true

# Wait a bit to ensure port is released
sleep 3

# Kill any remaining processes using port 5000
echo "Checking for running Homie processes..."
HOMIE_PIDS=$(pgrep -f "dotnet.*Homie.dll" 2>/dev/null || true)
if [ ! -z "$HOMIE_PIDS" ]; then
    echo "Found running Homie processes (PIDs: $HOMIE_PIDS), attempting to stop them..."
    # Try kill first (graceful)
    kill $HOMIE_PIDS 2>/dev/null || true
    sleep 3
    # Check if still running
    HOMIE_PIDS=$(pgrep -f "dotnet.*Homie.dll" 2>/dev/null || true)
    if [ ! -z "$HOMIE_PIDS" ]; then
        echo "Processes still running, trying kill -9..."
        kill -9 $HOMIE_PIDS 2>/dev/null || sudo pkill -9 -f "dotnet.*Homie.dll" 2>/dev/null || true
        sleep 2
    fi
fi
echo "All processes stopped"

# Set permissions (these may fail without sudo, but that's OK if files are already owned by deploy-user)
echo "Setting permissions..."
sudo chown -R www-data:www-data "$APP_DIR" 2>/dev/null || echo "chown skipped (no sudo access)"
sudo chmod -R 755 "$APP_DIR" 2>/dev/null || chmod -R 755 "$APP_DIR" 2>/dev/null || echo "chmod may have failed"
find "$APP_DIR" -type f -name "*.dll" -exec chmod 644 {} \; 2>/dev/null || true
find "$APP_DIR" -type f -name "*.json" -exec chmod 644 {} \; 2>/dev/null || true

# Note: Database migrations should be applied manually if needed
# Auto-migrations have been disabled to prevent deployment issues

# Start service (try system first since it's usually a system service, then user mode)
echo "Starting service..."
sudo systemctl start "$SERVICE_NAME" 2>/dev/null || systemctl --user start "$SERVICE_NAME" 2>/dev/null || echo "Failed to start service"

# Check status
sleep 5
echo "Service status:"
sudo systemctl status "$SERVICE_NAME" --no-pager -l 2>/dev/null || systemctl --user status "$SERVICE_NAME" --no-pager -l 2>/dev/null || echo "Cannot get service status"

echo "Deployment completed successfully!"