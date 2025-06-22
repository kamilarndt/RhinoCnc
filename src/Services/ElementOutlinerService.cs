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
    /// Service for managing elements in the Element Outliner
    /// </summary>
    public class ElementOutlinerService
    {
        private readonly string _elementsFilePath;
        private List<ElementInfo> _elements;
        private readonly MaterialCatalogService _materialCatalogService;
        private readonly object _lock = new object();

        /// <summary>
        /// Event fired when the elements collection changes
        /// </summary>
        public event EventHandler<ElementChangedEventArgs> ElementsChanged;

        /// <summary>
        /// Gets a read-only list of all elements
        /// </summary>
        public IReadOnlyList<ElementInfo> Elements
        {
            get
            {
                lock (_lock)
                {
                    return _elements.AsReadOnly();
                }
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public ElementOutlinerService(string elementsFilePath, MaterialCatalogService materialCatalogService)
        {
            if (string.IsNullOrEmpty(elementsFilePath))
                throw new ArgumentNullException(nameof(elementsFilePath));

            _elementsFilePath = elementsFilePath;
            _materialCatalogService = materialCatalogService ?? throw new ArgumentNullException(nameof(materialCatalogService));
            _elements = new List<ElementInfo>();
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
        public async Task LoadElementsAsync()
        {
            try
            {
                var directory = Path.GetDirectoryName(_elementsFilePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                if (File.Exists(_elementsFilePath))
                {
                    var json = await Task.Run(() => File.ReadAllText(_elementsFilePath));
                    var loadedElements = JsonConvert.DeserializeObject<List<ElementInfo>>(json) ?? new List<ElementInfo>();
                    lock (_lock)
                    {
                        _elements = loadedElements;
                    }
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
                lock (_lock)
                {
                    _elements = new List<ElementInfo>();
                }
                // Re-throw to indicate that initialization failed
                throw;
            }
        }

        /// <summary>
        /// Saves the current elements to the JSON file
        /// </summary>
        public async Task SaveElementsAsync()
        {
            List<ElementInfo> elementsToSave;
            lock (_lock)
            {
                elementsToSave = new List<ElementInfo>(_elements);
            }

            try
            {
                var directory = Path.GetDirectoryName(_elementsFilePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                var jsonContent = JsonConvert.SerializeObject(elementsToSave, Formatting.Indented);
                await Task.Run(() => File.WriteAllText(_elementsFilePath, jsonContent));
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

            lock (_lock)
            {
                if (_elements.Any(e => e.Id == element.Id))
                    return false;

                _elements.Add(element);
            }

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

            lock (_lock)
            {
                // Filter out elements that already exist
                var newElements = elementsToAdd.Where(toAdd => !_elements.Any(e => e.Id == toAdd.Id)).ToList();
                if (!newElements.Any())
                    return false;

                _elements.AddRange(newElements);
            }

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

            lock (_lock)
            {
                var existingIndex = _elements.FindIndex(e => e.Id == element.Id);
                if (existingIndex == -1)
                    return false;

                element.ModifiedDate = DateTime.Now;
                _elements[existingIndex] = element;
            }

            await SaveElementsAsync();
            OnElementsChanged(new ElementChangedEventArgs(ElementChangeType.Updated, element));
            return true;
        }

        /// <summary>
        /// Removes an element
        /// </summary>
        /// <param name="elementId">ID of element to remove</param>
        public async Task<bool> RemoveElementAsync(string elementId)
        {
            if (string.IsNullOrEmpty(elementId)) return false;

            ElementInfo removedElement = null;
            lock (_lock)
            {
                removedElement = _elements.FirstOrDefault(e => e.Id == elementId);
                if (removedElement == null)
                    return false;

                _elements.Remove(removedElement);
            }

            await SaveElementsAsync();
            OnElementsChanged(new ElementChangedEventArgs(ElementChangeType.Removed, removedElement));
            return true;
        }

        /// <summary>
        /// Gets an element by its ID
        /// </summary>
        /// <param name="elementId">Element ID</param>
        /// <returns>Element if found, null otherwise</returns>
        public ElementInfo GetElementById(string elementId)
        {
            if (string.IsNullOrEmpty(elementId)) return null;

            lock (_lock)
            {
                return _elements.FirstOrDefault(e => e.Id == elementId);
            }
        }

        /// <summary>
        /// Gets an element by its Rhino ID
        /// </summary>
        /// <param name="rhinoId">Rhino object/block ID</param>
        /// <returns>Element if found, null otherwise</returns>
        public ElementInfo GetElementByRhinoId(string rhinoId)
        {
            if (string.IsNullOrEmpty(rhinoId)) return null;

            lock (_lock)
            {
                return _elements.FirstOrDefault(e => e.RhinoId == rhinoId);
            }
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

            lock (_lock)
            {
                return _elements
                    .Where(e =>
                        (e.Name != null && e.Name.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0) ||
                        (e.Description != null && e.Description.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0) ||
                        (e.Tags != null && e.Tags.Any(tag => tag.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0))
                    )
                    .ToList();
            }
        }

        /// <summary>
        /// Gets elements by type
        /// </summary>
        /// <param name="type">Element type</param>
        /// <returns>List of elements of the specified type</returns>
        public List<ElementInfo> GetElementsByType(ElementType type)
        {
            lock (_lock)
            {
                return _elements.Where(e => e.Type == type).ToList();
            }
        }

        /// <summary>
        /// Gets elements by status
        /// </summary>
        /// <param name="status">Element status</param>
        /// <returns>List of elements with the specified status</returns>
        public List<ElementInfo> GetElementsByStatus(ElementStatus status)
        {
            lock (_lock)
            {
                return _elements.Where(e => e.Status == status).ToList();
            }
        }

        /// <summary>
        /// Gets elements by material
        /// </summary>
        /// <param name="materialId">Material ID</param>
        /// <returns>List of elements with the specified material</returns>
        public List<ElementInfo> GetElementsByMaterial(string materialId)
        {
            if (string.IsNullOrEmpty(materialId)) return new List<ElementInfo>();

            lock (_lock)
            {
                return _elements.Where(e => e.MaterialId == materialId).ToList();
            }
        }

        /// <summary>
        /// Attaches a file to an element
        /// </summary>
        /// <param name="elementId">Element ID</param>
        /// <param name="filePath">Path to the file to attach</param>
        /// <param name="description">Description of the file</param>
        /// <param name="category">File category</param>
        public async Task<bool> AttachFileToElementAsync(string elementId, string filePath, string description = null, FileCategory category = FileCategory.Other)
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
        public async Task<bool> RemoveAttachedFileAsync(string elementId, string fileId)
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
        public async Task<ElementInfo> CreateElementFromBlockAsync(string blockName, string blockId)
        {
            var element = new ElementInfo(blockName, blockId, ElementType.Block);
            
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
            lock (_lock)
            {
                _elements = new List<ElementInfo>();
            }
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