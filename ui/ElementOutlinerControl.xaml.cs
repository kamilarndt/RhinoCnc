using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using Rhino;
using RhinoCncSuite.Models;
using RhinoCncSuite.Services;
using Rhino.DocObjects;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace RhinoCncSuite.ui
{
    /// <summary>
    /// Interaction logic for ElementOutlinerControl.xaml
    /// </summary>
    public partial class ElementOutlinerControl : UserControl
    {
        private ElementOutlinerService _elementService;
        private MaterialCatalogService _materialCatalogService;
        private List<ElementInfo> _filteredElements;
        private ElementInfo _selectedElement;
        public ObservableCollection<ElementInfo> Elements { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public ElementOutlinerControl()
        {
            InitializeComponent();
            
            _filteredElements = new List<ElementInfo>();
            
            // Defer service loading until the control is loaded
            this.Loaded += UserControl_Loaded;
            
            // Set default filter
            TypeFilterComboBox.SelectedIndex = 0;
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await InitializeDataAsync();
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine($"RhinoCNC: Failed to initialize Element Outliner: {ex.Message}");
            }
        }

        private async Task InitializeDataAsync()
        {
            try
            {
                var plugin = RhinoCncPlugin.Instance;
                if (plugin == null)
                {
                    RhinoApp.WriteLine("RhinoCNC: Plugin instance is null during ElementOutliner initialization");
                    return;
                }

                await plugin.EnsureServicesInitializedAsync();

                _elementService = plugin.ElementOutliner;
                _materialCatalogService = plugin.MaterialCatalog;

                if (_elementService != null)
                {
                    _elementService.ElementsChanged += ElementService_ElementsChanged;
                    LoadElements();
                }
                else
                {
                    RhinoApp.WriteLine("RhinoCNC: ElementOutlinerService is null after initialization.");
                }
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine($"RhinoCNC: Error in ElementOutliner InitializeDataAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Loads and displays elements
        /// </summary>
        private void LoadElements()
        {
            try
            {
                if (_elementService?.Elements != null)
                {
                    _filteredElements = _elementService.Elements.ToList();
                    ApplyFilters();
                    RefreshElementTree();
                    UpdateElementCount();
                }
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine($"RhinoCNC: Error loading elements: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates the element count display
        /// </summary>
        private void UpdateElementCount()
        {
            if (ElementCountText != null)
            {
                var count = _filteredElements.Count;
                ElementCountText.Text = count == 1 ? "1 element" : $"{count} elements";
            }
        }

        /// <summary>
        /// Applies search and type filters to the elements list
        /// </summary>
        private void ApplyFilters()
        {
            var allElements = _elementService?.Elements?.ToList() ?? new List<ElementInfo>();
            
            // Apply search filter
            var searchText = SearchTextBox.Text?.Trim().ToLower();
            if (!string.IsNullOrEmpty(searchText))
            {
                allElements = allElements.Where(e => 
                    e.Name.ToLower().Contains(searchText) ||
                    (e.Description != null && e.Description.ToLower().Contains(searchText)) ||
                    e.Tags.Any(tag => tag.ToLower().Contains(searchText))
                ).ToList();
            }

            // Apply type filter
            var selectedType = TypeFilterComboBox.SelectedItem as ComboBoxItem;
            if (selectedType != null && selectedType.Content.ToString() != "All")
            {
                if (Enum.TryParse<ElementType>(selectedType.Content.ToString(), out var elementType))
                {
                    allElements = allElements.Where(e => e.Type == elementType).ToList();
                }
            }

            _filteredElements = allElements;
            UpdateElementCount();
        }

        /// <summary>
        /// Refreshes the element tree view
        /// </summary>
        private void RefreshElementTree()
        {
            ElementTreeView.Items.Clear();

            // Group elements by type for better organization
            var groupedElements = _filteredElements.GroupBy(e => e.Type).OrderBy(g => g.Key);

            foreach (var group in groupedElements)
            {
                var typeNode = new TreeViewItem
                {
                    Header = $"{group.Key} ({group.Count()})",
                    IsExpanded = true
                };

                foreach (var element in group.OrderBy(e => e.Name))
                {
                    var elementNode = new TreeViewItem
                    {
                        Header = CreateElementHeader(element),
                        Tag = element
                    };
                    typeNode.Items.Add(elementNode);
                }

                ElementTreeView.Items.Add(typeNode);
            }
        }

        /// <summary>
        /// Creates a formatted header for an element tree node
        /// </summary>
        private string CreateElementHeader(ElementInfo element)
        {
            var header = element.Name;
            
            if (element.Quantity > 1)
            {
                header += $" (×{element.Quantity})";
            }

            if (element.AttachedFiles.Any())
            {
                header += " 📎";
            }

            return header;
        }

        /// <summary>
        /// Updates the details panel with selected element information
        /// </summary>
        private void UpdateDetailsPanel(ElementInfo element)
        {
            if (element == null)
            {
                ElementInfoGrid.Visibility = Visibility.Collapsed;
                DetailsExpander.IsExpanded = false;
                return;
            }

            _selectedElement = element;

            // Update element info
            ElementNameText.Text = element.Name ?? "N/A";
            ElementTypeText.Text = element.Type.ToString();
            ElementStatusText.Text = element.Status.ToString();
            ElementNotesTextBox.Text = element.ManufacturingNotes ?? "";

            ElementInfoGrid.Visibility = Visibility.Visible;

            // Update attachments
            AttachedFilesListBox.ItemsSource = element.AttachedFiles;

            // Update button states
            EditElementButton.IsEnabled = true;
            DeleteElementButton.IsEnabled = true;

            DetailsExpander.IsExpanded = true;
        }

        /// <summary>
        /// Gets material name by ID
        /// </summary>
        private string GetMaterialName(Guid? materialId)
        {
            if (!materialId.HasValue || materialId.Value == Guid.Empty)
                return "N/A";

            var material = _materialCatalogService?.GetMaterialById(materialId.Value);
            return material?.Name ?? "Unknown Material";
        }

        /// <summary>
        /// Handles element service changes
        /// </summary>
        private void ElementService_ElementsChanged(object sender, ElementChangedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                LoadElements();
            });
        }

        /// <summary>
        /// Handles tree view selection changes
        /// </summary>
        private void ElementTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var selectedItem = e.NewValue as TreeViewItem;
            var element = selectedItem?.Tag as ElementInfo;
            UpdateDetailsPanel(element);
        }

        /// <summary>
        /// Handles search text changes
        /// </summary>
        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
            RefreshElementTree();
        }

        /// <summary>
        /// Handles type filter changes
        /// </summary>
        private void TypeFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
            RefreshElementTree();
        }

        /// <summary>
        /// Handles add element button click
        /// </summary>
        private async void AddElementButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new ElementEditDialog();
                if (dialog.ShowDialog() == true)
                {
                    var element = dialog.Element;
                    if (_elementService != null)
                    {
                        await _elementService.AddElementAsync(element);
                    }
                    RhinoApp.WriteLine($"RhinoCNC: Added element '{element.Name}'.");
                }
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine($"RhinoCNC: Error adding element: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles edit element button click
        /// </summary>
        private async void EditElementButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_selectedElement != null)
                {
                    var dialog = new ElementEditDialog(_selectedElement.Clone()); // Use a clone
                    if (dialog.ShowDialog() == true)
                    {
                        if (_elementService != null)
                        {
                            await _elementService.UpdateElementAsync(dialog.Element);
                        }
                        RhinoApp.WriteLine($"RhinoCNC: Updated element '{dialog.Element.Name}'.");
                    }
                }
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine($"RhinoCNC: Error editing element: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles delete element button click
        /// </summary>
        private async void DeleteElementButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_selectedElement != null)
                {
                    if (MessageBox.Show($"Are you sure you want to delete '{_selectedElement.Name}'?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                    {
                        var success = await _elementService.RemoveElementAsync(_selectedElement.Id);
                        if (success)
                        {
                            RhinoApp.WriteLine($"RhinoCNC: Element '{_selectedElement.Name}' deleted.");
                            UpdateDetailsPanel(null);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine($"RhinoCNC: Error deleting element: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles refresh button click - scans Rhino for blocks
        /// </summary>
        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var doc = RhinoDoc.ActiveDoc;
                if (doc == null || _elementService == null) return;

                int addedCount = 0;
                var blockTable = doc.InstanceDefinitions;
                var newElements = new List<ElementInfo>();

                foreach (var blockDef in blockTable)
                {
                    if (blockDef.IsDeleted) continue;

                    var existingElement = _elementService.GetElementByRhinoId(blockDef.Id.ToString());
                    if (existingElement == null)
                    {
                        var element = new ElementInfo(blockDef.Name, blockDef.Id.ToString(), ElementType.Block);
                        newElements.Add(element);
                    }
                    else
                    {
                        UpdateElementInfoProperties(existingElement, doc.Objects.FindId(Guid.Parse(existingElement.RhinoId)));
                        await _elementService.UpdateElementAsync(existingElement);
                    }
                }

                if (newElements.Any())
                {
                    await _elementService.AddElementsAsync(newElements);
                    addedCount = newElements.Count;
                }

                if (addedCount > 0)
                {
                    RhinoApp.WriteLine($"RhinoCNC: Added {addedCount} new element(s) from Rhino blocks.");
                }
                else
                {
                    RhinoApp.WriteLine("RhinoCNC: No new blocks found to add.");
                }
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine($"RhinoCNC: Error refreshing from blocks: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles attach file button click
        /// </summary>
        private async void AddFileButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedElement == null)
                return;

            var openFileDialog = new OpenFileDialog
            {
                Title = "Select file to attach",
                Filter = "All Files (*.*)|*.*|PDF Files (*.pdf)|*.pdf|DXF Files (*.dxf)|*.dxf|DWG Files (*.dwg)|*.dwg",
                Multiselect = true
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    foreach (var filename in openFileDialog.FileNames)
                    {
                        var fileInfo = new FileInfo(filename);
                        var attachedFile = new AttachedFile
                        {
                            Id = Guid.NewGuid(),
                            FileName = fileInfo.Name,
                            FilePath = filename,
                            FileSize = fileInfo.Length,
                            AttachedDate = DateTime.Now
                        };

                        _selectedElement.AttachedFiles.Add(attachedFile);
                    }

                    // Refresh the attachments display
                    AttachedFilesListBox.ItemsSource = null;
                    AttachedFilesListBox.ItemsSource = _selectedElement.AttachedFiles;

                    // Save changes
                    await _elementService?.SaveElementsAsync();

                    RhinoApp.WriteLine($"RhinoCNC: Attached {openFileDialog.FileNames.Length} file(s) to {_selectedElement.Name}");
                }
                catch (Exception ex)
                {
                    RhinoApp.WriteLine($"RhinoCNC: Error attaching files: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Handles save element changes button click
        /// </summary>
        private async void SaveElementButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedElement == null)
                return;

            try
            {
                // Update element with UI changes
                _selectedElement.ManufacturingNotes = ElementNotesTextBox.Text;

                // Save changes
                await _elementService?.SaveElementsAsync();

                RhinoApp.WriteLine($"RhinoCNC: Saved changes to {_selectedElement.Name}");
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine($"RhinoCNC: Error saving element changes: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles revert element changes button click
        /// </summary>
        private void RevertElementButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedElement == null)
                return;

            try
            {
                // Reload element data from service
                var originalElement = _elementService?.Elements.FirstOrDefault(e => e.Id == _selectedElement.Id);
                if (originalElement != null)
                {
                    UpdateDetailsPanel(originalElement);
                    RhinoApp.WriteLine($"RhinoCNC: Reverted changes to {_selectedElement.Name}");
                }
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine($"RhinoCNC: Error reverting element changes: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles open file button click
        /// </summary>
        private void OpenFileButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedFile = AttachedFilesListBox.SelectedItem as AttachedFile;
                if (selectedFile == null)
                {
                    RhinoApp.WriteLine("RhinoCNC: Please select a file to open.");
                    return;
                }

                if (File.Exists(selectedFile.FilePath))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = selectedFile.FilePath,
                        UseShellExecute = true
                    });
                }
                else
                {
                    RhinoApp.WriteLine($"RhinoCNC: File not found: {selectedFile.FilePath}");
                }
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine($"RhinoCNC: Error opening file: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles remove file button click
        /// </summary>
        private async void RemoveFileButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedFile = AttachedFilesListBox.SelectedItem as AttachedFile;
                if (selectedFile == null)
                {
                    RhinoApp.WriteLine("RhinoCNC: Please select a file to remove.");
                    return;
                }

                if (_selectedElement != null)
                {
                    var result = MessageBox.Show(
                        $"Are you sure you want to remove the file '{selectedFile.FileName}' from this element?",
                        "Confirm Removal",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        _selectedElement.AttachedFiles.Remove(selectedFile);

                        // Refresh the attachments display
                        AttachedFilesListBox.ItemsSource = null;
                        AttachedFilesListBox.ItemsSource = _selectedElement.AttachedFiles;

                        // Save changes
                        await _elementService?.SaveElementsAsync();

                        RhinoApp.WriteLine($"RhinoCNC: Removed file '{selectedFile.FileName}' from {_selectedElement.Name}");
                    }
                }
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine($"RhinoCNC: Error removing file: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles the Unloaded event to clean up resources
        /// </summary>
        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_elementService != null)
            {
                _elementService.ElementsChanged -= ElementService_ElementsChanged;
            }
            this.Loaded -= UserControl_Loaded;
        }

        private void UpdateElementInfoProperties(ElementInfo elementInfo, RhinoObject rhinoObject)
        {
            var materialIdStr = rhinoObject.Attributes.GetUserString("RhinoCncMaterialId");
            if (Guid.TryParse(materialIdStr, out Guid materialId))
            {
                elementInfo.MaterialId = materialId;
            }
        }

        private void SelectInRhinoButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedElement?.RhinoId != null && Guid.TryParse(_selectedElement.RhinoId, out Guid rhinoGuid))
            {
                var rhinoObject = RhinoDoc.ActiveDoc.Objects.FindId(rhinoGuid);
                if (rhinoObject != null)
                {
                    rhinoObject.Select(true, true);
                    RhinoDoc.ActiveDoc.Views.Redraw();
                }
                else
                {
                    MessageBox.Show("Could not find the corresponding object in the Rhino document.", "Object Not Found", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }
    }

    /// <summary>
    /// Simple dialog for editing element properties
    /// </summary>
    public partial class ElementEditDialog : Window
    {
        public ElementInfo Element { get; private set; }

        public ElementEditDialog(ElementInfo element = null)
        {
            Element = element ?? new ElementInfo();
            InitializeComponent();
            DataContext = Element;
        }

        private void InitializeComponent()
        {
            Title = Element.Name != null ? "Edit Element" : "Add Element";
            Width = 400;
            Height = 300;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(45, 45, 45));

            var grid = new Grid { Margin = new Thickness(16) };
            
            var stackPanel = new StackPanel();
            
            // Name field
            stackPanel.Children.Add(new TextBlock { Text = "Name:", Foreground = System.Windows.Media.Brushes.White, Margin = new Thickness(0, 0, 0, 4) });
            var nameTextBox = new TextBox 
            { 
                Text = Element.Name,
                Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(64, 64, 64)),
                Foreground = System.Windows.Media.Brushes.White,
                BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(85, 85, 85)),
                Margin = new Thickness(0, 0, 0, 8)
            };
            stackPanel.Children.Add(nameTextBox);

            // Buttons
            var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 16, 0, 0) };
            
            var okButton = new Button 
            { 
                Content = "OK", 
                Margin = new Thickness(0, 0, 8, 0), 
                Padding = new Thickness(16, 8, 16, 8),
                Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(51, 153, 255)),
                Foreground = System.Windows.Media.Brushes.White,
                BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(34, 136, 238))
            };
            okButton.Click += (s, e) => { Element.Name = nameTextBox.Text; DialogResult = true; };
            
            var cancelButton = new Button 
            { 
                Content = "Cancel", 
                Padding = new Thickness(16, 8, 16, 8),
                Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(64, 64, 64)),
                Foreground = System.Windows.Media.Brushes.White,
                BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(85, 85, 85))
            };
            cancelButton.Click += (s, e) => { DialogResult = false; };

            buttonPanel.Children.Add(okButton);
            buttonPanel.Children.Add(cancelButton);
            stackPanel.Children.Add(buttonPanel);

            grid.Children.Add(stackPanel);
            Content = grid;
        }
    }
} 