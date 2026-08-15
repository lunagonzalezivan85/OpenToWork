# Guia de Despliegue - OpenToWork

> **Objetivo:** Desplegar los 4 proyectos (API, AdminAPI, WEB, AdminWEB) desde tu maquina local a un Windows Server con IIS, sin entrar manualmente al servidor a subir archivos.

---

## Arquitectura de despliegue

```
Tu PC (Windows)                          Windows Server (IIS)
┌─────────────────┐    Web Deploy /     ┌──────────────────────┐
│  dotnet publish │ ── PowerShell ────> │  IIS                 │
│  (build local)  │    Remoting         │  ├── tudominio.com        (WEB)       │
│                 │                     │  ├── api.tudominio.com    (API)       │
│  GitHub repo    │                     │  ├── admin.tudominio.com  (AdminWEB)  │
└─────────────────┘                     │  └── admin-api.tudominio.com (AdminAPI)│
                                        │  MySQL (localhost:3306)              │
                                        └──────────────────────┘
```

---

## Opcion A: Web Deploy (recomendado)

Microsoft Web Deploy (`msdeploy`) permite publicar directamente desde tu PC al IIS remoto en un solo comando.

### 1. Configurar el servidor (una sola vez)

#### Instalar Web Deploy en el servidor
1. Descargar **Web Deploy 3.6+** desde Microsoft: `https://www.iis.net/downloads/microsoft/web-deploy`
2. Instalar con la opcion **Complete**
3. Verificar que el servicio **Web Deployment Agent Service** (MsDepSvc) este corriendo:
   ```powershell
   Get-Service MsDepSvc
   # Si no corre:
   Start-Service MsDepSvc
   Set-Service MsDepSvc -StartupType Automatic
   ```

#### Abrir puerto en firewall
```powershell
# En el servidor: abrir puerto 8172 (Web Deploy) o 80/443
New-NetFirewallRule -DisplayName "Web Deploy" -Direction Inbound -Protocol TCP -LocalPort 8172 -Action Allow
```

#### Crear sitios en IIS (una sola vez)
```powershell
# En el servidor via PowerShell (ejecutar como Admin)
Import-Module WebAdministration

# Crear App Pools (No Managed Code - .NET lo maneja el modulo)
@("OpenToWork-API","OpenToWork-AdminAPI","OpenToWork-WEB","OpenToWork-AdminWEB") | ForEach-Object {
    New-WebAppPool -Name $_
    Set-ItemProperty "IIS:\AppPools\$_" managedRuntimeVersion ""
}

# Crear sitios
New-Website -Name "OpenToWork-API" -PhysicalPath "C:\inetpub\OpenToWork\API" -ApplicationPool "OpenToWork-API" -Port 443 -HostHeader "api.tudominio.com" -Ssl
New-Website -Name "OpenToWork-AdminAPI" -PhysicalPath "C:\inetpub\OpenToWork\AdminAPI" -ApplicationPool "OpenToWork-AdminAPI" -Port 443 -HostHeader "admin-api.tudominio.com" -Ssl
New-Website -Name "OpenToWork-WEB" -PhysicalPath "C:\inetpub\OpenToWork\WEB" -ApplicationPool "OpenToWork-WEB" -Port 443 -HostHeader "tudominio.com" -Ssl
New-Website -Name "OpenToWork-AdminWEB" -PhysicalPath "C:\inetpub\OpenToWork\AdminWEB" -ApplicationPool "OpenToWork-AdminWEB" -Port 443 -HostHeader "admin.tudominio.com" -Ssl
```

### 2. Crear perfil de publicacion (.pubxml) en tu PC

Crear archivos `.pubxml` en cada proyecto bajo `Properties/PublishProfiles/`:

#### `src/OpenToWork.API/Properties/PublishProfiles/Production.pubxml`
```xml
<?xml version="1.0" encoding="utf-8"?>
<Project>
  <PropertyGroup>
    <DeleteExistingFiles>False</DeleteExistingFiles>
    <ExcludeApp_Data>False</ExcludeApp_Data>
    <LaunchSiteAfterPublish>True</LaunchSiteAfterPublish>
    <LastUsedBuildConfiguration>Release</LastUsedBuildConfiguration>
    <LastUsedPlatform>Any CPU</LastUsedPlatform>
    <PublishProvider>MSDeploy</PublishProvider>
    <PublishMethod>MSDeploy</PublishMethod>
    <MSDeployServiceURL>https://TU_SERVIDOR:8172/msdeploy.axd</MSDeployServiceURL>
    <DeployIisAppPath>OpenToWork-API</DeployIisAppPath>
    <RemoteAgentPhysicalPath />
    <AuthType>Basic</AuthType>
    <UserName>Administrator</UserName>
    <_SavePWD>true</_SavePWD>
    <EnableMSDeployBackup>True</EnableMSDeployBackup>
  </PropertyGroup>
</Project>
```

