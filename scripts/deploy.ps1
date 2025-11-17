param(
    [string]$Environment = "Release",
    [string]$ConfigFile = "../.deploy/config.json"
)

# Read configuration
if (-not (Test-Path $ConfigFile)) {
    Write-Error "Config file not found: $ConfigFile"
    exit 1
}

$config = Get-Content $ConfigFile -Raw | ConvertFrom-Json

# Resolve service name (default to Homie.service if not provided)
$serviceName = if ($config.PSObject.Properties.Name -contains 'ServiceName' -and $config.ServiceName) { $config.ServiceName } else { 'Homie.service' }
Write-Host "Using service name: $serviceName" -ForegroundColor Green

# Check key
if (-not (Test-Path $config.SshKeyPath)) {
    Write-Error "SSH key not found: $($config.SshKeyPath)"
    exit 1
}

# Function for SSH commands with key authentication
function Invoke-SSHCommand {
    param($Command, $KeyPath, $Server, $Port, $Username)
    
    $sshCommand = "ssh -p $Port -i `"$KeyPath`" -o StrictHostKeyChecking=no ${Username}@${Server} `"$Command`""
    Write-Host "Executing: $sshCommand" -ForegroundColor Yellow
    Invoke-Expression $sshCommand
}

# Function for SCP with key authentication
function Invoke-SCP {
    param($LocalPath, $RemotePath, $KeyPath, $Server, $Port, $Username)
    
    $scpCommand = "scp -P $Port -i `"$KeyPath`" -o StrictHostKeyChecking=no `"$LocalPath`" ${Username}@${Server}:`"$RemotePath`""
    Write-Host "Executing: $scpCommand" -ForegroundColor Yellow
    Invoke-Expression $scpCommand
}

# Build project
Write-Host "Building project..." -ForegroundColor Green
$projectPath = "../$($config.ProjectPath)"

if (-not (Test-Path $projectPath)) {
    Write-Error "Project file not found: $projectPath"
    Write-Host "Available project files:" -ForegroundColor Yellow
    Get-ChildItem ../ -Recurse -Filter *.csproj | ForEach-Object { Write-Host "  $($_.FullName)" }
    exit 1
}

dotnet publish $projectPath -c $Environment -r linux-x64 --self-contained false -o ../publish --nologo

# Check if publish succeeded
if (-not (Test-Path "../publish")) {
    Write-Error "Publish failed - publish directory not created"
    exit 1
}

# Clean unnecessary files
Write-Host "Cleaning files..." -ForegroundColor Green
foreach ($pattern in $config.ExcludePatterns) {
    Get-ChildItem ../publish -Recurse -Include $pattern | Remove-Item -Force
}

# Remove appsettings.json from publish to avoid overwriting server version
if (Test-Path "../publish/appsettings.json") {
    Remove-Item "../publish/appsettings.json" -Force
    Write-Host "Removed appsettings.json from publish to prevent overwrite on server" -ForegroundColor Yellow
}

# === ADD SCRIPTS TO PUBLISH BEFORE CREATING ARCHIVE (ONLY ONCE) ===
Write-Host "Checking if scripts are included in publish..." -ForegroundColor Green

# Create scripts directory in publish
$scriptsPublishPath = "../publish/scripts"
if (-not (Test-Path $scriptsPublishPath)) {
    New-Item -ItemType Directory -Path $scriptsPublishPath -Force
    Write-Host "Created scripts directory in publish" -ForegroundColor Green
}

# Copy scripts
if (Test-Path "deploy.sh") {
    Copy-Item "deploy.sh" $scriptsPublishPath -Force
    Write-Host "Copied deploy.sh to publish" -ForegroundColor Green
} else {
    Write-Host "Warning: deploy.sh not found in current directory" -ForegroundColor Yellow
}

if (Test-Path "deploy.ps1") {
    Copy-Item "deploy.ps1" $scriptsPublishPath -Force
    Write-Host "Copied deploy.ps1 to publish" -ForegroundColor Green
}

