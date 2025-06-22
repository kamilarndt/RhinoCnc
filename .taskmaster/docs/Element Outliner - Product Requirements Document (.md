<img src="https://r2cdn.perplexity.ai/pplx-full-logo-primary-dark%402x.png" class="logo" width="120"/>

# Element Outliner - Product Requirements Document (PRD)

## 1. Wprowadzenie i Wizja

### 1.1 Cel Produktu

Element Outliner to zaawansowany komponent wtyczki Rhino Production Suite, zaprojektowany jako narzędzie do zarządzania, organizacji i przygotowania elementów (bloków) do produkcji CNC [^1]. System ma na celu zastąpienie standardowego zarządzania blokami Rhino przez inteligentny outliner oferujący funkcje edycji in-place wzorowane na wtyczce Block Edit New [^1].

### 1.2 Zakres Dokumentu

Niniejszy PRD definiuje kompletne wymagania funkcjonalne i niefunkcjonalne dla komponentu Element Outliner w ramach wtyczki Rhino Production Suite dla Rhino 8 na platformie Windows [^2][^3]. Dokument obejmuje specyfikację interfejsu użytkownika, architektury danych, funkcjonalności oraz kryteriów akceptacji [^4][^5].

## 2. Kontekst i Analiza Rynku

### 2.1 Problem Biznesowy

Standardowe narzędzia Rhino do zarządzania blokami nie oferują zaawansowanych funkcji potrzebnych w środowisku produkcyjnym CNC [^1]. Użytkownicy potrzebują:

- Hierarchicznego zarządzania elementami z możliwością zagnieżdżania [^6][^7]
- Edycji in-place bez niszczenia relacji między instancjami [^1]
- Przypisywania dokumentacji technicznej do elementów [^8]
- Integracji z workflow produkcyjnym [^8]


### 2.2 Użytkownicy Docelowi

- **Projektanci CAD** pracujący z elementami powtarzalnymi
- **Inżynierowie produkcji** przygotowujący modele do obróbki CNC
- **Menedżerowie projektów** nadzorujący proces projektowo-produkcyjny


## 3. Wymagania Funkcjonalne

### 3.1 Zarządzanie Hierarchią Elementów

#### F-01: TreeView Interface

System musi implementować kontrolkę TreeView dla Windows Forms zapewniającą [^6][^7]:

- Hierarchiczne wyświetlanie elementów z możliwością rozwijania/zwijania węzłów
- Obsługę wielopoziomowego zagnieżdżenia elementów
- Ikonografię statusu dla każdego elementu (edytowany, zablokowany, zawiera dokumentację)
- Drag \& drop do reorganizacji hierarchii elementów [^9][^10]


#### F-02: Operacje na Elementach

**Tworzenie elementów:**

- Funkcja "Create Element" grupująca wybrane obiekty Rhino w nowy element (blok)
- Automatyczne nadawanie nazw z możliwością późniejszej edycji
- Zachowanie właściwości materiałowych obiektów składowych

**Zarządzanie wyborem:**

- Select/Hide/Isolate dla elementów bezpośrednio z outlinera [^7]
- Selekcja wielokrotna z użyciem checkboxów [^10]
- Preview zawartości elementu w tooltip lub sub-panelu


### 3.2 System Edycji In-Place

#### F-03: Mechanizm Wejścia w Tryb Edycji

Implementacja dokładnie wzorowana na Block Edit New:

- **Podwójne kliknięcie** na element w outlinerze lub w viewporcie otwiera tryb edycji
- Automatyczne ukrywanie/blokowanie obiektów zewnętrznych (konfigurowalne)
- Wyświetlenie panelu kontrolnego trybu edycji [^1]
- Obsługa zagnieżdżonych elementów z ciągłym zagłębianiem się


#### F-04: Panel Kontrolny Trybu Edycji

**Nagłówek informacyjny:**

- Edytowalna nazwa elementu
- Licznik instancji (np. "x3" dla trzech kopii)
- Wskaźnik poziomu zagnieżdżenia (breadcrumb navigation)

**Podstawowe operacje:**

- **Save** - zapisz zmiany i kontynuuj edycję
- **Cancel** - anuluj zmiany i wyjdź z edycji
- **Add** - dodaj obiekty z zewnątrz do elementu
- **Cut** - usuń obiekty z elementu z opcją wyboru miejsca wklejenia


#### F-05: Wyjście z Trybu Edycji

- **Podwójne kliknięcie w pustą przestrzeń** - automatyczne zapisanie i zamknięcie
- **Klawisz ESC** - konfigurowalne działanie (brak akcji/zapisz/anuluj)
- Automatyczne przywracanie ukrytych/zablokowanych obiektów


### 3.3 Zarządzanie Dokumentacją

#### F-06: Przypisywanie Plików

- Funkcja "Attach Documentation" umożliwiająca linkowanie zewnętrznych plików
- Obsługiwane formaty: PDF, DXF, STEP, obrazy (JPG, PNG)
- Przechowywanie ścieżek jako metadata w User Attributes definicji bloku
- Miniaturki podglądu załączonych plików [^11]


#### F-07: Kategoryzacja Dokumentów

- System tagowania dokumentów (instrukcje, szablony, modele 3D)
- Version control dla załączników
- Quick preview bez otwierania zewnętrznych aplikacji


### 3.4 Zaawansowane Funkcje

#### F-08: Batch Operations

- **Batch Rename** - wzorcowe przemianowywanie wielu elementów jednocześnie
- Grupowe operacje na właściwościach elementów
- Export list elementów do produkcji [^8]


#### F-09: Integracja z Material Palette

- **Material Compatibility** - określenie kompatybilnych materiałów dla elementu
- Walidacja czy element jest gotowy do obróbki CNC
- **Production Status** tracking (do obróbki, w realizacji, ukończone)


## 4. Wymagania Niefunkcjonalne

### 4.1 Wydajność

- Responsywność interfejsu TreeView przy obsłudze 500+ elementów [^10]
- Czas ładowania outlinera nie dłuższy niż 2 sekundy dla typowego projektu
- Płynne operacje drag \& drop bez opóźnień widocznych dla użytkownika [^9]


### 4.2 Użyteczność

- Zgodność z wzorcami UX znanych z systemów CAD [^12]
- Intuicyjny interfejs nie wymagający szkolenia dla użytkowników Rhino
- Zachowanie standardowych skrótów klawiszowych (F2 do edycji nazw) [^10]


### 4.3 Zgodność i Integracja

- Pełna kompatybilność z Rhino 8 na Windows 10/11 [^2][^3]
- Wykorzystanie Windows Forms jako framework UI [^13]
- Zachowanie spójności z rodzimym interfejsem Rhino [^12]


## 5. Specyfikacja Techniczna

### 5.1 Architektura UI

**Framework:** Windows Forms .NET [^13]
**Główna kontrolka:** Standard TreeView z custom node renderingiem [^6][^7]
**Panel typu:** Dockable w środowisku Rhino 8
**Wymiary:** 350-400px szerokości, wysokość dopasowana do okna Rhino

### 5.2 Struktura Danych

#### Element Data Schema

```json
{
  "id": "guid",
  "name": "string",
  "parentId": "guid",
  "blockDefinitionId": "guid", 
  "instanceCount": "number",
  "productionStatus": "enum",
  "materialCompatibility": ["materialIds"],
  "documentation": [
    {
      "filePath": "string",
      "category": "enum",
      "version": "string",
      "thumbnail": "base64"
    }
  ]
}
```


#### Metadata Storage

- **Object-to-element** assignments w User Attributes obiektów Rhino
- **Element documentation** w User Attributes definicji bloków
- **Element properties** w JSON format w Block Definition metadata


### 5.3 Kluczowe Komponenty

#### TreeViewManager

- Zarządzanie hierarchią elementów
- Synchronizacja z definicjami bloków Rhino
- Obsługa drag \& drop operations [^9]


#### InPlaceEditor

- Implementacja mechanizmów wejścia/wyjścia z trybu edycji
- Panel kontrolny trybu edycji
- Zarządzanie widocznością obiektów zewnętrznych


#### DocumentationManager

- Attachment/detachment plików do elementów
- Preview generation dla różnych formatów plików
- Version control system [^11]


## 6. Kryteria Akceptacji

### 6.1 Funkcjonalne

- [ ] TreeView wyświetla wszystkie elementy w projekcie w strukturze hierarchicznej
- [ ] Podwójne kliknięcie na element otwiera tryb edycji in-place
- [ ] Panel kontrolny trybu edycji oferuje wszystkie zdefiniowane operacje
- [ ] System przypisywania dokumentacji działa dla wszystkich obsługiwanych formatów
- [ ] Batch rename umożliwia przemianowanie wielu elementów według wzorca


### 6.2 Wydajnościowe

- [ ] Outliner ładuje się w mniej niż 2 sekundy dla projektów z 500+ elementami
- [ ] Drag \& drop działa płynnie bez zauważalnych opóźnień
- [ ] Tryb edycji in-place aktywuje się w mniej niż 1 sekundę


### 6.3 Użyteczności

- [ ] Interfejs jest intuicyjny dla użytkowników znających Rhino
- [ ] Wszystkie operacje można wykonać za pomocą kontekstowych menu
- [ ] Panel jest w pełni dockable i resizable w środowisku Rhino


## 7. Ograniczenia i Założenia

### 7.1 Ograniczenia Techniczne

- Windows Forms jako jedyna obsługiwana technologia UI [^13]
- Kompatybilność wyłącznie z Rhino 8 na Windows [^2][^3]
- Brak obsługi collaborative editing w czasie rzeczywistym


### 7.2 Założenia Biznesowe

- Użytkownicy posiadają podstawową wiedzę o zarządzaniu blokami w Rhino [^1]
- Projekty nie przekraczają 1000 elementów w hierarchii
- Załączniki dokumentacji przechowywane lokalnie lub w dostępnej sieci


## 8. Harmonogram i Milestones

### Faza 1: Core TreeView (4 tygodnie)

- Implementacja podstawowego TreeView interface
- Podstawowe operacje Select/Hide/Isolate
- Integracja z systemem bloków Rhino


### Faza 2: In-Place Editing (6 tygodni)

- Mechanizmy wejścia/wyjścia z trybu edycji
- Panel kontrolny z wszystkimi operacjami
- Obsługa zagnieżdżonych elementów


### Faza 3: Documentation System (3 tygodnie)

- System przypisywania plików
- Preview i thumbnails generation
- Kategoryzacja i version control


### Faza 4: Advanced Features (3 tygodnie)

- Batch operations
- Material integration
- Production status tracking


## 9. Metryki Sukcesu

### 9.1 Wskaźniki Adopcji

- 80% użytkowników Rhino Production Suite aktywnie używa Element Outliner w ciągu 3 miesięcy od wdrożenia
- Średnio 15+ operacji na element na projekt (edycja, przypisywanie dokumentacji)


### 9.2 Wskaźniki Wydajności

- Redukcja czasu zarządzania blokami o 40% w porównaniu do standardowych narzędzi Rhino
- Wzrost produktywności w przygotowaniu projektów do produkcji CNC o 25%


### 9.3 Wskaźniki Jakości

- Mniej niż 2 krytyczne błędy na 100 godzin użytkowania
- Średnia ocena użyteczności 4.5/5 w ankietach użytkowników

<div style="text-align: center">⁂</div>

[^1]: https://novedge.com/blogs/design-news/rhino-3d-tip-maximizing-efficiency-with-block-instances-in-rhino-3d-for-enhanced-project-organization

[^2]: https://www.rhino3d.com/8/system-requirements/

[^3]: https://rhino3d.online/pl/faqs/sistemnye-trebovaniya-rhino-7

[^4]: https://productschool.com/blog/product-strategy/product-template-requirements-document-prd

[^5]: https://www.aha.io/roadmapping/guide/requirements-management/what-is-a-good-product-requirements-document-template

[^6]: https://learn.microsoft.com/en-us/windows/apps/design/controls/tree-view

[^7]: https://learn.microsoft.com/th-th/dotnet/desktop/winforms/controls/treeview-control-overview-windows-forms?view=netframeworkdesktop-4.8

[^8]: https://files.solidworks.com/partners/pdfs/br_cad-interface_en1101.pdf

[^9]: https://docs.telerik.com/devtools/winforms/controls/treeview/treeview

[^10]: https://developer.mescius.com/componentone/winforms-ui-controls/treeview-control-winforms

[^11]: https://scribehow.com/library/technical-requirements-document

[^12]: https://lab.interface-design.co.uk/cad-software-ui-design-patterns-benchmarking-97cc7834ad02?gi=f2c1264a4fa0

[^13]: https://stackoverflow.com/questions/595469/ui-design-pattern-for-windows-forms-like-mvvm-for-wpf

[^14]: https://developer.rhino3d.com/api/RhinoScriptSyntax

[^15]: https://www.rhino3d.com/inside/revit/1.0/reference/release-notes

[^16]: https://developer.rhino3d.com/api/rhinocommon/rhino.geometry.linecurve

[^17]: https://www.rhino3d.com/inside/revit/1.0/discover/

[^18]: https://www.rhino3d.com/inside/revit/beta/reference/release-notes

[^19]: https://www.rhino3d.pl/wymagania-sprzetowe-rhino-3d

[^20]: https://www.reddit.com/r/rhino/comments/1bq2mi7/getting_into_rhino_for_jewelry_design/

[^21]: https://discourse.mcneel.com/t/outline-rendering-per-rhino/179929

[^22]: https://www.rhino3d.com/inside/revit/beta/discover/

[^23]: https://learn.microsoft.com/en-us/dotnet/desktop/wpf/controls/treeview-styles-and-templates

[^24]: https://www.bridging-the-gap.com/how-to-create-a-user-interface-specification/

[^25]: https://homepages.inf.ed.ac.uk/da/teach/HCI/rules.pdf

[^26]: https://www.notion.com/templates/category/product-requirements-doc

[^27]: https://www.atlassian.com/software/confluence/templates/product-requirements

[^28]: https://www.smartsheet.com/content/free-product-requirements-document-template

[^29]: https://github.com/ahnopologetic/prd

[^30]: https://www.template.net/edit-online/359409/software-development-product-requirements-document

[^31]: https://developer.rhino3d.com/api/RhinoCommon/html/T_Rhino_Geometry_PolyCurve.htm

[^32]: https://www.youtube.com/watch?v=EpBiR5ZRyZs

[^33]: https://www.epectec.com/user-interfaces/data-requirements.html

[^34]: https://techdocs.broadcom.com/us/en/ca-enterprise-software/it-operations-management/ca-2e/8-7/Standards/ibm-i-general-design-standards/design-standards-for-user-interfaces.html

[^35]: https://essentialdata.com/how-to-document-technical-requirements/

[^36]: https://creately.com/diagram/example/kgCfGtog7aa/prd-template

