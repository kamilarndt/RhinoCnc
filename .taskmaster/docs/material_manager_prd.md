# Material Manager PRD
## Product Requirements Document - Menedżer Materiałów

### 1. Przegląd Produktu

**Nazwa produktu:** Material Manager (Menedżer Materiałów)  
**Wersja:** 1.0  
**Data:** 2024  
**Platforma:** Wtyczka Rhino 3D (.NET 8.0, WPF)

**Opis:**
Menedżer Materiałów to zaawansowany system zarządzania materiałami budowlanymi zintegrowany z Rhino 3D. Umożliwia przeglądanie, organizowanie i przypisywanie materiałów do obiektów 3D z intuicyjnym interfejsem kafelkowym i listowym.

### 2. Cele Biznesowe

- **Główny cel:** Stworzenie efektywnego narzędzia do zarządzania biblioteką materiałów w projektach architektonicznych i konstrukcyjnych
- **Cele dodatkowe:**
  - Przyspieszenie procesu wyboru materiałów
  - Standaryzacja biblioteki materiałów w firmie
  - Integracja z procesem projektowania w Rhino 3D
  - Możliwość zarządzania grupami materiałów (np. różne grubości MDF)

### 3. Użytkownicy Docelowi

**Główni użytkownicy:**
- Architekci pracujący w Rhino 3D
- Projektanci mebli i elementów konstrukcyjnych
- Inżynierowie budowlani
- Specjaliści ds. kosztorysowania

**Poziom doświadczenia:** Średni do zaawansowanego w Rhino 3D

### 4. Funkcjonalności Główne

#### 4.1 Architektura Systemu Materiałów

**Główne komponenty:**

1. **Baza Materiałowa (Material Catalog)** - Pełna biblioteka wszystkich dostępnych materiałów
2. **Paleta Projektu (Project Palette)** - Widok/placeholder pokazujący tylko materiały oznaczone jako "wybrane dla projektu"

#### 4.2 Baza Materiałowa (Material Catalog)

**Funkcjonalności:**
- Przeglądanie pełnej biblioteki materiałów
- Dodawanie nowych materiałów do bazy
- Edycja istniejących grup materiałów
- Kategoryzacja materiałów
- Wyszukiwanie i filtrowanie
- Import/export całej bazy

**Interfejs Bazy Materiałowej:**
- Widok tabelaryczny z kolumnami: Material Name, Type, Thickness, Dimensions, Price/m², Notes
- Panel kategorii po lewej stronie (MDF, SKLEJKA, WTOROWA, etc.)
- Przycisk "Add Selected Materials" do dodawania do palety projektu
- Możliwość wielokrotnego wyboru materiałów (Ctrl+Click)

#### 4.3 Paleta Projektu (Project Palette)

**Funkcjonalności:**
- Wyświetlanie tylko materiałów z bazy, które mają flagę `IsInProjectPalette = true`
- Dodawanie materiałów do palety (ustawienie flagi `IsInProjectPalette = true`)
- Usuwanie materiałów z palety (ustawienie flagi `IsInProjectPalette = false`)
- Zmiana kolejności materiałów w palecie (drag & drop)
- Przypisywanie materiałów do obiektów w Rhino

**Uwaga:** Paleta to tylko widok/filtr materiałów z głównej bazy - nie przechowuje osobnych danych!

**Interfejs Palety Projektu:**

**Widok Kafelkowy:**
- Kafelki materiałów z kolorowym tłem odpowiadającym kategorii
- Ikony funkcyjne: żarówka, ręka, kursor, kłódka
- Nazwa materiału (np. "WTOROWA LAMINOWANA")
- Wymiary (np. "2800x2070")
- Grubość z wyróżnieniem kolorystycznym (np. "18" w czerwonym kolorze)
- Responsywny układ kafelków

**Widok Listowy:**
- Kompaktowy widok poziomy
- Ikona kategorii po lewej stronie
- Nazwa materiału na kolorowym tle kategorii
- Grubość z wyróżnieniem
- Wymiary
- Ikony funkcyjne po prawej stronie
- Responsywny design

#### 4.4 System Kategorii

**Kategorie materiałów z przypisanymi kolorami:**
- **MDF** - kolor pomarańczowy/brzoskwiniowy
- **SKLEJKA** - kolor żółty/złoty
- **WTOROWA** - kolor czerwony
- **Inne kategorie** - według potrzeb projektu

