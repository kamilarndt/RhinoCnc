using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using RhinoCncSuite.Commands;
using RhinoCncSuite.Models;
using RhinoCncSuite.Services;
using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;
using System.Linq;
using System.Drawing;
using System.Collections.Generic;
using RhinoMaterial = Rhino.DocObjects.Material;
using Material = RhinoCncSuite.Models.Material;

namespace RhinoCncSuite.ViewModels
{
    /// <summary>
    /// ViewModel dla Material Palette zgodnie z wzorcem MVVM
    /// ViewModel for Material Palette following MVVM pattern
    /// Oddziela zarządzanie danymi od interfejsu użytkownika
    /// Separates data management from UI concerns
    /// </summary>
    public class MaterialPaletteViewModel : BaseViewModel
    {
        #region Pola prywatne / Private Fields
        private readonly MaterialCatalogService _materialCatalogService;
        private ObservableCollection<Material> _materials;
        private ViewMode _currentViewMode = ViewMode.Tiles;
        private double _tileSize = 110;
        private string _statusText = "Ładowanie materiałów...";
        private bool _isLoading = true;
        private const double MinTileSize = 80;
        private const double MaxTileSize = 200;
        #endregion

        #region Właściwości publiczne / Public Properties
        /// <summary>
        /// Kolekcja materiałów do bindowania danych
        /// Collection of materials for data binding
        /// </summary>
        public ObservableCollection<Material> Materials
        {
            get => _materials;
            private set
            {
                if (SetProperty(ref _materials, value))
                {
                    UpdateStatusText();
                }
            }
        }

        /// <summary>
        /// Aktualny tryb widoku (Kafelki lub Lista)
        /// Current view mode (Tiles or List)
        /// </summary>
        public ViewMode CurrentViewMode
        {
            get => _currentViewMode;
            set
            {
                if (SetProperty(ref _currentViewMode, value))
                {
                    OnPropertyChanged(nameof(IsTileView));
                    OnPropertyChanged(nameof(IsListView));
                }
            }
        }

        /// <summary>
        /// Aktualny rozmiar kafelków dla widoku kafelków
        /// Current tile size for tiles view
        /// </summary>
        public double TileSize
        {
            get => _tileSize;
            set => SetProperty(ref _tileSize, Math.Max(MinTileSize, Math.Min(MaxTileSize, value)));
        }

        /// <summary>
        /// Tekst statusu dla interfejsu użytkownika
        /// Status text for UI display
        /// </summary>
        public string StatusText
        {
            get => _statusText;
            private set => SetProperty(ref _statusText, value);
        }

        /// <summary>
        /// Wskaźnik stanu ładowania
        /// Loading state indicator
        /// </summary>
        public bool IsLoading
        {
            get => _isLoading;
            private set => SetProperty(ref _isLoading, value);
        }

        /// <summary>
        /// Właściwość pomocnicza dla bindowania XAML
        /// Helper property for XAML binding
        /// </summary>
        public bool IsTileView => CurrentViewMode == ViewMode.Tiles;

        /// <summary>
        /// Właściwość pomocnicza dla bindowania XAML
        /// Helper property for XAML binding
        /// </summary>
        public bool IsListView => CurrentViewMode == ViewMode.List;
        #endregion

        #region Komendy / Commands
        public ICommand SwitchViewModeCommand { get; }
        public ICommand IncreaseTileSizeCommand { get; }
        public ICommand DecreaseTileSizeCommand { get; }
        public ICommand AddMaterialCommand { get; }
        public ICommand RefreshCommand { get; }

        // 5 głównych operacji Material Manager-a / 5 main Material Manager operations
        public ICommand EyeCommand { get; }       // Show/Hide objects
        public ICommand BrushCommand { get; }     // Assign material + apply color
        public ICommand TargetCommand { get; }    // Select objects with material
        public ICommand LockCommand { get; }      // Lock/unlock objects
        public ICommand PlusCommand { get; }      // Insert material geometry
        #endregion

