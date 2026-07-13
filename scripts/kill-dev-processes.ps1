# Kill Development Processes Script for Windows (PowerShell)
# This script stops all running development servers for TaskManager

Write-Host "🛑 Stopping TaskManager development processes..." -ForegroundColor Yellow
Write-Host ""

# Function to kill processes by name
function Stop-ProcessByName {
    param(
        [string]$ProcessName,
        [string]$DisplayName
    )

    $processes = Get-Process -Name $ProcessName -ErrorAction SilentlyContinue

    if ($processes) {
        Write-Host "  Stopping $DisplayName..." -ForegroundColor Cyan
        $processes | ForEach-Object {
            try {
                Stop-Process -Id $_.Id -Force -ErrorAction Stop
                Write-Host "    ✓ Stopped PID $($_.Id)" -ForegroundColor Green
            }
            catch {
                Write-Host "    ✗ Failed to stop PID $($_.Id)" -ForegroundColor Red
            }
        }
    }
    else {
        Write-Host "  ℹ No $DisplayName processes running" -ForegroundColor Gray
    }
}

# Stop Node.js (Frontend Vite)
Stop-ProcessByName -ProcessName "node" -DisplayName "Node.js (Frontend)"

# Stop .NET Backend
Stop-ProcessByName -ProcessName "dotnet" -DisplayName ".NET (Backend)"

# Stop TaskMgr API
Stop-ProcessByName -ProcessName "TaskMgr.Api" -DisplayName "TaskMgr API"

Write-Host ""
Write-Host "📋 Checking ports..." -ForegroundColor Yellow

# Check if ports are free
$ports = @(65454, 3000, 3001, 5173)
$portsInUse = @()

foreach ($port in $ports) {
    $connection = Get-NetTCPConnection -LocalPort $port -State Listen -ErrorAction SilentlyContinue
    if ($connection) {
        $portsInUse += $port
        Write-Host "  ⚠ Port $port still in use by PID $($connection.OwningProcess)" -ForegroundColor Yellow
    }
}

if ($portsInUse.Count -eq 0) {
    Write-Host "  ✓ All development ports are free" -ForegroundColor Green
}
else {
    Write-Host ""
    Write-Host "  ℹ If you need to free these ports, kill the processes manually:" -ForegroundColor Gray
    foreach ($port in $portsInUse) {
        $connection = Get-NetTCPConnection -LocalPort $port -State Listen -ErrorAction SilentlyContinue
        if ($connection) {
            Write-Host "    Stop-Process -Id $($connection.OwningProcess) -Force" -ForegroundColor Gray
        }
    }
}

Write-Host ""
Write-Host "✅ Done!" -ForegroundColor Green