**Funkcjonalności kategorii:**
- Filtrowanie materiałów według kategorii
- Kolorystyczne oznaczenie w całym interfejsie
- Możliwość dodawania nowych kategorii

#### 4.5 Zarządzanie Grupami Materiałów

**Edycja grup materiałów:**
- Edycja tylko na poziomie grupy (np. "MDF")
- Automatyczne generowanie wariantów grubości
- Definicja dostępnych grubości w grupie
- Wspólne właściwości dla całej grupy (nazwa, kategoria, wymiary standardowe)

**Właściwości grupy materiałów:**
- Nazwa grupy (np. "MDF")
- Kategoria (wpływa na kolor)
- Wymiary standardowe (np. "2800x2070")
- Lista dostępnych grubości
- Opis grupy

#### 4.6 Funkcje Interakcji

**Ikony funkcyjne (w kolejności):**
1. **Żarówka** - Podświetlenie/zaznaczenie materiału
2. **Ręka** - Funkcja manipulacji/przenoszenia
3. **Kursor** - Funkcja selekcji/wskazania
4. **Kłódka** - Blokowanie obiektów w przestrzeni Rhino

**Drag & Drop:**
- Zmiana kolejności kafelków w palecie
- Zmiana kolejności elementów w widoku listowym
- Intuicyjne przenoszenie między pozycjami

#### 4.7 Integracja z Rhino 3D

**Funkcje blokowania:**
- Blokowanie wybranych obiektów w przestrzeni 3D
- Odblokowywanie obiektów
- Wizualne oznaczenie zablokowanych obiektów

**Przypisywanie materiałów:**
- Przypisywanie materiału do wybranych obiektów
- Podgląd materiału na obiekcie
- Zarządzanie właściwościami materiału obiektu

### 5. Wymagania Techniczne

#### 5.1 Architektura

**Framework:** .NET 8.0  
**UI Framework:** WPF (Windows Presentation Foundation)  
**Wzorzec architektoniczny:** MVVM (Model-View-ViewModel)  
**Serializacja danych:** JSON (Newtonsoft.Json)

#### 5.2 Struktura Klas

**Modele:**
```csharp
// Grupa materiałów (np. MDF) - w bazie materiałowej
public class MaterialGroup
{
    public string Id { get; set; }             // Unikalny identyfikator
    public string Name { get; set; }           // "MDF"
    public string Category { get; set; }       // "MDF"
    public string StandardDimensions { get; set; } // "2800x2070"
    public List<int> AvailableThicknesses { get; set; } // [8, 10, 12, 16, 18, 22]
    public string Description { get; set; }
    public string ColorHex { get; set; }       // Kolor kategorii
    public decimal PricePerSquareMeter { get; set; } // Cena za m²
    public string Notes { get; set; }          // Dodatkowe notatki
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }
}

// Pojedynczy materiał (wygenerowany z grupy)
public class Material : INotifyPropertyChanged
{
    public string Id { get; set; }
    public string GroupId { get; set; }        // Referencja do MaterialGroup
    public string GroupName { get; set; }      // "MDF"
    public string DisplayName { get; set; }    // "MDF"
    public string Category { get; set; }       // "MDF"
    public int Thickness { get; set; }         // 18
    public string Dimensions { get; set; }     // "2800x2070"
    public string ColorHex { get; set; }       // Kolor kategorii
    public bool IsLocked { get; set; }         // Stan blokady
    public int SortOrder { get; set; }         // Kolejność w palecie
    public decimal PricePerSquareMeter { get; set; }
    public string Notes { get; set; }
    public bool IsInProjectPalette { get; set; } // Czy jest w palecie projektu
    
    // Nowe właściwości dla funkcjonalności wstawiania geometrii
    public MaterialType Type { get; set; }     // Sheet (płyta) / Linear (liniowy)
    public double MaxLength { get; set; }      // Maksymalna długość (dla materiałów liniowych)
    public string CrossSection { get; set; }   // Przekrój (np. "50x100mm" dla belek)
    
    // Właściwości dla UI
    public string ThicknessDisplay => $"{Thickness}";
    public string PriceDisplay => $"€{PricePerSquareMeter:F2}";
    public SolidColorBrush CategoryBrush => new SolidColorBrush((Color)ColorConverter.ConvertFromString(ColorHex));
    public bool IsSheetMaterial => Type == MaterialType.Sheet;
    public bool IsLinearMaterial => Type == MaterialType.Linear;
}

// Enum dla typu materiału
public enum MaterialType
{
    Sheet,  // Płyta (np. MDF, Sklejka) - wstawiamy prostokąt
    Linear  // Liniowy (np. belka, profil) - wstawiamy przekrój + długość
}

// Uwaga: Paleta projektu to tylko widok - nie potrzebujemy osobnej klasy!
// Materiały w palecie to te z bazy, które mają IsInProjectPalette = true
```

