using Luny.Engine.Identity;
using Luny.Exceptions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Luny.Engine.Registries
{
	/// <summary>
	/// Marker interface for engine-agnostic services: APIs such as Debug, Input, etc.
	/// Implementations are auto-discovered and registered at startup.
	/// </summary>
	public interface ILunyEngineService {}

	public interface ILunyServiceRegistry {}

	/// <summary>
	/// Generic service registry that discovers and holds engine services.
	/// </summary>
	/// <typeparam name="T">Service interface type that must implement IEngineProvider</typeparam>
	internal sealed class LunyServiceRegistry<T> : ILunyServiceRegistry where T : class, ILunyEngineService
	{
		private readonly Dictionary<Type, T> _registeredServices = new();

		private static Type GetServiceInterface(Type implementationType)
		{
			// Get all interfaces that derive from IEngineServiceProvider (but are not IEngineServiceProvider itself)
			var serviceInterfaces = implementationType.GetInterfaces()
				.Where(i => i != typeof(T) && typeof(T).IsAssignableFrom(i))
				.ToArray();

			// Must implement exactly one specific service interface
			if (serviceInterfaces.Length != 1)
			{
				if (serviceInterfaces.Length == 0)
					LunyThrow.ServiceMustImplementSpecificInterfaceException(implementationType.Name);

				LunyThrow.ServiceImplementsMultipleInterfacesException(implementationType.Name,
					String.Join(", ", serviceInterfaces.Select(i => i.Name)));
			}

			return serviceInterfaces[0];
		}

		internal LunyServiceRegistry() => DiscoverAndInstantiateServices();
		~LunyServiceRegistry() => LunyLogger.LogInfo($"finalized {GetHashCode()}", this);

		private void DiscoverAndInstantiateServices()
		{
			var sw = Stopwatch.StartNew();

			var serviceTypes = TypeDiscovery.FindAll<T>();

			foreach (var type in serviceTypes)
			{
				// Find the specific service interface (not IEngineServiceProvider directly)
				var serviceInterface = GetServiceInterface(type);

				LunyLogger.LogInfo($"{serviceInterface.Name} => {type.FullName} registered", this);
				var service = (T)Activator.CreateInstance(type);
				_registeredServices[serviceInterface] = service;
			}

			sw.Stop();

			var ms = (Int32)Math.Round(sw.Elapsed.TotalMilliseconds, MidpointRounding.AwayFromZero);
			LunyLogger.LogInfo($"Registered {_registeredServices.Count} {typeof(T).Name} instances in {ms} ms.", this);
		}

		internal TService Get<TService>() where TService : class, T, ILunyEngineService =>
			_registeredServices.TryGetValue(typeof(TService), out var service)
				? (TService)service
				: throw new LunyServiceException($"Required service {typeof(TService).FullName} not registered.");

		internal Boolean TryGet<TService>(out TService service) where TService : class, T, ILunyEngineService
		{
			if (_registeredServices.TryGetValue(typeof(TService), out var registeredService))
			{
				service = (TService)registeredService;
				return true;
			}

			service = null;
			return false;
		}

		internal Boolean Has<TService>() where TService : class, T, ILunyEngineService => _registeredServices.ContainsKey(typeof(TService));
	}
}
