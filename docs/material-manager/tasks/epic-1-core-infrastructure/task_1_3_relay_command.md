# Task 1.3: RelayCommand Implementation

**Epic**: 1 - Core Infrastructure  
**Priorytet**: Critical  
**Szacowany czas**: 1 dzień  
**Assignee**: TBD  
**Status**: Nie rozpoczęty

## Cel
Implementacja RelayCommand i AsyncRelayCommand dla WPF data binding z pełnym support dla CanExecute, async operations, error handling i proper disposal. Commands będą używane we wszystkich ViewModels dla user actions.

## Szczegółowy Opis

### Wymagania Funkcjonalne
1. **ICommand implementation** zgodna z WPF binding
2. **CanExecute support** z automatic UI updates
3. **Async command support** bez blocking UI
4. **Error handling** z graceful failure recovery
5. **Parameter passing** dla generic use cases
6. **Busy state integration** z BaseViewModel

### Wymagania Techniczne
- **.NET 8.0** compatible
- **Thread-safe** operations
- **Performance optimized** dla frequent executions
- **Memory efficient** event handling
- **Exception handling** z proper error reporting

## Interface Definition

```csharp
public interface IRelayCommand : ICommand
{
    void NotifyCanExecuteChanged();
}

public interface IAsyncRelayCommand : IRelayCommand
{
    Task ExecuteAsync(object parameter = null);
    bool IsRunning { get; }
}
```

## Implementation Plan

### 1. RelayCommand Class
```csharp
public class RelayCommand : IRelayCommand
{
    private readonly Action<object> _execute;
    private readonly Func<object, bool> _canExecute;
    private readonly WeakEventManager _weakEventManager = new WeakEventManager();

    public RelayCommand(Action execute, Func<bool> canExecute = null)
    public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
    
    public bool CanExecute(object parameter);
    public void Execute(object parameter);
    public void NotifyCanExecuteChanged();
    
    public event EventHandler CanExecuteChanged
    {
        add => _weakEventManager.AddEventHandler(value);
        remove => _weakEventManager.RemoveEventHandler(value);
    }
}
```

### 2. AsyncRelayCommand Class
```csharp
public class AsyncRelayCommand : IAsyncRelayCommand
{
    private readonly Func<object, Task> _execute;
    private readonly Func<object, bool> _canExecute;
    private readonly WeakEventManager _weakEventManager = new WeakEventManager();
    private volatile bool _isRunning;

    public AsyncRelayCommand(Func<Task> execute, Func<bool> canExecute = null)
    public AsyncRelayCommand(Func<object, Task> execute, Func<object, bool> canExecute = null)
    
    public bool IsRunning 
    { 
        get => _isRunning; 
        private set => SetIsRunning(value);
    }
    
    public async Task ExecuteAsync(object parameter = null);
    public bool CanExecute(object parameter);
    public async void Execute(object parameter); // ICommand interface
    public void NotifyCanExecuteChanged();
}
```

### 3. WeakEventManager for Memory Safety
```csharp
internal class WeakEventManager
{
    private readonly List<WeakReference> _handlers = new List<WeakReference>();
    private readonly object _lock = new object();
    
    public void AddEventHandler(EventHandler handler);
    public void RemoveEventHandler(EventHandler handler);
    public void RaiseEvent(object sender, EventArgs e);
    private void CleanupDeadReferences();
}
```

### 4. Error Handling Strategy
```csharp
public class CommandExecutionException : Exception
{
    public string CommandName { get; }
    public object Parameter { get; }
    
    public CommandExecutionException(string commandName, object parameter, Exception innerException)
        : base($"Command '{commandName}' failed to execute with parameter '{parameter}'", innerException)
    {
        CommandName = commandName;
        Parameter = parameter;
    }
}
```

## Kryteria Akceptacji

### Funkcjonalne
- [ ] ICommand interface jest poprawnie implementowany
- [ ] CanExecute changes automatycznie aktualizują UI
- [ ] AsyncRelayCommand nie blokuje UI thread
- [ ] Error handling nie crash'uje aplikacji
- [ ] Parameter passing działa dla obu typów commands
- [ ] IsRunning property jest thread-safe

