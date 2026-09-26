# Despliegue de Trato Directo en Windows Server + IIS

> Actualizado el 26-Sep-2026. Reemplaza la guia anterior (4 subdominios publicos con Web Deploy).
> Todo lo necesario esta en la carpeta `deploy/` del repositorio.

## Arquitectura

```
Internet ──HTTPS──> IIS (Windows Server)
                     ├── tratodirecto.es / www.tratodirecto.es  -> TD-WEB       (portal, Blazor Server)
                     ├── admin.tratodirecto.es                  -> TD-AdminWEB  (panel, Blazor Server)
                     ├── 127.0.0.1:5000  (solo local)           -> TD-API
                     └── 127.0.0.1:5001  (solo local)           -> TD-AdminAPI
                    MySQL 8 (localhost:3306, base TratoDirectoDb, usuario tratodirecto)
                    C:\TratoDirecto\storage  (CV y fotos, privado)
```

- **Los API no se publican en Internet.** El portal y el admin son Blazor Server: llaman a los API desde el propio
  servidor, nunca desde el navegador. Solo hacen falta 3 registros DNS y los API quedan fuera del alcance de terceros.
- **Secretos solo en el servidor.** El repositorio es publico: las claves JWT y la cadena de conexion de
  `appsettings.json` son de desarrollo. En el servidor, `instalar.ps1` genera `appsettings.Production.json` con claves
  y contraseña nuevas. Si un API arranca en produccion con claves del repositorio, root o sin `Storage:Root`, **no
  arranca** y el error dice que falta (`ProductionConfigGuard`).
- **Base limpia.** Se crean las tablas con las migraciones y se cargan solo catalogos y configuracion (tipos de
  puesto, precios, planes, habilidades, documentos, pasos del asistente, datos de la empresa y flags). No se llevan
  usuarios, candidatos, empresas, contratos ni la configuracion SMTP.

## Estructura de `deploy/`

| Archivo | Donde se ejecuta | Para que |
|---|---|---|
| `publish.ps1` | PC de desarrollo | Genera `deploy/out/TratoDirecto-<fecha>.zip` (los 4 sitios, migraciones y catalogos) |
| `server/instalar.ps1` | Servidor, una vez | IIS, carpetas, configuracion con secretos nuevos, base de datos, sitios y permisos |
| `server/actualizar.ps1` | Servidor, en cada version | Copia de seguridad, modo mantenimiento, archivos nuevos y migraciones pendientes |
| `server/migrar.ps1` | (lo llaman los anteriores) | Aplica `db/efbundle.exe` y, en la instalacion, `db/catalogos.sql` |
| `server/crear-admin.sql` | Servidor, una vez | Convierte tu cuenta registrada en SuperAdmin |
| `server/config/*.Production.json` | (plantillas) | Base de los `appsettings.Production.json` del servidor |

`deploy/out/` y cualquier `appsettings.Production.json` estan en `.gitignore`: nunca se suben al repositorio.

---

## Primera instalacion

### 1. DNS (en el proveedor del dominio)

Crear registros **A** hacia la IP publica del servidor:

| Nombre | Tipo | Valor |
|---|---|---|
| `tratodirecto.es` (@) | A | IP del servidor |
| `www` | A | IP del servidor |
| `admin` | A | IP del servidor |

Abrir en el firewall del servidor (y del proveedor, si tiene) los puertos **80 y 443**. No abrir 3306, 5000 ni 5001.

### 2. Requisitos en el servidor

1. **.NET 10 Hosting Bundle**: https://dotnet.microsoft.com/download/dotnet/10.0 → "Hosting Bundle". Instalar y
   ejecutar `iisreset`.
