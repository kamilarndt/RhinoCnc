# Material Manager - Development Guide

## Cel Dokumentu
Komprehensywny przewodnik dla developerów pracujących nad Material Manager komponenten w Rhino 3D Plugin. Zawiera setup instructions, development workflow, coding standards i troubleshooting.

## Technologie i Narzędzia

### Framework i Runtime
- **.NET 8.0** - Najnowsza wersja wspierana przez Rhino 8.20+
- **WPF** - Windows Presentation Foundation do tworzenia interfejsu użytkownika
- **MVVM** - Model-View-ViewModel pattern dla separacji logiki biznesowej
- **RhinoCommon API** - Główne API do integracji z Rhino 3D
- **Windows 11** - Docelowy system operacyjny

### Kompatybilność Rhino
- **Rhino 8.20+** - Domyślnie używa .NET 8.0 (Release Candidate, oficjalna wersja czerwiec 2025)
- **Rhino 8.19 i wcześniejsze** - Używają .NET 7.0 jako domyślne
- **Backward compatibility** - Plugin będzie działał na .NET 7.0 jeśli potrzebne

### Wymagania Systemowe
- **Windows 11** (64-bit)
- **Rhino 8.20 lub nowszy** z domyślnym .NET 8.0
- **Visual Studio 2022** z .NET 8.0 SDK
- **.NET 8.0 Desktop Runtime** (automatycznie instalowany z Rhino 8.20)

## Environment Setup

### Wymagania
- **Visual Studio 2022** (17.4 lub nowszy)
- **.NET 8.0 SDK** 
- **Rhino 3D 8** (najnowsza wersja)
- **Git** dla version control
- **NuGet Package Manager** dla dependency management

### ⚠️ Ważne Build Notes
- **Zawsze zamknij Rhino.exe przed build** - pliki DLL będą zablokowane
- Użyj **Build > Clean Solution** jeśli masz problemy z DLL locking
- W przypadku błędów "file in use" - sprawdź Task Manager dla Rhino procesów

## Project Structure

### Core Architecture
```
RhinoCnc/
├── src/
│   ├── MaterialManager/
│   │   ├── Models/              # Data models
│   │   ├── ViewModels/          # MVVM ViewModels  
│   │   ├── Views/               # UI Views (code-only)
│   │   ├── Services/            # Business services
│   │   ├── Commands/            # Rhino commands
│   │   └── Infrastructure/      # Core infrastructure
│   └── Common/                  # Shared utilities
├── tests/
│   ├── Unit/                    # Unit tests
│   └── Integration/             # Integration tests
└── docs/                        # Documentation
```

## Development Workflow

### Coding Standards
- **English names** dla classes/methods
- **Polish comments** i documentation
- **MVVM pattern** dla UI components
- **No XAML** - programmatic UI creation only
- **TDD approach** - tests first

### Branch Strategy
```bash
git checkout -b feature/material-palette-icons
git commit -m "feat(ui): add material operation icons"
git push origin feature/material-palette-icons
```

## Rhino Integration Patterns

### Object Manipulation
```csharp
public void AssignMaterial(Guid objectId, Material material)
{
    var rhinoObject = RhinoDoc.ActiveDoc.Objects.Find(objectId);
    if (rhinoObject == null) return;
    
    // Store material metadata
    rhinoObject.SetUserString("MaterialId", material.Id);
    rhinoObject.SetUserString("MaterialName", material.Name);
    
    RhinoDoc.ActiveDoc.Views.Redraw();
}
```

### Thread Safety
```csharp
RhinoApp.InvokeOnUiThread(() =>
{
    // Rhino operations here
    RhinoDoc.ActiveDoc.Views.Redraw();
});
```

## Common Issues & Solutions

### DLL Locking
```bash
taskkill /f /im Rhino.exe
dotnet clean
dotnet build
```

### Material Assignment Not Visible
```csharp
// Force view refresh
RhinoDoc.ActiveDoc.Views.Redraw();
```

## Testing Strategy
- **Unit Tests** dla business logic
- **Integration Tests** dla Rhino API
- **Mock Rhino dependencies** w unit tests
- **TDD approach** - write tests first

## Best Practices
1. Always close Rhino before building
2. Use programmatic UI (no XAML)
3. Handle threading properly z RhinoApp.InvokeOnUiThread
4. Dispose Rhino objects properly
5. Use Polish comments, English code names
6. Test everything, especially Rhino integrations

## Architecture Components
- **Service Locator** dla dependency injection
- **BaseViewModel** dla MVVM infrastructure
- **RelayCommand** dla command binding
- **JSON database** (material_catalog.json)
- **Flag-based filtering** (IsInProjectPalette)

## Material Operations (5 key operations)
1. **👁️ Eye** - Show/Hide materials
2. **🖌️ Brush** - Assign material + color objects
3. **🎯 Target** - Select all objects z material
4. **🔒 Lock** - Lock all objects z material  
5. **➕ Plus** - Insert material geometry

## Categories & Colors
- **MDF** - Orange (#FF6B35)
- **SKLEJKA** - Yellow (#FFD700)  
- **WTOROWA** - Red (#FF0000)
- **INNE** - Blue (#0066CC)

Pamiętaj: Quality over speed. Każdy komponent should be robust, well-tested, i maintainable.