**ViewModels:**
```csharp
// ViewModel dla bazy materiałowej
public class MaterialCatalogViewModel : BaseViewModel
{
    public ObservableCollection<Material> AllMaterials { get; set; }
    public ObservableCollection<Material> FilteredMaterials { get; set; }
    public ObservableCollection<string> Categories { get; set; }
    public ObservableCollection<Material> SelectedMaterials { get; set; }
    public string SelectedCategory { get; set; }
    public string SearchText { get; set; }
    
    // Komendy
    public ICommand FilterByCategoryCommand { get; set; }
    public ICommand SearchCommand { get; set; }
    public ICommand AddNewMaterialGroupCommand { get; set; }
    public ICommand EditMaterialGroupCommand { get; set; }
    public ICommand AddSelectedToPaletteCommand { get; set; } // Ustawia IsInProjectPalette = true
    public ICommand SelectAllCommand { get; set; }
    public ICommand ClearSelectionCommand { get; set; }
}

// ViewModel dla palety projektu
public class MaterialPaletteViewModel : BaseViewModel
{
    public ObservableCollection<Material> ProjectMaterials { get; set; } // Filtrowane: IsInProjectPalette = true
    public ObservableCollection<string> Categories { get; set; }
    public string SelectedCategory { get; set; }
    public bool IsTileView { get; set; } = true;
    
    // Komendy podstawowe
    public ICommand ToggleViewCommand { get; set; }
    public ICommand SelectCategoryCommand { get; set; }
    public ICommand ReorderMaterialsCommand { get; set; }
    public ICommand RemoveFromPaletteCommand { get; set; } // Ustawia IsInProjectPalette = false
    public ICommand OpenMaterialCatalogCommand { get; set; }
    
    // Nowe komendy - 5 głównych funkcji z kafelków
    public ICommand ToggleObjectsVisibilityCommand { get; set; } // Oko - pokaż/ukryj obiekty
    public ICommand AssignMaterialToObjectCommand { get; set; }  // Pędzel - przypisz + koloruj
    public ICommand SelectObjectsWithMaterialCommand { get; set; } // Selekcja - zaznacz obiekty
    public ICommand LockObjectsWithMaterialCommand { get; set; } // Kłódka - zablokuj obiekty
    public ICommand InsertMaterialGeometryCommand { get; set; }  // Plus - wstaw geometrię
}
```

**Serwisy:**
```csharp
// Serwis do zarządzania bazą materiałową
public class MaterialCatalogService
{
    public List<MaterialGroup> LoadAllMaterialGroups();
    public List<Material> GenerateMaterialsFromGroups(List<MaterialGroup> groups);
    public void SaveMaterialGroups(List<MaterialGroup> groups);
    public MaterialGroup AddNewMaterialGroup(MaterialGroup group);
    public void UpdateMaterialGroup(MaterialGroup group);
    public void DeleteMaterialGroup(string groupId);
    public List<Material> SearchMaterials(string searchText, string category = null);
    public void ImportMaterialsFromFile(string filePath);
    public void ExportMaterialsToFile(string filePath);
}

// Serwis do obsługi operacji Rhino
public class RhinoMaterialService
{
    // Operacje na obiektach z materiałem
    public void ToggleObjectsVisibility(string materialId);
    public void AssignMaterialToObjects(Material material, List<RhinoObject> objects);
    public List<RhinoObject> SelectObjectsWithMaterial(string materialId);
    public void LockObjectsWithMaterial(string materialId);
    
    // Wstawianie geometrii
    public RhinoObject InsertSheetGeometry(Material material); // Dla płyt
    public RhinoObject InsertLinearGeometry(Material material, double length); // Dla materiałów liniowych
    
    // Pomocnicze
    public List<RhinoObject> GetObjectsWithMaterial(string materialId);
    public void SetObjectMaterial(RhinoObject obj, Material material);
    public void SetObjectColor(RhinoObject obj, string colorHex);
    public string GetObjectMaterial(RhinoObject obj);
}

// Uwaga: Nie potrzebujemy osobnego serwisu dla palety!
// Paleta to tylko filtrowane materiały z MaterialCatalogService gdzie IsInProjectPalette = true
```