Repetir para AdminAPI, WEB y AdminWEB cambiando `<DeployIisAppPath>`:
- AdminAPI → `OpenToWork-AdminAPI`
- WEB → `OpenToWork-WEB`
- AdminWEB → `OpenToWork-AdminWEB`

### 3. Desplegar desde tu PC (un solo comando por proyecto)

```powershell
# Desde c:\Proyectos\OpenToWork\src
dotnet publish OpenToWork.API\OpenToWork.API.csproj /p:PublishProfile=Production /p:Password=PASSWORD_ADMIN_SERVER
dotnet publish OpenToWork.AdminAPI\OpenToWork.AdminAPI.csproj /p:PublishProfile=Production /p:Password=PASSWORD_ADMIN_SERVER
dotnet publish OpenToWork.WEB\OpenToWork.WEB.csproj /p:PublishProfile=Production /p:Password=PASSWORD_ADMIN_SERVER
dotnet publish OpenToWork.AdminWEB\OpenToWork.AdminWEB.csproj /p:PublishProfile=Production /p:Password=PASSWORD_ADMIN_SERVER
```

> **Importante:** El password es el del usuario de Windows del servidor (Administrator o un usuario con permisos de IIS).

---

## Opcion B: PowerShell Remoting (sin Web Deploy)

Si no quieres instalar Web Deploy en el servidor, puedes usar PowerShell Remoting para copilar localmente y copiar via red.

### 1. Habilitar PSRemoting en el servidor (una sola vez)
```powershell
# En el servidor (como Admin)
Enable-PSRemoting -Force
Set-Item WSMan:\localhost\Client\TrustedHosts -Value "*" -Force
```

### 2. Script de despliegue desde tu PC

Crear `deploy.ps1` en la raiz del proyecto:

```powershell
# deploy.ps1 - Despliegue remoto de OpenToWork via PowerShell Remoting
# Uso: .\deploy.ps1 -ServerIp "192.168.1.100" -ServerUser "Administrator" -ServerPass "tu_password"

param(
    [Parameter(Mandatory=$true)]
    [string]$ServerIp,
    [Parameter(Mandatory=$true)]
    [string]$ServerUser,
    [Parameter(Mandatory=$true)]
    [string]$ServerPass
)

$ErrorActionPreference = "Stop"
$projects = @("API","AdminAPI","WEB","AdminWEB")
$publishBase = "C:\Proyectos\OpenToWork\src"
$remoteBase = "C:\inetpub\OpenToWork"

Write-Host "=== OpenToWork Deploy ===" -ForegroundColor Cyan

# 1. Build local
foreach ($proj in $projects) {
    $csproj = "$publishBase\OpenToWork.$proj\OpenToWork.$proj.csproj"
    $outDir = "$publishBase\publish\$proj"
    Write-Host "[BUILD] OpenToWork.$proj..." -ForegroundColor Yellow
    dotnet publish $csproj -c Release -o $outDir --no-restore
    if ($LASTEXITCODE -ne 0) { Write-Host "ERROR en build de $proj" -ForegroundColor Red; exit 1 }
}

# 2. Conectar al servidor
$securePass = ConvertTo-SecureString $ServerPass -AsPlainText -Force
$cred = New-Object System.Management.Automation.PSCredential($ServerUser, $securePass)
$session = New-PSSession -ComputerName $ServerIp -Credential $cred

Write-Host "[CONNECT] Sesion remota establecida con $ServerIp" -ForegroundColor Green

# 3. Crear carpetas remotas (si no existen)
Invoke-Command -Session $session -ScriptBlock {
    foreach ($proj in $using:projects) {
        $path = "$using:remoteBase\$proj"
        if (!(Test-Path $path)) { New-Item -ItemType Directory -Path $path -Force }
    }
}

# 4. Copiar archivos al servidor
foreach ($proj in $projects) {
    $localPath = "$publishBase\publish\$proj\*"
    $remotePath = "$remoteBase\$proj"
    Write-Host "[COPY] $proj -> $remotePath" -ForegroundColor Yellow
    Copy-Item -Path $localPath -Destination $remotePath -Recurse -Force -ToSession $session
}

# 5. Aplicar migraciones en el servidor
Write-Host "[MIGRATE] Aplicando migraciones..." -ForegroundColor Yellow
Invoke-Command -Session $session -ScriptBlock {
    $apiPath = "$using:remoteBase\API"
    $conn = "Server=localhost;Database=OpenToWorkDb;User=root;Password=;"
    & dotnet "$apiPath\OpenToWork.API.dll" --ef-migrate --connection "$conn" 2>$null
    # Alternativa: ejecutar SQL script
}

# 6. Reciclar App Pools
Invoke-Command -Session $session -ScriptBlock {
    Import-Module WebAdministration
    foreach ($proj in $using:projects) {
        $pool = "OpenToWork-$proj"
        Restart-WebAppPool -Name $pool
        Write-Host "[RECYCLE] AppPool $pool reciclado" -ForegroundColor Green
    }
}

Remove-PSSession $session
Write-Host "=== Despliegue completado ===" -ForegroundColor Cyan
```

