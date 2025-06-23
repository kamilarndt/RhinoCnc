using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Drawing;
using Rhino;
using RhinoCncSuite.Services;
using System.Windows;
using System.IO;
using System.Reflection;
using RhinoCncSuite.ViewModels;
using System.Resources;

namespace RhinoCncSuite.ui
{
    /// <summary>
    /// Material Palette Panel - compliant with Rhino's instantiation requirements.
    /// </summary>
    [Guid("7A435887-2342-4F53-A338-92715A0E880B")]
    public class MaterialPalettePanel : UserControl
    {
        private MaterialPaletteControl _wpfControl;
        private MaterialPaletteViewModel _viewModel;

        /// <summary>
        /// Parameterless constructor required by Rhino's panel registration system.
        /// </summary>
        public MaterialPalettePanel()
        {
            Initialize();
        }

        private async void Initialize()
        {
            try
            {
                // --- Dependency Resolution using Service Locator (Plugin Instance) ---
                var materialCatalogService = RhinoCncPlugin.Instance?.MaterialCatalog;
                if (materialCatalogService == null)
                {
                    throw new InvalidOperationException("MaterialCatalogService is not available.");
                }

                // The panel now creates its own ViewModel
                _viewModel = new MaterialPaletteViewModel(materialCatalogService);

                // Load theme resources (DataTemplates for tiles/list)
                var themeResources = LoadThemeResources();

                // Create the programmatic WPF control
                _wpfControl = new MaterialPaletteControl(_viewModel, themeResources);

                // Host the WPF control
                var elementHost = new ElementHost
                {
                    Dock = DockStyle.Fill,
                    Child = _wpfControl
                };

                Controls.Add(elementHost);

                // Asynchronously load the material data
                await _viewModel.InitializeAsync();
            }
            catch (Exception ex)
            {
                DisplayError(ex);
            }
        }

        private ResourceDictionary LoadThemeResources()
        {
            try
            {
                var assembly = GetType().Assembly;
                var resourceName = "RhinoCncSuite.Themes.Materials.xaml"; 
                using (var stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null) throw new MissingManifestResourceException($"Cannot find resource: {resourceName}");
                    
                    var reader = new System.Windows.Markup.XamlReader();
                    return (ResourceDictionary)reader.LoadAsync(stream);
                }
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine($"RhinoCNC: Failed to load theme resources. {ex.Message}");
                return null;
            }
        }

        private void DisplayError(Exception ex)
        {
            Controls.Clear();
            var errorLabel = new Label
            {
                Text = $"Material Palette Error: {ex.Message}\n\nPlease check the Rhino command line for details.",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.Red
            };
            Controls.Add(errorLabel);
            RhinoApp.WriteLine($"RhinoCNC: Error initializing Material Palette panel: {ex.Message}");
            }

        /// <summary>
        /// Panel ID for registration
        /// </summary>
        public static Guid PanelId => typeof(MaterialPalettePanel).GUID;

        /// <summary>
        /// Clean up resources when panel is disposed
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _viewModel?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
} 