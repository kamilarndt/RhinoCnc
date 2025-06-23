using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace RhinoCncSuite.Models
{
    /// <summary>
    /// Kolekcja materiałów z metadanymi i funkcjami zarządzania
    /// Collection of materials with metadata and management functions
    /// </summary>
    public class MaterialCollection : INotifyPropertyChanged
    {
        private string _name;
        private string _description;
        private DateTime _lastModified;
        private List<Material> _materials;

        /// <summary>
        /// Nazwa kolekcji materiałów
        /// Name of the material collection
        /// </summary>
        [JsonProperty("name")]
        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged();
                    UpdateLastModified();
                }
            }
        }

        /// <summary>
        /// Opis kolekcji materiałów
        /// Description of the material collection
        /// </summary>
        [JsonProperty("description")]
        public string Description
        {
            get => _description;
            set
            {
                if (_description != value)
                {
                    _description = value;
                    OnPropertyChanged();
                    UpdateLastModified();
                }
            }
        }

        /// <summary>
        /// Data ostatniej modyfikacji
        /// Date of last modification
        /// </summary>
        [JsonProperty("lastModified")]
        public DateTime LastModified
        {
            get => _lastModified;
            private set
            {
                if (_lastModified != value)
                {
                    _lastModified = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Lista materiałów w kolekcji
        /// List of materials in the collection
        /// </summary>
        [JsonProperty("materials")]
        public List<Material> Materials
        {
            get => _materials ?? (_materials = new List<Material>());
            set
            {
                if (_materials != value)
                {
                    _materials = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Count));
                    UpdateLastModified();
                }
            }
        }

        /// <summary>
        /// Liczba materiałów w kolekcji
        /// Number of materials in the collection
        /// </summary>
        [JsonIgnore]
        public int Count => Materials?.Count ?? 0;

        /// <summary>
        /// Konstruktor domyślny
        /// Default constructor
        /// </summary>
        public MaterialCollection()
        {
            _materials = new List<Material>();
            _lastModified = DateTime.Now;
        }

        /// <summary>
        /// Konstruktor z nazwą
        /// Constructor with name
        /// </summary>
        /// <param name="name">Nazwa kolekcji / Collection name</param>
        public MaterialCollection(string name) : this()
        {
            Name = name;
        }

        /// <summary>
        /// Dodaje materiał do kolekcji
        /// Adds a material to the collection
        /// </summary>
        /// <param name="material">Materiał do dodania / Material to add</param>
        public void AddMaterial(Material material)
        {
            if (material != null && !Materials.Contains(material))
            {
                Materials.Add(material);
                OnPropertyChanged(nameof(Materials));
                OnPropertyChanged(nameof(Count));
                UpdateLastModified();
            }
        }

        /// <summary>
        /// Usuwa materiał z kolekcji
        /// Removes a material from the collection
        /// </summary>
        /// <param name="material">Materiał do usunięcia / Material to remove</param>
        /// <returns>True jeśli usunięto / True if removed</returns>
        public bool RemoveMaterial(Material material)
        {
            if (material != null && Materials.Remove(material))
            {
                OnPropertyChanged(nameof(Materials));
                OnPropertyChanged(nameof(Count));
                UpdateLastModified();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Czyści kolekcję materiałów
        /// Clears the material collection
        /// </summary>
        public void Clear()
        {
            Materials.Clear();
            OnPropertyChanged(nameof(Materials));
            OnPropertyChanged(nameof(Count));
            UpdateLastModified();
        }

        /// <summary>
        /// Aktualizuje datę ostatniej modyfikacji
        /// Updates the last modified date
        /// </summary>
        private void UpdateLastModified()
        {
            LastModified = DateTime.Now;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 