# Ensure service name in deploy.sh matches configuration
if (Test-Path "$scriptsPublishPath/deploy.sh") {
    $raw = Get-Content "$scriptsPublishPath/deploy.sh" -Raw
    $lines = $raw -split "`r?`n"
    $replaced = $false
    for ($i = 0; $i -lt $lines.Length; $i++) {
        if ($lines[$i] -match '^\s*SERVICE_NAME=') {
            $lines[$i] = 'SERVICE_NAME="' + $serviceName + '"'
            $replaced = $true
            break
        }
    }
    if (-not $replaced) {
        # Insert after APP_DIR if present, else after shebang, else at top
        $insertIdx = 0
        for ($j = 0; $j -lt $lines.Length; $j++) {
            if ($lines[$j] -match '^\s*APP_DIR=') { $insertIdx = $j + 1; break }
            if ($lines[$j] -match '^#!') { $insertIdx = [Math]::Max($insertIdx, $j + 1) }
        }
        $newLine = 'SERVICE_NAME="' + $serviceName + '"'
        if ($insertIdx -le 0) {
            $lines = @($newLine) + $lines
        } elseif ($insertIdx -ge $lines.Length) {
            $lines = $lines + @($newLine)
        } else {
            $before = @()
            if ($insertIdx -gt 0) { $before = $lines[0..($insertIdx-1)] }
            $after = @()
            if ($insertIdx -lt $lines.Length) { $after = $lines[$insertIdx..($lines.Length-1)] }
            $lines = $before + @($newLine) + $after
        }
    }
    $out = ($lines -join "`n")
    Set-Content -Path "$scriptsPublishPath/deploy.sh" -Value $out -NoNewline
    Write-Host "SERVICE_NAME in deploy.sh set to: $serviceName" -ForegroundColor Green
}

# Check that scripts are copied
if (Test-Path "$scriptsPublishPath/deploy.sh") {
    Write-Host "Deploy scripts are ready for archiving" -ForegroundColor Green
    
    # Check deploy.sh format
    Write-Host "Checking deploy.sh format..." -ForegroundColor Green
    $firstLine = Get-Content "$scriptsPublishPath/deploy.sh" -First 1
    if ($firstLine -eq "#!/bin/bash") {
        Write-Host "deploy.sh format is correct" -ForegroundColor Green
    } else {
        Write-Host "Warning: deploy.sh might have wrong format" -ForegroundColor Yellow
        Write-Host "First line: $firstLine" -ForegroundColor Gray
    }
    
    # Check file size
    $file = Get-Item "$scriptsPublishPath/deploy.sh"
    Write-Host "deploy.sh size: $($file.Length) bytes" -ForegroundColor Green
} else {
    Write-Host "Creating basic deploy.sh..." -ForegroundColor Yellow
    # Create basic deploy.sh
    $deployShContent = @'
#!/bin/bash

APP_DIR="/var/netcore"
SERVICE_NAME="Homie.service"

echo "Starting deployment..."

# Stop service
echo "Stopping service..."
sudo systemctl stop $SERVICE_NAME 2>/dev/null || true

# Wait a bit
sleep 3

# Set permissions
echo "Setting permissions..."
sudo chown -R www-data:www-data $APP_DIR 2>/dev/null || echo "chown skipped"
sudo chmod -R 755 $APP_DIR 2>/dev/null || echo "chmod skipped"

# Apply database migrations (if any)
echo "Applying database migrations..."
cd $APP_DIR
export ASPNETCORE_ENVIRONMENT=Production
export DOTNET_ROOT=/usr/lib/dotnet

if [ -f "Homie.dll" ]; then
    dotnet Homie.dll --migrate 2>/dev/null || echo "Migrations completed or not required"
fi

# Start service
echo "Starting service..."
sudo systemctl start $SERVICE_NAME

# Check status
sleep 5
echo "Service status:"
sudo systemctl status $SERVICE_NAME --no-pager -l

echo "Deployment completed successfully!"
'@
    Set-Content -Path "$scriptsPublishPath/deploy.sh" -Value $deployShContent
    Write-Host "Basic deploy.sh created" -ForegroundColor Green
    # Align service name in generated script
    (Get-Content "$scriptsPublishPath/deploy.sh" -Raw) -replace 'SERVICE_NAME="[^"]+"', ('SERVICE_NAME="' + $serviceName + '"') | Set-Content "$scriptsPublishPath/deploy.sh"
}

# Create archive
$timestamp = Get-Date -Format "yyyyMMddHHmmss"
$archiveName = "deploy-$timestamp.tar.gz"
Write-Host "Creating archive: $archiveName" -ForegroundColor Green

# Change to project root for archiving
Push-Location ..

