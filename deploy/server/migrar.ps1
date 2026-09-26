# Aplica las migraciones pendientes (efbundle.exe del paquete) a la base de Trato Directo del servidor.
# Lo llaman instalar.ps1 (con -WithCatalogs, base nueva) y actualizar.ps1. Usa la cadena de conexion de
# C:\TratoDirecto\sites\API\appsettings.Production.json, asi la contraseña nunca se escribe a mano.

param(
    [string]$Root = "C:\TratoDirecto",
    [string]$MySqlBin = "C:\Program Files\MySQL\MySQL Server 8.0\bin",
    [string]$Package = (Split-Path $PSScriptRoot -Parent),
    [switch]$WithCatalogs
)

$ErrorActionPreference = "Stop"
$config = Get-Content "$Root\sites\API\appsettings.Production.json" -Raw | ConvertFrom-Json
$cs = $config.ConnectionStrings.DefaultConnection

Write-Host "   Aplicando migraciones..." -ForegroundColor Cyan
& "$Package\db\efbundle.exe" --connection $cs
if ($LASTEXITCODE -ne 0) { throw "Fallo al aplicar las migraciones" }

if ($WithCatalogs) {
    Write-Host "   Cargando catalogos y configuracion..." -ForegroundColor Cyan
    $parts = @{}
    foreach ($kv in $cs.Split(";")) { if ($kv -match "^\s*([^=]+)=(.*)$") { $parts[$Matches[1].Trim().ToLower()] = $Matches[2] } }
    $env:MYSQL_PWD = $parts["password"]
    try {
        Get-Content "$Package\db\catalogos.sql" -Raw -Encoding UTF8 |
            & "$MySqlBin\mysql.exe" "-h$($parts['server'])" "-P$($parts['port'])" "-u$($parts['user'])" --default-character-set=utf8mb4 $parts["database"]
        if ($LASTEXITCODE -ne 0) { throw "Fallo al cargar catalogos.sql" }
    } finally {
        Remove-Item Env:\MYSQL_PWD -ErrorAction SilentlyContinue
    }
}
