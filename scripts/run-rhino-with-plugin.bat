@echo off
setlocal

echo ============================================
echo RhinoCNC Plugin Development Workflow
echo ============================================

:: Set paths
set "PLUGIN_PATH=%~dp0..\bin\Debug\net48\RhinoCncSuite.dll"
set "RHINO_EXE=C:\Program Files\Rhino 8\System\Rhino.exe"

:: Check if Rhino exists
if not exist "%RHINO_EXE%" (
    echo ERROR: Rhino 8 not found at: %RHINO_EXE%
    echo Please update the RHINO_EXE path in this script.
    pause
    exit /b 1
)

:: Check if plugin exists
if not exist "%PLUGIN_PATH%" (
    echo ERROR: Plugin not found at: %PLUGIN_PATH%
    echo Please build the project first with: dotnet build
    pause
    exit /b 1
)

echo Found Rhino at: %RHINO_EXE%

:: Always close existing Rhino processes to avoid DLL lock during build
echo Closing any existing Rhino processes to unlock DLL...
taskkill /F /IM Rhino.exe >NUL 2>&1
if %errorlevel% equ 0 (
    echo Rhino processes closed.
    timeout /T 2 >NUL
) else (
    echo No existing Rhino processes found.
)

:: Build the project
echo Building project...
cd /d "%~dp0.."
dotnet build
if %errorlevel% neq 0 (
    echo Build failed!
    pause
    exit /b 1
)
echo Build successful!
echo.

echo Found Plugin at: %PLUGIN_PATH%
echo.

echo Starting Rhino 8 with RhinoCNC plugin...
echo.

:: Start Rhino with the plugin
:: Note: Rhino will load the plugin automatically when the DLL is dropped onto it
start "" "%RHINO_EXE%"

:: Wait a moment for Rhino to start
timeout /T 3 >NUL

:: Try to load the plugin via command line (this may not work in all versions)
:: Alternative: We'll create a script file that Rhino can run
echo Creating Rhino script to load plugin...
set "SCRIPT_PATH=%TEMP%\rhinocnc_load_plugin.rvb"

echo ' RhinoCNC Plugin Auto-loader > "%SCRIPT_PATH%"
echo Call Rhino.Application.LoadPlugIn("%PLUGIN_PATH%") >> "%SCRIPT_PATH%"
echo Call Rhino.Application.RunScript("RhinoCncMaterialPalette", 0) >> "%SCRIPT_PATH%"

:: Note: The above script approach may need manual execution in Rhino
:: For now, we'll just open Rhino and display instructions

echo.
echo ============================================
echo INSTRUCTIONS:
echo ============================================
echo 1. Rhino 8 should now be starting...
echo 2. Once Rhino is loaded, drag and drop this file into Rhino:
echo    %PLUGIN_PATH%
echo 3. Or use PlugInManager to load the plugin manually
echo 4. Then type: RhinoCncMaterialPalette
echo 5. The Material Palette should appear!
echo ============================================
echo.

:: Keep the window open so user can see the instructions
echo Press any key to close this window...
pause >NUL

endlocal 