#### 5.3 Zarządzanie Danymi

**Plik konfiguracyjny grup materiałów (JSON):**
```json
{
  "materialGroups": [
    {
      "name": "MDF",
      "category": "MDF",
      "standardDimensions": "2800x2070",
      "availableThicknesses": [8, 10, 12, 16, 18, 22],
      "description": "Płyta MDF wysokiej jakości",
      "colorHex": "#FFA500"
    },
    {
      "name": "SKLEJKA",
      "category": "SKLEJKA", 
      "standardDimensions": "2800x2070",
      "availableThicknesses": [12, 15, 18, 21],
      "description": "Sklejka brzozowa",
      "colorHex": "#FFD700"
    }
  ]
}
```

### 6. Interfejs Użytkownika - Specyfikacja Szczegółowa

#### 6.1 Kafelek Materiału (Widok Kafelkowy)

**Wymiary:** 200x120 pikseli (proporcjonalne skalowanie)  
**Struktura:**
- **Górna sekcja (30px):** Czarne tło z ikonami funkcyjnymi (białe ikony, 16x16px)
- **Środkowa sekcja:** Kolorowe tło kategorii z nazwą materiału (biały tekst, pogrubiony)
- **Dolna sekcja (30px):** Czarne tło z wymiarami i grubością

**Ikony funkcyjne (16x16px, białe, odstęp 8px):**
1. **Oko (👁️)** - Pokaż/Ukryj obiekty z tym materiałem
2. **Pędzel (🖌️)** - Przypisz materiał do obiektu + automatyczne kolorowanie
3. **Selekcja (🎯)** - Zaznacz wszystkie obiekty z tym materiałem  
4. **Kłódka (🔒)** - Zablokuj wszystkie obiekty z tym materiałem
5. **Plus (➕)** - Wstaw geometrię (płyta/przekrój) z tym materiałem

**Typografia:**
- Nazwa materiału: Bold, 12px, biały
- Wymiary: Regular, 10px, biały
- Grubość: Bold, 16px, kolor akcentowy (czerwony/pomarańczowy)

#### 6.2 Element Listy (Widok Listowy)

**Wysokość:** 50px  
**Struktura pozioma:**
- **Ikona kategorii (40px):** Po lewej, na czarnym tle
- **Nazwa materiału:** Na kolorowym tle kategorii, biały tekst
- **Grubość:** Wyróżniona kolorystycznie
- **Wymiary:** Standardowy tekst
- **Ikony funkcyjne:** Po prawej stronie

### 7. Przepływ Użytkownika

#### 7.1 Podstawowe Scenariusze

**Scenariusz 1: Przeglądanie palety projektu**
1. Użytkownik otwiera panel Material Palette
2. Widzi tylko materiały wybrane dla bieżącego projektu
3. Może przełączać między widokiem kafelkowym a listowym
4. Może filtrować według kategorii

**Scenariusz 2: Dodawanie materiałów do palety**
1. Użytkownik klika "Add Materials" w palecie projektu
2. Otwiera się okno Material Catalog z pełną bazą materiałów
3. Użytkownik filtruje/wyszukuje potrzebne materiały
4. Zaznacza materiały (wielokrotny wybór Ctrl+Click)
5. Klika "Add to Palette"
6. Materiały otrzymują flagę `IsInProjectPalette = true` i pojawiają się w palecie

**Scenariusz 3: Dodawanie nowego materiału do bazy**
1. Użytkownik otwiera Material Catalog
2. Klika "Add New Material Group"
3. Wypełnia formularz: nazwa, kategoria, wymiary, grubości, cena
4. System generuje wszystkie warianty grubości
5. Nowe materiały są dostępne w bazie

