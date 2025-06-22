# RhinoCNC Plugin Development Workflow
param(
    [switch]$Build,
    [string]$RhinoPath = "",
    [switch]$Force = $false
)

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "RhinoCNC Plugin Development Workflow" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan

# Get script directory and project paths
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectDir = Split-Path -Parent $ScriptDir
$PluginPath = Join-Path $ProjectDir "bin\Debug\net8.0-windows\RhinoCncSuite.dll"

# Find Rhino installation
$PossibleRhinoPaths = @(
    "C:\Program Files\Rhino 8\System\Rhino.exe",
    "C:\Program Files\Rhino 7\System\Rhino.exe"
)

$RhinoExe = ""
if ($RhinoPath -and (Test-Path $RhinoPath)) {
    $RhinoExe = $RhinoPath
} else {
    foreach ($path in $PossibleRhinoPaths) {
        if (Test-Path $path) {
            $RhinoExe = $path
            break
        }
    }
}

if (-not $RhinoExe) {
    Write-Host "ERROR: Rhino not found!" -ForegroundColor Red
    Write-Host "Searched paths:" -ForegroundColor Yellow
    foreach ($path in $PossibleRhinoPaths) {
        Write-Host "  - $path" -ForegroundColor Gray
    }
    exit 1
}

Write-Host "Found Rhino at: $RhinoExe" -ForegroundColor Green

# Always close existing Rhino instances before building to avoid DLL lock
Write-Host "Checking for existing Rhino processes..." -ForegroundColor Yellow
$rhinoProcesses = Get-Process -Name "Rhino" -ErrorAction SilentlyContinue
if ($rhinoProcesses) {
    Write-Host "Closing existing Rhino instances to unlock DLL..." -ForegroundColor Yellow
    $rhinoProcesses | Stop-Process -Force
    Start-Sleep -Seconds 2
    Write-Host "Rhino processes closed." -ForegroundColor Green
} else {
    Write-Host "No existing Rhino processes found." -ForegroundColor Green
}

# Build the project if requested
if ($Build.IsPresent) {
    Write-Host "Building project..." -ForegroundColor Yellow
    Push-Location $ProjectDir
    
    try {
        dotnet build
        if ($LASTEXITCODE -ne 0) {
            Write-Host "Build failed!" -ForegroundColor Red
            exit 1
        }
        Write-Host "Build successful!" -ForegroundColor Green
    }
    finally {
        Pop-Location
    }
}

# Clear plugin settings to force a clean load
$PluginSettingsDir = Join-Path ([Environment]::GetFolderPath('ApplicationData')) "McNeel\Rhinoceros\8.0\Plug-ins\RhinoCNC Production Suite (B7A84A1C-4F2D-4E8A-9C5B-8D3F7E6A5B2C)"
if (Test-Path $PluginSettingsDir) {
    Write-Host "Clearing cached plugin settings at: $PluginSettingsDir" -ForegroundColor Yellow
    Remove-Item -Path $PluginSettingsDir -Recurse -Force
    Write-Host "Plugin settings cleared." -ForegroundColor Green
}

# Check if plugin exists
if (-not (Test-Path $PluginPath)) {
    Write-Host "ERROR: Plugin not found at: $PluginPath" -ForegroundColor Red
    exit 1
}

Write-Host "Found Plugin at: $PluginPath" -ForegroundColor Green

# Note: Rhino instances are already closed above before building

# Start Rhino
Write-Host "Starting Rhino..." -ForegroundColor Yellow
Start-Process -FilePath $RhinoExe

# Wait for Rhino to start
Start-Sleep -Seconds 3

# Display instructions
Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "NEXT STEPS:" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "1. Rhino should now be starting..." -ForegroundColor White
Write-Host "2. Drag and drop this file into Rhino:" -ForegroundColor White
Write-Host "   $PluginPath" -ForegroundColor Yellow
Write-Host "3. Or type 'PlugInManager' and load manually" -ForegroundColor White
Write-Host "4. Then type: RhinoCncMaterialPalette" -ForegroundColor Green
Write-Host "5. The Material Palette should appear!" -ForegroundColor White
Write-Host "============================================" -ForegroundColor Cyan

# Copy plugin path to clipboard for easy access
try {
    Set-Clipboard -Value $PluginPath
    Write-Host ""
    Write-Host "Plugin path copied to clipboard!" -ForegroundColor Green
} catch {
    Write-Host ""
    Write-Host "Could not copy to clipboard" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Press any key to continue..." -ForegroundColor Gray
Read-Host 