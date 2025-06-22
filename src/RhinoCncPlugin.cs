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
        private Task<MaterialCatalogService> _materialCatalogServiceTask;
        private Task<ElementOutlinerService> _elementOutlinerServiceTask;
        private readonly object _materialLock = new object();
        private readonly object _elementLock = new object();

        /// <summary>
        /// Gets the singleton instance of the plugin
        /// </summary>
        public static RhinoCncPlugin Instance { get; private set; }

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

        public Task<MaterialCatalogService> GetMaterialCatalogAsync()
        {
            lock (_materialLock)
            {
                if (_materialCatalogServiceTask == null)
                {
                    var catalogFilePath = Path.Combine(GetDataDirectory(), "materials.json");
                    _materialCatalogServiceTask = MaterialCatalogService.CreateAsync(catalogFilePath);
                }
            }
            return _materialCatalogServiceTask;
        }

        public Task<ElementOutlinerService> GetElementOutlinerAsync()
        {
            lock (_elementLock)
            {
                if (_elementOutlinerServiceTask == null)
                {
                    // This creates a dependency: ElementOutliner needs the MaterialCatalog.
                    // We chain the tasks to ensure correct initialization order.
                    _elementOutlinerServiceTask = InitializeElementOutlinerService();
                }
            }
            return _elementOutlinerServiceTask;
        }

        private async Task<ElementOutlinerService> InitializeElementOutlinerService()
        {
            var materialCatalog = await GetMaterialCatalogAsync();
            var elementsFilePath = Path.Combine(GetDataDirectory(), "elements.json");
            var elementOutlinerService = new ElementOutlinerService(elementsFilePath, materialCatalog);
            await elementOutlinerService.InitializeAsync();
            return elementOutlinerService;
        }

        /// <summary>
        /// Called when the plugin is loaded
        /// </summary>
        protected override LoadReturnCode OnLoad(ref string errorMessage)
        {
            try
            {
                RhinoApp.WriteLine("RhinoCNC Production Suite: Plugin loading...");
                RhinoApp.WriteLine($"RhinoCNC: Plugin ID = {this.Id}");
                
                // Check if plugin ID is valid before registering panels
                if (this.Id == Guid.Empty)
                {
                    errorMessage = "Plugin GUID is Guid.Empty - assembly configuration issue";
                    RhinoApp.WriteLine("RhinoCNC: ERROR - Plugin ID is Guid.Empty!");
                    return LoadReturnCode.ErrorShowDialog;
                }
                
                // Register panels immediately - the GUID should be available now
                try
                {
                    RhinoApp.WriteLine("RhinoCNC: Registering Element Outliner panel...");
                    Rhino.UI.Panels.RegisterPanel(this, typeof(ElementOutlinerPanel), "Element Outliner", null);
                    RhinoApp.WriteLine("RhinoCNC: Element Outliner panel registered successfully");
                }
                catch (Exception panelEx)
                {
                    RhinoApp.WriteLine($"RhinoCNC: Error registering Element Outliner panel: {panelEx.Message}");
                    throw;
                }
                
                try
                {
                    RhinoApp.WriteLine("RhinoCNC: Registering Material Palette panel...");
                    Rhino.UI.Panels.RegisterPanel(this, typeof(MaterialPalettePanel), "Material Palette", null);
                    RhinoApp.WriteLine("RhinoCNC: Material Palette panel registered successfully");
                }
                catch (Exception panelEx)
                {
                    RhinoApp.WriteLine($"RhinoCNC: Error registering Material Palette panel: {panelEx.Message}");
                    throw;
                }

                RhinoApp.WriteLine("RhinoCNC Production Suite: Plugin loaded successfully!");
                return LoadReturnCode.Success;
            }
            catch (System.Exception ex)
            {
                errorMessage = $"Failed to load RhinoCNC plugin: {ex.Message}";
                RhinoApp.WriteLine($"RhinoCNC: Error during plugin load: {ex.Message}");
                RhinoApp.WriteLine($"RhinoCNC: Stack trace: {ex.StackTrace}");
                return LoadReturnCode.ErrorShowDialog;
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
            base.OnShutdown();
        }
    }
} 