**Scenariusz 4: Zarządzanie kolejnością w palecie**
1. Użytkownik przeciąga kafelek materiału w palecie projektu
2. System aktualizuje właściwość `SortOrder` materiałów
3. Nowa kolejność jest zapisywana w bazie materiałów

**Scenariusz 5: Usuwanie materiału z palety**
1. Użytkownik klika prawym przyciskiem na materiał w palecie
2. Wybiera "Remove from Palette"
3. Materiał otrzymuje flagę `IsInProjectPalette = false` i znika z palety (ale pozostaje w bazie)

**Scenariusz 6: Operacje na obiektach z materiałem**

*6A. Pokaż/Ukryj obiekty:*
1. Użytkownik klika ikonę oka na kafelku materiału
2. Wszystkie obiekty z tym materiałem zostają ukryte/pokazane w Rhino
3. Toggle visibility - kolejne kliknięcie przywraca widoczność

*6B. Przypisz materiał do obiektu:*
1. Użytkownik klika ikonę pędzla na kafelku materiału
2. Wybiera obiekty w Rhino (tryb selekcji)
3. Materiał zostaje przypisany do wybranych obiektów
4. **Automatyczne kolorowanie** - obiekty otrzymują kolor materiału

*6C. Zaznacz wszystkie obiekty:*
1. Użytkownik klika ikonę selekcji na kafelku materiału
2. Wszystkie obiekty z tym materiałem zostają automatycznie zaznaczone w Rhino
3. Ułatwia grupowe operacje (przesuwanie, kopiowanie, etc.)

*6D. Zablokuj wszystkie obiekty:*
1. Użytkownik klika ikonę kłódki na kafelku materiału
2. Wszystkie obiekty z tym materiałem zostają zablokowane w Rhino
3. Uniemożliwia przypadkowe modyfikacje

**Scenariusz 7: Wstawianie geometrii materiału**

*7A. Wstaw płytę (materiały płytowe):*
1. Użytkownik klika ikonę plus na kafelku materiału płytowego (np. MDF)
2. System wstawia prostokąt o wymiarach płyty (np. 2800x2070mm)
3. Automatycznie przypisuje materiał i kolor do wstawionej geometrii
4. Grubość płyty jest zapisana w metadanych obiektu

*7B. Wstaw przekrój (materiały liniowe):*
1. Użytkownik klika ikonę plus na kafelku materiału liniowego (np. belka)
2. System wstawia przekrój elementu (np. profil 50x100mm)
3. Pokazuje maksymalną dostępną długość materiału
4. Użytkownik może dostosować długość do potrzeb projektu

### 8. Wymagania Niefunkcjonalne

#### 8.1 Wydajność
- Płynne przewijanie listy materiałów (60 FPS)
- Czas ładowania palety < 2 sekundy
- Responsywny drag & drop < 100ms opóźnienia

#### 8.2 Użyteczność
- Intuicyjny interfejs zgodny z konwencjami Windows
- Tooltips dla wszystkich ikon funkcyjnych
- Keyboard shortcuts dla podstawowych akcji
- Zgodność z motywami Windows (jasny/ciemny)

#### 8.3 Niezawodność
- Automatyczne zapisywanie zmian kolejności
- Obsługa błędów ładowania danych
- Backup konfiguracji materiałów
- Graceful degradation przy problemach z danymi

### 9. Harmonogram Implementacji

#### Faza 1: Podstawowa Struktura (Tydzień 1)
- [ ] Implementacja modeli danych (Material, MaterialGroup)
- [ ] Podstawowy MaterialCatalogService
- [ ] Szkielet MaterialPaletteViewModel

#### Faza 2: Interfejs Użytkownika (Tydzień 2)
- [ ] Programistyczne tworzenie kafelków materiałów
- [ ] Implementacja widoku listowego
- [ ] Przełączanie między widokami
- [ ] System kolorów kategorii

#### Faza 3: Funkcjonalności Interakcji (Tydzień 3)
- [ ] Drag & Drop do zmiany kolejności
- [ ] Implementacja ikon funkcyjnych
- [ ] Filtrowanie według kategorii
- [ ] Tooltips i keyboard shortcuts

