# Task 1.1: Service Locator Implementation

**Epic**: 1 - Core Infrastructure  
**Priorytet**: Critical  
**Szacowany czas**: 1 dzień  
**Assignee**: TBD  
**Status**: Nie rozpoczęty

## Cel
Implementacja Service Locator pattern jako centralnego rejestru dla wszystkich usług w Material Manager. Service Locator będzie zarządzać czasem życia usług, zapewniać thread-safe access i obsługiwać dependency injection.

## Szczegółowy Opis

### Wymagania Funkcjonalne
1. **Rejestracja usług** z różnymi lifetime policies (Singleton, Transient, Scoped)
2. **Thread-safe access** do wszystkich operacji
3. **Lazy initialization** dla Singleton services
4. **Generic type resolution** z support dla interfejsów
5. **Error handling** z informacyjnymi komunikatami błędów

### Wymagania Techniczne
- **.NET 8.0** compatible
- **Thread-safe** operations (ConcurrentDictionary)
- **Memory efficient** (weak references gdzie możliwe)
- **Exception handling** z custom exceptions

## Interface Definition

```csharp
public interface IServiceLocator
{
    void RegisterSingleton<TInterface, TImplementation>() 
        where TImplementation : class, TInterface;
    
    void RegisterTransient<TInterface, TImplementation>() 
        where TImplementation : class, TInterface;
        
    void RegisterInstance<TInterface>(TInterface instance);
    
    TInterface Resolve<TInterface>();
    
    bool IsRegistered<TInterface>();
    
    void Clear();
}
```

## Implementation Plan

### 1. ServiceLifetime Enum
```csharp
public enum ServiceLifetime
{
    Singleton,
    Transient,
    Instance
}
```

### 2. ServiceDescriptor Class
```csharp
internal class ServiceDescriptor
{
    public Type ServiceType { get; set; }
    public Type ImplementationType { get; set; }
    public ServiceLifetime Lifetime { get; set; }
    public object Instance { get; set; }
    public Func<object> Factory { get; set; }
}
```

### 3. Main ServiceLocator Class
- **Thread-safe collections** (ConcurrentDictionary)
- **Lazy<T> pattern** dla Singleton services
- **Factory pattern** dla Transient services
- **Custom exceptions** (ServiceNotRegisteredException)

## Kryteria Akceptacji

### Funkcjonalne
- [ ] Rejestracja Singleton services działa poprawnie
- [ ] Rejestracja Transient services działa poprawnie  
- [ ] Rejestracja Instance services działa poprawnie
- [ ] Resolve zwraca poprawne instancje
- [ ] IsRegistered zwraca poprawne wartości
- [ ] Clear usuwa wszystkie rejestracje

### Techniczne
- [ ] Thread-safe operations (przetestowane z multiple threads)
- [ ] Memory leaks nie występują (przetestowane z profiler)
- [ ] Performance jest akceptowalne (< 1ms per resolve)
- [ ] Exception handling działa poprawnie

### Testy
- [ ] Unit tests coverage > 95%
- [ ] Integration tests z MaterialCatalogService
- [ ] Thread safety tests
- [ ] Memory leak tests

## Pliki do Stworzenia

### Production Code
```
src/Services/
├── IServiceLocator.cs
├── ServiceLocator.cs
├── ServiceDescriptor.cs
├── ServiceLifetime.cs
└── Exceptions/
    └── ServiceNotRegisteredException.cs
```

### Test Code
```
tests/Unit/Services/
├── ServiceLocatorTests.cs
├── ServiceDescriptorTests.cs
└── Integration/
    └── ServiceLocatorIntegrationTests.cs
```

## Przykład Użycia

```csharp
// Startup/initialization
var serviceLocator = new ServiceLocator();

// Rejestracja usług
serviceLocator.RegisterSingleton<IMaterialCatalogService, MaterialCatalogService>();
serviceLocator.RegisterTransient<IMaterialValidator, MaterialValidator>();

// Użycie w ViewModels
public class MaterialPaletteViewModel : BaseViewModel
{
    private readonly IMaterialCatalogService _materialService;
    
    public MaterialPaletteViewModel()
    {
        _materialService = ServiceLocator.Instance.Resolve<IMaterialCatalogService>();
    }
}
```

## Zależności
- Brak (Base infrastructure)

## Notatki Implementacyjne
1. **Singleton Thread Safety**: Użyć Lazy<T> pattern zamiast double-checked locking
2. **Memory Management**: Ważne żeby Transient services nie były cache'owane
3. **Error Messages**: Informacyjne wiadomości błędów z nazwami typów
4. **Performance**: Optymalizacja dla częstego resolve wywołania

## Definition of Done
- [ ] Kod zaimplementowany i przetestowany
- [ ] Code review przeprowadzone  
- [ ] Integration tests przechodzą
- [ ] Documentation zaktualizowana
- [ ] Performance benchmarks w akceptowalnych limitach 