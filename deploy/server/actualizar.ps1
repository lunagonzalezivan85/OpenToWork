# Actualiza Trato Directo en el servidor con un paquete nuevo (generado con deploy\publish.ps1).
# Ejecutar como Administrador desde la carpeta del paquete descomprimido:
#
#   powershell -ExecutionPolicy Bypass -File server\actualizar.ps1
#
# Pone los sitios en mantenimiento (app_offline.htm), copia los archivos nuevos sin tocar
# appsettings.Production.json ni storage, aplica las migraciones pendientes y vuelve a abrir.

param(
    [string]$Root = "C:\TratoDirecto",
    [string]$MySqlBin = "C:\Program Files\MySQL\MySQL Server 8.0\bin"
)

$ErrorActionPreference = "Stop"
$package = Split-Path $PSScriptRoot -Parent
$names = "API", "AdminAPI", "WEB", "AdminWEB"
$offline = "<html><body style='font-family:sans-serif;text-align:center;padding:4rem'><h2>Trato Directo</h2><p>Estamos actualizando el sistema. Vuelve en unos minutos.</p></body></html>"

# Copia de seguridad de la base antes de migrar (pide la contraseña de root de MySQL).
$backup = "$Root\backups\TratoDirectoDb-$(Get-Date -Format 'yyyyMMdd-HHmm').sql"
New-Item -ItemType Directory -Force "$Root\backups" | Out-Null
Write-Host "Copia de seguridad de la base en $backup (escribe la contraseña de root de MySQL):" -ForegroundColor Yellow
& "$MySqlBin\mysqldump.exe" -uroot -p --single-transaction --routines --result-file=$backup TratoDirectoDb
if ($LASTEXITCODE -ne 0) { throw "Fallo la copia de seguridad; no se actualizo nada." }

try {
    foreach ($n in $names) { Set-Content "$Root\sites\$n\app_offline.htm" $offline -Encoding UTF8 }
    Start-Sleep -Seconds 3

    foreach ($n in $names) {
        robocopy "$package\sites\$n" "$Root\sites\$n" /MIR /XF appsettings.Production.json app_offline.htm /NFL /NDL /NJH /NJS /NP | Out-Null
        if ($LASTEXITCODE -ge 8) { throw "Fallo al copiar $n" }
    }

    & "$PSScriptRoot\migrar.ps1" -Root $Root -MySqlBin $MySqlBin -Package $package
}
finally {
    foreach ($n in $names) { Remove-Item "$Root\sites\$n\app_offline.htm" -ErrorAction SilentlyContinue }
}

Write-Host "Actualizacion terminada." -ForegroundColor Green
