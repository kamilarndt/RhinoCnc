using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using RhinoCncSuite.Models;
using RhinoCncSuite.Services;
using System.Windows.Media;

namespace RhinoCncSuite.ui
{
    /// <summary>
    /// Interaction logic for MaterialSelectionDialog.xaml
    /// </summary>
    public class CategoryViewModel
    {
        public string Name { get; set; }
        public Brush Background { get; set; }
    }

    public partial class MaterialSelectionDialog : Window
    {
        private List<Material> _allMaterials;
        private readonly IEnumerable<Material> _projectMaterials;
        private readonly MaterialCatalogService _materialCatalogService;
        public List<Material> SelectedMaterials { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="materialCatalogService">The service for catalog operations.</param>
        /// <param name="projectMaterials">List of project materials</param>
        public MaterialSelectionDialog(MaterialCatalogService materialCatalogService, IEnumerable<Material> projectMaterials)
        {
            InitializeComponent();
            _materialCatalogService = materialCatalogService;
            _allMaterials = _materialCatalogService.GetCatalog();
            _projectMaterials = projectMaterials; // Store project materials
            SelectedMaterials = new List<Material>();

            PopulateCategories();
            // Select "ALL MATERIALS" by default
            if (CategoryListBox.Items.Count > 1)
            {
                CategoryListBox.SelectedIndex = 1;
            }
            else if (CategoryListBox.Items.Count > 0)
            {
                CategoryListBox.SelectedIndex = 0;
            }
        }

        private string GetBaseCategoryName(string displayName)
        {
            string upperName = displayName.ToUpper();
            if (upperName.StartsWith("MDF")) return "MDF";
            if (upperName.StartsWith("SKLEJKA")) return "SKLEJKA";
            if (upperName.StartsWith("PŁYTA WIOROWA")) return "PŁYTA WIOROWA";
            if (upperName.StartsWith("HDF")) return "HDF";
            if (upperName.StartsWith("PCV")) return "PCV";
            if (upperName.StartsWith("PLEKSI")) return "PLEKSI";
            if (upperName.StartsWith("DILITE")) return "DILITE";
            if (upperName.StartsWith("DIBOND")) return "DIBOND";
            if (upperName.StartsWith("CETRIS")) return "CETRIS";
            if (upperName.StartsWith("EASYDECHO")) return "EASYDECHO";
            if (upperName.StartsWith("SYNDERBOARD")) return "SYNDERBOARD";

            // Fallback for simple names
            var parts = displayName.Split(' ');
            return parts.Length > 0 ? parts[0] : displayName;
        }

        private void PopulateCategories()
        {
            var categoryViewModels = new List<CategoryViewModel>();
            var colorConverter = new BrushConverter();

            // Add special categories first
            categoryViewModels.Add(new CategoryViewModel { Name = "UŻYTE W PROJEKCIE", Background = (Brush)colorConverter.ConvertFromString("#009CCC") });
            categoryViewModels.Add(new CategoryViewModel { Name = "WSZYSTKIE MATERIAŁY", Background = (Brush)colorConverter.ConvertFromString("#4C4C4C") });

            var consolidatedCategories = _allMaterials
                .Select(m => GetBaseCategoryName(m.DisplayName))
                .Distinct()
                .OrderBy(name => name);

            foreach (var catName in consolidatedCategories)
            {
                var representativeMaterial = _allMaterials.First(m => GetBaseCategoryName(m.DisplayName) == catName);
                Brush background = Brushes.DarkGray;

                if (!string.IsNullOrEmpty(representativeMaterial.ColorHex))
                {
                    try { background = (Brush)colorConverter.ConvertFromString(representativeMaterial.ColorHex); } catch { }
                }
                categoryViewModels.Add(new CategoryViewModel { Name = catName, Background = background });
            }

            CategoryListBox.ItemsSource = categoryViewModels;
        }

        private void RefreshData()
        {
            // Remember selected category
            var selectedCategoryName = (CategoryListBox.SelectedItem as CategoryViewModel)?.Name;

            // Reload all materials from the service
            _allMaterials = _materialCatalogService.GetCatalog();
            
            // Repopulate categories
            PopulateCategories();

            // Try to restore selection
            if (!string.IsNullOrEmpty(selectedCategoryName))
            {
                var categoryToRestore = (CategoryListBox.ItemsSource as List<CategoryViewModel>)
                    .FirstOrDefault(c => c.Name == selectedCategoryName);
                
                if (categoryToRestore != null)
                {
                    CategoryListBox.SelectedItem = categoryToRestore;
                }
            }
            
            // If no selection, default to "ALL"
            if (CategoryListBox.SelectedItem == null)
            {
                CategoryListBox.SelectedIndex = (CategoryListBox.Items.Count > 1) ? 1 : 0;
            }

            // This will trigger the selection changed event to refresh the material list
        }

        private async void CreateNewMaterial_Click(object sender, RoutedEventArgs e)
        {
            // Open the edit dialog in "create" mode by passing a new, empty material
            var editDialog = new MaterialEditDialog(new Material 
            {
                Name = "New Material",
                ColorHex = "#CCCCCC",
                Width = 2800,
                Length = 2070,
                Thickness = 18,
                Type = MaterialType.Sheet
            });

            if (editDialog.ShowDialog() == true)
            {
                var newMaterial = editDialog.EditedMaterial;
                await _materialCatalogService.AddOrUpdateMaterialAsync(newMaterial);
                RefreshData();
            }
        }

        private void CategoryListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CategoryListBox.SelectedItem is CategoryViewModel selectedCategory)
            {
                List<Material> filteredMaterials;
                switch (selectedCategory.Name)
                {
                    case "WSZYSTKIE MATERIAŁY":
                        filteredMaterials = _allMaterials;
                        break;
                    case "UŻYTE W PROJEKCIE":
                        filteredMaterials = _projectMaterials.ToList();
                        break;
                    default:
                        filteredMaterials = _allMaterials
                            .Where(m => GetBaseCategoryName(m.DisplayName) == selectedCategory.Name)
                            .ToList();
                        break;
                }
                MaterialsListView.ItemsSource = filteredMaterials;
                UpdateMaterialCount(filteredMaterials.Count);
            }
        }

        private void PopulateMaterials(IEnumerable<Material> materials)
        {
            var materialList = materials.OrderBy(m => m.Name).ToList();
            MaterialsListView.ItemsSource = materialList;
            
            // Update material count
            var count = materialList.Count;
            MaterialCountText.Text = count == 1 ? "1 material" : $"{count} materials";
        }
        
        private void MaterialsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateSelectionCount();
        }

        private void UpdateSelectionCount()
        {
            var count = MaterialsListView.SelectedItems.Count;
            SelectionCountText.Text = count == 1 ? "1 selected" : $"{count} selected";
            SelectButton.IsEnabled = count > 0;
        }

        private void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedMaterials = MaterialsListView.SelectedItems.Cast<Material>().ToList();
            if (SelectedMaterials.Any())
            {
                DialogResult = true;
            }
            else
            {
                MessageBox.Show("Please select at least one material.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        /// <summary>
        /// Helper method to find a visual child of a specific type
        /// </summary>
        private static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
                if (child is T result)
                {
                    return result;
                }

                var childOfChild = FindVisualChild<T>(child);
                if (childOfChild != null)
                {
                    return childOfChild;
                }
            }
            return null;
        }

        private void UpdateMaterialCount(int count)
        {
            MaterialCountText.Text = count == 1 ? "1 material" : $"{count} materials";
        }
    }
} 