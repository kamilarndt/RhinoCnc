# Material Manager - Product Requirements Document

**Wersja**: 1.0  
**Data**: Styczeń 2024  
**Autor**: RhinoCNC Development Team  
**Status**: Draft

## 1. Przegląd Produktu

### 1.1 Cel Biznesowy
Material Manager to komponent systemu RhinoCNC Suite umożliwiający efektywne zarządzanie materiałami w projektach CNC. System pozwala operatorom na szybkie przypisywanie materiałów do obiektów, zarządzanie paletą materiałów projektowych oraz wykonywanie operacji na obiektach Rhino na podstawie materiałów.

### 1.2 Zakres Produktu
- **In Scope**: Paleta materiałów projektowych, operacje na obiektach Rhino, katalog materiałów, import/export bazy materiałów
- **Out of Scope**: Cenniki materiałów, kalkulacje kosztów, integracja z systemami ERP, automatyczne rozpoznawanie materiałów

### 1.3 Kluczowi Użytkownicy
- **Operatorzy CNC** - codzienne użycie palety materiałów
- **Projektanci** - przypisywanie materiałów w projektach
- **Administratorzy** - zarządzanie katalogiem materiałów

## 2. Wymagania Funkcjonalne

### 2.1 Paleta Materiałów Projektowych

#### 2.1.1 Podstawowe Funkcjonalności
**Requirement ID**: FM-001  
**Priorytet**: Critical

**Opis**: Paleta wyświetla materiały oznaczone jako `IsInProjectPalette = true` z głównej bazy danych.

**Funkcjonalności**:
- Widok kafelkowy z kolorowymi kafelkami materiałów
- Widok listowy z tabelą (Nazwa, Kategoria, Typ, Grubość, Wymiary)
- Przełączanie między widokami jednym przyciskiem
- Sortowanie materiałów według `SortOrder`
- Drag & drop reordering w ramach palety

**Kryteria Akceptacji**:
- [ ] Paleta ładuje się w czasie < 200ms
- [ ] Przełączanie widoków jest natychmiastowe
- [ ] Drag & drop działa płynnie bez flickering
- [ ] Materiały są sortowane zgodnie z SortOrder

#### 2.1.2 Operacje na Materiałach
**Requirement ID**: FM-002  
**Priorytet**: Critical

**Opis**: Każdy materiał w palecie ma 5 ikon operacji:

1. **👁️ Show/Hide** - Pokazuje/ukrywa obiekty z tym materiałem
2. **🖌️ Assign** - Przypisuje materiał do zaznaczonych obiektów + automatyczne kolorowanie
3. **🎯 Select** - Zaznacza wszystkie obiekty z tym materiałem  
4. **🔒 Lock** - Blokuje wszystkie obiekty z tym materiałem w przestrzeni Rhino
5. **➕ Insert** - Wstawia geometrię (prostokąty dla sheets, cross-sections dla linear)

**Kryteria Akceptacji**:
- [ ] Wszystkie operacje wykonują się w czasie < 100ms
- [ ] Show/Hide używa Rhino visibility API
- [ ] Assign automatycznie koloruje obiekty
- [ ] Select zaznacza obiekty w Rhino viewport
- [ ] Lock blokuje obiekty (nie edycję materiału)
- [ ] Insert generuje odpowiednią geometrię

## 3. Wymagania Techniczne

### 3.1 Architektura
- **.NET 8.0** - Target framework
- **WPF** - UI framework (programmatically created, no XAML)
- **MVVM** - Architecture pattern
- **Rhino 3D API** - Integration layer
- **Newtonsoft.Json** - Serialization

### 3.2 Data Structure

#### 3.2.1 Single Database Approach
**Design Decision**: Jeden plik `material_catalog.json` z flag-based filtering

```csharp
public class Material
{
    public string Id { get; set; }
    public string Name { get; set; }
    public MaterialCategory Category { get; set; }
    public MaterialType Type { get; set; } // Sheet, Linear
    public double? Thickness { get; set; }
    public MaterialDimensions Dimensions { get; set; }
    public string Color { get; set; } // Hex color
    public bool IsInProjectPalette { get; set; } // KEY FLAG
    public int SortOrder { get; set; }
    public Dictionary<string, object> Properties { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }
}
```

## 4. User Scenarios

### 4.1 Typowy Workflow Operatora

**Scenario**: Operator rozpoczyna pracę nad nowym projektem CNC

1. **Otwarcie palety materiałów**
   - Operator klika przycisk "Material Palette" lub używa skrótu
   - System wyświetla paletę z materiałami `IsInProjectPalette = true`

2. **Przypisywanie materiałów**
   - Operator zaznacza obiekty w Rhino
   - Klika ikonę 🖌️ na wybranym materiale
   - System przypisuje materiał i automatycznie koloruje obiekty

3. **Praca z obiektami materiałowymi**
   - Operator używa 🎯 do zaznaczenia wszystkich obiektów z materiałem
   - Używa 🔒 do zablokowania obiektów przed przypadkową edycją
   - Używa 👁️ do temporalnego ukrycia materiałów

## 5. Development Phases

### 5.1 MVP (Minimum Viable Product) - 2 tygodnie
**Cel**: Podstawowa funkcjonalność palety materiałów

**Features**:
- Podstawowy Material model z JSON persistence
- Project Palette z tile/list views
- Basic Rhino operations (assign, select, show/hide)
- Simple material catalog

### 5.2 Extended Version - 3 tygodnie
**Cel**: Pełna funkcjonalność operacyjna

**Features**:
- Wszystkie 5 operacji palety (+ lock, insert)
- Category system z color coding
- Search i filtering
- Material catalog z CRUD operations

---

**Document Status**: Draft - Ready for Review 