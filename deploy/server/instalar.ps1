# Primera instalacion de Trato Directo en Windows Server + IIS. Ejecutar UNA vez, como Administrador,
# desde la carpeta del paquete descomprimido:
#
#   powershell -ExecutionPolicy Bypass -File server\instalar.ps1
#
# Requisitos previos (ver docs/DEPLOYMENT.md): .NET 10 Hosting Bundle y MySQL 8 instalados, y los DNS de
# tratodirecto.es, www.tratodirecto.es y admin.tratodirecto.es apuntando a este servidor.
#
# Que hace:
#   - Activa IIS con WebSockets (Blazor Server los necesita).
#   - Crea C:\TratoDirecto\{sites,storage,logs}, copia los 4 sitios y genera appsettings.Production.json con
#     claves JWT y contraseña de base de datos nuevas y aleatorias (solo quedan en este servidor).
#   - Crea la base de datos y su usuario (pide la contraseña de root de MySQL), aplica migraciones y catalogos.
#   - Crea los App Pools y sitios de IIS:
#       tratodirecto.es / www      -> portal (WEB), publico
#       admin.tratodirecto.es      -> panel (AdminWEB), publico
#       127.0.0.1:5000 / :5001     -> API y AdminAPI, SOLO locales (los usan WEB y AdminWEB desde el servidor)

param(
    [string]$Domain = "tratodirecto.es",
    [string]$Root = "C:\TratoDirecto",
    [string]$MySqlBin = "C:\Program Files\MySQL\MySQL Server 8.0\bin"
)

$ErrorActionPreference = "Stop"
$package = Split-Path $PSScriptRoot -Parent
$sites = @(
    # Bindings: IP, puerto, host. HTTPS se agrega despues con win-acme.
    @{ Name = "API";      Pool = "TD-API";      Bindings = @(,@("127.0.0.1", 5000, "")) },
    @{ Name = "AdminAPI"; Pool = "TD-AdminAPI"; Bindings = @(,@("127.0.0.1", 5001, "")) },
    @{ Name = "WEB";      Pool = "TD-WEB";      Bindings = @(@("*", 80, $Domain), @("*", 80, "www.$Domain")) },
    @{ Name = "AdminWEB"; Pool = "TD-AdminWEB"; Bindings = @(,@("*", 80, "admin.$Domain")) }
)

