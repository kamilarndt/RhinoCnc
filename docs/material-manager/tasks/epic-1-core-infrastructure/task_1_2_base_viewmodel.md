# Task 1.2: BaseViewModel Implementation

**Epic**: 1 - Core Infrastructure  
**Priorytet**: Critical  
**Szacowany czas**: 1 dzień  
**Assignee**: TBD  
**Status**: Nie rozpoczęty

## Cel
Stworzenie BaseViewModel klasy implementującej INotifyPropertyChanged pattern z dodatkowymi utility methods dla property validation, error handling i async operations. Wszystkie ViewModels w Material Manager będą dziedziczyć z tej klasy.

## Szczegółowy Opis

### Wymagania Funkcjonalne
1. **INotifyPropertyChanged** implementation z SetProperty helper
2. **Property validation** z DataAnnotations support
3. **Error collection** dla property-level errors
4. **Busy state management** dla async operations
5. **Disposal pattern** dla proper cleanup
6. **Command property helpers** dla RelayCommand binding

### Wymagania Techniczne
- **.NET 8.0** compatible
- **Thread-safe** property change notifications
- **Performance optimized** SetProperty method
- **Memory efficient** event handling
- **Exception handling** z graceful error reporting

## Interface Definition

```csharp
public abstract class BaseViewModel : INotifyPropertyChanged, IDisposable
{
    public event PropertyChangedEventHandler PropertyChanged;
    
    // Property management
    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null);
    protected bool SetProperty<T>(ref T field, T value, Action onChanged, [CallerMemberName] string propertyName = null);
    
    // Validation
    protected bool ValidateProperty<T>(T value, [CallerMemberName] string propertyName = null);
    protected void ClearErrors([CallerMemberName] string propertyName = null);
    
    // Error handling
    public bool HasErrors { get; }
    public IEnumerable<string> GetErrors(string propertyName);
    
    // Busy state
    public bool IsBusy { get; protected set; }
    protected void SetBusyState(bool isBusy);
    
    // Async operations
    protected async Task ExecuteAsync(Func<Task> operation, [CallerMemberName] string operationName = null);
    protected async Task<T> ExecuteAsync<T>(Func<Task<T>> operation, [CallerMemberName] string operationName = null);
}
```

## Implementation Plan

### 1. Core Property Change Implementation
```csharp
private readonly object _lockObject = new object();
private PropertyChangedEventHandler _propertyChanged;

public event PropertyChangedEventHandler PropertyChanged
{
    add
    {
        lock (_lockObject)
        {
            _propertyChanged += value;
        }
    }
    remove
    {
        lock (_lockObject)
        {
            _propertyChanged -= value;
        }
    }
}

protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
{
    _propertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
```

### 2. SetProperty Helper Method
```csharp
protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
{
    if (EqualityComparer<T>.Default.Equals(field, value))
        return false;
        
    field = value;
    OnPropertyChanged(propertyName);
    return true;
}
```

### 3. Validation Framework
```csharp
private readonly Dictionary<string, List<string>> _errors = new Dictionary<string, List<string>>();

protected bool ValidateProperty<T>(T value, [CallerMemberName] string propertyName = null)
{
    var context = new ValidationContext(this) { MemberName = propertyName };
    var validationResults = new List<ValidationResult>();
    
    bool isValid = Validator.TryValidateProperty(value, context, validationResults);
    
    if (isValid)
    {
        ClearErrors(propertyName);
    }
    else
    {
        _errors[propertyName] = validationResults.Select(r => r.ErrorMessage).ToList();
        OnPropertyChanged(nameof(HasErrors));
    }
    
    return isValid;
}
```

### 4. Async Operations Support
```csharp
protected async Task ExecuteAsync(Func<Task> operation, [CallerMemberName] string operationName = null)
{
    try
    {
        SetBusyState(true);
        await operation();
    }
    catch (Exception ex)
    {
        HandleError(ex, operationName);
    }
    finally
    {
        SetBusyState(false);
    }
}
```

## Kryteria Akceptacji

### Funkcjonalne
- [ ] INotifyPropertyChanged events są wysyłane poprawnie
- [ ] SetProperty porównuje wartości i wywołuje events tylko gdy potrzeba
- [ ] Property validation działa z DataAnnotations
- [ ] Error collection jest thread-safe
- [ ] Busy state management działa z async operations
- [ ] Disposal pattern czyści wszystkie resources

### Techniczne
- [ ] Thread-safe property change events
- [ ] Performance: SetProperty < 0.1ms execution time
- [ ] Memory: No memory leaks przy frequent property changes
- [ ] Exception handling nie crash'uje aplikacji

### Testy
- [ ] Unit tests coverage > 95%
- [ ] Property change notification tests
- [ ] Validation tests z różnymi scenarios
- [ ] Thread safety tests
- [ ] Memory leak tests
- [ ] Disposal tests

## Pliki do Stworzenia

### Production Code
```
src/ViewModels/
├── BaseViewModel.cs
├── Interfaces/
│   └── IValidatableViewModel.cs
└── Exceptions/
    └── ViewModelException.cs
```

### Test Code
```
tests/Unit/ViewModels/
├── BaseViewModelTests.cs
├── PropertyValidationTests.cs
├── AsyncOperationTests.cs
└── ThreadSafetyTests.cs
```

## Przykład Użycia

```csharp
public class MaterialPaletteViewModel : BaseViewModel
{
    private string _searchText;
    private bool _isLoading;
    private ObservableCollection<Material> _materials;

    [Required]
    [StringLength(100)]
    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
            {
                ValidateProperty(value);
                _ = SearchMaterialsAsync();
            }
        }
    }

    public bool IsLoading
    {
        get => _isLoading;
        private set => SetProperty(ref _isLoading, value);
    }

    private async Task SearchMaterialsAsync()
    {
        await ExecuteAsync(async () =>
        {
            var results = await _materialService.SearchAsync(SearchText);
            Materials.Clear();
            foreach (var material in results)
            {
                Materials.Add(material);
            }
        });
    }
}
```

## Zależności
- **Task 1.1**: Service Locator (dla error logging service)

## Notatki Implementacyjne
1. **Performance**: SetProperty musi być bardzo szybkie bo będzie używane często
2. **Thread Safety**: Property change events muszą być thread-safe
3. **Memory**: Ważne żeby event handlers nie powodowały memory leaks
4. **Validation**: DataAnnotations integration dla spójności z WPF

## Definition of Done
- [ ] Kod zaimplementowany zgodnie ze specyfikacją
- [ ] Wszystkie testy jednostkowe przechodzą
- [ ] Performance benchmarks w akceptowalnych limitach
- [ ] Memory leak tests przechodzą
- [ ] Code review ukończone
- [ ] Integration z MaterialPaletteViewModel przetestowana 