@echo off
REM Levanta MySQL (XAMPP) + los 4 servidores de Trato Directo, cada uno en su propia ventana.
REM Doble clic para correr, o ejecutar desde una terminal.

set REPO=%~dp0..

echo ============================================
echo  Iniciando MySQL (XAMPP)...
echo ============================================
call C:\xampp\mysql_start.bat
timeout /t 3 /nobreak >nul

echo ============================================
echo  Levantando OpenToWork.API (puerto 5000)...
echo ============================================
start "API - 5000" cmd /k "cd /d "%REPO%" && dotnet run --project src\OpenToWork.API --urls http://localhost:5000"

echo ============================================
echo  Levantando OpenToWork.AdminAPI (puerto 5001)...
echo ============================================
start "AdminAPI - 5001" cmd /k "cd /d "%REPO%" && dotnet run --project src\OpenToWork.AdminAPI --urls http://localhost:5001"

echo ============================================
echo  Levantando OpenToWork.WEB (puerto 5100)...
echo ============================================
start "WEB - 5100" cmd /k "cd /d "%REPO%" && dotnet run --project src\OpenToWork.WEB --urls http://localhost:5100"

echo ============================================
echo  Levantando OpenToWork.AdminWEB (puerto 5101)...
echo ============================================
start "AdminWEB - 5101" cmd /k "cd /d "%REPO%" && dotnet run --project src\OpenToWork.AdminWEB --urls http://localhost:5101"

echo.
echo Todo lanzado. Cada servidor tarda 10-20s en compilar la primera vez.
echo.
echo   Portal candidatos/empresas: http://localhost:5100
echo   Portal administrador:      http://localhost:5101
echo.
echo Dejá abiertas las 4 ventanas de consola mientras trabajás (ahi ves los logs).
echo Para bajar todo, corré stop-dev.bat o simplemente cerrá las ventanas.
echo.
pause
