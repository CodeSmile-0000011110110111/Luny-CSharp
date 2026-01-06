using Luny.Engine.Identity;
using Luny.Engine.Services;
using Luny.Exceptions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Luny.Engine
{
	public interface ILunyServiceRegistry {}

	internal interface ILunyServiceRegistryInternal
	{
		static void ThrowDoesNotImplementServiceInterface(Type serviceType) => throw new LunyServiceException(
			$"{serviceType?.Name} must implement an interface derived from {nameof(ILunyEngineService)}.");

		static void ThrowImplementsMultipleServiceInterfaces(Type serviceType, Type[] interfaces) => throw new LunyServiceException(
			$"{serviceType?.Name} implements more than one {nameof(ILunyEngineService)}-derived interfaces: " +
			String.Join(", ", interfaces?.Select(i => i.Name)));
	}

	/// <summary>
	/// Generic service registry that discovers and holds engine services.
	/// </summary>
	/// <typeparam name="T">Service interface type that must implement IEngineProvider</typeparam>
	internal sealed class LunyServiceRegistry<T> : ILunyServiceRegistry, ILunyServiceRegistryInternal where T : class, ILunyEngineService
	{
		private readonly Dictionary<Type, T> _registeredServices = new();

		private static Type GetServiceInterface(Type implementationType)
		{
			// Get all interfaces that derive from IEngineServiceProvider (but are not IEngineServiceProvider itself)
			var serviceInterfaces = implementationType.GetInterfaces()
				.Where(i => i != typeof(T) && typeof(T).IsAssignableFrom(i))
				.ToArray();

			// Must implement exactly one specific service interface
			if (serviceInterfaces.Length == 0)
				ILunyServiceRegistryInternal.ThrowDoesNotImplementServiceInterface(implementationType);
			if (serviceInterfaces.Length >= 2)
				ILunyServiceRegistryInternal.ThrowImplementsMultipleServiceInterfaces(implementationType, serviceInterfaces);

			return serviceInterfaces[0];
		}

		internal LunyServiceRegistry() => DiscoverAndInstantiateServices();
		~LunyServiceRegistry() => LunyTraceLogger.LogInfoFinalized(this);

		private void DiscoverAndInstantiateServices()
		{
			var sw = Stopwatch.StartNew();

			var serviceTypes = LunyTypeDiscovery.FindAll<T>();

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
