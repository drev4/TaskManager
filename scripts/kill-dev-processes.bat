@echo off
REM Kill Development Processes Script for Windows (Batch)
REM This script stops all running development servers for TaskManager

echo.
echo [33m🛑 Stopping TaskManager development processes...[0m
echo.

echo [36m  Stopping Node.js (Frontend)...[0m
taskkill /F /IM node.exe /T >nul 2>&1
if %errorlevel% equ 0 (
    echo [32m    ✓ Node.js processes stopped[0m
) else (
    echo [90m    ℹ No Node.js processes running[0m
)

echo [36m  Stopping .NET Backend...[0m
taskkill /F /IM dotnet.exe /T >nul 2>&1
if %errorlevel% equ 0 (
    echo [32m    ✓ .NET processes stopped[0m
) else (
    echo [90m    ℹ No .NET processes running[0m
)

echo [36m  Stopping TaskMgr API...[0m
taskkill /F /IM TaskMgr.Api.exe /T >nul 2>&1
if %errorlevel% equ 0 (
    echo [32m    ✓ TaskMgr API stopped[0m
) else (
    echo [90m    ℹ No TaskMgr API running[0m
)

echo.
echo [33m📋 Checking ports...[0m

REM Check if common development ports are in use
netstat -ano | findstr ":65454 :3000 :3001 :5173" | findstr "LISTENING" >nul 2>&1
if %errorlevel% equ 0 (
    echo [33m  ⚠ Some ports may still be in use[0m
    echo [90m  Run 'netstat -ano | findstr "LISTENING"' to check[0m
) else (
    echo [32m  ✓ All development ports are free[0m
)

echo.
echo [32m✅ Done![0m
echo.
pause
