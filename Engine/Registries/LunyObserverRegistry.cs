using Luny.Attributes;
using Luny.Engine.Identity;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Luny.Engine.Registries
{
	/// <summary>
	/// Lifecycle observer interface - receives callbacks from LunyEngine.
	/// </summary>
	public interface ILunyEngineObserver
	{
		public Boolean Enabled => true;

		void OnStartup();
		void OnFixedStep(Double fixedDeltaTime);
		void OnUpdate(Double deltaTime);
		void OnLateUpdate(Double deltaTime);
		void OnShutdown();
	}

	public interface ILunyObserverRegistry {}

	/// <summary>
	/// Registry that discovers, manages, and enables/disables lifecycle observers.
	/// </summary>
	internal sealed class LunyObserverRegistry : ILunyObserverRegistry
	{
		private readonly Dictionary<Type, ILunyEngineObserver> _registeredObservers = new();
		private readonly List<ILunyEngineObserver> _enabledObservers = new();

		public IEnumerable<ILunyEngineObserver> EnabledObservers => _enabledObservers;

		public LunyObserverRegistry(Boolean isSmokeTestScene) => DiscoverAndInstantiateObservers(isSmokeTestScene);

		~LunyObserverRegistry() => LunyLogger.LogInfo($"finalized {GetHashCode()}", this);

		private void DiscoverAndInstantiateObservers(Boolean isSmokeTestScene)
		{
			var sw = Stopwatch.StartNew();

			var observerTypes = TypeDiscovery.FindAll<ILunyEngineObserver>();

			// TODO: sort observers deterministically
			// TODO: configure observer enabled states

			foreach (var type in observerTypes)
			{
				// Skip [LunyTestable] types unless in smoke test scenes
				if (!isSmokeTestScene && type.HasAttribute<LunyTestableAttribute>())
					continue;

				LunyLogger.LogInfo($"{type.FullName} registered", this);
				var observer = (ILunyEngineObserver)Activator.CreateInstance(type);
				_registeredObservers[type] = observer;

				if (observer.Enabled)
					_enabledObservers.Add(observer);
			}

			sw.Stop();

			var ms = (Int32)Math.Round(sw.Elapsed.TotalMilliseconds, MidpointRounding.AwayFromZero);
			LunyLogger.LogInfo($"Registered {_registeredObservers.Count} (enabled: {_enabledObservers.Count}) " +
			                   $"{nameof(ILunyEngineObserver)}s in {ms} ms.", this);
		}

		public Boolean IsObserverEnabled<T>() where T : ILunyEngineObserver =>
			TryGetObserver<T>(out var observer) && _enabledObservers.Contains(observer);

		public void EnableObserver<T>() where T : ILunyEngineObserver
		{
			if (TryGetObserver<T>(out var observer))
			{
				if (!_enabledObservers.Contains(observer))
					_enabledObservers.Add(observer);
			}
		}

		public void DisableObserver<T>() where T : ILunyEngineObserver
		{
			if (TryGetObserver<T>(out var observer))
				_enabledObservers.Remove(observer);
		}

		public T GetObserver<T>() where T : ILunyEngineObserver => TryGetObserver(out T observer) ? observer : default;

		private Boolean TryGetObserver<T>(out T observer) where T : ILunyEngineObserver =>
			(observer = _registeredObservers.TryGetValue(typeof(T), out var o) ? (T)o : default) is not null;
	}
}
