# Epic 3: Material Palette UI

**Priorytet**: High  
**Szacowany czas**: 6-7 dni  
**Status**: Nie rozpoczęty

## Cel
Implementacja kompletnego interfejsu Material Palette z tile/list views, 5 operacjami materiałowymi (👁️🖌️🎯🔒➕), drag & drop reordering i responsive design.

## Zadania

### Task 3.1: MaterialPaletteControl Implementation
**Priorytet**: High  
**Szacowany czas**: 3 dni  
**Plik**: `task_3_1_material_palette_control.md`

Główny control palety z tile/list view switching, programmatic UI creation (no XAML).

### Task 3.2: Material Operations Implementation  
**Priorytet**: High  
**Szacowany czas**: 2 dni  
**Plik**: `task_3_2_material_operations.md`

Implementacja 5 ikon operacji: Show/Hide, Assign, Select, Lock, Insert.

### Task 3.3: Drag & Drop Functionality
**Priorytet**: Medium  
**Szacowany czas**: 1 dzień  
**Plik**: `task_3_3_drag_drop.md`

Drag & drop reordering materiałów w palecie z visual feedback.

### Task 3.4: Search and Filtering
**Priorytet**: Medium  
**Szacowany czas**: 1 dzień  
**Plik**: `task_3_4_search_filtering.md`

Search box i filtering po kategoriach, typach materiałów.

## Zależności
- **Epic 1**: Core Infrastructure (BaseViewModel, RelayCommand)
- **Epic 2**: Data Models (Material, MaterialCollection)

## Kluczowe Features

### Tile View Mode
- Kolorowe kafelki według kategorii materiału
- Ikony operacji na hover/focus
- Material name i basic info
- Visual feedback dla aktywnych operacji

### List View Mode  
- Tabela z kolumnami: Nazwa, Kategoria, Typ, Grubość, Wymiary
- Sortowanie po kolumnach
- Checkbox selection dla bulk operations
- Compact display dla wielu materiałów

### 5 Material Operations
1. **👁️ Show/Hide** - Toggle visibility w Rhino
2. **🖌️ Assign** - Przypisanie + auto-coloring  
3. **🎯 Select** - Select wszystkie obiekty
4. **🔒 Lock** - Lock wszystkie obiekty
5. **➕ Insert** - Insert geometry (sheet/linear)

## UI Design Principles
- **No XAML**: Programmatic UI creation tylko
- **Responsive**: Działanie w różnych rozmiarach panelu
- **Performance**: Smooth scrolling dla dużych kolekcji
- **Accessibility**: Keyboard navigation support

## Kryteria Ukończenia
- [ ] Tile i List view modes działają poprawnie
- [ ] Wszystkie 5 operacji są functional
- [ ] Drag & drop reordering działa
- [ ] Search i filtering functional
- [ ] Performance < 100ms dla view switching
- [ ] Memory usage optimized dla large collections

## Pliki do Stworzenia
- `ui/MaterialPaletteControl.cs` (już częściowo istnieje)
- `ui/Controls/MaterialTileControl.cs`
- `ui/Controls/MaterialListControl.cs` 
- `ui/Converters/CategoryToBrushConverter.cs`
- ViewModels dla controls
- Style i theme resources

## Notatki Techniczne
- Użyć VirtualizingStackPanel dla performance
- Implement proper disposal dla event handlers
- Error handling dla Rhino API calls
- Visual states dla loading/error conditions 