### Techniczne
- [ ] Thread-safe CanExecuteChanged events
- [ ] Performance: Execute < 1ms overhead
- [ ] Memory: WeakEventManager zapobiega memory leaks
- [ ] Exception handling z proper error reporting
- [ ] Async operations używają ConfigureAwait(false)

### Testy
- [ ] Unit tests coverage > 95%
- [ ] CanExecute notification tests
- [ ] Async execution tests
- [ ] Error handling tests
- [ ] Memory leak tests z WeakEventManager
- [ ] Thread safety tests

## Pliki do Stworzenia

### Production Code
```
src/Commands/
├── IRelayCommand.cs
├── IAsyncRelayCommand.cs
├── RelayCommand.cs
├── AsyncRelayCommand.cs
├── WeakEventManager.cs
└── Exceptions/
    └── CommandExecutionException.cs
```

### Test Code
```
tests/Unit/Commands/
├── RelayCommandTests.cs
├── AsyncRelayCommandTests.cs
├── WeakEventManagerTests.cs
├── ParameterPassingTests.cs
└── ErrorHandlingTests.cs
```

## Przykład Użycia

```csharp
public class MaterialPaletteViewModel : BaseViewModel
{
    private RelayCommand _refreshCommand;
    private AsyncRelayCommand _loadMaterialsCommand;
    private RelayCommand<Material> _selectMaterialCommand;

    public ICommand RefreshCommand => _refreshCommand ??= 
        new RelayCommand(RefreshMaterials, CanRefresh);

    public IAsyncRelayCommand LoadMaterialsCommand => _loadMaterialsCommand ??= 
        new AsyncRelayCommand(LoadMaterialsAsync, () => !IsBusy);

    public RelayCommand<Material> SelectMaterialCommand => _selectMaterialCommand ??= 
        new RelayCommand<Material>(SelectMaterial, material => material != null);

    private void RefreshMaterials()
    {
        // Synchronous refresh logic
        NotifyPropertyChanged(nameof(Materials));
        _refreshCommand.NotifyCanExecuteChanged();
    }

    private async Task LoadMaterialsAsync()
    {
        try
        {
            SetBusyState(true);
            var materials = await _materialService.GetAllAsync();
            Materials.Clear();
            Materials.AddRange(materials);
        }
        finally
        {
            SetBusyState(false);
            _loadMaterialsCommand.NotifyCanExecuteChanged();
        }
    }

    private void SelectMaterial(Material material)
    {
        SelectedMaterial = material;
        // Trigger material operations
    }

    private bool CanRefresh() => !IsBusy && IsConnected;
}
```

## Generic Command Support

```csharp
public class RelayCommand<T> : IRelayCommand
{
    private readonly Action<T> _execute;
    private readonly Func<T, bool> _canExecute;

    public RelayCommand(Action<T> execute, Func<T, bool> canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public bool CanExecute(object parameter)
    {
        if (parameter is T typedParameter)
            return _canExecute?.Invoke(typedParameter) ?? true;
        return parameter == null && default(T) == null;
    }

    public void Execute(object parameter)
    {
        if (parameter is T typedParameter)
            _execute(typedParameter);
        else if (parameter == null && default(T) == null)
            _execute(default(T));
    }
}
```

## Zależności
- **Task 1.2**: BaseViewModel (dla busy state integration)

## Notatki Implementacyjne
1. **WeakEventManager**: Krityczne dla zapobiegania memory leaks w WPF
2. **ConfigureAwait(false)**: Wszystkie await calls w AsyncRelayCommand
3. **Thread Safety**: IsRunning property musi być volatile
4. **Error Handling**: Graceful handling bez crash'owania UI

## Definition of Done
- [ ] Kod zaimplementowany zgodnie ze specyfikacją
- [ ] Wszystkie testy jednostkowe przechodzą
- [ ] Performance benchmarks w akceptowalnych limitach
- [ ] Memory leak tests z WeakEventManager przechodzą
- [ ] Integration z WPF binding przetestowana
- [ ] Code review ukończone
- [ ] Documentation examples zweryfikowane 