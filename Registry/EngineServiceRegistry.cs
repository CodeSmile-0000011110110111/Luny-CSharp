using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Luny.Reflection;

namespace Luny
{
    /// <summary>
    /// Generic service registry that discovers and holds engine provider services.
    /// </summary>
    /// <typeparam name="T">Service interface type that must implement IEngineProvider</typeparam>
    public sealed class EngineServiceRegistry<T> where T : class, IEngineServiceProvider
    {
        private readonly Dictionary<Type, T> _registeredServices = new();

        public EngineServiceRegistry() => DiscoverAndInstantiateServices();

        private void DiscoverAndInstantiateServices()
        {
            var sw = Stopwatch.StartNew();

            var serviceTypes = TypeDiscovery.FindAll<T>();

            foreach (var type in serviceTypes)
            {
                // Find the specific service interface (not IEngineServiceProvider directly)
                var serviceInterface = GetServiceInterface(type);

                LunyLogger.LogInfo($"Registering service: {type.Name} as {serviceInterface.Name} (Assembly: {type.Assembly.GetName().Name})", this);
                var service = (T)Activator.CreateInstance(type);
                _registeredServices[serviceInterface] = service;
            }

            sw.Stop();

            var ms = (Int32)Math.Round(sw.Elapsed.TotalMilliseconds, MidpointRounding.AwayFromZero);
            LunyLogger.LogInfo($"Registered {_registeredServices.Count} {typeof(T).Name} services in {ms} ms.", this);
        }

        private static Type GetServiceInterface(Type implementationType)
        {
            // Get all interfaces that derive from IEngineServiceProvider (but are not IEngineServiceProvider itself)
            var serviceInterfaces = implementationType.GetInterfaces()
                .Where(i => i != typeof(T) && typeof(T).IsAssignableFrom(i))
                .ToArray();

            // Must implement exactly one specific service interface
            if (serviceInterfaces.Length == 0)
                LunyThrow.ServiceMustImplementSpecificInterfaceException(implementationType.Name);

            if (serviceInterfaces.Length > 1)
            {
                // Check if any interface directly inherits from IEngineServiceProvider
                var directInterfaces = serviceInterfaces
                    .Where(i => i.GetInterfaces().Contains(typeof(T)))
                    .ToArray();

                if (directInterfaces.Length != 1)
                {
                    var interfaceNames = String.Join(", ", serviceInterfaces.Select(i => i.Name));
                    LunyThrow.ServiceImplementsMultipleInterfacesException(implementationType.Name, interfaceNames);
                }

                return directInterfaces[0];
            }

            return serviceInterfaces[0];
        }

        public TService Get<TService>() where TService : class, T, IEngineServiceProvider
        {
            if (_registeredServices.TryGetValue(typeof(TService), out var service))
                return service as TService;

            LunyThrow.ServiceNotFoundException(typeof(TService).FullName);
            return null;
        }

        public Boolean Has<TService>() where TService : class, T, IEngineServiceProvider =>
            _registeredServices.ContainsKey(typeof(TService));
    }
}
