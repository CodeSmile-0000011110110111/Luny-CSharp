using Luny.Attributes;
using Luny.Diagnostics;
using Luny.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Luny.Registries
{
	/// <summary>
	/// Registry that discovers, manages, and enables/disables lifecycle observers.
	/// </summary>
	internal sealed class EngineObserverRegistry
	{
		private readonly Dictionary<Type, IEngineObserver> _registeredObservers = new();
		private readonly List<IEngineObserver> _enabledObservers = new();

		public IEnumerable<IEngineObserver> EnabledObservers => _enabledObservers;

		public EngineObserverRegistry(Boolean isSmokeTestScene)
		{
			DiscoverAndInstantiateObservers(isSmokeTestScene);
		}

		private void DiscoverAndInstantiateObservers(Boolean isSmokeTestScene)
		{
			var sw = Stopwatch.StartNew();

			var observerTypes = TypeDiscovery.FindAll<IEngineObserver>();

			// TODO: sort observers deterministically
			// TODO: configure observer enabled states

			foreach (var type in observerTypes)
			{
				// Skip [LunyTestable] types unless in smoke test scenes
				if (!isSmokeTestScene && type.HasAttribute<LunyTestableAttribute>())
					continue;

				LunyLogger.LogInfo($"{type.FullName} registered", this);
				var observer = (IEngineObserver)Activator.CreateInstance(type);
				_registeredObservers[type] = observer;

				if (observer.Enabled)
					_enabledObservers.Add(observer);
			}

			sw.Stop();

			var ms = (Int32)Math.Round(sw.Elapsed.TotalMilliseconds, MidpointRounding.AwayFromZero);
			LunyLogger.LogInfo($"Registered {_registeredObservers.Count} (enabled: {_enabledObservers.Count}) " +
			                   $"{nameof(IEngineObserver)}s in {ms} ms.", this);
		}

		public Boolean IsObserverEnabled<T>() where T : IEngineObserver =>
			TryGetObserver<T>(out var observer) && _enabledObservers.Contains(observer);

		public void EnableObserver<T>() where T : IEngineObserver
		{
			if (TryGetObserver<T>(out var observer))
			{
				if (!_enabledObservers.Contains(observer))
					_enabledObservers.Add(observer);
			}
		}

		public void DisableObserver<T>() where T : IEngineObserver
		{
			if (TryGetObserver<T>(out var observer))
				_enabledObservers.Remove(observer);
		}

		public T GetObserver<T>() where T : IEngineObserver => TryGetObserver(out T observer) ? observer : default;

		private Boolean TryGetObserver<T>(out T observer) where T : IEngineObserver =>
			(observer = _registeredObservers.TryGetValue(typeof(T), out var o) ? (T)o : default) is not null;
	}
}