function New-Secret([int]$bytes) {
    $b = New-Object byte[] $bytes
    [System.Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($b)
    return [Convert]::ToBase64String($b).Replace("+", "A").Replace("/", "B").TrimEnd("=")
}

# --- 1. IIS y requisitos --------------------------------------------------------------------------
Write-Host "1/6 IIS y WebSockets..." -ForegroundColor Cyan
Install-WindowsFeature Web-Server, Web-WebSockets, Web-Mgmt-Console | Out-Null
$runtimes = & dotnet --list-runtimes 2>$null
if (-not ($runtimes -match "Microsoft.AspNetCore.App 10\.")) {
    throw "Falta el .NET 10 Hosting Bundle (https://dotnet.microsoft.com/download/dotnet/10.0). Instalalo, reinicia IIS (iisreset) y vuelve a ejecutar."
}
if (-not (Test-Path "$MySqlBin\mysql.exe")) { throw "No encuentro mysql.exe en $MySqlBin. Pasa la ruta con -MySqlBin." }
Import-Module WebAdministration

# --- 2. Carpetas y archivos ------------------------------------------------------------------------
Write-Host "2/6 Carpetas y sitios..." -ForegroundColor Cyan
$storage = "$Root\storage"
New-Item -ItemType Directory -Force "$Root\sites", "$storage\cv", "$storage\photos", "$Root\logs" | Out-Null
foreach ($s in $sites) {
    robocopy "$package\sites\$($s.Name)" "$Root\sites\$($s.Name)" /MIR /XF appsettings.Production.json /NFL /NDL /NJH /NJS /NP | Out-Null
    if ($LASTEXITCODE -ge 8) { throw "Fallo al copiar $($s.Name)" }
}

# --- 3. Configuracion de produccion con secretos nuevos -------------------------------------------
Write-Host "3/6 Configuracion de produccion..." -ForegroundColor Cyan
$apiConfig = "$Root\sites\API\appsettings.Production.json"
if (Test-Path $apiConfig) {
    Write-Host "   appsettings.Production.json ya existe: se conserva (no se regeneran claves)." -ForegroundColor Yellow
    $dbPassword = $null
} else {
    $dbPassword = New-Secret 24
    $values = @{
        "__DB_PASSWORD__" = $dbPassword
        "__JWT_PORTAL__"  = New-Secret 48
        "__JWT_ADMIN__"   = New-Secret 48
        "__STORAGE__"     = $storage.Replace("\", "\\")
        "__DOMAIN__"      = $Domain
    }
    foreach ($s in $sites) {
        $text = Get-Content "$PSScriptRoot\config\$($s.Name).Production.json" -Raw
        foreach ($k in $values.Keys) { $text = $text.Replace($k, $values[$k]) }
        [System.IO.File]::WriteAllText("$Root\sites\$($s.Name)\appsettings.Production.json", $text, (New-Object System.Text.UTF8Encoding $false))
    }
}

# --- 4. Base de datos --------------------------------------------------------------------------------
Write-Host "4/6 Base de datos..." -ForegroundColor Cyan
if ($dbPassword) {
    $createSql = @"
CREATE DATABASE IF NOT EXISTS TratoDirectoDb CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
CREATE USER IF NOT EXISTS 'tratodirecto'@'localhost' IDENTIFIED BY '$dbPassword';
CREATE USER IF NOT EXISTS 'tratodirecto'@'127.0.0.1' IDENTIFIED BY '$dbPassword';
GRANT ALL PRIVILEGES ON TratoDirectoDb.* TO 'tratodirecto'@'localhost';
GRANT ALL PRIVILEGES ON TratoDirectoDb.* TO 'tratodirecto'@'127.0.0.1';
FLUSH PRIVILEGES;
"@
    Write-Host "   Escribe la contraseña de root de MySQL cuando la pida:" -ForegroundColor Yellow
    $createSql | & "$MySqlBin\mysql.exe" -uroot -p
    if ($LASTEXITCODE -ne 0) { throw "No se pudo crear la base de datos" }
}
& "$PSScriptRoot\migrar.ps1" -Root $Root -MySqlBin $MySqlBin -Package $package -WithCatalogs

# --- 5. IIS: App Pools y sitios ------------------------------------------------------------------
Write-Host "5/6 App Pools y sitios de IIS..." -ForegroundColor Cyan
foreach ($s in $sites) {
    if (-not (Test-Path "IIS:\AppPools\$($s.Pool)")) { New-WebAppPool -Name $s.Pool | Out-Null }
    Set-ItemProperty "IIS:\AppPools\$($s.Pool)" -Name managedRuntimeVersion -Value ""
    Set-ItemProperty "IIS:\AppPools\$($s.Pool)" -Name startMode -Value "AlwaysRunning"
    Set-ItemProperty "IIS:\AppPools\$($s.Pool)" -Name processModel.loadUserProfile -Value $true   # claves de Data Protection
    Set-ItemProperty "IIS:\AppPools\$($s.Pool)" -Name processModel.idleTimeout -Value "00:00:00"

    $siteName = "TD-$($s.Name)"
    if (-not (Test-Path "IIS:\Sites\$siteName")) {
        $first = $s.Bindings[0]
        New-Website -Name $siteName -PhysicalPath "$Root\sites\$($s.Name)" -ApplicationPool $s.Pool `
            -IPAddress $first[0] -Port $first[1] -HostHeader $first[2] | Out-Null
        foreach ($b in $s.Bindings | Select-Object -Skip 1) {
            New-WebBinding -Name $siteName -Protocol http -IPAddress $b[0] -Port $b[1] -HostHeader $b[2]
        }
    }

    # Cada App Pool lee su sitio y escribe en logs; solo los API escriben en storage (CV y fotos).
    icacls "$Root\sites\$($s.Name)" /grant "IIS AppPool\$($s.Pool):(OI)(CI)RX" /T /Q | Out-Null
    icacls "$Root\logs" /grant "IIS AppPool\$($s.Pool):(OI)(CI)M" /Q | Out-Null
}
foreach ($pool in "TD-API", "TD-AdminAPI") {
    icacls $storage /grant "IIS AppPool\${pool}:(OI)(CI)M" /T /Q | Out-Null
}
# Nadie mas que Administradores y los App Pools de los API puede leer los CV.
icacls $storage /inheritance:r /grant "Administrators:(OI)(CI)F" "SYSTEM:(OI)(CI)F" /Q | Out-Null

# --- 6. Arranque y comprobacion ------------------------------------------------------------------
Write-Host "6/6 Arrancando..." -ForegroundColor Cyan
foreach ($s in $sites) { Start-WebAppPool -Name $s.Pool -ErrorAction SilentlyContinue; Start-Website -Name "TD-$($s.Name)" }
Start-Sleep -Seconds 5
foreach ($u in "http://127.0.0.1:5000/api/legal/identity", "http://127.0.0.1:5001/") {
    try { $r = Invoke-WebRequest $u -UseBasicParsing -TimeoutSec 30; Write-Host "   $u -> $($r.StatusCode)" }
    catch { Write-Host "   $u -> $($_.Exception.Message)" -ForegroundColor Yellow }
}

Write-Host ""
Write-Host "Instalacion terminada. Siguientes pasos (docs/DEPLOYMENT.md):" -ForegroundColor Green
Write-Host "  1. Certificados HTTPS con win-acme para $Domain, www.$Domain y admin.$Domain."
Write-Host "  2. Registrar la cuenta del administrador en https://$Domain/register y ejecutar server\crear-admin.sql."
Write-Host "  3. Rellenar Datos de la Empresa, Correo (SMTP) y demas ajustes en https://admin.$Domain."