try {
    # Check that publish folder exists
    if (-not (Test-Path "./publish")) {
        Write-Error "Publish directory not found!"
        exit 1
    }
    
    Write-Host "Publish directory content:" -ForegroundColor Yellow
    Get-ChildItem "./publish" | Select-Object Name, Length | Format-Table -AutoSize
    
    # Check for scripts in publish directory
    Write-Host "Checking for scripts in publish directory..." -ForegroundColor Green
    if (Test-Path "./publish/scripts/deploy.sh") {
        Write-Host "✓ deploy.sh found in publish directory" -ForegroundColor Green
    } else {
        Write-Host "✗ deploy.sh NOT found in publish directory" -ForegroundColor Red
    }
    
    if (Get-Command 7z -ErrorAction SilentlyContinue) {
        Write-Host "Using 7z for archiving..." -ForegroundColor Green
        7z a -ttar -so . "./publish" | 7z a -si -tgzip "./scripts/$archiveName"
    } else {
        Write-Host "Using tar for archiving..." -ForegroundColor Green
        # Use correct paths for tar
        tar -czf "./scripts/$archiveName" -C "./publish" .
    }
    
    # Check that archive is created
    if (-not (Test-Path "./scripts/$archiveName")) {
        Write-Error "Archive creation failed!"
        exit 1
    }
    
    Write-Host "Archive created successfully: $((Get-Item "./scripts/$archiveName").Length / 1MB) MB" -ForegroundColor Green
    
    # Check archive content
    Write-Host "Checking archive content..." -ForegroundColor Green
    try {
        $archiveContent = tar -tzf "./scripts/$archiveName" | Select-String "scripts/" | Select-Object -First 10
        if ($archiveContent) {
            Write-Host "Archive contains scripts:" -ForegroundColor Green
            $archiveContent | ForEach-Object { Write-Host "  $($_.Line)" -ForegroundColor Gray }
        } else {
            Write-Host "Warning: No scripts found in archive" -ForegroundColor Yellow
            # Show first 20 files in archive
            Write-Host "First 20 files in archive:" -ForegroundColor Yellow
            tar -tzf "./scripts/$archiveName" | Select-Object -First 20 | ForEach-Object { Write-Host "  $_" -ForegroundColor Gray }
        }
    }
    catch {
        Write-Host "Warning: Could not verify archive content" -ForegroundColor Yellow
    }
}
finally {
    Pop-Location
}

# Check if archive was created
if (-not (Test-Path $archiveName)) {
    Write-Error "Archive creation failed: $archiveName"
    exit 1
}

# Copy to server
Write-Host "Copying to server..." -ForegroundColor Green
Invoke-SCP -LocalPath $archiveName -RemotePath "/tmp/$archiveName" -KeyPath $config.SshKeyPath -Server $config.Server -Port $config.Port -Username $config.Username

# Run deploy on server
Write-Host "Running deploy on server..." -ForegroundColor Green

# Command 1: Reliable cleanup while preserving appsettings.json
$extractCommand = "cd /tmp && echo '=== Starting deployment ===' && if [ -d '/var/netcore' ]; then echo 'Backing up appsettings.json...' && if [ -f '/var/netcore/appsettings.json' ]; then sudo cp /var/netcore/appsettings.json /tmp/appsettings.backup && echo 'Backup created'; else echo 'No appsettings.json to backup'; fi && echo 'Cleaning directory...' && sudo find /var/netcore -mindepth 1 \! -name 'appsettings.json' -exec rm -rf {} \; 2>/dev/null || true && echo 'Restoring appsettings.json...' && if [ -f '/tmp/appsettings.backup' ]; then sudo mv /tmp/appsettings.backup /var/netcore/appsettings.json && echo 'appsettings.json restored'; fi; else echo 'Creating directory...' && sudo mkdir -p /var/netcore; fi && echo 'Extracting archive...' && sudo tar -xzf deploy-$timestamp.tar.gz -C /var/netcore && rm -f deploy-$timestamp.tar.gz && echo 'Extraction completed'"

Invoke-SSHCommand -Command $extractCommand -KeyPath $config.SshKeyPath -Server $config.Server -Port $config.Port -Username $config.Username

# Command 1.5: Check extracted scripts
$checkScriptsCommand = "echo '=== Checking for deploy scripts ===' && " +
                       "if [ -f '/var/netcore/scripts/deploy.sh' ]; then " +
                       "echo 'Deploy script found in archive' && " +
                       "ls -la /var/netcore/scripts/deploy.sh; " +
                       "else " +
                       "echo 'Deploy script not found in archive'; " +
                       "fi"

Invoke-SSHCommand -Command $checkScriptsCommand -KeyPath $config.SshKeyPath -Server $config.Server -Port $config.Port -Username $config.Username                       

