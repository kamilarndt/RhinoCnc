using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Linq;
using RhinoCncSuite.Commands;
using RhinoCncSuite.Models;
using RhinoCncSuite.Services;
using Rhino;

namespace RhinoCncSuite.ViewModels
{
    /// <summary>
    /// ViewModel dla Element Outliner zgodnie z wzorcem MVVM
    /// ViewModel for Element Outliner following MVVM pattern
    /// Oddziela zarządzanie danymi od interfejsu użytkownika
    /// Separates data management from UI concerns
    /// </summary>
    public class ElementOutlinerViewModel : BaseViewModel
    {
        #region Pola prywatne / Private Fields
        private readonly ElementOutlinerService _elementOutlinerService;
        private readonly MaterialCatalogService _materialCatalogService;
        private ObservableCollection<ElementInfo> _elements;
        private ObservableCollection<Material> _availableMaterials;
        private ElementInfo _selectedElement;
        private string _searchText = string.Empty;
        private ElementStatus _statusFilter = ElementStatus.Design;
        private ElementPriority _priorityFilter = ElementPriority.Normal;
        private string _statusText = "Ładowanie elementów...";
        private bool _isLoading = true;
        private bool _showCompletedElements = true;
        private int _totalElements;
        private int _completedElements;
        #endregion

        #region Właściwości publiczne / Public Properties
        /// <summary>
        /// Kolekcja elementów do bindowania danych
        /// Collection of elements for data binding
        /// </summary>
        public ObservableCollection<ElementInfo> Elements
        {
            get => _elements;
            private set
            {
                _elements = value;
                OnPropertyChanged();
                UpdateStatusText();
            }
        }

        /// <summary>
        /// Dostępne materiały do przypisania
        /// Available materials for assignment
        /// </summary>
        public ObservableCollection<Material> AvailableMaterials
        {
            get => _availableMaterials;
            private set
            {
                _availableMaterials = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Aktualnie wybrany element
        /// Currently selected element
        /// </summary>
        public ElementInfo SelectedElement
        {
            get => _selectedElement;
            set
            {
                _selectedElement = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsElementSelected));
            }
        }

