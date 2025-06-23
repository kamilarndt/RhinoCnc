using Rhino;
using Rhino.PlugIns;
using Rhino.UI;
using RhinoCncSuite.Services;
using RhinoCncSuite.ui;
using RhinoCncSuite.Commands;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System;

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

                // Initialize services asynchronously and register panels after completion
                _initializationTask = InitializeServicesAndPanelsAsync();

                RhinoApp.WriteLine("RhinoCNC Production Suite: Plugin loaded successfully! Services initializing in background...");
                return LoadReturnCode.Success;
            }
            catch (System.Exception e)
            {
                errorMessage = e.Message;
                RhinoApp.WriteLine($"RhinoCNC: CRITICAL LOAD FAILURE: {e.Message}");
                return LoadReturnCode.ErrorShowDialog;
            }
        }

        /// <summary>
        /// Initializes services and registers panels asynchronously
        /// </summary>
        private async Task InitializeServicesAndPanelsAsync()
        {
            try
            {
                await InitializeServicesAsync();
                
                // Register panels only after services are ready
                await InitializePanelsAsync();
                
                RhinoApp.WriteLine("RhinoCNC: All services and panels initialized successfully.");
            }
            catch (Exception ex)
            {
                // Log the error
                RhinoApp.WriteLine($"RhinoCNC: Critical initialization error: {ex.Message}");
                RhinoApp.WriteLine($"RhinoCNC: Stack trace: {ex.StackTrace}");
                
                // Show error dialog to user for immediate visibility
                var errorMessage = $"RhinoCNC Plugin Initialization Failed:\n\n{ex.Message}\n\nPlease check the Rhino command line for details.";
                
                // Use Rhino's UI thread to show the error dialog
                RhinoApp.InvokeOnUiThread(new Action(() =>
                {
                    try
                    {
                        System.Windows.Forms.MessageBox.Show(
                            errorMessage,
                            "RhinoCNC Plugin Error",
                            System.Windows.Forms.MessageBoxButtons.OK,
                            System.Windows.Forms.MessageBoxIcon.Error);
                    }
                    catch
                    {
                        // If even the error dialog fails, at least we logged it
                        RhinoApp.WriteLine("RhinoCNC: Failed to show error dialog to user.");
                    }
                }));
                
                // Don't rethrow - let the plugin continue in degraded mode
                // The panels will show error messages if services aren't available
            }
        }

        private async Task InitializeServicesAsync()
        {
            try
            {
                var dataDir = GetDataDirectory();
                var materialCatalogPath = Path.Combine(dataDir, "materials.json");
                var elementsPath = Path.Combine(dataDir, "elements.json");

                RhinoApp.WriteLine("RhinoCNC: Starting parallel service initialization...");

                // Initialize services in parallel for better performance
                var materialTask = MaterialCatalogService.CreateAsync(materialCatalogPath);
                var elementTask = Task.Run(async () =>
                {
                    // ElementOutliner depends on MaterialCatalog, so we need to wait for it
                    var materialCatalog = await materialTask;
                    var service = new ElementOutlinerService(elementsPath, materialCatalog);
                    await service.InitializeAsync();
                    return service;
                });

                // Wait for both services to complete
                MaterialCatalog = await materialTask;
                ElementOutliner = await elementTask;
                
                RhinoApp.WriteLine("RhinoCNC: All services initialized successfully.");
            }
            catch (Exception e)
            {
                RhinoApp.WriteLine($"RhinoCNC: CRITICAL SERVICE INIT FAILURE: {e.Message}");
                RhinoApp.WriteLine($"RhinoCNC: Stack trace: {e.StackTrace}");
                
                // Ensure we have at least basic services even if initialization fails
                if (MaterialCatalog == null)
                {
                    try
                    {
                        var dataDir = GetDataDirectory();
                        var materialCatalogPath = Path.Combine(dataDir, "materials.json");
                        MaterialCatalog = await MaterialCatalogService.CreateAsync(materialCatalogPath);
                        RhinoApp.WriteLine("RhinoCNC: Fallback MaterialCatalog initialization successful.");
                    }
                    catch (Exception fallbackEx)
                    {
                        RhinoApp.WriteLine($"RhinoCNC: Fallback initialization also failed: {fallbackEx.Message}");
                    }
                }
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

        /// <summary>
        /// Registers the plugin panels with Rhino
        /// </summary>
        private async Task InitializePanelsAsync()
        {
            try
            {
                // Register custom panels (without icons for now)
                Panels.RegisterPanel(this, typeof(ElementOutlinerPanel), "Element Outliner", null);
                Panels.RegisterPanel(this, typeof(MaterialPalettePanel), "Material Palette", null);

                RhinoApp.WriteLine("RhinoCNC: Panels registered successfully.");
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine($"RhinoCNC: Error registering panels: {ex.Message}");
                throw; // Re-throw to be handled by the calling method
            }
        }

        private void OnRhinoAppInitialized(object sender, EventArgs e)
        {
            RhinoApp.Initialized -= OnRhinoAppInitialized;

            // Defer panel initialization until the app is fully idle
            RhinoApp.Idle += OnRhinoAppIdle;
        }

        private void OnRhinoAppIdle(object sender, EventArgs e)
        {
            RhinoApp.Idle -= OnRhinoAppIdle;

            try
            {
                if (Panels.GetPanel(typeof(MaterialPalettePanel).GUID) is MaterialPalettePanel panel)
                {
                    // The service initialization is now handled inside the panel's constructor via the ViewModel
                    RhinoApp.WriteLine("RhinoCNC: Material Palette panel found and already initialized.");
                }
                else
                {
                    RhinoApp.WriteLine("RhinoCNC: Material Palette panel not found after idle. It will initialize when opened.");
                }
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine($"RhinoCNC: Error during deferred panel service initialization: {ex.Message}");
            }
        }
    }
} 