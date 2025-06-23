using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using RhinoCncSuite.Models;

namespace RhinoCncSuite.ui
{
    /// <summary>
    /// Interaction logic for MaterialEditDialog.xaml
    /// </summary>
    public partial class MaterialEditDialog : Window
    {
        public Material EditedMaterial { get; private set; }

        public MaterialEditDialog(Material material)
        {
            InitializeComponent();
            
            // Clone the material to avoid modifying the original object until "Save" is clicked
            EditedMaterial = material.Clone();
            
            this.DataContext = EditedMaterial;
            
            // Initialize ComboBox
            TypeComboBox.ItemsSource = Enum.GetValues(typeof(MaterialType));
            TypeComboBox.SelectedItem = EditedMaterial.Type;

            UpdateColorPreview();
        }

        private void LoadMaterialData()
        {
            NameTextBox.Text = EditedMaterial.Name;
            TypeComboBox.SelectedItem = EditedMaterial.Type;
            ThicknessTextBox.Text = EditedMaterial.Thickness.ToString(CultureInfo.InvariantCulture);
            WidthTextBox.Text = EditedMaterial.Width.ToString(CultureInfo.InvariantCulture);
            LengthTextBox.Text = EditedMaterial.Length.ToString(CultureInfo.InvariantCulture);
            DensityTextBox.Text = EditedMaterial.Density.ToString(CultureInfo.InvariantCulture);
            PriceTextBox.Text = EditedMaterial.PricePerSquareMeter.ToString("F2", CultureInfo.InvariantCulture);
            ColorTextBox.Text = EditedMaterial.ColorHex ?? "#888888";
            
            UpdateColorPreview();
        }

        private void ColorTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            UpdateColorPreview();
        }

        private void UpdateColorPreview()
        {
            try
            {
                var colorText = ColorTextBox.Text;
                if (!colorText.StartsWith("#"))
                    colorText = "#" + colorText;

                if (colorText.Length == 7)
                {
                    var color = (Color)ColorConverter.ConvertFromString(colorText);
                    ColorPreview.Background = new SolidColorBrush(color);
                }
                else
                {
                    ColorPreview.Background = new SolidColorBrush(Colors.Gray);
                }
            }
            catch
            {
                ColorPreview.Background = new SolidColorBrush(Colors.Gray);
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validate and update the cloned material
                if (!ValidateInput())
                    return;

                EditedMaterial.Name = NameTextBox.Text.Trim();
                EditedMaterial.Type = (MaterialType)TypeComboBox.SelectedItem;
                EditedMaterial.Thickness = double.Parse(ThicknessTextBox.Text, CultureInfo.InvariantCulture);
                EditedMaterial.Width = double.Parse(WidthTextBox.Text, CultureInfo.InvariantCulture);
                EditedMaterial.Length = double.Parse(LengthTextBox.Text, CultureInfo.InvariantCulture);
                EditedMaterial.Density = double.Parse(DensityTextBox.Text, CultureInfo.InvariantCulture);
                EditedMaterial.PricePerSquareMeter = double.Parse(PriceTextBox.Text, CultureInfo.InvariantCulture);
                EditedMaterial.ColorHex = ColorTextBox.Text.StartsWith("#") ? ColorTextBox.Text : "#" + ColorTextBox.Text;

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving material: {ex.Message}", "Validation Error", 
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private bool ValidateInput()
        {
            // Validate name
            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                MessageBox.Show("Material name cannot be empty.", "Validation Error", 
                               MessageBoxButton.OK, MessageBoxImage.Warning);
                NameTextBox.Focus();
                return false;
            }

            // Validate type
            if (TypeComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please select a material type.", "Validation Error", 
                               MessageBoxButton.OK, MessageBoxImage.Warning);
                TypeComboBox.Focus();
                return false;
            }

            // Validate numeric fields
            if (!double.TryParse(ThicknessTextBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var thickness) || thickness <= 0)
            {
                MessageBox.Show("Thickness must be a positive number.", "Validation Error", 
                               MessageBoxButton.OK, MessageBoxImage.Warning);
                ThicknessTextBox.Focus();
                return false;
            }

            if (!double.TryParse(WidthTextBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var width) || width <= 0)
            {
                MessageBox.Show("Width must be a positive number.", "Validation Error", 
                               MessageBoxButton.OK, MessageBoxImage.Warning);
                WidthTextBox.Focus();
                return false;
            }

            if (!double.TryParse(LengthTextBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var height) || height <= 0)
            {
                MessageBox.Show("Height must be a positive number.", "Validation Error", 
                               MessageBoxButton.OK, MessageBoxImage.Warning);
                LengthTextBox.Focus();
                return false;
            }

            if (!double.TryParse(DensityTextBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var density) || density <= 0)
            {
                MessageBox.Show("Density must be a positive number.", "Validation Error", 
                               MessageBoxButton.OK, MessageBoxImage.Warning);
                DensityTextBox.Focus();
                return false;
            }

            if (!double.TryParse(PriceTextBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var price) || price < 0)
            {
                MessageBox.Show("Price must be a non-negative number.", "Validation Error", 
                               MessageBoxButton.OK, MessageBoxImage.Warning);
                PriceTextBox.Focus();
                return false;
            }

            // Validate color
            try
            {
                var colorText = ColorTextBox.Text;
                if (!colorText.StartsWith("#"))
                    colorText = "#" + colorText;

                if (colorText.Length != 7)
                    throw new FormatException();

                ColorConverter.ConvertFromString(colorText);
            }
            catch
            {
                MessageBox.Show("Color must be a valid hex color (e.g., #FF0000).", "Validation Error", 
                               MessageBoxButton.OK, MessageBoxImage.Warning);
                ColorTextBox.Focus();
                return false;
            }

            return true;
        }
    }
} 