        #region Konstruktor / Constructor
        public MaterialPaletteViewModel(MaterialCatalogService materialCatalogService)
        {
            _materialCatalogService = materialCatalogService ?? throw new ArgumentNullException(nameof(materialCatalogService));
            _materials = new ObservableCollection<Material>();

            // Inicjalizuj komendy / Initialize commands
            SwitchViewModeCommand = new RelayCommand(SwitchViewMode);
            IncreaseTileSizeCommand = new RelayCommand(IncreaseTileSize, () => TileSize < MaxTileSize);
            DecreaseTileSizeCommand = new RelayCommand(DecreaseTileSize, () => TileSize > MinTileSize);
            AddMaterialCommand = new RelayCommand(AddMaterial);
            RefreshCommand = new RelayCommand(async () => await LoadMaterialsAsync());

            // Inicjalizuj główne operacje / Initialize main operations
            EyeCommand = new RelayCommand<Material>(ExecuteEyeOperation);
            BrushCommand = new RelayCommand<Material>(ExecuteBrushOperation);
            TargetCommand = new RelayCommand<Material>(ExecuteTargetOperation);
            LockCommand = new RelayCommand<Material>(ExecuteLockOperation);
            PlusCommand = new RelayCommand<Material>(ExecutePlusOperation);

            // Subskrybuj zmiany w katalogu / Subscribe to catalog changes
            _materialCatalogService.CatalogChanged += OnCatalogChanged;
        }
        #endregion

