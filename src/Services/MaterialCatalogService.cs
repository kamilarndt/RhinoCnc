using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Rhino;
using RhinoCncSuite.Models;

namespace RhinoCncSuite.Services
{
    public class MaterialCatalogChangedEventArgs : EventArgs
    {
        public bool IsReload { get; }
        public MaterialCatalogChangedEventArgs(bool isReload) { IsReload = isReload; }
    }

    /// <summary>
    /// Service for managing the material catalog with lazy initialization for better performance.
    /// </summary>
    public class MaterialCatalogService
    {
        public event EventHandler<MaterialCatalogChangedEventArgs> CatalogChanged;

        private readonly Lazy<Task<List<Material>>> _materials;
        private readonly string _filePath;
        private List<Material> _cachedMaterials; // Cache for frequent access
        private bool _isCacheValid = false;

        private MaterialCatalogService(string filePath)
        {
            _filePath = filePath;
            _materials = new Lazy<Task<List<Material>>>(LoadMaterialsAsync);
        }

        /// <summary>
        /// Creates a MaterialCatalogService instance with lazy initialization
        /// </summary>
        public static Task<MaterialCatalogService> CreateAsync(string filePath)
        {
            var service = new MaterialCatalogService(filePath);
            return Task.FromResult(service);
        }

        /// <summary>
        /// Gets materials with caching for better performance
        /// </summary>
        public async Task<List<Material>> GetMaterialsAsync()
        {
            if (_isCacheValid && _cachedMaterials != null)
            {
                return _cachedMaterials;
            }

            _cachedMaterials = await _materials.Value;
            _isCacheValid = true;
            return _cachedMaterials;
        }

        /// <summary>
        /// Gets materials synchronously (for compatibility)
        /// </summary>
        public List<Material> GetCatalog()
        {
            if (_isCacheValid && _cachedMaterials != null)
            {
                return _cachedMaterials;
            }

            // If not loaded yet, return empty list to avoid blocking
            return new List<Material>();
        }

        /// <summary>
        /// Invalidates cache and reloads materials
        /// </summary>
        public async Task ReloadAsync()
        {
            _isCacheValid = false;
            _cachedMaterials = await LoadMaterialsAsync();
            _isCacheValid = true;
            
            RhinoApp.InvokeOnUiThread(new Action(() => 
            {
                OnCatalogChanged(new MaterialCatalogChangedEventArgs(true));
            }));
        }

        private async Task<List<Material>> LoadMaterialsAsync()
        {
            try
            {
                // Ensure the AppData directory exists
                var appDataDir = System.IO.Path.GetDirectoryName(_filePath);
                if (!Directory.Exists(appDataDir))
                {
                    Directory.CreateDirectory(appDataDir);
                }

                // If the catalog doesn't exist in AppData, create it from the embedded resource
                if (!File.Exists(_filePath))
                {
                    RhinoApp.WriteLine("RhinoCNC: Materials catalog not found. Creating from default...");
                    await CreateDefaultCatalogFromFileAsync();
                }

                RhinoApp.WriteLine($"RhinoCNC: Loading materials from {_filePath}...");
                var json = await File.ReadAllTextAsync(_filePath);
                var materials = JsonConvert.DeserializeObject<List<Material>>(json) ?? new List<Material>();

                RhinoApp.WriteLine($"RhinoCNC: Loaded {materials.Count} materials successfully.");
                return materials;
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine($"RhinoCNC: Error loading materials: {ex.Message}. Attempting to restore from default.");
                try
                {
                    await CreateDefaultCatalogFromFileAsync();
                    var json = await File.ReadAllTextAsync(_filePath);
                    return JsonConvert.DeserializeObject<List<Material>>(json) ?? new List<Material>();
                }
                catch (Exception restoreEx)
                {
                    RhinoApp.WriteLine($"RhinoCNC: CRITICAL: Failed to restore default materials: {restoreEx.Message}");
                    return new List<Material>(); // Return empty list as a last resort
                }
            }
        }

