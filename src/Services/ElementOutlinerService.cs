using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RhinoCncSuite.Models;

namespace RhinoCncSuite.Services
{
    /// <summary>
    /// Service for providing element information for the outliner.
    /// </summary>
    public class ElementOutlinerService
    {
        private readonly string _filePath;
        private readonly MaterialCatalogService _materialCatalogService;
        public List<ElementInfo> Elements { get; private set; }

        /// <summary>
        /// Event fired when the elements collection changes
        /// </summary>
        public event EventHandler<ElementChangedEventArgs> ElementsChanged;

        /// <summary>
        /// Constructor
        /// </summary>
        public ElementOutlinerService(string filePath, MaterialCatalogService materialCatalogService)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));

            _filePath = filePath;
            _materialCatalogService = materialCatalogService ?? throw new ArgumentNullException(nameof(materialCatalogService));
            Elements = new List<ElementInfo>();
        }

        /// <summary>
        /// Initializes the service and loads existing elements
        /// </summary>
        public async Task InitializeAsync()
        {
            await LoadElementsAsync();
        }

        /// <summary>
        /// Loads elements from the JSON file
        /// </summary>
        private async Task LoadElementsAsync()
        {
            try
            {
                if (File.Exists(_filePath))
                {
                    var json = await File.ReadAllTextAsync(_filePath);
                    Elements = JsonConvert.DeserializeObject<List<ElementInfo>>(json) ?? new List<ElementInfo>();
                }
                else
                {
                    InitializeDefaultElements();
                    await SaveElementsAsync();
                }
                OnElementsChanged(new ElementChangedEventArgs(ElementChangeType.CollectionReloaded, null));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading elements: {ex.Message}");
                Elements = new List<ElementInfo>();
                // Re-throw to indicate that initialization failed
                throw;
            }
        }

        /// <summary>
        /// Saves the current elements to the JSON file
        /// </summary>
        public async Task SaveElementsAsync()
        {
            try
            {
                var jsonContent = JsonConvert.SerializeObject(Elements, Formatting.Indented);
                await File.WriteAllTextAsync(_filePath, jsonContent);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving elements: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Adds a new element
        /// </summary>
        /// <param name="element">Element to add</param>
        public async Task<bool> AddElementAsync(ElementInfo element)
        {
            if (element == null) return false;

            if (Elements.Any(e => e.Id == element.Id))
                return false;

            Elements.Add(element);
            await SaveElementsAsync();
            OnElementsChanged(new ElementChangedEventArgs(ElementChangeType.Added, element));
            return true;
        }

        /// <summary>
        /// Adds multiple new elements
        /// </summary>
        /// <param name="elementsToAdd">Elements to add</param>
        public async Task<bool> AddElementsAsync(IEnumerable<ElementInfo> elementsToAdd)
        {
            if (elementsToAdd == null || !elementsToAdd.Any()) return false;

            // Filter out elements that already exist
            var newElements = elementsToAdd.Where(toAdd => !Elements.Any(e => e.Id == toAdd.Id)).ToList();
            if (!newElements.Any())
                return false;

            Elements.AddRange(newElements);
            await SaveElementsAsync();
            // Fire a general "reloaded" event as multiple items were added
            OnElementsChanged(new ElementChangedEventArgs(ElementChangeType.CollectionReloaded, null));
            return true;
        }

        /// <summary>
        /// Updates an existing element
        /// </summary>
        /// <param name="element">Updated element</param>
        public async Task<bool> UpdateElementAsync(ElementInfo element)
        {
            if (element == null) return false;

            var existingIndex = Elements.FindIndex(e => e.Id == element.Id);
            if (existingIndex == -1)
                return false;

            element.ModifiedDate = DateTime.Now;
            Elements[existingIndex] = element;
            await SaveElementsAsync();
            OnElementsChanged(new ElementChangedEventArgs(ElementChangeType.Updated, element));
            return true;
        }

        /// <summary>
        /// Removes an element
        /// </summary>
        /// <param name="elementId">ID of element to remove</param>
        public async Task<bool> RemoveElementAsync(Guid elementId)
        {
            if (elementId == Guid.Empty) return false;

            var removedElement = Elements.FirstOrDefault(e => e.Id == elementId);
            if (removedElement != null && Elements.Remove(removedElement))
            {
                await SaveElementsAsync();
                OnElementsChanged(new ElementChangedEventArgs(ElementChangeType.Removed, removedElement));
                return true;
            }
            return false;
        }

        /// <summary>
        /// Gets an element by its ID
        /// </summary>
        /// <param name="elementId">Element ID</param>
        /// <returns>Element if found, null otherwise</returns>
        public ElementInfo GetElementById(Guid elementId)
        {
            if (elementId == Guid.Empty) return null;
            return Elements.FirstOrDefault(e => e.Id == elementId);
        }

        /// <summary>
        /// Gets an element by its Rhino ID
        /// </summary>
        /// <param name="rhinoId">Rhino object/block ID</param>
        /// <returns>Element if found, null otherwise</returns>
        public ElementInfo GetElementByRhinoId(string rhinoId)
        {
            if (string.IsNullOrEmpty(rhinoId)) return null;

            return Elements.FirstOrDefault(e => e.RhinoId == rhinoId);
        }

        /// <summary>
        /// Searches elements by name (case-insensitive partial match)
        /// </summary>
        /// <param name="searchTerm">Search term</param>
        /// <returns>List of matching elements</returns>
        public List<ElementInfo> SearchElements(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return Elements.ToList();

            return Elements
                .Where(e =>
                    (e.Name != null && e.Name.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (e.Description != null && e.Description.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (e.Tags != null && e.Tags.Any(tag => tag.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0))
                )
                .ToList();
        }

        /// <summary>
        /// Gets elements by type
        /// </summary>
        /// <param name="type">Element type</param>
        /// <returns>List of elements of the specified type</returns>
        public List<ElementInfo> GetElementsByType(ElementType type)
        {
            return Elements.Where(e => e.Type == type).ToList();
        }

        /// <summary>
        /// Gets elements by status
        /// </summary>
        /// <param name="status">Element status</param>
        /// <returns>List of elements with the specified status</returns>
        public List<ElementInfo> GetElementsByStatus(ElementStatus status)
        {
            return Elements.Where(e => e.Status == status).ToList();
        }

        /// <summary>
        /// Gets elements by material
        /// </summary>
        /// <param name="materialId">Material ID</param>
        /// <returns>List of elements with the specified material</returns>
        public List<ElementInfo> GetElementsByMaterial(Guid? materialId)
        {
            if (!materialId.HasValue || materialId.Value == Guid.Empty) return new List<ElementInfo>();
            return Elements.Where(e => e.MaterialId == materialId).ToList();
        }

        /// <summary>
        /// Attaches a file to an element
        /// </summary>
        /// <param name="elementId">Element ID</param>
        /// <param name="filePath">Path to the file to attach</param>
        /// <param name="description">Description of the file</param>
        /// <param name="category">File category</param>
        public async Task<bool> AttachFileToElementAsync(Guid elementId, string filePath, string description = null, FileCategory category = FileCategory.Other)
        {
            var element = GetElementById(elementId);
            if (element == null || !File.Exists(filePath))
                return false;

            var attachedFile = new AttachedFile(filePath)
            {
                Description = description,
                Category = category
            };

            element.AttachedFiles.Add(attachedFile);
            return await UpdateElementAsync(element);
        }

        /// <summary>
        /// Removes an attached file from an element
        /// </summary>
        /// <param name="elementId">Element ID</param>
        /// <param name="fileId">Attached file ID</param>
        public async Task<bool> RemoveAttachedFileAsync(Guid elementId, Guid fileId)
        {
            var element = GetElementById(elementId);
            if (element == null)
                return false;

            var fileToRemove = element.AttachedFiles.FirstOrDefault(f => f.Id == fileId);
            if (fileToRemove == null)
                return false;

            element.AttachedFiles.Remove(fileToRemove);
            return await UpdateElementAsync(element);
        }

        /// <summary>
        /// Creates an element from a Rhino block
        /// </summary>
        /// <param name="blockName">Name of the Rhino block</param>
        /// <param name="blockId">Rhino block ID</param>
        /// <returns>Created element</returns>
        public async Task<ElementInfo> CreateElementFromBlockAsync(string blockName, Guid blockId)
        {
            var element = new ElementInfo(blockName, blockId.ToString(), ElementType.Block);
            
            if (await AddElementAsync(element))
            {
                return element;
            }
            
            return null;
        }

        /// <summary>
        /// Initializes default elements for demo purposes
        /// </summary>
        private void InitializeDefaultElements()
        {
            Elements = new List<ElementInfo>();
        }

        /// <summary>
        /// Raises the ElementsChanged event
        /// </summary>
        protected virtual void OnElementsChanged(ElementChangedEventArgs e)
        {
            ElementsChanged?.Invoke(this, e);
        }
    }

    /// <summary>
    /// Event arguments for element changes
    /// </summary>
    public class ElementChangedEventArgs : EventArgs
    {
        public ElementChangeType ChangeType { get; }
        public ElementInfo Element { get; }

        public ElementChangedEventArgs(ElementChangeType changeType, ElementInfo element)
        {
            ChangeType = changeType;
            Element = element;
        }
    }

    /// <summary>
    /// Types of changes that can occur to elements
    /// </summary>
    public enum ElementChangeType
    {
        Added,
        Updated,
        Removed,
        CollectionReloaded
    }
} 