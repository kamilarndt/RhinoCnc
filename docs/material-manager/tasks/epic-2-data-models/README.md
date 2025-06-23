# Epic 2: Data Models

**Priorytet**: Critical  
**Szacowany czas**: 4-5 dni  
**Status**: Nie rozpoczęty

## Cel
Stworzenie kompletnych modeli danych dla Material Manager z Material, MaterialCollection, JSON serialization/deserialization, validation rules i data persistence strategy.

## Zadania

### Task 2.1: Material Model Implementation
**Priorytet**: Critical  
**Szacowany czas**: 2 dni  
**Plik**: `task_2_1_material_model.md`

Implementacja podstawowego Material model z wszystkimi properties, validation rules i JSON attributes.

### Task 2.2: Material Collection Implementation  
**Priorytet**: Critical  
**Szacowany czas**: 1 dzień  
**Plik**: `task_2_2_material_collection.md`

Stworzenie MaterialCollection z CRUD operations, filtering, sorting i change tracking.

### Task 2.3: JSON Serialization Strategy
**Priorytet**: Critical  
**Szacowany czas**: 1 dzień  
**Plik**: `task_2_3_json_serialization.md`

Implementacja JSON serialization/deserialization z custom converters, versioning i migration support.

### Task 2.4: Data Validation Framework
**Priorytet**: High  
**Szacowany czas**: 1 dzień  
**Plik**: `task_2_4_data_validation.md`

Stworzenie validation framework z business rules, cross-property validation i error reporting.

## Zależności
- **Epic 1**: Core Infrastructure (BaseViewModel, Service Locator)

## Kryteria Ukończenia
- [ ] Material model z wszystkimi properties i validation
- [ ] MaterialCollection z CRUD operations
- [ ] JSON persistence działa poprawnie
- [ ] Validation framework kompletny
- [ ] Wszystkie testy jednostkowe przechodzą
- [ ] Performance tests w akceptowalnych limitach

## Kluczowe Decyzje Architektoniczne

### Single Database Approach
- **Decyzja**: Jeden `material_catalog.json` z flag-based filtering
- **Uzasadnienie**: Prostota, atomic operations, łatwiejsze backup/restore
- **Alternative**: Separate project_palette.json (odrzucone ze względu na synchronizację)

### Category System
- **MDF**: Orange (#FF6B35)
- **SKLEJKA**: Yellow (#FFD700)  
- **WTOROWA**: Red (#FF4444)
- **INNE**: Blue (#4169E1)

### Material Types
- **Sheet Materials**: MDF, Plywood - prostokąty przy insert
- **Linear Materials**: Profiles, beams - cross-sections przy insert

## Pliki do Stworzenia
- `src/Models/Material.cs`
- `src/Models/MaterialCollection.cs`
- `src/Models/MaterialCategory.cs`
- `src/Models/MaterialType.cs`
- `src/Models/MaterialDimensions.cs`
- JSON serialization helpers
- Validation attributes i rules
- Odpowiednie testy jednostkowe

## Notatki
Epic 2 tworzy foundation dla wszystkich operacji na danych. Design musi wspierać future requirements jak material groups, pricing, supplier info etc. 