using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Rhino;
using Rhino.Geometry;
using Rhino.DocObjects;
using RhinoCncSuite.Models;
using RhinoCncSuite.Services;
using CncMaterial = RhinoCncSuite.Models.Material;

namespace RhinoCncSuite.ui
{
    /// <summary>
    /// Interaction logic for MaterialPaletteControl.xaml
    /// </summary>
    public partial class MaterialPaletteControl : UserControl, INotifyPropertyChanged
    {
        private MaterialCatalogService _materialCatalogService;
        public ObservableCollection<CncMaterial> ProjectMaterials { get; set; }

        private System.Windows.Point? _startPoint;

        // View mode and tile size properties
        public enum ViewMode { Tiles, List }
        private ViewMode _currentViewMode = ViewMode.Tiles;
        private double _tileSize = 110; // Default tile size
        private const double MinTileSize = 80;
        private const double MaxTileSize = 200;

        /// <summary>
        /// Constructor
        /// </summary>
        public MaterialPaletteControl()
        {
            try
            {
                InitializeComponent();
                ProjectMaterials = new ObservableCollection<CncMaterial>();
                MaterialItemsControl.ItemsSource = ProjectMaterials;
                MaterialsListView.ItemsSource = ProjectMaterials;
            
                this.DataContext = this;

                // Defer service loading until the control is loaded
                this.Loaded += UserControl_Loaded;
                
                // Set initial view mode state
                UpdateViewMode();
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine($"RhinoCNC: CRITICAL FAILURE in MaterialPaletteControl Constructor: {ex}");
            }
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // This event handler is now async void
            try
            {
                RhinoApp.WriteLine("RhinoCNC: MaterialPaletteControl loaded. Initializing data...");
                await InitializeDataAsync();
                RhinoApp.WriteLine("RhinoCNC: Data initialization complete.");

                // Defer the view mode update until after the layout pass
                Dispatcher.BeginInvoke(new Action(() => UpdateViewMode()), System.Windows.Threading.DispatcherPriority.Render);
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine($"RhinoCNC: CRITICAL FAILURE during UserControl_Loaded/InitializeDataAsync: {ex}");
            }
        }

        private async Task InitializeDataAsync()
        {
            try
            {
                // Check if plugin instance is available
                if (RhinoCncPlugin.Instance == null)
                {
                    RhinoApp.WriteLine("RhinoCNC: Plugin instance is null during MaterialPalette initialization");
                    return;
                }

                _materialCatalogService = await RhinoCncPlugin.Instance.GetMaterialCatalogAsync();
                
                if (_materialCatalogService != null)
                {
                    _materialCatalogService.CatalogChanged += OnCatalogChanged;
                    LoadProjectMaterials();
                }
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine($"RhinoCNC: Error in MaterialPalette InitializeDataAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Initializes the control and sets up data binding
        /// </summary>
        private void LoadProjectMaterials()
        {
            try
            {
                RhinoApp.WriteLine("RhinoCNC: Loading project materials...");
                ProjectMaterials.Clear();
                if (_materialCatalogService?.Materials != null && _materialCatalogService.Materials.Any())
                {
                    var random = new Random();
                    var defaultMaterials = _materialCatalogService.Materials
                        .Where(m => m.Type == MaterialType.Sheet)
                        .OrderBy(m => random.Next()) // Randomize
                        .Take(5); // Take 5 random materials
                    
                    foreach(var material in defaultMaterials)
                    {
                        ProjectMaterials.Add(material);
                    }
                }
                UpdateMaterialCount();
                RhinoApp.WriteLine($"RhinoCNC: Loaded {ProjectMaterials.Count} materials.");
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine($"RhinoCNC: Error loading project materials: {ex}");
            }
        }

        private void UpdateMaterialCount() { /* header removed */ }

        /// <summary>
        /// Handles catalog change events to refresh the material list
        /// </summary>
        private void OnCatalogChanged(object sender, MaterialCatalogChangedEventArgs e)
        {
            // Refresh materials on UI thread
            Dispatcher.BeginInvoke(new Action(() =>
            {
                LoadProjectMaterials();
            }));
        }

        #region Drag and Drop
        private void Tile_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _startPoint = e.GetPosition(null);
        }

        private void Tile_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (_startPoint == null || e.LeftButton != MouseButtonState.Pressed)
                return;

            var mousePos = e.GetPosition(null);
            var diff = _startPoint.Value - mousePos;

            if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
            {
                if ((sender as FrameworkElement)?.DataContext is CncMaterial material)
                {
                    DragDrop.DoDragDrop((DependencyObject)sender, material, DragDropEffects.Move);
                    _startPoint = null;
                }
            }
        }

        private void Tile_Drop(object sender, DragEventArgs e)
        {
            var droppedMaterial = e.Data.GetData(typeof(CncMaterial)) as CncMaterial;
            if (droppedMaterial != null && (sender as FrameworkElement)?.DataContext is CncMaterial targetMaterial && droppedMaterial != targetMaterial)
            {
                int oldIndex = ProjectMaterials.IndexOf(droppedMaterial);
                int newIndex = ProjectMaterials.IndexOf(targetMaterial);

                if (oldIndex != -1 && newIndex != -1)
                {
                    ProjectMaterials.Move(oldIndex, newIndex);
                }
            }
        }
        #endregion

        #region Tile Button Click Handlers
        private void AssignMaterial_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is CncMaterial material)
            {
                AssignMaterialToSelected(material);
            }
        }