        private async Task CreateDefaultCatalogFromFileAsync()
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var resourceName = "RhinoCncSuite.data.materials.json";

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    RhinoApp.WriteLine($"RhinoCNC: ERROR: Embedded default material resource not found: {resourceName}");
                    return;
                }
                using (var reader = new StreamReader(stream))
                {
                    var defaultJson = await reader.ReadToEndAsync();
                    await File.WriteAllTextAsync(_filePath, defaultJson);
                    RhinoApp.WriteLine($"RhinoCNC: Default materials catalog created at {_filePath}");
                }
            }
        }

        public async Task SaveMaterialsAsync()
        {
            if (_materials.IsValueCreated)
            {
                var materials = await _materials.Value;
                await SaveMaterialsAsync(materials);
            }
        }

        private async Task SaveMaterialsAsync(List<Material> materials)
        {
            try
            {
                var json = JsonConvert.SerializeObject(materials, Formatting.Indented);
                await File.WriteAllTextAsync(_filePath, json);
                RhinoApp.WriteLine("RhinoCNC: Material catalog saved.");
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine($"RhinoCNC: Error saving materials: {ex.Message}");
            }
        }

        protected virtual void OnCatalogChanged(MaterialCatalogChangedEventArgs e)
        {
            CatalogChanged?.Invoke(this, e);
        }

        public async Task<Material> GetMaterialByIdAsync(Guid materialId)
        {
            var materials = await GetMaterialsAsync();
            return materials.FirstOrDefault(m => m.Id == materialId);
        }

        public Material GetMaterialById(Guid materialId)
        {
            var materials = GetCatalog();
            return materials.FirstOrDefault(m => m.Id == materialId);
        }

        public async Task AddOrUpdateMaterialAsync(Material material)
        {
            var materials = await GetMaterialsAsync();
            
            if (material.Id == Guid.Empty)
            {
                material.Id = Guid.NewGuid();
            }

            var existing = materials.FirstOrDefault(m => m.Id == material.Id);
            if (existing != null)
            {
                // Simple property copy for update
                existing.Name = material.Name;
                existing.Thickness = material.Thickness;
                existing.Description = material.Description;
                existing.ColorHex = material.ColorHex;
                existing.IsLocked = material.IsLocked;
            }
            else
            {
                materials.Add(material);
            }
            
            await SaveMaterialsAsync(materials);
            OnCatalogChanged(new MaterialCatalogChangedEventArgs(false));
        }

        private List<Material> GetDefaultMaterials()
        {
            // This method is now a fallback in case the embedded resource fails.
            return new List<Material>
            {
                new Material 
                { 
                    Id = Guid.NewGuid(),
                    Name = "Plywood 18mm", 
                    Thickness = 18.0, 
                    Description = "Standard Birch Plywood",
                    ColorHex = "#DEB887",
                    Width = 1220,
                    Length = 2440,
                    Type = MaterialType.Sheet
                },
                new Material 
                { 
                    Id = Guid.NewGuid(),
                    Name = "MDF 12mm", 
                    Thickness = 12.0, 
                    Description = "Medium-Density Fiberboard",
                    ColorHex = "#A0522D",
                    Width = 1220,
                    Length = 2440,
                    Type = MaterialType.Sheet
                },
                new Material 
                { 
                    Id = Guid.NewGuid(),
                    Name = "Acrylic 3mm", 
                    Thickness = 3.0, 
                    Description = "Clear Acrylic Sheet",
                    ColorHex = "#F0F8FF",
                    Width = 1000,
                    Length = 2000,
                    Type = MaterialType.Sheet
                },
                new Material 
                { 
                    Id = Guid.NewGuid(),
                    Name = "OSB 15mm", 
                    Thickness = 15.0, 
                    Description = "Oriented Strand Board",
                    ColorHex = "#DAA520",
                    Width = 1220,
                    Length = 2440,
                    Type = MaterialType.Sheet
                },
                new Material 
                { 
                    Id = Guid.NewGuid(),
                    Name = "Aluminum 2mm", 
                    Thickness = 2.0, 
                    Description = "Aluminum Composite Panel",
                    ColorHex = "#D3D3D3",
                    Width = 1220,
                    Length = 2440,
                    Type = MaterialType.Sheet
                }
            };
        }
    }
} 