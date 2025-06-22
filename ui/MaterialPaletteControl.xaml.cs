using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Rhino;
using Rhino.Geometry;
using Rhino.DocObjects;
using RhinoCncSuite.Models;
using RhinoCncSuite.Services;
using CncMaterial = RhinoCncSuite.Models.Material;
using RhinoCncSuite.ui.Converters;
using System.Linq;

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

        // --- Reusable UI Resources ---
        private Style _iconButtonStyle;
        private Geometry _iconVisible, _iconSelect, _iconAssign, _iconLock, _iconLightBulb;
        private Geometry _iconTiles, _iconList; // Add these for view mode switching

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
            InitializeComponent();
            ProjectMaterials = new ObservableCollection<CncMaterial>();
            
            // Initialize resources for event handlers
            InitializeResources();
            
            // Set data context for binding
            this.DataContext = this;
            
            // Set initial view mode
            UpdateViewMode();
            
            // Wire up events
            this.Loaded += UserControl_Loaded;
            this.Unloaded += UserControl_Unloaded;
            
            // Enable drag and drop
            this.AllowDrop = true;
            
            RhinoApp.WriteLine("RhinoCNC: MaterialPaletteControl initialized.");
        }

        private void InitializeResources()
        {
            // --- Button Style ---
            _iconButtonStyle = new Style(typeof(Button));
            _iconButtonStyle.Setters.Add(new Setter(Control.BackgroundProperty, Brushes.Transparent));
            _iconButtonStyle.Setters.Add(new Setter(Control.BorderThicknessProperty, new Thickness(0)));
            _iconButtonStyle.Setters.Add(new Setter(Control.WidthProperty, 20.0));
            _iconButtonStyle.Setters.Add(new Setter(Control.HeightProperty, 20.0));
            _iconButtonStyle.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(2)));
            _iconButtonStyle.Setters.Add(new Setter(Control.VerticalAlignmentProperty, VerticalAlignment.Center));

            // --- Icon Geometries ---
            _iconVisible = Geometry.Parse("M12,9A3,3 0 0,0 9,12A3,3 0 0,0 12,15A3,3 0 0,0 15,12A3,3 0 0,0 12,9M12,17A5,5 0 0,1 7,12A5,5 0 0,1 12,7A5,5 0 0,1 17,12A5,5 0 0,1 12,17M12,4.5C7,4.5 2.73,7.61 1,12C2.73,16.39 7,19.5 12,19.5C17,19.5 21.27,16.39 23,12C21.27,7.61 17,4.5 12,4.5Z");
            _iconSelect = Geometry.Parse("M13.6,13.28L15.23,12L13.17,10.28L14.22,9.03L17.5,11.5L14.22,13.97L13.17,12.72M10,13.28L8.37,12L10.43,10.28L9.38,9.03L6.1,11.5L9.38,13.97L10.43,12.72M13,2A2,2 0 0,1 15,4V15A2,2 0 0,1 13,17H2V19H13A4,4 0 0,0 17,15V4A4,4 0 0,0 13,0H4V2H13Z");
            _iconAssign = Geometry.Parse("M12.7,13.7L13.3,16.4L15.3,15.8L14.7,13.1L12.7,13.7M11.6,9.9L14.2,7.2L16.3,7.8L13.7,10.5L11.6,9.9M7,12.1L9.6,9.5L11.7,10.1L9.1,12.8L7,12.1M17,2H21V6H17V2M17,8H21V12H17V8M17,14H21V18H17V14M15,20V22H5A2,2 0 0,1 3,20V4A2,2 0 0,1 5,2H15V4H5V20H15Z");
            _iconLock = Geometry.Parse("M12,17C10.89,17 10,16.1 10,15C10,13.89 10.89,13 12,13A2,2 0 0,1 14,15A2,2 0 0,1 12,17M18,8A2,2 0 0,1 20,10V20A2,2 0 0,1 18,22H6A2,2 0 0,1 4,20V10C4,8.89 4.9,8 6,8H7V6A5,5 0 0,1 12,1A5,5 0 0,1 17,6V8H18M12,3A3,3 0 0,0 9,6V8H15V6A3,3 0 0,0 12,3Z");
            _iconLightBulb = Geometry.Parse("M12,2A7,7 0 0,0 5,9C5,11.38 6.19,13.47 8,14.74V17A1,1 0 0,0 9,18H15A1,1 0 0,0 16,17V14.74C17.81,13.47 19,11.38 19,9A7,7 0 0,0 12,2M9,21A1,1 0 0,0 10,22H14A1,1 0 0,0 15,21V20H9V21Z");
            
            // --- View Mode Icons ---
            _iconTiles = Geometry.Parse("M4,4H8V8H4V4M10,4H14V8H10V4M16,4H20V8H16V4M4,10H8V14H4V10M10,10H14V14H10V10M16,10H20V14H16V10M4,16H8V20H4V16M10,16H14V20H10V16M16,16H20V20H16V16Z");
            _iconList = Geometry.Parse("M3,5H9V3H3V5M5,7V9H3V11H9V9H7V7H5M3,13V15H5V17H3V19H9V17H7V15H9V13H3M11,9H21V7H11V9M11,5H21V3H11V5M11,19H21V17H11V19M11,15H21V13H11V15Z");
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
                var plugin = RhinoCncPlugin.Instance;
                if (plugin == null)
                {
                    RhinoApp.WriteLine("RhinoCNC: Plugin instance is null during MaterialPalette initialization");
                    return;
                }

                await plugin.EnsureServicesInitializedAsync();
                
                _materialCatalogService = plugin.MaterialCatalog;
                
                if (_materialCatalogService != null)
                {
                    _materialCatalogService.CatalogChanged += OnCatalogChanged;
                    LoadProjectMaterials();
                }
                else
                {
                    RhinoApp.WriteLine("RhinoCNC: MaterialCatalogService is null after initialization.");
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

        private async void ToggleLock_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is FrameworkElement fe && fe.DataContext is CncMaterial material)
                {
                    material.IsLocked = !material.IsLocked;

                    // Persist the change to the catalog
                    if (_materialCatalogService != null)
                    {
                        await _materialCatalogService.AddOrUpdateMaterialAsync(material);
                        RhinoApp.WriteLine($"RhinoCNC: Material '{material.Name}' IsLocked set to {material.IsLocked} and saved.");
                    }
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

                // Try to create the MaterialSelectionDialog
                MaterialSelectionDialog dialog = null;
                try
                {
                    dialog = new MaterialSelectionDialog(_materialCatalogService, ProjectMaterials);
                }
                catch (Exception dialogEx)
                {
                    RhinoApp.WriteLine($"RhinoCNC: Error creating MaterialSelectionDialog: {dialogEx.Message}");
                    
                    // Fallback: Show available materials in a simple message
                    var catalogMaterials = _materialCatalogService.GetCatalog();
                    var availableMaterials = catalogMaterials
                        .Where(m => !ProjectMaterials.Any(p => p.Name == m.Name && p.Thickness == m.Thickness))
                        .Take(10) // Show first 10 available materials
                        .ToList();
                    
                    if (availableMaterials.Any())
                    {
                        var materialsList = string.Join("\n", availableMaterials.Select(m => $"- {m.Name} ({m.Thickness}mm)"));
                        var message = $"Material catalog dialog failed to load. Available materials:\n\n{materialsList}\n\nNote: You can still assign materials to objects by selecting them first, then using the assign button on existing palette materials.";
                        MessageBox.Show(message, "Material Catalog", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Material catalog dialog failed to load and no new materials are available to add.", "Material Catalog Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    return;
                }

                // If dialog was created successfully, show it
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
                RhinoApp.WriteLine($"RhinoCNC: Error in AddMaterialButton_Click: {ex.Message}");
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
            var doc = RhinoDoc.ActiveDoc;
            if (doc == null || material == null)
                return;

            var selectedObjects = doc.Objects.GetSelectedObjects(false, false);
            if (!selectedObjects.Any())
            {
                RhinoApp.WriteLine("RhinoCNC: No objects selected. Select objects first to assign a material.");
                return;
            }

            int changedCount = 0;
            foreach (var rhinoObject in selectedObjects)
            {
                rhinoObject.Attributes.SetUserString("RhinoCncMaterialId", material.Id.ToString());
                rhinoObject.CommitChanges();
                changedCount++;
            }

            if (changedCount > 0)
            {
                RhinoApp.WriteLine($"RhinoCNC: Assigned material '{material.Name}' to {changedCount} object(s).");
                doc.Views.Redraw();
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
            if (material == null) return;
            var doc = RhinoDoc.ActiveDoc;
            if (doc == null) return;

            var rhinoObjects = doc.Objects.FindByUserString("RhinoCncMaterialId", material.Id.ToString(), true);
            
            doc.Objects.UnselectAll();

            if (rhinoObjects != null && rhinoObjects.Any())
            {
                // Count visible and hidden objects
                var visibleObjects = rhinoObjects.Where(o => !o.IsHidden).ToList();
                var hiddenObjects = rhinoObjects.Where(o => o.IsHidden).ToList();
                
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
        
        private void ToggleVisibilityForMaterial(CncMaterial material)
        {
            if (material == null) return;

            var doc = RhinoDoc.ActiveDoc;
            if (doc == null) return;

            var rhinoObjects = doc.Objects.FindByUserString("RhinoCncMaterialId", material.Id.ToString(), true);

            if (rhinoObjects != null && rhinoObjects.Any())
            {
                // Separate visible vs hidden
                var visible = rhinoObjects.Where(o => !o.IsHidden).ToList();
                var hidden = rhinoObjects.Where(o => o.IsHidden).ToList();

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
            else
            {
                RhinoApp.WriteLine($"RhinoCNC: No objects found with material '{material.Name}'.");
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
                
                // Set content to list icon to indicate "switch to list view"
                ViewModeButton.Content = this.FindResource("IconList");
                ViewModeButton.ToolTip = "Switch to List View";
            }
            else
            {
                MaterialItemsControl.Visibility = Visibility.Collapsed;
                MaterialsListView.Visibility = Visibility.Visible;
                
                // Set content to tiles icon to indicate "switch to tiles view"
                ViewModeButton.Content = this.FindResource("IconTiles");
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