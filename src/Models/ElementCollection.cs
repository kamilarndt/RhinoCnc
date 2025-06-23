using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Linq;
using Newtonsoft.Json;

namespace RhinoCncSuite.Models
{
    /// <summary>
    /// Kolekcja elementów z metadanymi i funkcjami zarządzania
    /// Collection of elements with metadata and management functions
    /// </summary>
    public class ElementCollection : INotifyPropertyChanged
    {
        private string _name;
        private string _description;
        private DateTime _lastModified;
        private List<ElementInfo> _elements;

        /// <summary>
        /// Nazwa kolekcji elementów
        /// Name of the element collection
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
        /// Opis kolekcji elementów
        /// Description of the element collection
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
        /// Lista elementów w kolekcji
        /// List of elements in the collection
        /// </summary>
        [JsonProperty("elements")]
        public List<ElementInfo> Elements
        {
            get => _elements ?? (_elements = new List<ElementInfo>());
            set
            {
                if (_elements != value)
                {
                    _elements = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Count));
                    OnPropertyChanged(nameof(CompletedCount));
                    OnPropertyChanged(nameof(PendingCount));
                    UpdateLastModified();
                }
            }
        }

        /// <summary>
        /// Liczba elementów w kolekcji
        /// Number of elements in the collection
        /// </summary>
        [JsonIgnore]
        public int Count => Elements?.Count ?? 0;

        /// <summary>
        /// Liczba ukończonych elementów
        /// Number of completed elements
        /// </summary>
        [JsonIgnore]
        public int CompletedCount => Elements?.Count(e => e.Status == ElementStatus.Completed) ?? 0;

        /// <summary>
        /// Liczba oczekujących elementów
        /// Number of pending elements
        /// </summary>
        [JsonIgnore]
        public int PendingCount => Elements?.Count(e => e.Status != ElementStatus.Completed && e.Status != ElementStatus.Cancelled) ?? 0;

        /// <summary>
        /// Procent ukończenia
        /// Completion percentage
        /// </summary>
        [JsonIgnore]
        public double CompletionPercentage => Count > 0 ? (double)CompletedCount / Count * 100 : 0;

        /// <summary>
        /// Konstruktor domyślny
        /// Default constructor
        /// </summary>
        public ElementCollection()
        {
            _elements = new List<ElementInfo>();
            _lastModified = DateTime.Now;
        }

        /// <summary>
        /// Konstruktor z nazwą
        /// Constructor with name
        /// </summary>
        /// <param name="name">Nazwa kolekcji / Collection name</param>
        public ElementCollection(string name) : this()
        {
            Name = name;
        }

        /// <summary>
        /// Dodaje element do kolekcji
        /// Adds an element to the collection
        /// </summary>
        /// <param name="element">Element do dodania / Element to add</param>
        public void AddElement(ElementInfo element)
        {
            if (element != null && !Elements.Contains(element))
            {
                Elements.Add(element);
                OnPropertyChanged(nameof(Elements));
                OnPropertyChanged(nameof(Count));
                OnPropertyChanged(nameof(CompletedCount));
                OnPropertyChanged(nameof(PendingCount));
                OnPropertyChanged(nameof(CompletionPercentage));
                UpdateLastModified();
            }
        }

        /// <summary>
        /// Usuwa element z kolekcji
        /// Removes an element from the collection
        /// </summary>
        /// <param name="element">Element do usunięcia / Element to remove</param>
        /// <returns>True jeśli usunięto / True if removed</returns>
        public bool RemoveElement(ElementInfo element)
        {
            if (element != null && Elements.Remove(element))
            {
                OnPropertyChanged(nameof(Elements));
                OnPropertyChanged(nameof(Count));
                OnPropertyChanged(nameof(CompletedCount));
                OnPropertyChanged(nameof(PendingCount));
                OnPropertyChanged(nameof(CompletionPercentage));
                UpdateLastModified();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Znajduje element po ID
        /// Finds an element by ID
        /// </summary>
        /// <param name="id">ID elementu / Element ID</param>
        /// <returns>Element lub null / Element or null</returns>
        public ElementInfo FindElement(Guid id)
        {
            return Elements?.FirstOrDefault(e => e.Id == id);
        }

        /// <summary>
        /// Znajduje elementy po statusie
        /// Finds elements by status
        /// </summary>
        /// <param name="status">Status elementu / Element status</param>
        /// <returns>Lista elementów / List of elements</returns>
        public List<ElementInfo> FindElementsByStatus(ElementStatus status)
        {
            return Elements?.Where(e => e.Status == status).ToList() ?? new List<ElementInfo>();
        }

        /// <summary>
        /// Znajduje elementy po priorytecie
        /// Finds elements by priority
        /// </summary>
        /// <param name="priority">Priorytet elementu / Element priority</param>
        /// <returns>Lista elementów / List of elements</returns>
        public List<ElementInfo> FindElementsByPriority(ElementPriority priority)
        {
            return Elements?.Where(e => e.Priority == priority).ToList() ?? new List<ElementInfo>();
        }

        /// <summary>
        /// Czyści kolekcję elementów
        /// Clears the element collection
        /// </summary>
        public void Clear()
        {
            Elements.Clear();
            OnPropertyChanged(nameof(Elements));
            OnPropertyChanged(nameof(Count));
            OnPropertyChanged(nameof(CompletedCount));
            OnPropertyChanged(nameof(PendingCount));
            OnPropertyChanged(nameof(CompletionPercentage));
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