### 3. Ejecutar el despliegue
```powershell
.\deploy.ps1 -ServerIp "192.168.1.100" -ServerUser "Administrator" -ServerPass "tu_password"
```

---

## Opcion C: GitHub Actions CI/CD (automatizado)

Despliegue automatico cada vez que haces push a `main`. No necesitas ejecutar nada manualmente.

### 1. Crear ` .github/workflows/deploy.yml`

```yaml
name: Deploy to Production

on:
  push:
    branches: [ main ]

jobs:
  deploy:
    runs-on: windows-latest
    steps:
      - uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'

      - name: Build API
        run: dotnet publish src/OpenToWork.API/OpenToWork.API.csproj -c Release -o publish/API

      - name: Build AdminAPI
        run: dotnet publish src/OpenToWork.AdminAPI/OpenToWork.AdminAPI.csproj -c Release -o publish/AdminAPI

      - name: Build WEB
        run: dotnet publish src/OpenToWork.WEB/OpenToWork.WEB.csproj -c Release -o publish/WEB

      - name: Build AdminWEB
        run: dotnet publish src/OpenToWork.AdminWEB/OpenToWork.AdminWEB.csproj -c Release -o publish/AdminWEB

      - name: Deploy via Web Deploy
        uses: azure/webapps-deploy@v3
        with:
          publish-profile: ${{ secrets.AZURE_PUBLISH_PROFILE }}
          package: publish/

      # Alternativa: deploy via PowerShell Remoting
      - name: Deploy via PSRemoting
        if: false  # Cambiar a true si se usa PSRemoting
        shell: pwsh
        env:
          SERVER_IP: ${{ secrets.SERVER_IP }}
          SERVER_USER: ${{ secrets.SERVER_USER }}
          SERVER_PASS: ${{ secrets.SERVER_PASS }}
        run: |
          ./deploy.ps1 -ServerIp $env:SERVER_IP -ServerUser $env:SERVER_USER -ServerPass $env:SERVER_PASS
```

### 2. Configurar secrets en GitHub
- `SERVER_IP` — IP del servidor
- `SERVER_USER` — usuario administrador
- `SERVER_PASS` — password del usuario

---

## Configuracion de produccion

### appsettings.Production.json (por proyecto)

Cada proyecto necesita su `appsettings.Production.json` en la carpeta de publicacion:

#### API
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=127.0.0.1;Database=OpenToWorkDb;User=opentowork;Password=PASSWORD_SEGURO;Port=3306;"
  },
  "Jwt": {
    "Key": "CLAVE_PRODUCCION_MIN_32_CARACTERES_ALEATORIA",
    "Issuer": "https://api.tudominio.com",
    "Audience": "https://tudominio.com",
    "ExpireMinutes": 60,
    "RefreshTokenExpireDays": 7
  },
  "GoogleOAuth": {
    "ClientId": "",
    "ClientSecret": ""
  }
}
```

#### AdminAPI
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=127.0.0.1;Database=OpenToWorkDb;User=opentowork;Password=PASSWORD_SEGURO;Port=3306;"
  },
  "Jwt": {
    "Key": "CLAVE_PRODUCCION_ADMIN_DISTINTA_MIN_32_CHARS",
    "Issuer": "https://admin-api.tudominio.com",
    "Audience": "https://admin.tudominio.com",
    "ExpireMinutes": 60,
    "RefreshTokenExpireDays": 1
  }
}
```

