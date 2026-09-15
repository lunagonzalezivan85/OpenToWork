@echo off
REM Baja los 4 servidores de Trato Directo y MySQL (XAMPP).
REM OJO: mata TODOS los procesos dotnet.exe abiertos en la maquina, no solo los de este proyecto.
REM Si tenes otra app .NET corriendo, mejor cerra manualmente las 4 ventanas de start-dev.bat.

echo ============================================
echo  Deteniendo procesos dotnet.exe...
echo ============================================
taskkill /F /IM dotnet.exe /T >nul 2>&1

echo ============================================
echo  Deteniendo MySQL (XAMPP)...
echo ============================================
call C:\xampp\mysql_stop.bat

echo.
echo Listo, todo detenido.
echo.
pause
