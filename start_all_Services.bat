@echo off
setlocal enabledelayedexpansion

echo Starting TrendplusProdavnica services...
echo.

REM Get the directory where this batch file is located
set ROOT_DIR=%~dp0

REM Start Backend (Aspire AppHost)
echo Starting Backend (Aspire)...
start cmd /k "cd /d !ROOT_DIR! && dotnet run --project TrendplusProdavnica.AppHost"

REM Wait a few seconds for backend to start
timeout /t 5 /nobreak

REM Start Frontend (Next.js)
echo Starting Frontend (Next.js)...
start cmd /k "cd /d !ROOT_DIR!TrendplusProdavnica.Web && npm run dev"

REM Wait for frontend to be ready
timeout /t 8 /nobreak

REM Open browser
echo Opening website in default browser...
start http://localhost:3000

echo.
echo Services started!
echo Frontend: http://localhost:3000
echo Backend API: http://localhost:5000
echo Aspire Dashboard: http://localhost:18888
echo.
pause
