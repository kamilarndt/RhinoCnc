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
    /// Service for managing the material catalog.
    /// </summary>
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
            if (!File.Exists(_filePath))
            {
                Materials = GetDefaultMaterials();
                await SaveMaterialsAsync();
            }
            else
            {
                var json = await File.ReadAllTextAsync(_filePath);
                Materials = JsonConvert.DeserializeObject<List<Material>>(json) ?? GetDefaultMaterials();
            }

            bool idsAssigned = false;
            foreach (var material in Materials.Where(m => m.Id == Guid.Empty))
            {
                material.Id = Guid.NewGuid();
                idsAssigned = true;
            }

            if (idsAssigned)
            {
                await SaveMaterialsAsync();
            }

            OnCatalogChanged(new MaterialCatalogChangedEventArgs(true));
        }

        public async Task SaveMaterialsAsync()
        {
            var json = JsonConvert.SerializeObject(Materials, Formatting.Indented);
            await File.WriteAllTextAsync(_filePath, json);
            RhinoApp.WriteLine("RhinoCNC: Material catalog saved.");
        }

        protected virtual void OnCatalogChanged(MaterialCatalogChangedEventArgs e)
        {
            CatalogChanged?.Invoke(this, e);
        }

        public Material GetMaterialById(Guid materialId)
        {
            return Materials.FirstOrDefault(m => m.Id == materialId);
        }

        public async Task AddOrUpdateMaterialAsync(Material material)
        {
            if (material.Id == Guid.Empty)
            {
                material.Id = Guid.NewGuid();
            }

            var existing = Materials.FirstOrDefault(m => m.Id == material.Id);
            if (existing != null)
            {
                // Simple property copy for update
                existing.Name = material.Name;
                existing.Thickness = material.Thickness;
                existing.Description = material.Description;
                existing.ColorHex = material.ColorHex;
            }
            else
            {
                Materials.Add(material);
            }
            await SaveMaterialsAsync();
            OnCatalogChanged(new MaterialCatalogChangedEventArgs(false));
        }

        private List<Material> GetDefaultMaterials()
        {
            return new List<Material>
            {
                new Material { Name = "Plywood", Thickness = 18.0, Description = "Standard Plywood" },
                new Material { Name = "MDF", Thickness = 12.0, Description = "Medium-Density Fiberboard" }
            };
        }
    }
} 