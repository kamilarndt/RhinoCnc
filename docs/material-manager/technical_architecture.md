# Material Manager - Architektura Techniczna

**Wersja**: 1.0  
**Data**: Styczeń 2024  
**Status**: Design Phase

## Przegląd Architektury

Material Manager wykorzystuje architekturę **MVVM (Model-View-ViewModel)** z **Service Locator pattern** dla dependency management. System jest zbudowany na .NET 8.0 z WPF dla UI i integruje się z Rhino 3D API.

## Stack Technologiczny

### Podstawowe Technologie
- **.NET 8.0** - Najnowsza wersja wspierana przez Rhino 8.20+ (Windows 11)
- **.NET 8.0** - Najnowsza wersja wspierana przez Rhino 8.20+ (Windows 11)
- **C# 12.0** - Najnowsze funkcje języka C#
- **WPF (Windows Presentation Foundation)** - UI framework
- **MVVM (Model-View-ViewModel)** - Architectural pattern
- **RhinoCommon API** - Rhino 3D integration layer

### Architektura Pattern
- **MVVM** - Model-View-ViewModel separation
- **Service Locator** - Dependency injection pattern
- **Command Pattern** - RelayCommand dla UI actions
- **Repository Pattern** - Data access abstraction

## Core Components

### 1. Service Locator
Centralny rejestr dla wszystkich usług z thread-safe access i lifetime management.

```csharp
// Rejestracja usług
ServiceLocator.Instance.RegisterSingleton<IMaterialCatalogService, MaterialCatalogService>();
ServiceLocator.Instance.RegisterTransient<IMaterialValidator, MaterialValidator>();

// Użycie w ViewModels
var materialService = ServiceLocator.Instance.Resolve<IMaterialCatalogService>();
```

### 2. BaseViewModel
Bazowa klasa dla wszystkich ViewModels z INotifyPropertyChanged, validation i error handling.

```csharp
public abstract class BaseViewModel : INotifyPropertyChanged, IDisposable
{
    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null);
    protected bool ValidateProperty<T>(T value, [CallerMemberName] string propertyName = null);
    protected async Task ExecuteAsync(Func<Task> operation);
}
```

### 3. RelayCommand
Command implementation dla WPF binding z async support i CanExecute.

```csharp
public class RelayCommand : ICommand
{
    public RelayCommand(Action execute, Func<bool> canExecute = null);
    public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null);
}

public class AsyncRelayCommand : IAsyncRelayCommand
{
    public bool IsRunning { get; }
    public async Task ExecuteAsync(object parameter = null);
}
```

## Data Architecture

### Material Model
```csharp
public class Material
{
    public string Id { get; set; }
    public string Name { get; set; }
    public MaterialCategory Category { get; set; }
    public MaterialType Type { get; set; }
    public MaterialDimensions Dimensions { get; set; }
    public bool IsInProjectPalette { get; set; }
    public Color Color { get; set; }
    // ... additional properties
}
```

### JSON Persistence Strategy
Single `material_catalog.json` file z flag-based filtering:

```json
{
  "version": "1.0",
  "lastModified": "2024-01-01T00:00:00Z",
  "materials": [
    {
      "id": "uuid-string",
      "name": "MDF 18mm", 
      "category": "MDF",
      "type": "Sheet",
      "isInProjectPalette": true,
      "dimensions": {
        "thickness": 18,
        "width": 2800,
        "height": 2070
      },
      "color": "#FF6B35"
    }
  ]
}
```

## Service Layer

### Material Catalog Service
```csharp
public interface IMaterialCatalogService
{
    Task<IEnumerable<Material>> GetAllMaterialsAsync();
    Task<IEnumerable<Material>> GetPaletteMaterialsAsync();
    Task<Material> GetMaterialByIdAsync(string id);
    Task SaveMaterialAsync(Material material);
    Task DeleteMaterialAsync(string id);
    Task<IEnumerable<Material>> SearchMaterialsAsync(string query, MaterialFilter filter);
}
```

### Material Assignment Service
```csharp
public interface IMaterialAssignmentService
{
    Task AssignMaterialToObjectsAsync(Material material, IEnumerable<Guid> objectIds);
    Task<Material> GetMaterialFromObjectAsync(Guid objectId);
    Task<IEnumerable<Guid>> GetObjectsByMaterialAsync(string materialId);
    Task RemoveMaterialFromObjectsAsync(IEnumerable<Guid> objectIds);
}
```

### Object Operations Service
```csharp
public interface IObjectOperationsService
{
    Task ShowObjectsByMaterialAsync(string materialId);
    Task HideObjectsByMaterialAsync(string materialId);
    Task SelectObjectsByMaterialAsync(string materialId);
    Task LockObjectsByMaterialAsync(string materialId);
    Task UnlockObjectsByMaterialAsync(string materialId);
}
```

## UI Architecture

### Material Palette Control
Główny control z tile/list view modes:

