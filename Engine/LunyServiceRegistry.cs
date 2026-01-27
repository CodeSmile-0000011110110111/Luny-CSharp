using Luny.Engine.Identity;
using Luny.Exceptions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Luny.Engine
{
	public interface ILunyServiceRegistry {}

	public interface ILunyServiceRegistryInternal
	{
		static void ThrowDoesNotImplementServiceInterface(Type serviceType) => throw new LunyServiceException(
			$"{serviceType?.Name} must implement an interface derived from {nameof(ILunyEngineService)}.");

		static void ThrowImplementsMultipleServiceInterfaces(Type serviceType, Type[] interfaces) => throw new LunyServiceException(
			$"{serviceType?.Name} implements more than one {nameof(ILunyEngineService)}-derived interfaces: " +
			String.Join(", ", interfaces?.Select(i => i.Name)));

		void RegisterService(LunyEngineServiceBase service);
	}

	/// <summary>
	/// Generic service registry that discovers and holds engine services.
	/// </summary>
	/// <typeparam name="T">Service interface type that must implement IEngineProvider</typeparam>
	internal sealed class LunyServiceRegistry : ILunyServiceRegistry, ILunyServiceRegistryInternal
	{
		private readonly Dictionary<Type, LunyEngineServiceBase> _registeredServices = new();

		private static Type GetServiceInterface(Type implementationType)
		{
			// Get all interfaces that derive from IEngineServiceProvider (but are not IEngineServiceProvider itself)
			var serviceInterfaces = implementationType.GetInterfaces()
				.Where(i => i != typeof(ILunyEngineService) && typeof(ILunyEngineService).IsAssignableFrom(i))
				.ToArray();

			// Must implement exactly one specific service interface
			if (serviceInterfaces.Length == 0)
				ILunyServiceRegistryInternal.ThrowDoesNotImplementServiceInterface(implementationType);
			if (serviceInterfaces.Length >= 2)
				ILunyServiceRegistryInternal.ThrowImplementsMultipleServiceInterfaces(implementationType, serviceInterfaces);

			return serviceInterfaces[0];
		}

		internal LunyServiceRegistry(NativeEngine engine)
		{
			DiscoverAndInstantiateServices(engine);
			InitializeServices();
		}

		void ILunyServiceRegistryInternal.RegisterService(LunyEngineServiceBase service)
		{
			var baseType = service.GetType().BaseType;
			if (baseType == null || baseType == typeof(LunyEngineServiceBase))
			{
				throw new LunyServiceException(
					$"Service {service.GetType().Name} does not inherit from service-specific subclass of {nameof(LunyEngineServiceBase)}");
			}

			_registeredServices[baseType] = service;
		}

		~LunyServiceRegistry() => LunyTraceLogger.LogInfoFinalized(this);

		private void InitializeServices()
		{
			foreach (var service in _registeredServices.Values)
				service.Initialize();
		}

		internal void Startup()
		{
			foreach (var service in _registeredServices.Values)
				service.Startup();
		}

		internal void Shutdown()
		{
			foreach (var service in _registeredServices.Values)
				service.Shutdown();
		}

		internal void OnEnginePreUpdate()
		{
			foreach (var service in _registeredServices.Values)
				service.PreUpdate();
		}

		internal void OnEnginePostUpdate()
		{
			foreach (var service in _registeredServices.Values)
				service.PostUpdate();
		}

		private void DiscoverAndInstantiateServices(NativeEngine engine)
		{
			var sw = Stopwatch.StartNew();

			var engineName = engine.ToString();
			var serviceTypes = LunyTypeDiscovery.FindAll<ILunyEngineService>();

			foreach (var type in serviceTypes)
			{
				// Filter services by engine (they must follow naming convention)
				if (!type.Name.StartsWith(engineName))
				{
					LunyLogger.LogWarning($"Service '{type.Name}' does not start with '{engineName}' => ignored", this);
					continue;
				}

				// Find the specific service interface (not IEngineServiceProvider directly)
				var serviceInterface = GetServiceInterface(type);

				LunyLogger.LogInfo($"{serviceInterface.Name} => {type.FullName} registered", this);
				var service = Activator.CreateInstance(type) as LunyEngineServiceBase;
				if (service == null)
					throw new LunyServiceException($"{serviceInterface} does not inherit from {nameof(LunyEngineServiceBase)}");

				var baseType = service.GetType().BaseType;
				if (baseType == null || baseType == typeof(LunyEngineServiceBase))
				{
					throw new LunyServiceException($"{serviceInterface} implemented by {service.GetType().Name} does not inherit from " +
					                               $"service-specific subclass of {nameof(LunyEngineServiceBase)}. Actual base class is: " +
					                               $"{baseType?.Name}");
				}

				_registeredServices[baseType] = service;
			}

			sw.Stop();

			var ms = (Int32)Math.Round(sw.Elapsed.TotalMilliseconds, MidpointRounding.AwayFromZero);
			LunyLogger.LogInfo($"Registered {_registeredServices.Count} {nameof(LunyEngineServiceBase)} instances in {ms} ms.", this);
		}

		internal TService Get<TService>() where TService : LunyEngineServiceBase =>
			_registeredServices.TryGetValue(typeof(TService), out var service)
				? (TService)service
				: throw new LunyServiceException($"Requested service {typeof(TService).FullName} not registered.");

		internal Boolean TryGet<TService>(out TService service) where TService : LunyEngineServiceBase
		{
			if (_registeredServices.TryGetValue(typeof(TService), out var registeredService))
			{
				service = (TService)registeredService;
				return true;
			}

			service = null;
			return false;
		}

		internal Boolean Has<TService>() where TService : LunyEngineServiceBase => _registeredServices.ContainsKey(typeof(TService));
	}
}