        /// <summary>
        /// Tekst wyszukiwania
        /// Search text
        /// </summary>
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                _ = FilterElementsAsync();
            }
        }

        /// <summary>
        /// Filtr statusu
        /// Status filter
        /// </summary>
        public ElementStatus StatusFilter
        {
            get => _statusFilter;
            set
            {
                _statusFilter = value;
                OnPropertyChanged();
                _ = FilterElementsAsync();
            }
        }

        /// <summary>
        /// Filtr priorytetu
        /// Priority filter
        /// </summary>
        public ElementPriority PriorityFilter
        {
            get => _priorityFilter;
            set
            {
                _priorityFilter = value;
                OnPropertyChanged();
                _ = FilterElementsAsync();
            }
        }

        /// <summary>
        /// Tekst statusu dla interfejsu użytkownika
        /// Status text for UI display
        /// </summary>
        public string StatusText
        {
            get => _statusText;
            private set
            {
                _statusText = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Wskaźnik stanu ładowania
        /// Loading state indicator
        /// </summary>
        public bool IsLoading
        {
            get => _isLoading;
            private set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Czy pokazywać ukończone elementy
        /// Whether to show completed elements
        /// </summary>
        public bool ShowCompletedElements
        {
            get => _showCompletedElements;
            set
            {
                _showCompletedElements = value;
                OnPropertyChanged();
                _ = FilterElementsAsync();
            }
        }

        /// <summary>
        /// Czy element jest wybrany
        /// Whether an element is selected
        /// </summary>
        public bool IsElementSelected => SelectedElement != null;

        /// <summary>
        /// Całkowita liczba elementów
        /// Total number of elements
        /// </summary>
        public int TotalElements
        {
            get => _totalElements;
            private set
            {
                _totalElements = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CompletionPercentage));
            }
        }

        /// <summary>
        /// Liczba ukończonych elementów
        /// Number of completed elements
        /// </summary>
        public int CompletedElements
        {
            get => _completedElements;
            private set
            {
                _completedElements = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CompletionPercentage));
            }
        }

        /// <summary>
        /// Procent ukończenia
        /// Completion percentage
        /// </summary>
        public double CompletionPercentage => TotalElements > 0 ? (double)CompletedElements / TotalElements * 100 : 0;
        #endregion

        #region Komendy / Commands
        public ICommand AddElementCommand { get; }
        public ICommand EditElementCommand { get; }
        public ICommand DeleteElementCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand AssignMaterialCommand { get; }
        public ICommand ClearFiltersCommand { get; }
        public ICommand ExportElementsCommand { get; }
        #endregion

        #region Konstruktor / Constructor
        public ElementOutlinerViewModel(ElementOutlinerService elementOutlinerService, MaterialCatalogService materialCatalogService)
        {
            _elementOutlinerService = elementOutlinerService ?? throw new ArgumentNullException(nameof(elementOutlinerService));
            _materialCatalogService = materialCatalogService ?? throw new ArgumentNullException(nameof(materialCatalogService));
            _elements = new ObservableCollection<ElementInfo>();
            _availableMaterials = new ObservableCollection<Material>();

            // Inicjalizuj komendy / Initialize commands
            AddElementCommand = new RelayCommand(AddElement);
            EditElementCommand = new RelayCommand(EditElement, () => IsElementSelected);
            DeleteElementCommand = new RelayCommand(DeleteElement, () => IsElementSelected);
            RefreshCommand = new RelayCommand(async () => await LoadElementsAsync());
            AssignMaterialCommand = new RelayCommand(AssignMaterial, () => IsElementSelected);
            ClearFiltersCommand = new RelayCommand(ClearFilters);
            ExportElementsCommand = new RelayCommand(ExportElements);

            // Subskrybuj zmiany w serwisach / Subscribe to service changes
            _elementOutlinerService.ElementsChanged += OnElementsChanged;
        }
        #endregion

        #region Metody publiczne / Public Methods
        /// <summary>
        /// Inicjalizuje ViewModel z ładowaniem danych
        /// Initialize the ViewModel with data loading
        /// </summary>
        public async Task InitializeAsync()
        {
            try
            {
                IsLoading = true;
                StatusText = "Ładowanie elementów...";
                
                await Task.WhenAll(
                    LoadElementsAsync(),
                    LoadMaterialsAsync()
                );
                
                RhinoApp.WriteLine($"RhinoCNC: ElementOutlinerViewModel zainicjalizowany z {Elements.Count} elementami.");
            }
            catch (Exception ex)
            {
                StatusText = $"Błąd ładowania elementów: {ex.Message}";
                RhinoApp.WriteLine($"RhinoCNC: Błąd inicjalizacji ElementOutlinerViewModel: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }
        #endregion

        #region Metody prywatne / Private Methods
        private async Task LoadElementsAsync()
        {
            try
            {
                var allElements = await _elementOutlinerService.GetElementsAsync();
                
                Elements.Clear();
                foreach (var element in allElements)
                {
                    Elements.Add(element);
                }

                TotalElements = allElements.Count;
                CompletedElements = allElements.Count(e => e.Status == ElementStatus.Completed);

                await FilterElementsAsync();
                UpdateStatusText();
                RhinoApp.WriteLine($"RhinoCNC: Załadowano {Elements.Count} elementów do ViewModel.");
            }
            catch (Exception ex)
            {
                StatusText = $"Błąd ładowania elementów: {ex.Message}";
                RhinoApp.WriteLine($"RhinoCNC: Błąd ładowania elementów w ViewModel: {ex.Message}");
            }
        }

        private async Task LoadMaterialsAsync()
        {
            try
            {
                var materials = await _materialCatalogService.GetMaterialsAsync();
                
                AvailableMaterials.Clear();
                foreach (var material in materials)
                {
                    AvailableMaterials.Add(material);
                }

                RhinoApp.WriteLine($"RhinoCNC: Załadowano {AvailableMaterials.Count} materiałów do ViewModel.");
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine($"RhinoCNC: Błąd ładowania materiałów w ElementOutlinerViewModel: {ex.Message}");
            }
        }

        private async Task FilterElementsAsync()
        {
            if (_elementOutlinerService == null) return;

            try
            {
                var filteredElements = await _elementOutlinerService.GetFilteredElementsAsync(
                    searchText: SearchText,
                    status: StatusFilter,
                    priority: PriorityFilter,
                    includeCompleted: ShowCompletedElements
                );

                Elements.Clear();
                foreach (var element in filteredElements)
                {
                    Elements.Add(element);
                }

                UpdateStatusText();
            }
            catch (Exception ex)
            {
                StatusText = $"Błąd filtrowania elementów: {ex.Message}";
                RhinoApp.WriteLine($"RhinoCNC: Błąd filtrowania elementów: {ex.Message}");
            }
        }

        private void UpdateStatusText()
        {
            if (Elements == null)
            {
                StatusText = "Brak elementów";
                return;
            }

            var count = Elements.Count;
            StatusText = count switch
            {
                0 => "Brak elementów",
                1 => "1 element",
                _ when count < 5 => $"{count} elementy",
                _ => $"{count} elementów"
            };
        }

        private void OnElementsChanged(object sender, EventArgs e)
        {
            // Odśwież listę elementów asynchronicznie
            // Refresh elements list asynchronously
            _ = LoadElementsAsync();
        }

        private void AddElement()
        {
            try
            {
                // Tutaj można otworzyć dialog dodawania elementu
                // Here you can open element addition dialog
                RhinoApp.WriteLine("RhinoCNC: Dodawanie nowego elementu...");
                // TODO: Implementuj dialog dodawania elementu
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine($"RhinoCNC: Błąd dodawania elementu: {ex.Message}");
            }
        }

        private void EditElement()
        {
            if (SelectedElement == null) return;

            try
            {
                // Tutaj można otworzyć dialog edycji elementu
                // Here you can open element edit dialog
                RhinoApp.WriteLine($"RhinoCNC: Edycja elementu: {SelectedElement.Name}");
                // TODO: Implementuj dialog edycji elementu
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine($"RhinoCNC: Błąd edycji elementu: {ex.Message}");
            }
        }

        private async void DeleteElement()
        {
            if (SelectedElement == null) return;

            try
            {
                // Tutaj można dodać potwierdzenie usunięcia
                // Here you can add deletion confirmation
                await _elementOutlinerService.RemoveElementAsync(SelectedElement.Id);
                RhinoApp.WriteLine($"RhinoCNC: Usunięto element: {SelectedElement.Name}");
                SelectedElement = null;
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine($"RhinoCNC: Błąd usuwania elementu: {ex.Message}");
            }
        }

        private void AssignMaterial()
        {
            if (SelectedElement == null) return;

            try
            {
                // Tutaj można otworzyć dialog wyboru materiału
                // Here you can open material selection dialog
                RhinoApp.WriteLine($"RhinoCNC: Przypisywanie materiału do elementu: {SelectedElement.Name}");
                // TODO: Implementuj dialog wyboru materiału
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine($"RhinoCNC: Błąd przypisywania materiału: {ex.Message}");
            }
        }

        private void ClearFilters()
        {
            SearchText = string.Empty;
            StatusFilter = ElementStatus.Design;
            PriorityFilter = ElementPriority.Normal;
            ShowCompletedElements = true;
        }

        private void ExportElements()
        {
            try
            {
                // Tutaj można implementować eksport elementów
                // Here you can implement elements export
                RhinoApp.WriteLine("RhinoCNC: Eksportowanie elementów...");
                // TODO: Implementuj eksport elementów
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine($"RhinoCNC: Błąd eksportu elementów: {ex.Message}");
            }
        }

        /// <summary>
        /// Zwalnia zasoby używane przez ViewModel
        /// Releases resources used by the ViewModel
        /// </summary>
        protected override void OnDisposing()
        {
            // Odsubskrybuj eventy / Unsubscribe from events
            if (_elementOutlinerService != null)
            {
                _elementOutlinerService.ElementsChanged -= OnElementsChanged;
            }
            
            base.OnDisposing();
        }
        #endregion
    }
} 