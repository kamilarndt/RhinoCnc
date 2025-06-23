# Epic 5: Material Catalog

**Priorytet**: Medium  
**Szacowany czas**: 4-5 dni  
**Status**: Nie rozpoczęty

## Cel
Implementacja kompletnego systemu zarządzania katalogiem materiałów z CRUD operations, search, filtering, oraz palette management (dodawanie/usuwanie z palety projektowej).

## Zadania

### Task 5.1: Material CRUD Operations
**Priorytet**: Medium  
**Szacowany czas**: 2 dni  
**Plik**: `task_5_1_material_crud.md`

Create, Read, Update, Delete operations dla materiałów w katalogu.

### Task 5.2: Catalog Search & Filtering  
**Priorytet**: Medium  
**Szacowany czas**: 1 dzień  
**Plik**: `task_5_2_catalog_search.md`

Advanced search i filtering po kategorii, typie, wymiarach, nazwie.

### Task 5.3: Palette Management
**Priorytet**: Medium  
**Szacowany czas**: 2 dni  
**Plik**: `task_5_3_palette_management.md`

Dodawanie/usuwanie materiałów z palety projektowej (IsInProjectPalette flag).

## Zależności
- **Epic 1**: Core Infrastructure
- **Epic 2**: Data Models
- **Epic 3**: Material Palette UI (dla palette operations)

## Kluczowe Features

### Material CRUD Interface
- **Create**: Nowy materiał z validation
- **Read**: Browse wszystkie materiały
- **Update**: Edit właściwości materiału  
- **Delete**: Usunięcie z confirmation

### Search & Filter Capabilities
- **Text Search**: Nazwa, opis, producent
- **Category Filter**: MDF, SKLEJKA, WTOROWA, INNE
- **Type Filter**: Sheet, Linear, Custom
- **Dimension Filter**: Grubość, szerokość ranges
- **In-Palette Filter**: Tylko materiały w palecie

### Palette Management
- **Add to Palette**: Set IsInProjectPalette = true
- **Remove from Palette**: Set IsInProjectPalette = false  
- **Bulk Operations**: Multiple materials at once
- **Validation**: Check for duplicates, limits

## Data Architecture

### Single JSON Database
```json
{
  "version": "1.0",
  "materials": [
    {
      "id": "uuid",
      "name": "MDF 18mm",
      "category": "MDF", 
      "isInProjectPalette": true,
      "dimensions": {...},
      "metadata": {...}
    }
  ]
}
```

### IsInProjectPalette Logic
- **true**: Materiał widoczny w palecie projektowej
- **false**: Materiał tylko w katalogu, nie w palecie
- **Palette View**: Filter by isInProjectPalette = true
- **Catalog View**: Show wszystkie materiały

## Services do Implementacji

### IMaterialCatalogService
- `GetAllMaterials()`
- `SearchMaterials(string query, MaterialFilter filter)`
- `CreateMaterial(Material material)`
- `UpdateMaterial(Material material)`
- `DeleteMaterial(string materialId)`
- `GetMaterialById(string id)`

### IPaletteManagerService
- `AddToPalette(string materialId)`
- `RemoveFromPalette(string materialId)`
- `GetPaletteMaterials()`
- `IsMaterialInPalette(string materialId)`
- `ClearPalette()`

## UI Components

### Catalog Browser
- Grid/List view wszystkich materiałów
- Search box z real-time filtering
- Category/Type filter dropdowns
- Add/Remove from palette buttons

### Material Editor Dialog
- Form do tworzenia/edycji materiału
- Validation dla wszystkich fields
- Preview thumbnail
- Category/Type selection

## Kryteria Ukończenia
- [ ] CRUD operations functional
- [ ] Search i filtering działa smoothly
- [ ] Palette add/remove operations
- [ ] Data persistence reliable
- [ ] UI responsive dla large catalogs
- [ ] Error handling comprehensive

## Pliki do Stworzenia
- `src/Services/IMaterialCatalogService.cs`
- `src/Services/MaterialCatalogService.cs`
- `src/Services/IPaletteManagerService.cs` 
- `src/Services/PaletteManagerService.cs`
- `ui/Dialogs/MaterialEditorDialog.cs`
- `ui/Controls/CatalogBrowserControl.cs`
- ViewModels dla catalog operations

## Performance Considerations
- **Large Catalogs**: Lazy loading, virtualization
- **Search Performance**: Indexed searching
- **JSON I/O**: Async operations, background saving
- **Memory Usage**: Efficient collection management

## Notatki
Epic 5 łączy catalog management z palette functionality. Design musi być scalable dla dużych kolekcji materiałów i efficient dla frequent palette operations. 