        #region Metody publiczne / Public Methods
        /// <summary>
        /// Inicjalizuje ViewModel z ładowaniem danych
        /// Initialize the ViewModel with data loading
        /// </summary>
        public async Task InitializeAsync()
        {
            try
            {
                IsLoading = true;
                StatusText = "Ładowanie materiałów...";
                
                await LoadMaterialsAsync();
                
                RhinoApp.WriteLine($"RhinoCNC: MaterialPaletteViewModel zainicjalizowany z {Materials.Count} materiałami.");
            }
            catch (Exception ex)
            {
                StatusText = $"Błąd ładowania materiałów: {ex.Message}";
                RhinoApp.WriteLine($"RhinoCNC: Błąd inicjalizacji MaterialPaletteViewModel: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }
        #endregion

        #region Główne operacje Material Manager-a / Main Material Manager Operations

        /// <summary>
        /// Operacja Eye - Pokaż/Ukryj obiekty z danym materiałem
        /// Eye Operation - Show/Hide objects with given material
        /// </summary>
        private void ExecuteEyeOperation(Material material)
        {
            if (material == null) return;

            try
            {
                RhinoApp.InvokeOnUiThread(() =>
                {
                    var doc = RhinoDoc.ActiveDoc;
                    if (doc == null) return;

                    var objectsWithMaterial = GetObjectsWithMaterial(doc, material);
                    
                    if (objectsWithMaterial.Count == 0)
                    {
                        StatusText = $"Brak obiektów z materiałem {material.Name}";
                        return;
                    }

                    // Sprawdź czy obiekty są ukryte / Check if objects are hidden
                    bool anyVisible = objectsWithMaterial.Any(obj => obj.Attributes.Visible);
                    
                    // Przełącz widoczność / Toggle visibility
                    foreach (var rhinoObject in objectsWithMaterial)
                    {
                        if (anyVisible)
                            rhinoObject.Attributes.Visible = false; // Ukryj / Hide
                        else
                            rhinoObject.Attributes.Visible = true;  // Pokaż / Show
                        
                        rhinoObject.CommitChanges();
                    }

                    doc.Views.Redraw();
                    
                    string action = anyVisible ? "ukryto" : "pokazano";
                    StatusText = $"{action} {objectsWithMaterial.Count} obiektów z materiałem {material.Name}";
                    RhinoApp.WriteLine($"RhinoCNC: {action} {objectsWithMaterial.Count} obiektów z materiałem {material.Name}");
                });
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine($"RhinoCNC: Błąd operacji Eye: {ex.Message}");
                StatusText = $"Błąd operacji Eye: {ex.Message}";
            }
        }

        /// <summary>
        /// Operacja Brush - Przypisz materiał do zaznaczonych obiektów i ustaw kolor
        /// Brush Operation - Assign material to selected objects and set color
        /// </summary>
        private void ExecuteBrushOperation(Material material)
        {
            if (material == null) return;

            try
            {
                RhinoApp.InvokeOnUiThread(() =>
                {
                    var doc = RhinoDoc.ActiveDoc;
                    if (doc == null) return;

                    var selectedObjects = doc.Objects.GetSelectedObjects(false, false);
                    if (selectedObjects.Count() == 0)
                    {
                        StatusText = "Zaznacz obiekty do przypisania materiału";
                        return;
                    }

                    // Przypisz materiał i ustaw kolor / Assign material and set color
                    var materialColor = GetMaterialColor(material);
                    
                    foreach (var rhinoObject in selectedObjects)
                    {
                        // Zapisz materiał w User Attributes / Save material in User Attributes
                        rhinoObject.Attributes.SetUserString("Material_Id", material.Id.ToString());
                        rhinoObject.Attributes.SetUserString("Material_Name", material.Name);
                        rhinoObject.Attributes.SetUserString("Material_Type", material.Type.ToString());
                        rhinoObject.Attributes.SetUserString("Material_Thickness", material.Thickness.ToString());
                        
                        // Ustaw kolor obiektu / Set object color
                        rhinoObject.Attributes.ColorSource = ObjectColorSource.ColorFromObject;
                        rhinoObject.Attributes.ObjectColor = materialColor;
                        
                        rhinoObject.CommitChanges();
                    }

                    doc.Views.Redraw();
                    StatusText = $"Przypisano materiał {material.Name} do {selectedObjects.Count()} obiektów";
                    RhinoApp.WriteLine($"RhinoCNC: Przypisano materiał {material.Name} do {selectedObjects.Count()} obiektów");
                });
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine($"RhinoCNC: Błąd operacji Brush: {ex.Message}");
                StatusText = $"Błąd operacji Brush: {ex.Message}";
            }
        }

        /// <summary>
        /// Operacja Target - Zaznacz wszystkie obiekty z danym materiałem
        /// Target Operation - Select all objects with given material
        /// </summary>
        private void ExecuteTargetOperation(Material material)
        {
            if (material == null) return;

            try
            {
                RhinoApp.InvokeOnUiThread(() =>
                {
                    var doc = RhinoDoc.ActiveDoc;
                    if (doc == null) return;

                    // Odznacz wszystko / Deselect all
                    doc.Objects.UnselectAll();

                    var objectsWithMaterial = GetObjectsWithMaterial(doc, material);
                    
                    if (objectsWithMaterial.Count == 0)
                    {
                        StatusText = $"Brak obiektów z materiałem {material.Name}";
                        return;
                    }

                    // Zaznacz obiekty z materiałem / Select objects with material
                    foreach (var rhinoObject in objectsWithMaterial)
                    {
                        rhinoObject.Select(true);
                    }

                    doc.Views.Redraw();
                    StatusText = $"Zaznaczono {objectsWithMaterial.Count} obiektów z materiałem {material.Name}";
                    RhinoApp.WriteLine($"RhinoCNC: Zaznaczono {objectsWithMaterial.Count} obiektów z materiałem {material.Name}");
                });
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine($"RhinoCNC: Błąd operacji Target: {ex.Message}");
                StatusText = $"Błąd operacji Target: {ex.Message}";
            }
        }

        /// <summary>
        /// Operacja Lock - Zablokuj/odblokuj obiekty z danym materiałem
        /// Lock Operation - Lock/unlock objects with given material
        /// </summary>
        private void ExecuteLockOperation(Material material)
        {
            if (material == null) return;

            try
            {
                RhinoApp.InvokeOnUiThread(() =>
                {
                    var doc = RhinoDoc.ActiveDoc;
                    if (doc == null) return;

                    var objectsWithMaterial = GetObjectsWithMaterial(doc, material);
                    
                    if (objectsWithMaterial.Count == 0)
                    {
                        StatusText = $"Brak obiektów z materiałem {material.Name}";
                        return;
                    }

                    // Sprawdź czy jakieś obiekty są odblokowane / Check if any objects are unlocked
                    bool anyUnlocked = objectsWithMaterial.Any(obj => !obj.IsLocked);
                    
                    // Przełącz stan blokady / Toggle lock state
                    foreach (var rhinoObject in objectsWithMaterial)
                    {
                        if (anyUnlocked)
                        {
                            rhinoObject.Attributes.Mode = ObjectMode.Locked; // Zablokuj / Lock
                        }
                        else
                        {
                            rhinoObject.Attributes.Mode = ObjectMode.Normal; // Odblokuj / Unlock
                        }
                        
                        rhinoObject.CommitChanges();
                    }

                    doc.Views.Redraw();
                    
                    string action = anyUnlocked ? "zablokowano" : "odblokowano";
                    StatusText = $"{action} {objectsWithMaterial.Count} obiektów z materiałem {material.Name}";
                    RhinoApp.WriteLine($"RhinoCNC: {action} {objectsWithMaterial.Count} obiektów z materiałem {material.Name}");
                });
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine($"RhinoCNC: Błąd operacji Lock: {ex.Message}");
                StatusText = $"Błąd operacji Lock: {ex.Message}";
            }
        }

        /// <summary>
        /// Operacja Plus - Wstaw geometrię materiału (arkusz lub profil)
        /// Plus Operation - Insert material geometry (sheet or profile)
        /// </summary>
        private void ExecutePlusOperation(Material material)
        {
            if (material == null) return;

            try
            {
                RhinoApp.InvokeOnUiThread(() =>
                {
                    var doc = RhinoDoc.ActiveDoc;
                    if (doc == null) return;

                    GeometryBase geometry = null;
                    
                    switch (material.Type)
                    {
                        case MaterialType.Sheet:
                            // Twórz prostokąt reprezentujący arkusz / Create rectangle representing sheet
                            if (material.Width > 0 && material.Length > 0)
                            {
                                var plane = Plane.WorldXY;
                                var rect = new Rectangle3d(plane, material.Width, material.Length);
                                geometry = rect.ToPolyline().ToPolylineCurve();
                            }
                            break;
                            
                        case MaterialType.Length:
                        case MaterialType.SolidWood:
                            // Twórz linię reprezentującą profil o standardowej długości 3000mm
                            // Create line representing profile with standard length 3000mm
                            double profileLength = 3000; // Standardowa długość profilu / Standard profile length
                            geometry = new LineCurve(Point3d.Origin, new Point3d(profileLength, 0, 0));
                            break;
                            
                        default:
                            // Twórz punkt reprezentujący materiał / Create point representing material
                            geometry = new Rhino.Geometry.Point(Point3d.Origin);
                            break;
                    }

                    if (geometry != null)
                    {
                        // Dodaj geometrię do dokumentu / Add geometry to document
                        var objRef = doc.Objects.Add(geometry);
                        
                        if (objRef != Guid.Empty)
                        {
                            var rhinoObject = doc.Objects.FindId(objRef);
                            if (rhinoObject != null)
                            {
                                // Przypisz materiał do geometrii / Assign material to geometry
                                rhinoObject.Attributes.SetUserString("Material_Id", material.Id.ToString());
                                rhinoObject.Attributes.SetUserString("Material_Name", material.Name);
                                rhinoObject.Attributes.SetUserString("Material_Type", material.Type.ToString());
                                rhinoObject.Attributes.SetUserString("Material_Thickness", material.Thickness.ToString());
                                
                                // Ustaw kolor / Set color
                                var materialColor = GetMaterialColor(material);
                                rhinoObject.Attributes.ColorSource = ObjectColorSource.ColorFromObject;
                                rhinoObject.Attributes.ObjectColor = materialColor;
                                
                                rhinoObject.CommitChanges();
                            }
                        }

                        doc.Views.Redraw();
                        StatusText = $"Wstawiono geometrię materiału {material.Name}";
                        RhinoApp.WriteLine($"RhinoCNC: Wstawiono geometrię materiału {material.Name}");
                    }
                });
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine($"RhinoCNC: Błąd operacji Plus: {ex.Message}");
                StatusText = $"Błąd operacji Plus: {ex.Message}";
            }
        }

        #endregion

        #region Metody pomocnicze / Helper Methods

        /// <summary>
        /// Pobiera wszystkie obiekty z przypisanym materiałem
        /// Gets all objects with assigned material
        /// </summary>
        private List<RhinoObject> GetObjectsWithMaterial(RhinoDoc doc, Material material)
        {
            var result = new List<RhinoObject>();
            
            foreach (var rhinoObject in doc.Objects)
            {
                var materialId = rhinoObject.Attributes.GetUserString("Material_Id");
                if (!string.IsNullOrEmpty(materialId) && materialId == material.Id.ToString())
                {
                    result.Add(rhinoObject);
                }
            }
            
            return result;
        }

        /// <summary>
        /// Pobiera kolor materiału na podstawie jego kategorii
        /// Gets material color based on its category
        /// </summary>
        private Color GetMaterialColor(Material material)
        {
            // Kolory kategorii zgodnie ze specyfikacją / Category colors according to specification
            string materialName = material.Name?.ToUpper() ?? "";
            
            if (materialName.Contains("MDF"))
                return Color.Orange;     // MDF - pomarańczowy
            else if (materialName.Contains("SKLEJKA") || materialName.Contains("PLYWOOD"))
                return Color.Yellow;     // SKLEJKA - żółty
            else if (materialName.Contains("WTOROWA") || materialName.Contains("SECONDARY"))
                return Color.Red;        // WTOROWA - czerwony
            else
                return Color.LightBlue;  // INNE - niebieski
        }

        #endregion

        #region Metody prywatne / Private Methods
        private async Task LoadMaterialsAsync()
        {
            try
            {
                var catalogMaterials = await _materialCatalogService.GetMaterialsAsync();
                
                Materials.Clear();
                foreach (var material in catalogMaterials)
                {
                    Materials.Add(material);
                }

                UpdateStatusText();
                RhinoApp.WriteLine($"RhinoCNC: Załadowano {Materials.Count} materiałów do ViewModel.");
            }
            catch (Exception ex)
            {
                StatusText = $"Błąd ładowania materiałów: {ex.Message}";
                RhinoApp.WriteLine($"RhinoCNC: Błąd ładowania materiałów w ViewModel: {ex.Message}");
            }
        }

        private void UpdateStatusText()
        {
            if (Materials == null)
            {
                StatusText = "Brak materiałów";
                return;
            }

            var count = Materials.Count;
            StatusText = count switch
            {
                0 => "Brak materiałów",
                1 => "1 materiał",
                _ when count < 5 => $"{count} materiały",
                _ => $"{count} materiałów"
            };
        }

        private void OnCatalogChanged(object sender, MaterialCatalogChangedEventArgs e)
        {
            RhinoApp.InvokeOnUiThread(async () =>
            {
                await LoadMaterialsAsync();
            });
        }

        private void SwitchViewMode()
        {
            CurrentViewMode = CurrentViewMode == ViewMode.Tiles ? ViewMode.List : ViewMode.Tiles;
        }

        private void IncreaseTileSize()
        {
            TileSize = Math.Min(MaxTileSize, TileSize + 10);
        }

        private void DecreaseTileSize()
        {
            TileSize = Math.Max(MinTileSize, TileSize - 10);
        }

        private void AddMaterial()
        {
            try
            {
                // TODO: Implementuj dialog dodawania materiału
                // TODO: Implement material addition dialog
                RhinoApp.WriteLine("RhinoCNC: Dodawanie materiału - funkcja w przygotowaniu");
                StatusText = "Funkcja dodawania materiału w przygotowaniu";
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine($"RhinoCNC: Błąd dodawania materiału: {ex.Message}");
                StatusText = $"Błąd: {ex.Message}";
            }
        }
        #endregion

        #region Disposal / Zwolnienie zasobów
        protected override void OnDisposing()
        {
            if (_materialCatalogService != null)
            {
                _materialCatalogService.CatalogChanged -= OnCatalogChanged;
            }
            
            base.OnDisposing();
        }
        #endregion
    }

    /// <summary>
    /// Enumeration for view modes
    /// Wyliczenie dla trybów widoku
    /// </summary>
    public enum ViewMode
    {
        /// <summary>Widok kafelków / Tiles view</summary>
        Tiles,
        /// <summary>Widok listy / List view</summary>
        List
    }
} 