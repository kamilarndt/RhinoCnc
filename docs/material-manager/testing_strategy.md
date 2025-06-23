# Material Manager - Testing Strategy

## Cel Dokumentu
Komprehensywna strategia testowania dla Material Manager komponenta, covering unit tests, integration tests, UI tests oraz manual testing procedures.

## Testing Pyramid

### 1. Unit Tests (Foundation)
**Cel**: Test individual components in isolation  
**Coverage**: 80%+ dla business logic  
**Tools**: NUnit, Moq, FluentAssertions

### 2. Integration Tests (Middle)
**Cel**: Test component interactions i external dependencies  
**Coverage**: Key integration points  
**Tools**: NUnit, TestContainers, Rhino headless

### 3. UI Tests (Top)
**Cel**: Test user workflows end-to-end  
**Coverage**: Critical user journeys  
**Tools**: Manual testing, Rhino automation

## Unit Testing Strategy

### Test Structure
```csharp
[TestFixture]
public class MaterialCatalogServiceTests
{
    private MaterialCatalogService _service;
    private Mock<IMaterialRepository> _mockRepository;
    
    [SetUp]
    public void Setup()
    {
        _mockRepository = new Mock<IMaterialRepository>();
        _service = new MaterialCatalogService(_mockRepository.Object);
    }
    
    [Test]
    public async Task GetMaterialsByCategory_ShouldReturnFilteredMaterials()
    {
        // Arrange - Przygotowanie danych testowych
        var materials = CreateTestMaterials();
        _mockRepository.Setup(r => r.GetByCategory("MDF"))
                      .ReturnsAsync(materials.Where(m => m.Category == "MDF"));
        
        // Act - Wykonanie operacji
        var result = await _service.GetMaterialsByCategory("MDF");
        
        // Assert - Sprawdzenie rezultatów
        result.Should().HaveCount(2);
        result.Should().OnlyContain(m => m.Category == "MDF");
    }
}
```

### Components do Unit Testing

#### 1. Models
- **Material.cs** - validation, serialization, property changes
- **MaterialCollection.cs** - collection operations, filtering
- **MaterialDimensions.cs** - calculations, conversions

#### 2. Services  
- **MaterialCatalogService** - CRUD operations, business logic
- **MaterialSearchService** - search algorithms, filtering
- **ImportExportService** - data transformation, validation

#### 3. ViewModels
- **MaterialPaletteViewModel** - command execution, property binding
- **MaterialEditorViewModel** - validation, data binding
- **SettingsViewModel** - configuration management

### Test Data Builders
```csharp
public class MaterialBuilder
{
    private Material _material = new Material();
    
    public MaterialBuilder WithName(string name)
    {
        _material.Name = name;
        return this;
    }
    
    public MaterialBuilder WithCategory(string category)
    {
        _material.Category = category;
        return this;
    }
    
    public MaterialBuilder AsSheetMaterial()
    {
        _material.Type = MaterialType.Sheet;
        _material.Dimensions = new MaterialDimensions
        {
            Width = 2800,
            Height = 2070,
            Thickness = 18
        };
        return this;
    }
    
    public Material Build() => _material;
}

// Usage
var material = new MaterialBuilder()
    .WithName("MDF 18mm")
    .WithCategory("MDF")
    .AsSheetMaterial()
    .Build();
```

## Integration Testing Strategy

### Rhino Integration Tests
```csharp
[TestFixture]
public class RhinoIntegrationTests
{
    private RhinoDoc _testDocument;
    
    [SetUp]
    public void Setup()
    {
        // Create headless Rhino document dla testing
        _testDocument = RhinoDoc.CreateHeadless("MaterialManagerTests");
    }
    
    [TearDown]
    public void TearDown()
    {
        _testDocument?.Dispose();
    }
    
    [Test]
    public void AssignMaterial_ShouldSetUserAttributes()
    {
        // Arrange
        var sphere = new Sphere(Point3d.Origin, 10);
        var sphereId = _testDocument.Objects.AddSphere(sphere);
        var material = CreateTestMaterial();
        var service = new RhinoObjectService();
        
        // Act
        service.AssignMaterial(sphereId, material);
        
        // Assert
        var rhinoObject = _testDocument.Objects.Find(sphereId);
        rhinoObject.GetUserString("MaterialId").Should().Be(material.Id);
        rhinoObject.GetUserString("MaterialName").Should().Be(material.Name);
    }
    
    [Test]
    public void InsertMaterialGeometry_ShouldCreateCorrectGeometry()
    {
        // Test geometry insertion based on material type
        var sheetMaterial = CreateSheetMaterial();
        var service = new GeometryInsertionService();
        
        var objectId = service.InsertMaterialGeometry(sheetMaterial, Point3d.Origin);
        
        objectId.Should().NotBe(Guid.Empty);
        var rhinoObject = _testDocument.Objects.Find(objectId);
        rhinoObject.Should().NotBeNull();
    }
}
```

## UI Testing Strategy