#### WEB
```json
{
  "ApiSettings": {
    "BaseUrl": "https://api.tudominio.com"
  }
}
```

#### AdminWEB
```json
{
  "ApiSettings": {
    "BaseUrl": "https://admin-api.tudominio.com"
  }
}
```

### web.config (autogenerado por dotnet publish)

`dotnet publish` genera automaticamente un `web.config` que configura el modulo de ASP.NET Core en IIS. No es necesario crearlo manualmente.

Si necesitas personalizarlo (por ejemplo, forzar HTTPS):

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <system.webServer>
    <rewrite>
      <rules>
        <rule name="HTTP to HTTPS" stopProcessing="true">
          <match url=".*" />
          <conditions>
            <add input="{HTTPS}" pattern="off" ignoreCase="true" />
          </conditions>
          <action type="Redirect" url="https://{HTTP_HOST}/{R:0}" redirectType="Permanent" />
        </rule>
      </rules>
    </rewrite>
  </system.webServer>
</configuration>
```

---

## Certificados SSL (Let's Encrypt gratis)

### En el servidor: instalar win-acme
```powershell
# Descargar win-acme
Invoke-WebRequest "https://github.com/win-acme/win-acme/releases/latest/download/win-acme.v2.vsix" -OutFile "C:\win-acme.zip"
Expand-Archive "C:\win-acme.zip" -DestinationPath "C:\win-acme\"

# Ejecutar (interactivo, genera certificados para todos los dominios)
C:\win-acme\wacs.exe
```

win-acme renovara los certificados automaticamente cada 60 dias.

---

## Migracion de base de datos

### Opcion 1: Script SQL (recomendado para produccion)
```powershell
# Desde tu PC: generar script de todas las migraciones
dotnet ef migrations script --project src/OpenToWork.Models --startup-project src/OpenToWork.API -o deploy-migrations.sql

# Copiar al servidor y ejecutar
Copy-Item deploy-migrations.sql \\TU_SERVIDOR\C$\temp\
# Luego en MySQL Workbench o mysql CLI:
# mysql -u root -p OpenToWorkDb < C:\temp\deploy-migrations.sql
```

### Opcion 2: dotnet ef desde el servidor
```powershell
# Via PSRemoting
Invoke-Command -ComputerName TU_SERVIDOR -Credential $cred -ScriptBlock {
    cd C:\inetpub\OpenToWork\API
    dotnet ef database update --project OpenToWork.Models --startup-project OpenToWork.API
}
```

---

## Checklist de despliegue

- [ ] .NET 10 Hosting Bundle instalado en el servidor
- [ ] URL Rewrite Module instalado en el servidor
- [ ] MySQL Server corriendo en el servidor (puerto 3306, solo localhost)
- [ ] Web Deploy instalado en el servidor (Opcion A) o PSRemoting habilitado (Opcion B)
- [ ] 4 sitios creados en IIS con sus App Pools (No Managed Code)
- [ ] DNS records apuntando al servidor (tudominio.com, api., admin., admin-api.)
- [ ] Certificados SSL instalados (Let's Encrypt)
- [ ] appsettings.Production.json configurado en cada proyecto
- [ ] Migraciones aplicadas en MySQL del servidor
- [ ] Firewall: solo puertos 80 (redirect) y 443 (HTTPS) abiertos
- [ ] CORS configurado en cada API para los dominios correctos
- [ ] JWT keys distintas y robustas (min 32 caracteres) en produccion
- [ ] Verificar que cada sitio responde correctamente en navegador

---

## Comandos rapidos de verificacion

```powershell
# Verificar sitios en IIS (remoto)
Invoke-Command -ComputerName TU_SERVIDOR -Credential $cred -ScriptBlock {
    Get-Website | Select-Object Name, State, PhysicalPath, @{N='Bindings';E={$_.bindings.Collection.bindingInformation -join '; '}}
}

# Verificar App Pools
Invoke-Command -ComputerName TU_SERVIDOR -Credential $cred -ScriptBlock {
    Get-ChildItem IIS:\AppPools | Select-Object Name, State
}

# Verificar conectividad
curl https://api.tudominio.com/health
curl https://admin-api.tudominio.com/health
curl https://tudominio.com
curl https://admin.tudominio.com
```
