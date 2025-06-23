# Epic 4: Rhino Integration

**Priorytet**: High  
**Szacowany czas**: 5-6 dni  
**Status**: Nie rozpoczęty

## Cel
Implementacja kompletnej integracji z Rhino 3D API dla wszystkich operacji materiałowych: object assignment, visibility control, selection, locking, geometry insertion i material persistence.

## Zadania

### Task 4.1: Material Assignment Service
**Priorytet**: High  
**Szacowany czas**: 2 dni  
**Plik**: `task_4_1_material_assignment.md`

Service do przypisywania materiałów do obiektów Rhino z automatic coloring i user attributes.

### Task 4.2: Object Operations Service  
**Priorytet**: High  
**Szacowany czas**: 2 dni  
**Plik**: `task_4_2_object_operations.md`

Show/Hide, Select All, Lock operations na obiektach na podstawie materiału.

### Task 4.3: Geometry Insertion Service
**Priorytet**: Medium  
**Szacowany czas**: 2 dni  
**Plik**: `task_4_3_geometry_insertion.md`

Insertion prostokątów (sheet materials) i cross-sections (linear materials).

### Task 4.4: Data Persistence Integration
**Priorytet**: Medium  
**Szacowany czas**: 1 dzień  
**Plik**: `task_4_4_data_persistence.md`

Saving material metadata w Rhino document User Data z backup/restore.

## Zależności
- **Epic 1**: Core Infrastructure (Service Locator)
- **Epic 2**: Data Models (Material, MaterialCollection)

## Kluczowe Integracje

### Rhino API Components
- **DocObjects**: Object manipulation i metadata
- **Geometry**: Shape creation i insertion  
- **Display**: Visibility i visual properties
- **Attributes**: Material assignment i user data
- **LayerTable**: Layer-based organization

### Material Assignment Strategy
```csharp
// Zapisywanie materiału w User Attributes
object.Attributes.UserData.Add("MaterialId", material.Id);
object.Attributes.UserData.Add("MaterialName", material.Name);
object.Attributes.ObjectColor = material.Color;
```

### Object Query Strategy
```csharp
// Finding objects by material
var objectsWithMaterial = doc.Objects.FindByUserData("MaterialId", materialId);
```

## Services do Implementacji

### IMaterialAssignmentService
- `AssignMaterialToObjects(Material material, IEnumerable<Guid> objectIds)`
- `GetMaterialFromObject(Guid objectId)`
- `GetObjectsByMaterial(string materialId)`
- `RemoveMaterialFromObjects(IEnumerable<Guid> objectIds)`

### IObjectOperationsService  
- `ShowObjectsByMaterial(string materialId)`
- `HideObjectsByMaterial(string materialId)`
- `SelectObjectsByMaterial(string materialId)`
- `LockObjectsByMaterial(string materialId)`
- `UnlockObjectsByMaterial(string materialId)`

### IGeometryInsertionService
- `InsertSheetMaterial(Material material, Point3d insertPoint)`
- `InsertLinearMaterial(Material material, Point3d startPoint, Point3d endPoint)`
- `CreateMaterialGeometry(Material material, MaterialType type)`

## Error Handling Strategy
- **Rhino API Exceptions**: Graceful handling z user feedback
- **Object Not Found**: Silent failure z logging
- **Permission Issues**: Error messages z suggestions
- **Document State**: Validation przed operations

## Performance Considerations
- **Bulk Operations**: Batch processing dla multiple objects
- **Change Notifications**: Minimal Rhino refreshes
- **Memory Management**: Proper disposal of Rhino objects
- **Threading**: UI thread considerations dla API calls

## Kryteria Ukończenia
- [ ] Material assignment z coloring działa
- [ ] Show/Hide operations functional
- [ ] Select/Lock operations functional  
- [ ] Geometry insertion dla both types
- [ ] Data persistence w User Data
- [ ] Error handling robust
- [ ] Performance w akceptowalnych limitach

## Pliki do Stworzenia
- `src/Services/IMaterialAssignmentService.cs`
- `src/Services/MaterialAssignmentService.cs`
- `src/Services/IObjectOperationsService.cs`
- `src/Services/ObjectOperationsService.cs`
- `src/Services/IGeometryInsertionService.cs`
- `src/Services/GeometryInsertionService.cs`
- Geometry creation helpers
- Error handling utilities

## Notatki Techniczne
- Always check if Rhino document is available
- Handle undo/redo operations properly
- Respect Rhino's object locking mechanisms
- Consider layer-based organization for materials
- Implement proper event handling dla document changes 