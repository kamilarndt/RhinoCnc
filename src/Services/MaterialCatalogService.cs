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

    public class MaterialCatalogService
    {
        public event EventHandler<MaterialCatalogChangedEventArgs> CatalogChanged;

        public List<Material> Materials { get; private set; }
        private readonly string _filePath;

        private MaterialCatalogService(string filePath)
        {
            _filePath = filePath;
            Materials = new List<Material>();
        }

        public static async Task<MaterialCatalogService> CreateAsync(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath), "File path for material catalog cannot be null or empty.");

            var service = new MaterialCatalogService(filePath);
            await service.LoadMaterialsAsync();
            return service;
        }

        public List<Material> GetCatalog()
        {
            return Materials ?? new List<Material>();
        }

        private async Task LoadMaterialsAsync()
        {
            try
            {
                var directory = Path.GetDirectoryName(_filePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                if (File.Exists(_filePath))
                {
                    var json = await Task.Run(() => File.ReadAllText(_filePath));
                    Materials = JsonConvert.DeserializeObject<List<Material>>(json) ?? new List<Material>();
                    RhinoApp.WriteLine($"RhinoCNC: Loaded {Materials.Count} materials from catalog.");
                }
                else
                {
                    Materials = new List<Material>();
                    await CreateDefaultCatalogAsync();
                    RhinoApp.WriteLine("RhinoCNC: Material catalog not found. Created a default catalog.");
                }

                bool idsAssigned = false;
                foreach (var material in Materials.Where(m => string.IsNullOrEmpty(m.Id)))
                {
                    material.Id = Guid.NewGuid().ToString();
                    idsAssigned = true;
                }
                if (idsAssigned)
                {
                    await SaveCatalogAsync();
                }
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine($"RhinoCNC: Error loading material catalog: {ex.Message}");
                Materials = new List<Material>();
            }

            OnCatalogChanged(new MaterialCatalogChangedEventArgs(true));
        }

        public async Task AddOrUpdateMaterialAsync(Material material)
        {
            if (material == null) return;

            if (string.IsNullOrEmpty(material.Id))
            {
                material.Id = Guid.NewGuid().ToString();
            }

            var existingMaterial = Materials.FirstOrDefault(m => m.Id == material.Id);

            if (existingMaterial != null)
            {
                existingMaterial.Name = material.Name;
                existingMaterial.Color = material.Color;
                existingMaterial.Width = material.Width;
                existingMaterial.Length = material.Length;
                existingMaterial.Thickness = material.Thickness;
                existingMaterial.Type = material.Type;
                // No need to set DisplayName as it's a calculated property
            }
            else
            {
                Materials.Add(material);
            }

            await SaveCatalogAsync();
            OnCatalogChanged(new MaterialCatalogChangedEventArgs(false));
        }

        private async Task SaveCatalogAsync()
        {
            try
            {
                var directory = Path.GetDirectoryName(_filePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var jsonContent = JsonConvert.SerializeObject(Materials, Formatting.Indented);
                await Task.Run(() => File.WriteAllText(_filePath, jsonContent));
                RhinoApp.WriteLine("RhinoCNC: Material catalog saved.");
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine($"RhinoCNC: Error saving material catalog: {ex.Message}");
            }
        }

        private async Task CreateDefaultCatalogAsync()
        {
            var defaultMaterials = new List<Material>
            {
                new Material { Id = Guid.NewGuid().ToString(), Name = "MDF 18mm", Color = "#D2B48C", Width = 2800, Length = 2070, Thickness = 18, Type = MaterialType.Sheet },
                new Material { Id = Guid.NewGuid().ToString(), Name = "Sklejka 12mm", Color = "#DEB887", Width = 2500, Length = 1250, Thickness = 12, Type = MaterialType.Sheet },
                new Material { Id = Guid.NewGuid().ToString(), Name = "PCV 3mm Białe", Color = "#FFFFFF", Width = 3050, Length = 1560, Thickness = 3, Type = MaterialType.Sheet },
                new Material { Id = Guid.NewGuid().ToString(), Name = "Pleksi 5mm Bezbarwna", Color = "#FFFFFF", Width = 3050, Length = 2050, Thickness = 5, Type = MaterialType.Sheet },
            };

            Materials.AddRange(defaultMaterials);
            await SaveCatalogAsync();
        }

        protected virtual void OnCatalogChanged(MaterialCatalogChangedEventArgs e)
        {
            CatalogChanged?.Invoke(this, e);
        }

        public Material GetMaterialById(string materialId)
        {
            if (string.IsNullOrEmpty(materialId)) return null;
            return Materials.FirstOrDefault(m => m.Id == materialId);
        }
    }
} 