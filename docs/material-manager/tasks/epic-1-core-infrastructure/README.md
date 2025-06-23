# Epic 1: Core Infrastructure

**Priorytet**: Critical  
**Szacowany czas**: 3-4 dni  
**Status**: Nie rozpoczęty

## Cel
Stworzenie podstawowej infrastruktury MVVM dla Material Manager z Service Locator pattern, BaseViewModel z property change notifications oraz implementacją RelayCommand dla WPF data binding.

## Zadania

### Task 1.1: Service Locator Implementation
**Priorytet**: Critical  
**Szacowany czas**: 1 dzień  
**Plik**: `task_1_1_service_locator.md`

Implementacja Service Locator pattern z thread-safe access, lifetime management i error handling.

### Task 1.2: BaseViewModel Implementation  
**Priorytet**: Critical  
**Szacowany czas**: 1 dzień  
**Plik**: `task_1_2_base_viewmodel.md`

Stworzenie BaseViewModel z INotifyPropertyChanged, property validation, error handling i utility methods.

### Task 1.3: RelayCommand Implementation
**Priorytet**: Critical  
**Szacowany czas**: 1 dzień  
**Plik**: `task_1_3_relay_command.md`

Implementacja RelayCommand dla WPF data binding z support dla async operations i CanExecute.

## Zależności
- Brak (Epic 1 to podstawa dla wszystkich innych)

## Kryteria Ukończenia
- [ ] Service Locator poprawnie zarządza usługami
- [ ] BaseViewModel obsługuje property changes
- [ ] RelayCommand działa z WPF binding
- [ ] Wszystkie testy jednostkowe przechodzą
- [ ] Kod coverage > 90%

## Pliki do stworzenia
- `src/Services/ServiceLocator.cs`
- `src/ViewModels/BaseViewModel.cs`  
- `src/Commands/RelayCommand.cs`
- Odpowiednie testy jednostkowe

## Notatki
Epic 1 jest fundamentem MVVM architecture. Wszystkie późniejsze ViewModels będą dziedziczyć z BaseViewModel, a wszystkie commands będą używać RelayCommand. 