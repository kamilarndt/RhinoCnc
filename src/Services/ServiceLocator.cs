using System;
using System.Collections.Generic;

namespace RhinoCncSuite.Services
{
    /// <summary>
    /// Service Locator pattern implementation for dependency injection
    /// Implementacja wzorca Service Locator dla dependency injection
    /// Umożliwia rejestrację i rozwiązywanie zależności w aplikacji
    /// Enables registration and resolution of dependencies in the application
    /// </summary>
    public sealed class ServiceLocator
    {
        private static readonly Lazy<ServiceLocator> _instance = new(() => new ServiceLocator());
        private readonly Dictionary<Type, object> _services = new();
        private readonly Dictionary<Type, Func<object>> _factories = new();
        private readonly object _lock = new();

        /// <summary>
        /// Singleton instance of ServiceLocator
        /// Instancja singleton ServiceLocator-a
        /// </summary>
        public static ServiceLocator Instance => _instance.Value;

        /// <summary>
        /// Prywatny konstruktor dla wzorca Singleton
        /// Private constructor for Singleton pattern
        /// </summary>
        private ServiceLocator()
        {
        }

        /// <summary>
        /// Rejestruje instancję serwisu w kontenerze
        /// Registers a service instance in the container
        /// </summary>
        /// <typeparam name="TInterface">Typ interfejsu / Interface type</typeparam>
        /// <param name="implementation">Implementacja serwisu / Service implementation</param>
        public void RegisterInstance<TInterface>(TInterface implementation) where TInterface : class
        {
            if (implementation == null)
                throw new ArgumentNullException(nameof(implementation));

            lock (_lock)
            {
                _services[typeof(TInterface)] = implementation;
            }
        }

        /// <summary>
        /// Rejestruje fabrykę serwisu w kontenerze
        /// Registers a service factory in the container
        /// </summary>
        /// <typeparam name="TInterface">Typ interfejsu / Interface type</typeparam>
        /// <param name="factory">Fabryka tworząca instancję / Factory creating instance</param>
        public void RegisterFactory<TInterface>(Func<TInterface> factory) where TInterface : class
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            lock (_lock)
            {
                _factories[typeof(TInterface)] = () => factory();
            }
        }

        /// <summary>
        /// Rejestruje typ jako singleton
        /// Registers a type as singleton
        /// </summary>
        /// <typeparam name="TInterface">Typ interfejsu / Interface type</typeparam>
        /// <typeparam name="TImplementation">Typ implementacji / Implementation type</typeparam>
        public void RegisterSingleton<TInterface, TImplementation>()
            where TInterface : class
            where TImplementation : class, TInterface, new()
        {
            lock (_lock)
            {
                _factories[typeof(TInterface)] = () =>
                {
                    if (_services.TryGetValue(typeof(TInterface), out var existing))
                        return existing;

                    var instance = new TImplementation();
                    _services[typeof(TInterface)] = instance;
                    return instance;
                };
            }
        }

        /// <summary>
        /// Rejestruje typ jako transient (nowa instancja za każdym razem)
        /// Registers a type as transient (new instance every time)
        /// </summary>
        /// <typeparam name="TInterface">Typ interfejsu / Interface type</typeparam>
        /// <typeparam name="TImplementation">Typ implementacji / Implementation type</typeparam>
        public void RegisterTransient<TInterface, TImplementation>()
            where TInterface : class
            where TImplementation : class, TInterface, new()
        {
            lock (_lock)
            {
                _factories[typeof(TInterface)] = () => new TImplementation();
            }
        }

        /// <summary>
        /// Rozwiązuje zależność i zwraca instancję serwisu
        /// Resolves dependency and returns service instance
        /// </summary>
        /// <typeparam name="TInterface">Typ interfejsu / Interface type</typeparam>
        /// <returns>Instancja serwisu / Service instance</returns>
        /// <exception cref="InvalidOperationException">Gdy serwis nie jest zarejestrowany / When service is not registered</exception>
        public TInterface Resolve<TInterface>() where TInterface : class
        {
            lock (_lock)
            {
                // Sprawdź czy istnieje gotowa instancja / Check if ready instance exists
                if (_services.TryGetValue(typeof(TInterface), out var instance))
                    return (TInterface)instance;

                // Sprawdź czy istnieje fabryka / Check if factory exists
                if (_factories.TryGetValue(typeof(TInterface), out var factory))
                    return (TInterface)factory();

                throw new InvalidOperationException($"Serwis typu {typeof(TInterface).Name} nie jest zarejestrowany / Service of type {typeof(TInterface).Name} is not registered");
            }
        }

        /// <summary>
        /// Sprawdza czy serwis jest zarejestrowany
        /// Checks if service is registered
        /// </summary>
        /// <typeparam name="TInterface">Typ interfejsu / Interface type</typeparam>
        /// <returns>True jeśli zarejestrowany / True if registered</returns>
        public bool IsRegistered<TInterface>() where TInterface : class
        {
            lock (_lock)
            {
                return _services.ContainsKey(typeof(TInterface)) || _factories.ContainsKey(typeof(TInterface));
            }
        }

        /// <summary>
        /// Wyrejestruje serwis z kontenera
        /// Unregisters service from container
        /// </summary>
        /// <typeparam name="TInterface">Typ interfejsu / Interface type</typeparam>
        public void Unregister<TInterface>() where TInterface : class
        {
            lock (_lock)
            {
                _services.Remove(typeof(TInterface));
                _factories.Remove(typeof(TInterface));
            }
        }

        /// <summary>
        /// Czyści wszystkie zarejestrowane serwisy
        /// Clears all registered services
        /// </summary>
        public void Clear()
        {
            lock (_lock)
            {
                _services.Clear();
                _factories.Clear();
            }
        }
    }
} 