        private void SelectObjects_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is CncMaterial material)
            {
                SelectObjectsWithMaterial(material);
            }
        }

        private void ToggleVisibility_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is CncMaterial material)
            {
                ToggleVisibilityForMaterial(material);
            }
        }

        private void SheetLayout_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is CncMaterial material)
            {
                CreateSheetLayoutForMaterial(material);
            }
        }

        private void ToggleLock_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is FrameworkElement fe && fe.DataContext is CncMaterial material)
                {
                    RhinoApp.WriteLine($"RhinoCNC: Lock toggle is not yet implemented for material '{material.Name}'.");
                }
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine($"RhinoCNC: Error in ToggleLock_Click: {ex.Message}");
            }
        }

        // New context menu handlers
        private async void EditMaterial_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is FrameworkElement fe && fe.DataContext is CncMaterial material)
                {
                    // Create and show material edit dialog
                    var editDialog = new MaterialEditDialog(material.Clone()); // Pass a clone to avoid modifying the original until saved
                    if (editDialog.ShowDialog() == true)
                    {
                        // Update the material in the catalog
                        if (_materialCatalogService != null)
                        {
                            await _materialCatalogService.AddOrUpdateMaterialAsync(editDialog.EditedMaterial);
                        }
                        
                        // The CatalogChanged event will trigger a reload of the palette
                        RhinoApp.WriteLine($"RhinoCNC: Material '{material.Name}' has been updated.");
                    }
                }
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine($"RhinoCNC: Error editing material: {ex.Message}");
            }
        }

        private void RemoveFromPalette_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is FrameworkElement fe && fe.DataContext is CncMaterial material)
                {
                    var result = MessageBox.Show(
                        $"Remove '{material.Name}' from palette?\n\nThis will only remove it from the current palette, not from the material catalog.",
                        "Remove Material",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        ProjectMaterials.Remove(material);
                        UpdateMaterialCount();
                        RhinoApp.WriteLine($"RhinoCNC: Removed '{material.Name}' from palette.");
                    }
                }
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine($"RhinoCNC: Error removing material from palette: {ex.Message}");
            }
        }
        #endregion

        #region Global Button Click Handlers
        private void AddMaterialButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_materialCatalogService == null)
                {
                    MessageBox.Show("Material catalog service is not available.", "Service Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Pass the service instance to the dialog
                var dialog = new MaterialSelectionDialog(_materialCatalogService, ProjectMaterials);
                if (dialog.ShowDialog() == true && dialog.SelectedMaterials.Any())
                {
                    foreach (var material in dialog.SelectedMaterials)
                    {
                        if (!ProjectMaterials.Any(p => p.Name == material.Name && p.Thickness == material.Thickness))
                        {
                            ProjectMaterials.Add(material);
                        }
                    }
                    UpdateMaterialCount();
                }
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine($"RhinoCNC: Error opening material selection dialog: {ex.Message}");
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GlobalSheetLayoutButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ProjectMaterials.Any())
                {
                    MessageBox.Show("Please add a material to the palette first.", "No Materials", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                
                var sheetMaterial = ProjectMaterials.FirstOrDefault(m => m.Type == MaterialType.Sheet && m.HasValidSheetDimensions());

                if (sheetMaterial == null)
                {
                    MessageBox.Show("No valid sheet materials available in the palette.", "No Sheet Material", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                CreateSheetLayoutForMaterial(sheetMaterial);
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine($"RhinoCNC: Error creating global sheet layout: {ex.Message}");
            }
        }

        // View control button handlers
        private void ViewModeButton_Click(object sender, RoutedEventArgs e)
        {
            SwitchViewMode();
        }

        private void IncreaseSizeButton_Click(object sender, RoutedEventArgs e)
        {
            IncreaseTileSize();
        }

        private void DecreaseSizeButton_Click(object sender, RoutedEventArgs e)
        {
            DecreaseTileSize();
        }
        #endregion

        #region Core Logic
        private void AssignMaterialToSelected(CncMaterial material)
        {
            try
            {
                var selectedObjects = RhinoDoc.ActiveDoc.Objects.GetSelectedObjects(false, false);
                int assignedCount = 0;
                
                // Ensure we have a valid color - use material color or fallback
                string colorString = material.Color;
                if (string.IsNullOrEmpty(colorString))
                {
                    colorString = GetFallbackColor(material.Name);
                }
                
                var drawingColor = System.Drawing.ColorTranslator.FromHtml(colorString);

                foreach (var obj in selectedObjects)
                {
                    obj.Attributes.SetUserString("cnc.material.id", material.Id);
                    obj.Attributes.SetUserString("cnc.material.name", material.Name);
                    obj.Attributes.ColorSource = Rhino.DocObjects.ObjectColorSource.ColorFromObject;
                    obj.Attributes.ObjectColor = drawingColor;
                    obj.CommitChanges();
                    assignedCount++;
                }

                if (assignedCount > 0)
                {
                    RhinoApp.WriteLine($"RhinoCNC: Assigned '{material.Name}' to {assignedCount} object(s) with color {colorString}.");
                    RhinoDoc.ActiveDoc.Views.Redraw();
                }
                else
                {
                    RhinoApp.WriteLine("RhinoCNC: No objects selected for material assignment.");
                }
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine($"RhinoCNC: Error assigning material: {ex.Message}");
            }
        }
        
        private string GetFallbackColor(string materialName)
        {
            var lowerCaseName = materialName.ToLower();

            if (lowerCaseName.Contains("mdf")) return "#A0522D";
            if (lowerCaseName.Contains("plywood") || lowerCaseName.Contains("sklejka")) return "#DEB887";
            if (lowerCaseName.Contains("osb")) return "#DAA520";
            if (lowerCaseName.Contains("plexi") || lowerCaseName.Contains("acrylic")) return "#F0F8FF";
            if (lowerCaseName.Contains("aluminum") || lowerCaseName.Contains("dibond")) return "#D3D3D3";
            if (lowerCaseName.Contains("pvc")) return "#FFFFFF";
            if (lowerCaseName.Contains("steel")) return "#808080";
            
            return "#8B4513"; // Default brown
        }

        private void SelectObjectsWithMaterial(CncMaterial material)
        {
            try
            {
                var doc = RhinoDoc.ActiveDoc;
                
                // Get ALL objects (both visible and hidden) that have this material assigned
                var allObjects = doc.Objects.GetObjectList(Rhino.DocObjects.ObjectType.AnyObject);
                var objectsWithMaterial = new List<Rhino.DocObjects.RhinoObject>();

                foreach (var obj in allObjects)
                {
                    var materialId = obj.Attributes.GetUserString("cnc.material.id");
                    if (materialId == material.Id)
                    {
                        objectsWithMaterial.Add(obj);
                    }
                }

                if (objectsWithMaterial.Any())
                {
                    // Count visible and hidden objects
                    var visibleObjects = objectsWithMaterial.Where(o => !o.IsHidden).ToList();
                    var hiddenObjects = objectsWithMaterial.Where(o => o.IsHidden).ToList();
                    
                    // Clear current selection
                    doc.Objects.UnselectAll();
                    
                    // Select only visible objects (can't select hidden objects)
                    int selectedCount = 0;
                    foreach (var obj in visibleObjects)
                    {
                        if (doc.Objects.Select(obj.Id))
                        {
                            selectedCount++;
                        }
                    }
                    
                    doc.Views.Redraw();
                    
                    if (hiddenObjects.Any())
                    {
                        RhinoApp.WriteLine($"RhinoCNC: Selected {selectedCount} visible object(s) with material '{material.Name}'. {hiddenObjects.Count} object(s) with this material are hidden.");
                    }
                    else
                    {
                        RhinoApp.WriteLine($"RhinoCNC: Selected {selectedCount} object(s) with material '{material.Name}'.");
                    }
                }
                else
                {
                    RhinoApp.WriteLine($"RhinoCNC: No objects found with material '{material.Name}'.");
                }
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine($"RhinoCNC: Error selecting objects: {ex.Message}");
            }
        }
        
        private void ToggleVisibilityForMaterial(CncMaterial material)
        {
            var doc = RhinoDoc.ActiveDoc;
            if (doc == null) return;

            try
            {
                // Build enumerator settings to include hidden and normal objects
                var settings = new Rhino.DocObjects.ObjectEnumeratorSettings
                {
                    ObjectTypeFilter = Rhino.DocObjects.ObjectType.AnyObject,
                    HiddenObjects = true,
                    NormalObjects = true,
                    LockedObjects = true
                };

                var objects = doc.Objects.GetObjectList(settings);
                var materialObjects = new List<Rhino.DocObjects.RhinoObject>();
                foreach (var obj in objects)
                {
                    if (obj == null) continue;
                    if (obj.Attributes.GetUserString("cnc.material.id") == material.Id)
                    {
                        materialObjects.Add(obj);
                    }
                }

                if (!materialObjects.Any())
                {
                    RhinoApp.WriteLine($"RhinoCNC: No objects found with material '{material.Name}'.");
                    return;
                }

                // Separate visible vs hidden
                var visible = materialObjects.Where(o => !o.IsHidden).ToList();
                var hidden = materialObjects.Where(o => o.IsHidden).ToList();

                if (visible.Any())
                {
                    int hiddenCount = 0;
                    foreach (var obj in visible)
                    {
                        if (doc.Objects.Hide(obj.Id, true)) hiddenCount++;
                    }
                    RhinoApp.WriteLine($"RhinoCNC: Hidden {hiddenCount} object(s) with material '{material.Name}'.");
                }
                else
                {
                    int shownCount = 0;
                    foreach (var obj in hidden)
                    {
                        if (doc.Objects.Show(obj.Id, true)) shownCount++;
                    }
                    RhinoApp.WriteLine($"RhinoCNC: Shown {shownCount} object(s) with material '{material.Name}'.");
                }

                doc.Views.Redraw();
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine($"RhinoCNC: Error toggling visibility for material '{material?.Name}': {ex.Message}");
            }
        }
        
        private void CreateSheetLayoutForMaterial(CncMaterial material)
        {
            if (material == null || material.Type != MaterialType.Sheet)
            {
                RhinoApp.WriteLine("RhinoCNC: Please select a valid sheet material to create a layout.");
                return;
            }

            try
            {
                var doc = RhinoDoc.ActiveDoc;
                if (doc == null) return;

                // Create a new layer for the sheet layout
                string layerName = $"Layout_{material.Name.Replace(" ", "_")}";
                int layerIndex = doc.Layers.Add(layerName, System.Drawing.Color.LightGray);
                if (layerIndex == -1)
                {
                    RhinoApp.WriteLine($"RhinoCNC: Failed to create layer '{layerName}'.");
                    return;
                }

                // Create the rectangle representing the sheet
                var rectangle = new Rectangle3d(Plane.WorldXY, material.Width, material.Length);
                var curve = rectangle.ToNurbsCurve();

                var attributes = new ObjectAttributes
                {
                    LayerIndex = layerIndex,
                    Name = $"Sheet_{material.Name}"
                };

                doc.Objects.AddCurve(curve, attributes);
                RhinoApp.WriteLine($"RhinoCNC: Created sheet layout for '{material.Name}' on layer '{layerName}'.");
                doc.Views.Redraw();
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine($"RhinoCNC: Error creating sheet layout: {ex.Message}");
            }
        }
        #endregion

        /// <summary>
        /// Handles the Unloaded event to clean up resources
        /// </summary>
        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            // Unsubscribe from catalog changes to prevent memory leaks
            if (_materialCatalogService != null)
            {
                _materialCatalogService.CatalogChanged -= OnCatalogChanged;
            }
            // Detach the loaded event handler as well
            this.Loaded -= UserControl_Loaded;
        }

        #region View Mode and Tile Size Management
        /// <summary>
        /// Toggles between Tiles and List view modes
        /// </summary>
        private void SwitchViewMode()
        {
            _currentViewMode = _currentViewMode == ViewMode.Tiles ? ViewMode.List : ViewMode.Tiles;
            UpdateViewMode();
        }

        /// <summary>
        /// Updates the visibility of the views based on the current mode
        /// </summary>
        private void UpdateViewMode()
        {
            if (_currentViewMode == ViewMode.Tiles)
            {
                MaterialItemsControl.Visibility = Visibility.Visible;
                MaterialsListView.Visibility = Visibility.Collapsed;
                ViewModeButton.Content = Resources["IconList"];
                ViewModeButton.ToolTip = "Switch to List View";
            }
            else
            {
                MaterialItemsControl.Visibility = Visibility.Collapsed;
                MaterialsListView.Visibility = Visibility.Visible;
                ViewModeButton.Content = Resources["IconTiles"];
                ViewModeButton.ToolTip = "Switch to Tiles View";
            }
        }

        /// <summary>
        /// Increases tile size
        /// </summary>
        private void IncreaseTileSize()
        {
            if (_tileSize < MaxTileSize)
            {
                var oldSize = _tileSize;
                _tileSize = Math.Min(_tileSize + 20, MaxTileSize);
                UpdateTileSize();
                RhinoApp.WriteLine($"RhinoCNC: Increased tile size from {oldSize} to {_tileSize}");
            }
            else
            {
                RhinoApp.WriteLine($"RhinoCNC: Already at maximum tile size ({MaxTileSize})");
            }
        }

        /// <summary>
        /// Decreases tile size
        /// </summary>
        private void DecreaseTileSize()
        {
            if (_tileSize > MinTileSize)
            {
                var oldSize = _tileSize;
                _tileSize = Math.Max(_tileSize - 20, MinTileSize);
                UpdateTileSize();
                RhinoApp.WriteLine($"RhinoCNC: Decreased tile size from {oldSize} to {_tileSize}");
            }
            else
            {
                RhinoApp.WriteLine($"RhinoCNC: Already at minimum tile size ({MinTileSize})");
            }
        }

        /// <summary>
        /// Updates tile size in the UI
        /// </summary>
        private void UpdateTileSize()
        {
            try
            {
                // This will be handled by binding in XAML
                OnPropertyChanged(nameof(TileSize));
                RhinoApp.WriteLine($"RhinoCNC: Updated tile size binding to {_tileSize}");
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine($"RhinoCNC: Error updating tile size: {ex.Message}");
            }
        }

        public double TileSize
        {
            get => _tileSize;
            set
            {
                _tileSize = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
} 