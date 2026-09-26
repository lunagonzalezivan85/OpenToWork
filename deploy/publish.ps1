# Genera el paquete de despliegue de Trato Directo en deploy\out (se ejecuta en el PC de desarrollo).
#
#   powershell -ExecutionPolicy Bypass -File deploy\publish.ps1
#
# Resultado: deploy\out\TratoDirecto-<fecha>.zip con
#   sites\API, sites\AdminAPI, sites\WEB, sites\AdminWEB   (dotnet publish Release, sin configuracion de desarrollo)
#   db\efbundle.exe                                         (aplica las migraciones pendientes)
#   db\catalogos.sql                                        (catalogos y configuracion, sin usuarios ni candidatos)
#   server\*                                                (scripts y plantillas para el servidor)
# deploy\out esta ignorado por git: el paquete nunca se sube al repositorio.

param(
    [string]$MySqlBin = "C:\xampp\mysql\bin",
    [string]$Database = "OpenToWorkDb"
)

$ErrorActionPreference = "Stop"
$root = Split-Path $PSScriptRoot -Parent
$out = Join-Path $PSScriptRoot "out"
$stage = Join-Path $out "package"

if (Test-Path $stage) { Remove-Item $stage -Recurse -Force }
New-Item -ItemType Directory -Force "$stage\sites", "$stage\db" | Out-Null

# 1. Los 4 proyectos
foreach ($p in "API", "AdminAPI", "WEB", "AdminWEB") {
    Write-Host "Publicando $p..." -ForegroundColor Cyan
    dotnet publish "$root\src\OpenToWork.$p\OpenToWork.$p.csproj" -c Release -o "$stage\sites\$p" --nologo -v q
    if ($LASTEXITCODE -ne 0) { throw "Fallo al publicar $p" }

    # Nunca llevar configuracion de desarrollo ni archivos subidos al servidor.
    Remove-Item "$stage\sites\$p\appsettings.Development.json" -ErrorAction SilentlyContinue
    Remove-Item "$stage\sites\$p\appsettings.Production.json" -ErrorAction SilentlyContinue
    Remove-Item "$stage\sites\$p\wwwroot\uploads" -Recurse -Force -ErrorAction SilentlyContinue
}

# 2. Migraciones como ejecutable (no hace falta el SDK de .NET en el servidor)
Write-Host "Generando efbundle.exe (migraciones)..." -ForegroundColor Cyan
dotnet ef migrations bundle --project "$root\src\OpenToWork.Models" --startup-project "$root\src\OpenToWork.Models" `
    --self-contained -r win-x64 -o "$stage\db\efbundle.exe" --force
if ($LASTEXITCODE -ne 0) { throw "Fallo al generar efbundle.exe" }

# 3. Catalogos y configuracion (base limpia: sin usuarios, candidatos, empresas ni contratos).
#    SMTP y codigos promocionales no se copian: se configuran desde el admin en el servidor.
Write-Host "Exportando catalogos..." -ForegroundColor Cyan
$dump = Join-Path $MySqlBin "mysqldump.exe"
$common = @("-uroot", "--no-create-info", "--replace", "--complete-insert", "--skip-triggers",
            "--default-character-set=utf8mb4", "--skip-comments", $Database)
$catalogTables = @("SY_DocumentTypes", "SY_WizardSteps", "PT_Plans", "PT_Skills",
                   "PT_JobTypes", "PT_JobLevels", "PT_JobTypePrices", "PT_JobTypeSkills")
$sql = @("-- Catalogos de Trato Directo generados el $(Get-Date -Format 'yyyy-MM-dd HH:mm'). Se aplican despues de efbundle.exe.",
         "SET NAMES utf8mb4;", "SET FOREIGN_KEY_CHECKS=0;")
$sql += & $dump @common @catalogTables
$sql += & $dump @common "SY_SystemConfig" "--where=``Key`` NOT LIKE 'smtp%'"
$sql += "SET FOREIGN_KEY_CHECKS=1;"
[System.IO.File]::WriteAllLines("$stage\db\catalogos.sql", $sql, (New-Object System.Text.UTF8Encoding $false))

# 4. Scripts y plantillas del servidor
Copy-Item "$PSScriptRoot\server" "$stage\server" -Recurse

# 5. Zip
$zip = Join-Path $out "TratoDirecto-$(Get-Date -Format 'yyyyMMdd-HHmm').zip"
Compress-Archive -Path "$stage\*" -DestinationPath $zip -Force
Write-Host "Paquete listo: $zip" -ForegroundColor Green
