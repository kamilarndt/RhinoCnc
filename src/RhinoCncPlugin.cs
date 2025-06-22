using Rhino;
using Rhino.PlugIns;
using RhinoCncSuite.Services;
using RhinoCncSuite.ui;
using RhinoCncSuite.Commands;
using System.IO;

namespace RhinoCncSuite
{
    /// <summary>
    /// Main plugin class for the RhinoCNC Production Suite
    /// </summary>
    public class RhinoCncPlugin : PlugIn
    {
        /// <summary>
        /// Gets the singleton instance of the plugin
        /// </summary>
        public static RhinoCncPlugin Instance { get; private set; }

        public MaterialCatalogService MaterialCatalog { get; private set; }
        public ElementOutlinerService ElementOutliner { get; private set; }

        private Task _initializationTask;

        /// <summary>
        /// Constructor
        /// </summary>
        public RhinoCncPlugin()
        {
            if (Instance == null) Instance = this;
        }

        /// <summary>
        /// Called by Rhino to create commands
        /// </summary>
        protected override void CreateCommands()
        {
            try
            {
                RhinoApp.WriteLine("RhinoCNC: Creating commands...");
                // Let Rhino automatically create commands from this assembly
                base.CreateCommands();
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine($"RhinoCNC: Error creating commands: {ex.Message}");
            }
        }

        /// <summary>
        /// Called when the plugin is loaded
        /// </summary>
        protected override LoadReturnCode OnLoad(ref string errorMessage)
        {
            try
            {
                Instance = this;
                RhinoApp.WriteLine("RhinoCNC Production Suite: Plugin loading...");

                // Defer heavy initialization to avoid blocking Rhino's startup process
                _initializationTask = InitializeServicesAsync();
                
                // Register panels immediately
                Rhino.UI.Panels.RegisterPanel(this, typeof(ElementOutlinerPanelHost), "Element Outliner", null);
                Rhino.UI.Panels.RegisterPanel(this, typeof(MaterialPalettePanelHost), "Material Palette", null);

                RhinoApp.WriteLine("RhinoCNC Production Suite: Plugin loaded successfully!");
                return LoadReturnCode.Success;
            }
            catch (System.Exception e)
            {
                errorMessage = e.Message;
                RhinoApp.WriteLine($"RhinoCNC: CRITICAL LOAD FAILURE: {e.Message}");
                return LoadReturnCode.ErrorShowDialog;
            }
        }

        private async Task InitializeServicesAsync()
        {
            try
            {
                var dataDir = GetDataDirectory();
                var materialCatalogPath = Path.Combine(dataDir, "materials.json");
                var elementsPath = Path.Combine(dataDir, "elements.json");

                MaterialCatalog = await MaterialCatalogService.CreateAsync(materialCatalogPath);
                
                ElementOutliner = new ElementOutlinerService(elementsPath, MaterialCatalog);
                await ElementOutliner.InitializeAsync();
                
                RhinoApp.WriteLine("RhinoCNC: All services initialized successfully.");
            }
            catch (Exception e)
            {
                RhinoApp.WriteLine($"RhinoCNC: CRITICAL SERVICE INIT FAILURE: {e.Message}");
            }
        }

        public async Task EnsureServicesInitializedAsync()
        {
            if (_initializationTask != null)
            {
                await _initializationTask;
            }
        }

        private string GetDataDirectory()
        {
            var pluginDataLocation = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "RhinoCncSuite"
            );
            if (!Directory.Exists(pluginDataLocation))
            {
                Directory.CreateDirectory(pluginDataLocation);
            }
            return pluginDataLocation;
        }

        /// <summary>
        /// Called when the plugin is unloaded
        /// </summary>
        protected override void OnShutdown()
        {
            RhinoApp.WriteLine("RhinoCNC Production Suite unloaded.");
            MaterialCatalog?.SaveMaterialsAsync().Wait();
            ElementOutliner?.SaveElementsAsync().Wait();
            Instance = null;
        }
    }
} 