### MVVM Testing
```csharp
[TestFixture] 
public class MaterialPaletteViewModelTests
{
    private MaterialPaletteViewModel _viewModel;
    private Mock<IMaterialCatalogService> _mockService;
    
    [SetUp]
    public void Setup()
    {
        _mockService = new Mock<IMaterialCatalogService>();
        _viewModel = new MaterialPaletteViewModel(_mockService.Object);
    }
    
    [Test]
    public void AddToProjectCommand_WhenExecuted_ShouldAddMaterialToPalette()
    {
        // Arrange
        var material = CreateTestMaterial();
        material.IsInProjectPalette = false;
        
        // Act
        _viewModel.AddToProjectCommand.Execute(material);
        
        // Assert
        material.IsInProjectPalette.Should().BeTrue();
        _mockService.Verify(s => s.UpdateMaterialAsync(material), Times.Once);
    }
    
    [Test]
    public void FilterMaterials_WhenCategorySelected_ShouldFilterResults()
    {
        // Test filtering functionality
        var materials = CreateTestMaterials();
        _viewModel.Materials = new ObservableCollection<Material>(materials);
        
        _viewModel.SelectedCategory = "MDF";
        
        _viewModel.FilteredMaterials.Should().OnlyContain(m => m.Category == "MDF");
    }
}
```

### Command Testing
```csharp
[Test]
public void MaterialOperationCommands_ShouldExecuteCorrectly()
{
    var material = CreateTestMaterial();
    var selectedObjects = new[] { Guid.NewGuid(), Guid.NewGuid() };
    
    // Test Eye command (show/hide)
    _viewModel.EyeCommand.Execute(material);
    // Verify visibility changes
    
    // Test Brush command (assign + color)
    _viewModel.BrushCommand.Execute(material);
    // Verify material assignment
    
    // Test Target command (select objects)
    _viewModel.TargetCommand.Execute(material);
    // Verify object selection
    
    // Test Lock command (lock objects)
    _viewModel.LockCommand.Execute(material);
    // Verify object locking
    
    // Test Plus command (insert geometry)
    _viewModel.PlusCommand.Execute(material);
    // Verify geometry insertion
}
```

## Performance Testing

### Load Testing
```csharp
[Test]
public async Task LoadLargeMaterialCatalog_ShouldPerformWell()
{
    // Test z 10,000 materials
    var materials = GenerateLargeMaterialSet(10000);
    
    var stopwatch = Stopwatch.StartNew();
    await _repository.LoadMaterialsAsync(materials);
    stopwatch.Stop();
    
    stopwatch.ElapsedMilliseconds.Should().BeLessThan(5000); // 5 seconds max
}

[Test]
public void FilterLargeMaterialSet_ShouldBeResponsive()
{
    var materials = GenerateLargeMaterialSet(10000);
    _viewModel.Materials = new ObservableCollection<Material>(materials);
    
    var stopwatch = Stopwatch.StartNew();
    _viewModel.SearchText = "MDF";
    stopwatch.Stop();
    
    stopwatch.ElapsedMilliseconds.Should().BeLessThan(500); // 500ms max dla UI
}
```

## Manual Testing Procedures

### Test Scenarios

#### Scenario 1: Material Palette Operations
1. Launch Material Manager w Rhino
2. Verify wszystkie categories są visible
3. Test każdą z 5 operations (👁️🖌️🎯🔒➕)
4. Verify operations work on multiple objects
5. Test undo/redo functionality

#### Scenario 2: Material Search & Filtering
1. Load large material catalog (100+ materials)
2. Test search by name, category, properties
3. Test filtering combinations
4. Verify search performance
5. Test search clearing

#### Scenario 3: Import/Export
1. Export current material catalog
2. Modify exported file
3. Import modified catalog
4. Verify merge handling
5. Test error scenarios (invalid files)

#### Scenario 4: Data Persistence
1. Add materials do project palette
2. Close i reopen Rhino
3. Verify materials persist
4. Test backup/restore
5. Verify data integrity

## Test Data Management

### Test Material Categories
```csharp
public static class TestMaterials
{
    public static IEnumerable<Material> MdfMaterials => new[]
    {
        new Material { Name = "MDF 6mm", Category = "MDF", Thickness = 6 },
        new Material { Name = "MDF 12mm", Category = "MDF", Thickness = 12 },
        new Material { Name = "MDF 18mm", Category = "MDF", Thickness = 18 },
        new Material { Name = "MDF 25mm", Category = "MDF", Thickness = 25 }
    };
    
    public static IEnumerable<Material> PlywoodMaterials => new[]
    {
        new Material { Name = "Plywood 12mm", Category = "SKLEJKA", Thickness = 12 },
        new Material { Name = "Plywood 18mm", Category = "SKLEJKA", Thickness = 18 }
    };
}
```

## Quality Gates

### Code Coverage Requirements
- **Unit Tests**: 80% minimum
- **Integration Tests**: Key workflows covered
- **Critical Components**: 90% minimum coverage

### Performance Benchmarks  
- **Material Loading**: < 2 seconds dla 1000 materials
- **Search Operations**: < 500ms response time
- **UI Responsiveness**: 60 FPS during operations
- **Memory Usage**: < 50MB dla normal usage

### Bug Escape Rate
- **Critical Bugs**: 0% escape rate
- **High Priority**: < 5% escape rate  
- **Medium Priority**: < 10% escape rate

Testing jest critical dla quality Material Manager'a. Follow tej strategy dla comprehensive coverage i high-quality deliverable.
