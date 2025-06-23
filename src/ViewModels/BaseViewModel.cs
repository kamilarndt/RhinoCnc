using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RhinoCncSuite.ViewModels
{
    /// <summary>
    /// Bazowa klasa ViewModel implementująca INotifyPropertyChanged
    /// Base ViewModel class implementing INotifyPropertyChanged
    /// Zapewnia wspólną funkcjonalność dla wszystkich ViewModels
    /// Provides common functionality for all ViewModels
    /// </summary>
    public abstract class BaseViewModel : INotifyPropertyChanged, IDisposable
    {
        private bool _isDisposed = false;

        /// <summary>
        /// Zdarzenie powiadamiające o zmianie właściwości
        /// Event notifying about property changes
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Wywołuje zdarzenie PropertyChanged dla określonej właściwości
        /// Raises the PropertyChanged event for a specific property
        /// </summary>
        /// <param name="propertyName">Nazwa właściwości / Property name</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Ustawia wartość właściwości i wywołuje PropertyChanged jeśli wartość się zmieniła
        /// Sets property value and raises PropertyChanged if value changed
        /// </summary>
        /// <typeparam name="T">Typ właściwości / Property type</typeparam>
        /// <param name="field">Pole przechowujące wartość / Field storing the value</param>
        /// <param name="value">Nowa wartość / New value</param>
        /// <param name="propertyName">Nazwa właściwości / Property name</param>
        /// <returns>True jeśli wartość się zmieniła / True if value changed</returns>
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// Sprawdza czy obiekt został już zwolniony
        /// Checks if the object has been disposed
        /// </summary>
        protected void ThrowIfDisposed()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().Name);
        }

        /// <summary>
        /// Zwalnia zasoby używane przez ViewModel
        /// Releases resources used by the ViewModel
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Zwalnia zasoby używane przez ViewModel
        /// Releases resources used by the ViewModel
        /// </summary>
        /// <param name="disposing">True jeśli wywołano z Dispose() / True if called from Dispose()</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed && disposing)
            {
                // Zwolnij zarządzane zasoby / Release managed resources
                OnDisposing();
                _isDisposed = true;
            }
        }

        /// <summary>
        /// Metoda wywoływana podczas zwalniania zasobów
        /// Method called during resource disposal
        /// Klasy pochodne mogą ją przesłonić aby zwolnić swoje zasoby
        /// Derived classes can override this to release their resources
        /// </summary>
        protected virtual void OnDisposing()
        {
            // Implementacja w klasach pochodnych / Implementation in derived classes
        }

        /// <summary>
        /// Destruktor
        /// Destructor
        /// </summary>
        ~BaseViewModel()
        {
            Dispose(false);
        }
    }
} 