2. **MySQL 8.0 Community Server** (https://dev.mysql.com/downloads/installer/): instalacion "Server only", puerto
   3306, contraseña de root segura. Guardala: los scripts la pediran (la escribes tu en la consola).

### 3. Generar el paquete (en tu PC)

```powershell
powershell -ExecutionPolicy Bypass -File deploy\publish.ps1
```

Necesita el SDK de .NET 10, `dotnet-ef` y la base local en marcha (de ella salen los catalogos). Si MySQL local no
esta en `C:\xampp\mysql\bin`, pasa `-MySqlBin "ruta"`.

### 4. Instalar (en el servidor)

1. Copiar el zip al servidor (por Escritorio Remoto) y descomprimirlo, por ejemplo en `C:\Paquetes\TratoDirecto-<fecha>`.
2. Abrir PowerShell **como Administrador** en esa carpeta y ejecutar:
   ```powershell
   powershell -ExecutionPolicy Bypass -File server\instalar.ps1
   ```
   Si MySQL no esta en `C:\Program Files\MySQL\MySQL Server 8.0\bin`, agregar `-MySqlBin "ruta"`.
3. Cuando lo pida, escribir la contraseña de root de MySQL (solo para crear la base y su usuario).

Al terminar muestra la respuesta de los dos API locales (debe ser 200).

### 5. HTTPS con Let's Encrypt (win-acme)

1. Descargar win-acme (https://www.win-acme.com/, version x64 "pluggable") y descomprimir en `C:\win-acme`.
2. Ejecutar `wacs.exe` como Administrador → **N** (nuevo certificado) → elegir el sitio **TD-WEB** (incluye
   `tratodirecto.es` y `www.tratodirecto.es`) → aceptar. Repetir con **TD-AdminWEB**.
3. win-acme agrega los bindings 443 y crea una tarea programada que renueva los certificados.

El portal y el admin redirigen solos de HTTP a HTTPS.

### 6. Primer administrador

1. Registrar tu cuenta en `https://tratodirecto.es/register`.
2. Editar `server\crear-admin.sql` con tu correo y ejecutarlo:
   ```powershell
   Get-Content server\crear-admin.sql | & "C:\Program Files\MySQL\MySQL Server 8.0\bin\mysql.exe" -uroot -p TratoDirectoDb
   ```
3. Entrar en `https://admin.tratodirecto.es` con ese correo y contraseña.

### 7. Configuracion desde el admin

- **Datos de la Empresa**: revisar razon social, CIF, domicilio, representante y **correo de privacidad**.
- **Correo (SMTP)**: servidor, usuario y contraseña reales; activar cuando la prueba de envio funcione.
- **Planes / precios / codigos promocionales**: revisar; los codigos de prueba no se copiaron.

### 8. Comprobacion final

- `https://tratodirecto.es` carga, registro e inicio de sesion funcionan, `/privacy` y `/terms` se ven.
- `https://admin.tratodirecto.es` permite entrar con el SuperAdmin.
- Subir un CV y una foto de perfil desde un candidato de prueba y verlos desde el admin (se guardan en
  `C:\TratoDirecto\storage`).
- Desde fuera del servidor, `http://IP:5000` y `http://IP:5001` **no** deben responder.

---

## Actualizar a una version nueva

1. En tu PC: `git pull` y `powershell -ExecutionPolicy Bypass -File deploy\publish.ps1`.
2. Copiar y descomprimir el zip nuevo en el servidor.
3. En el servidor, como Administrador:
   ```powershell
   powershell -ExecutionPolicy Bypass -File server\actualizar.ps1
   ```
   Hace copia de la base en `C:\TratoDirecto\backups` (pide la contraseña de root), muestra una pagina de
   mantenimiento, copia los archivos sin tocar `appsettings.Production.json` ni `storage`, aplica las migraciones
   pendientes y vuelve a abrir.

## Copias de seguridad

Programar en el Programador de tareas una copia diaria de la base y de `C:\TratoDirecto\storage` (CV y fotos).
La base sola no basta: sin `storage` se pierden los CV y las fotos.

## Problemas frecuentes

| Sintoma | Causa probable |
|---|---|
| HTTP 500.30 al abrir un sitio | El API no arranca. Ver el Visor de eventos (Aplicacion) o activar `stdoutLogEnabled="true"` en el `web.config` del sitio, con `stdoutLogFile=".\..\..\logs\stdout"`. Suele ser la configuracion de produccion (el mensaje dice que falta). |
| HTTP 500.19 | Falta el Hosting Bundle o no se hizo `iisreset` despues de instalarlo. |
| El portal carga pero se queda "reconectando" | Falta la caracteristica **WebSocket Protocol** de IIS (`instalar.ps1` la activa). |
| "Unknown column" en el log | Migraciones sin aplicar: ejecutar `server\actualizar.ps1` o `server\migrar.ps1`. |
| No se pueden subir CV o fotos | Permisos de `C:\TratoDirecto\storage` para `IIS AppPool\TD-API` y `TD-AdminAPI`. |

## Pendiente antes de abrir al publico

- Hacer **privado** el repositorio de GitHub y purgar los CV del historial (coordinar Darwin e Ivan).
- Revision legal de `/privacy` y `/terms`, y aviso legal (LSSI).
- Revision de seguridad, puntos 5 y 6 (busqueda de candidatos y controles de rol).
