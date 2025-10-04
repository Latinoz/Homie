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

# === ДОБАВЛЯЕМ СКРИПТЫ В PUBLISH ПЕРЕД СОЗДАНИЕМ АРХИВА (ТОЛЬКО ОДИН РАЗ) ===
Write-Host "Checking if scripts are included in publish..." -ForegroundColor Green

# Создаем директорию scripts в publish
$scriptsPublishPath = "../publish/scripts"
if (-not (Test-Path $scriptsPublishPath)) {
    New-Item -ItemType Directory -Path $scriptsPublishPath -Force
    Write-Host "Created scripts directory in publish" -ForegroundColor Green
}

# Копируем скрипты
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

# Проверка что скрипты скопированы
if (Test-Path "$scriptsPublishPath/deploy.sh") {
    Write-Host "Deploy scripts are ready for archiving" -ForegroundColor Green
    
    # Проверка формата deploy.sh
    Write-Host "Checking deploy.sh format..." -ForegroundColor Green
    $firstLine = Get-Content "$scriptsPublishPath/deploy.sh" -First 1
    if ($firstLine -eq "#!/bin/bash") {
        Write-Host "deploy.sh format is correct" -ForegroundColor Green
    } else {
        Write-Host "Warning: deploy.sh might have wrong format" -ForegroundColor Yellow
        Write-Host "First line: $firstLine" -ForegroundColor Gray
    }
    
    # Проверим права на файл
    $file = Get-Item "$scriptsPublishPath/deploy.sh"
    Write-Host "deploy.sh size: $($file.Length) bytes" -ForegroundColor Green
} else {
    Write-Host "Creating basic deploy.sh..." -ForegroundColor Yellow
    # Создаем базовый deploy.sh
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
}

# Create archive
$timestamp = Get-Date -Format "yyyyMMddHHmmss"
$archiveName = "deploy-$timestamp.tar.gz"
Write-Host "Creating archive: $archiveName" -ForegroundColor Green

# Change to project root for archiving
Push-Location ..