# Command 2: Set permissions
$permissionsCommand = "echo 'Setting permissions...' && sudo chown -R www-data:www-data /var/netcore 2>/dev/null || echo 'chown failed, continuing...' && sudo chmod -R 755 /var/netcore && sudo find /var/netcore -type f -name '*.dll' -exec chmod 644 {} \; 2>/dev/null || true && sudo find /var/netcore -type f -name '*.json' -exec chmod 644 {} \; 2>/dev/null || true && sudo find /var/netcore -type f -name '*.exe' -exec chmod 755 {} \; 2>/dev/null || true && echo 'Permissions set'"

Invoke-SSHCommand -Command $permissionsCommand -KeyPath $config.SshKeyPath -Server $config.Server -Port $config.Port -Username $config.Username

# Command 3: Run deploy
$deployCommand = "if [ -f '/var/netcore/scripts/deploy.sh' ]; then " +
                 "echo 'Running deploy script from archive...' && " +
                 "sed -i 's/\\r\$//' /var/netcore/scripts/deploy.sh 2>/dev/null || sudo sed -i 's/\\r\$//' /var/netcore/scripts/deploy.sh && " +
                 "chmod +x /var/netcore/scripts/deploy.sh 2>/dev/null || sudo chmod +x /var/netcore/scripts/deploy.sh && " +
                 "/var/netcore/scripts/deploy.sh; " +
                 "else " +
                 "echo 'Deploy script not found, performing basic restart...' && " +
                 "systemctl --user stop $serviceName 2>/dev/null || true && " +
                 "sleep 2 && " +
                 "systemctl --user start $serviceName 2>/dev/null || echo 'User service start failed, trying system...' && " +
                 "sudo systemctl stop $serviceName 2>/dev/null || true && " +
                 "sudo systemctl start $serviceName 2>/dev/null || echo 'System service start failed' && " +
                 "sleep 3 && " +
                 "echo 'Basic restart completed' && " +
                 "systemctl --user status $serviceName --no-pager -l 2>/dev/null || sudo systemctl status $serviceName --no-pager -l; " +
                 "fi"

Invoke-SSHCommand -Command $deployCommand -KeyPath $config.SshKeyPath -Server $config.Server -Port $config.Port -Username $config.Username

# Command 4: Deployment verification
$verifyCommand = "echo '=== Deployment verification ===' && " +
                 "echo 'Main application files:' && " +
                 "find /var/netcore -maxdepth 1 -name '*.dll' -o -name '*.exe' | head -10 && " +
                 "echo 'Scripts directory:' && " +
                 "ls -la /var/netcore/scripts/ 2>/dev/null || echo 'No scripts directory' && " +
                 "echo 'appsettings.json:' && " +
                 "ls -la /var/netcore/appsettings.json && " +
                 "echo 'Deployment completed!'"

Invoke-SSHCommand -Command $verifyCommand -KeyPath $config.SshKeyPath -Server $config.Server -Port $config.Port -Username $config.Username

# Check service logs if there are issues
$logsCommand = "echo '=== Service logs (last 50 lines) ===' && " +
              "journalctl -u $serviceName -n 50 --no-pager 2>/dev/null || sudo journalctl -u $serviceName -n 50 --no-pager 2>/dev/null || echo 'Cannot access logs'"

Invoke-SSHCommand -Command $logsCommand -KeyPath $config.SshKeyPath -Server $config.Server -Port $config.Port -Username $config.Username

# Cleanup archives only if verification succeeded
$cleanupOnSuccessCommand = "echo '=== Post-deploy cleanup ===' && " +
                          "if [ -f '/var/netcore/Homie.dll' ]; then " +
                          "  if systemctl is-active --quiet $serviceName 2>/dev/null || sudo systemctl is-active --quiet $serviceName 2>/dev/null; then " +
                          "    echo 'Verified OK, removing deploy archives from /tmp' && rm -f /tmp/deploy-*.tar.gz 2>/dev/null || sudo -n rm -f /tmp/deploy-*.tar.gz 2>/dev/null || true; " +
                          "  else echo 'Service inactive, keeping archives in /tmp'; fi; " +
                          "else echo 'Homie.dll not found, keeping archives in /tmp'; fi"

Invoke-SSHCommand -Command $cleanupOnSuccessCommand -KeyPath $config.SshKeyPath -Server $config.Server -Port $config.Port -Username $config.Username

# Cleanup
Write-Host "Cleaning up..." -ForegroundColor Green
Remove-Item $archiveName -Force -ErrorAction SilentlyContinue
Remove-Item "../publish" -Recurse -Force -ErrorAction SilentlyContinue

Write-Host "Deploy completed successfully!" -ForegroundColor Green