```csharp
public class MaterialPaletteControl : UserControl
{
    public MaterialPaletteViewModel ViewModel { get; set; }
    public ViewMode CurrentViewMode { get; set; } // Tile or List
    
    // 5 Material Operations
    public ICommand ShowHideCommand { get; }
    public ICommand AssignCommand { get; }
    public ICommand SelectCommand { get; }
    public ICommand LockCommand { get; }
    public ICommand InsertCommand { get; }
}
```

### View Modes
- **Tile View**: Kolorowe kafelki z ikonami operacji
- **List View**: Tabela z sortowaniem i bulk operations

## Rhino Integration

### Material Assignment w User Data
```csharp
// Zapisywanie materiału w obiekcie
rhinoObject.Attributes.UserData.Add("MaterialId", material.Id);
rhinoObject.Attributes.UserData.Add("MaterialName", material.Name);
rhinoObject.Attributes.ObjectColor = material.Color;

// Znajdowanie obiektów po materiale
var objects = doc.Objects.FindByUserData("MaterialId", materialId);
```

### Geometry Insertion
```csharp
public class GeometryInsertionService
{
    public Guid InsertSheetMaterial(Material material, Point3d insertPoint)
    {
        // Create rectangle based on material dimensions
        var rectangle = new Rectangle3d(plane, material.Dimensions.Width, material.Dimensions.Height);
        return doc.Objects.AddBrep(rectangle.ToBrep());
    }
    
    public Guid InsertLinearMaterial(Material material, Point3d startPoint, Point3d endPoint)
    {
        // Create cross-section extrusion
        var profile = CreateProfileCurve(material.Profile);
        var path = new Line(startPoint, endPoint).ToNurbsCurve();
        return doc.Objects.AddExtrusion(Extrusion.Create(profile, path));
    }
}
```

## Error Handling Strategy

### Service Level
```csharp
public class MaterialCatalogService
{
    public async Task<Material> GetMaterialByIdAsync(string id)
    {
        try
        {
            // Implementation
        }
        catch (FileNotFoundException ex)
        {
            _logger.LogError($"Material catalog file not found: {ex.Message}");
            throw new MaterialCatalogException("Catalog nie został znaleziony", ex);
        }
        catch (JsonException ex)
        {
            _logger.LogError($"Invalid catalog format: {ex.Message}");
            throw new MaterialCatalogException("Nieprawidłowy format katalogu", ex);
        }
    }
}
```

## Performance Considerations

### UI Virtualization
```csharp
// Dla dużych kolekcji materiałów
<VirtualizingStackPanel VirtualizationMode="Recycling" 
                        IsVirtualizing="True"
                        ScrollUnit="Item" />
```

### Async Operations
```csharp
// Wszystkie I/O operations async
public async Task SaveCatalogAsync()
{
    var json = JsonConvert.SerializeObject(materials, Formatting.Indented);
    await File.WriteAllTextAsync(catalogPath, json);
}
```

### Memory Management
```csharp
// Proper disposal w ViewModels
public void Dispose()
{
    _materialService?.Dispose();
    _cancellationTokenSource?.Dispose();
    GC.SuppressFinalize(this);
}
```

## Testing Strategy

### Unit Tests
- **Models**: Serialization, validation, property changes
- **Services**: Business logic, error handling, async operations
- **ViewModels**: Command execution, property binding, state management

### Integration Tests
- **Rhino API**: Object operations, material assignment
- **File I/O**: JSON persistence, backup/restore
- **UI Binding**: Data binding, command execution

## Deployment Architecture

### Plugin Structure
```
RhinoCNCMaterialManager.rhp
├── RhinoCNCMaterialManager.dll    # Main plugin assembly
├── Newtonsoft.Json.dll           # JSON serialization
├── resources/                    # Icons, templates
└── docs/                        # Help documentation
```

### Configuration Files
- `material_catalog.json` - Main material database
- `settings.json` - User preferences
- `templates/` - Material templates

## Notatki Implementacyjne

### Krytyczne Rzeczy do Zapamiętania
1. **Rhino Process**: Kill Rhino.exe przed builds żeby avoid DLL locking
2. **UI Thread**: Wszystkie Rhino API calls na UI thread
3. **Memory Leaks**: Proper disposal of event handlers i Rhino objects
4. **Error Recovery**: Graceful handling of Rhino API failures
5. **Data Integrity**: Validate material data przed save operations

Ta architektura zapewnia scalable, maintainable i extensible foundation dla Material Manager component w RhinoCNC Suite. 

### Rhino Integration
- **Rhino 8.20+** z domyślnym .NET 8.0 runtime
- **RhinoCommon NuGet Package** - Oficjalne API
- **Rhino Plugin Architecture** - .rhp plugin format
- **User Attributes** - Metadata storage w Rhino objects
- **Block Definitions** - Rhino geometry management

### Data Management
- **JSON Serialization** (Newtonsoft.Json) - Configuration i data storage
- **File System Storage** - Local material catalog files
- **In-Memory Caching** - Performance optimization
- **Observable Collections** - Real-time UI updates 