try {
    # Проверяем что папка publish существует
    if (-not (Test-Path "./publish")) {
        Write-Error "Publish directory not found!"
        exit 1
    }
    
    Write-Host "Publish directory content:" -ForegroundColor Yellow
    Get-ChildItem "./publish" | Select-Object Name, Length | Format-Table -AutoSize
    
    # Проверка архива перед созданием
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
        # Используем правильные пути для tar
        tar -czf "./scripts/$archiveName" -C "./publish" .
    }
    
    # Проверяем что архив создан
    if (-not (Test-Path "./scripts/$archiveName")) {
        Write-Error "Archive creation failed!"
        exit 1
    }
    
    Write-Host "Archive created successfully: $((Get-Item "./scripts/$archiveName").Length / 1MB) MB" -ForegroundColor Green
    
    # Проверка содержимого архива
    Write-Host "Checking archive content..." -ForegroundColor Green
    try {
        $archiveContent = tar -tzf "./scripts/$archiveName" | Select-String "scripts/" | Select-Object -First 10
        if ($archiveContent) {
            Write-Host "Archive contains scripts:" -ForegroundColor Green
            $archiveContent | ForEach-Object { Write-Host "  $($_.Line)" -ForegroundColor Gray }
        } else {
            Write-Host "Warning: No scripts found in archive" -ForegroundColor Yellow
            # Покажем что вообще есть в архиве
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

# === УДАЛЯЕМ ДУБЛИРОВАННЫЙ КОД ОТСЮДА ДО КОНЦА ФАЙЛА ===

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

# Команда 1: Надежная очистка с сохранением appsettings.json
$extractCommand = "cd /tmp && echo '=== Starting deployment ===' && if [ -d '/var/netcore' ]; then echo 'Backing up appsettings.json...' && if [ -f '/var/netcore/appsettings.json' ]; then cp /var/netcore/appsettings.json /tmp/appsettings.backup && echo 'Backup created'; else echo 'No appsettings.json to backup'; fi && echo 'Cleaning directory...' && find /var/netcore -mindepth 1 \! -name 'appsettings.json' -exec rm -rf {} \; 2>/dev/null || true && echo 'Restoring appsettings.json...' && if [ -f '/tmp/appsettings.backup' ]; then mv /tmp/appsettings.backup /var/netcore/appsettings.json && echo 'appsettings.json restored'; fi; else echo 'Creating directory...' && mkdir -p /var/netcore; fi && echo 'Extracting archive...' && tar -xzf deploy-$timestamp.tar.gz -C /var/netcore && rm deploy-$timestamp.tar.gz && echo 'Extraction completed'"

Invoke-SSHCommand -Command $extractCommand -KeyPath $config.SshKeyPath -Server $config.Server -Port $config.Port -Username $config.Username

# Команда 1.5: Проверка распакованных скриптов
$checkScriptsCommand = "echo '=== Checking for deploy scripts ===' && " +
                       "if [ -f '/var/netcore/scripts/deploy.sh' ]; then " +
                       "echo 'Deploy script found in archive' && " +
                       "ls -la /var/netcore/scripts/deploy.sh; " +
                       "else " +
                       "echo 'Deploy script not found in archive'; " +
                       "fi"

Invoke-SSHCommand -Command $checkScriptsCommand -KeyPath $config.SshKeyPath -Server $config.Server -Port $config.Port -Username $config.Username                       

# Команда 2: Установка прав
$permissionsCommand = "echo 'Setting permissions...' && chown -R www-data:www-data /var/netcore 2>/dev/null || echo 'chown failed, continuing...' && chmod -R 755 /var/netcore && find /var/netcore -type f -name '*.dll' -exec chmod 644 {} \; 2>/dev/null || true && find /var/netcore -type f -name '*.json' -exec chmod 644 {} \; 2>/dev/null || true && find /var/netcore -type f -name '*.exe' -exec chmod 755 {} \; 2>/dev/null || true && echo 'Permissions set'"

Invoke-SSHCommand -Command $permissionsCommand -KeyPath $config.SshKeyPath -Server $config.Server -Port $config.Port -Username $config.Username

# Команда 3: Запуск деплоя
$deployCommand = "if [ -f '/var/netcore/scripts/deploy.sh' ]; then " +
                 "echo 'Running deploy script from archive...' && " +
                 "sed -i 's/\\r\$//' /var/netcore/scripts/deploy.sh && " +
                 "chmod +x /var/netcore/scripts/deploy.sh && " +
                 "/var/netcore/scripts/deploy.sh; " +
                 "else " +
                 "echo 'Deploy script not found, performing basic restart...' && " +
                 "systemctl --user stop Homie.service 2>/dev/null || true && " +
                 "sleep 2 && " +
                 "systemctl --user start Homie.service 2>/dev/null || echo 'User service start failed, trying system...' && " +
                 "sudo systemctl stop Homie.service 2>/dev/null || true && " +
                 "sudo systemctl start Homie.service 2>/dev/null || echo 'System service start failed' && " +
                 "sleep 3 && " +
                 "echo 'Basic restart completed' && " +
                 "systemctl --user status Homie.service --no-pager -l 2>/dev/null || sudo systemctl status Homie.service --no-pager -l; " +
                 "fi"

Invoke-SSHCommand -Command $deployCommand -KeyPath $config.SshKeyPath -Server $config.Server -Port $config.Port -Username $config.Username

# Команда 4: Проверка деплоя
$verifyCommand = "echo '=== Deployment verification ===' && " +
                 "echo 'Main application files:' && " +
                 "find /var/netcore -maxdepth 1 -name '*.dll' -o -name '*.exe' | head -10 && " +
                 "echo 'Scripts directory:' && " +
                 "ls -la /var/netcore/scripts/ 2>/dev/null || echo 'No scripts directory' && " +
                 "echo 'appsettings.json:' && " +
                 "ls -la /var/netcore/appsettings.json && " +
                 "echo 'Deployment completed!'"

Invoke-SSHCommand -Command $verifyCommand -KeyPath $config.SshKeyPath -Server $config.Server -Port $config.Port -Username $config.Username

# Cleanup
Write-Host "Cleaning up..." -ForegroundColor Green
Remove-Item $archiveName -Force -ErrorAction SilentlyContinue
Remove-Item "../publish" -Recurse -Force -ErrorAction SilentlyContinue

Write-Host "Deploy completed successfully!" -ForegroundColor Green