#### Faza 4: Integracja z Rhino (Tydzień 4)
- [ ] Funkcja blokowania obiektów
- [ ] Przypisywanie materiałów do obiektów
- [ ] Synchronizacja z dokumentem Rhino
- [ ] Testowanie integracji

#### Faza 5: Zarządzanie Danymi (Tydzień 5)
- [ ] Edytor grup materiałów
- [ ] Import/export konfiguracji
- [ ] Walidacja danych
- [ ] Obsługa błędów

#### Faza 6: Testy i Optymalizacja (Tydzień 6)
- [ ] Testy jednostkowe
- [ ] Testy integracyjne
- [ ] Optymalizacja wydajności
- [ ] Dokumentacja użytkownika

### 10. Kryteria Akceptacji

**Funkcjonalność:**
- ✅ Wyświetlanie materiałów w widoku kafelkowym zgodnie ze wzorem
- ✅ Wyświetlanie materiałów w widoku listowym zgodnie ze wzorem
- ✅ Przełączanie między widokami
- ✅ Drag & Drop do zmiany kolejności
- ✅ Filtrowanie według kategorii materiałów
- ✅ Blokowanie obiektów w Rhino poprzez ikonę kłódki
- ✅ Edycja grup materiałów z automatycznym generowaniem wariantów

**Jakość:**
- ✅ Responsywny interfejs (< 100ms reakcja na akcje użytkownika)
- ✅ Zgodność z wyglądem i zachowaniem Windows
- ✅ Brak błędów krytycznych
- ✅ Intuicyjna obsługa bez potrzeby szkoleń

**Integracja:**
- ✅ Płynna integracja z Rhino 3D
- ✅ Zachowanie stanu między sesjami
- ✅ Kompatybilność z różnymi wersjami Rhino

### 11. Struktura Plików Danych

#### 11.1 Pliki Konfiguracyjne

**material_catalog.json** - Pełna baza materiałowa:
```json
{
  "version": "1.0",
  "materialGroups": [
    {
      "id": "mdf-001",
      "name": "MDF",
      "category": "MDF",
      "standardDimensions": "2800x2070",
      "availableThicknesses": [8, 10, 12, 16, 18, 22],
      "colorHex": "#FF8C00",
      "pricePerSquareMeter": 25.50,
      "notes": "Standard MDF board",
      "createdDate": "2024-01-01T00:00:00Z",
      "modifiedDate": "2024-01-01T00:00:00Z"
    }
  ]
}
```

**Uwaga:** Nie potrzebujemy osobnego pliku project_palette.json!
Paleta to tylko filtr materiałów z material_catalog.json gdzie `IsInProjectPalette = true`

**categories.json** - Definicje kategorii:
```json
{
  "version": "1.0",
  "categories": [
    {
      "id": "mdf",
      "name": "MDF",
      "colorHex": "#FF8C00",
      "description": "Płyty MDF"
    },
    {
      "id": "sklejka",
      "name": "SKLEJKA", 
      "colorHex": "#FFD700",
      "description": "Płyty sklejkowe"
    },
    {
      "id": "wtorowa",
      "name": "WTOROWA",
      "colorHex": "#DC143C",
      "description": "Płyty wtórne"
    }
  ]
}
```

#### 11.2 Uproszczona Architektura

**Material Catalog (Baza Materiałowa):**
- Przechowuje wszystkie dostępne materiały w jednym pliku
- Każdy materiał ma flagę `IsInProjectPalette` (true/false)
- Właściwość `SortOrder` określa kolejność w palecie
- Zarządza grupami materiałów
- Umożliwia dodawanie nowych materiałów

**Project Palette (Paleta Projektu):**
- To tylko **widok/filtr** materiałów z bazy gdzie `IsInProjectPalette = true`
- Nie przechowuje osobnych danych
- Nie potrzebuje osobnego pliku JSON
- Wszystkie operacje wykonywane są na głównej bazie materiałów

### 12. Załączniki

**Załącznik A:** Wzory interfejsu użytkownika (załączone zdjęcia)  
**Załącznik B:** Specyfikacja kolorów kategorii  
**Załącznik C:** Ikony funkcyjne (specyfikacja graficzna)  
**Załącznik D:** Przykładowa struktura danych JSON

---

**Zatwierdzone przez:** [Nazwa]  
**Data zatwierdzenia:** [Data]  
**Wersja dokumentu:** 1.0 