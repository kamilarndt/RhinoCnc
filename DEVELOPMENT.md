# RhinoCNC Development Workflow

## Quick Start

### Option 1: Simple Launcher (Recommended)
```bash
# Build and run Rhino with plugin
.\run-dev.bat

# Or with PowerShell directly
.\scripts\run-rhino-with-plugin.ps1
```

### Option 2: Auto-run after Build
```bash
# Enable auto-run Rhino after successful build
dotnet build -p:RunRhinoAfterBuild=true

# Regular build (no auto-run)
dotnet build
```

### Option 3: Manual Steps
```bash
# 1. Build the project
dotnet build

# 2. Open Rhino manually
# 3. Drag-drop: bin\Debug\net48\RhinoCncSuite.dll into Rhino
# 4. Type: RhinoCncMaterialPalette
```

## Script Options

### PowerShell Script Parameters
```powershell
# Build and run (default)
.\scripts\run-rhino-with-plugin.ps1

# Run without building
.\scripts\run-rhino-with-plugin.ps1 -Build:$false

# Force close existing Rhino instances
.\scripts\run-rhino-with-plugin.ps1 -Force

# Custom Rhino path
.\scripts\run-rhino-with-plugin.ps1 -RhinoPath "C:\Custom\Path\Rhino.exe"
```

## What the Scripts Do

1. **Auto-detect Rhino installation** (supports Rhino 7 & 8)
2. **Build the project** (optional)
3. **Close existing Rhino instances** (if -Force specified)
4. **Start Rhino**
5. **Copy plugin path to clipboard** for easy drag-drop
6. **Display clear instructions** for loading the plugin

## Development Cycle

1. Make code changes
2. Run `.\run-dev.bat`
3. Drag-drop the DLL into Rhino (path is in clipboard)
4. Type `RhinoCncMaterialPalette`
5. Test your changes
6. Repeat!

## Troubleshooting

### Rhino Not Found
- Update the path in `scripts\run-rhino-with-plugin.ps1`
- Or use `-RhinoPath` parameter

### PowerShell Execution Policy
```powershell
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

### Plugin Won't Load
- Check if DLL is locked by Rhino (close Rhino first)
- Verify the build was successful
- Check Rhino's PlugInManager for error messages

## Files Created

- `scripts/run-rhino-with-plugin.ps1` - Main PowerShell workflow
- `scripts/run-rhino-with-plugin.bat` - Batch version (legacy)
- `run-dev.bat` - Quick launcher in root directory
- Custom MSBuild target in `.csproj` for auto-run option 