using Luny.Attributes;
using Luny.Interfaces;
using Luny.Providers;
using Luny.Proxies;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Luny.Registries
{
	/// <summary>
	/// Registry that discovers, manages, and enables/disables lifecycle observers.
	/// </summary>
	internal sealed class EngineLifecycleObserverRegistry
	{
		private readonly Dictionary<Type, IEngineLifecycleObserver> _registeredObservers = new();
		private readonly List<IEngineLifecycleObserver> _enabledObservers = new();

		public IEnumerable<IEngineLifecycleObserver> EnabledObservers => _enabledObservers;

		private static Boolean IsSmokeTestScene(ISceneServiceProvider sceneServiceProvider)
		{
			var sceneName = sceneServiceProvider.CurrentSceneName;
			return sceneName.StartsWith("Luny") && sceneName.EndsWith("SmokeTest");
		}

		public EngineLifecycleObserverRegistry(ISceneServiceProvider sceneServiceProvider)
		{
			var isSmokeTestScene = IsSmokeTestScene(sceneServiceProvider);
			DiscoverAndInstantiateObservers(isSmokeTestScene);
		}

		private void DiscoverAndInstantiateObservers(Boolean isSmokeTestScene)
		{
			var sw = Stopwatch.StartNew();

			var observerTypes = TypeDiscovery.FindAll<IEngineLifecycleObserver>();

			// TODO: sort observers deterministically
			// TODO: configure observer enabled states

			foreach (var type in observerTypes)
			{
				// Skip [LunyTestable] types unless in smoke test scenes
				if (!isSmokeTestScene && type.HasAttribute<LunyTestableAttribute>())
				{
					LunyLogger.LogInfo($"Skipping test observer: {type.Name} (not running a smoke test scene)", this);
					continue;
				}

				LunyLogger.LogInfo($"Creating observer instance: {type.Name} (Assembly: {type.Assembly.GetName().Name})", this);
				var observer = (IEngineLifecycleObserver)Activator.CreateInstance(type);
				_registeredObservers[type] = observer;
				_enabledObservers.Add(observer); // enabled by default
			}

			sw.Stop();

			var ms = (Int32)Math.Round(sw.Elapsed.TotalMilliseconds, MidpointRounding.AwayFromZero);
			LunyLogger.LogInfo($"Registered {_enabledObservers.Count} {nameof(IEngineLifecycleObserver)} observers in {ms} ms.", this);
		}

		public Boolean IsObserverEnabled<T>() where T : IEngineLifecycleObserver =>
			TryGetObserver<T>(out var observer) && _enabledObservers.Contains(observer);

		public void EnableObserver<T>() where T : IEngineLifecycleObserver
		{
			if (TryGetObserver<T>(out var observer))
			{
				if (!_enabledObservers.Contains(observer))
					_enabledObservers.Add(observer);
			}
		}

		public void DisableObserver<T>() where T : IEngineLifecycleObserver
		{
			if (TryGetObserver<T>(out var observer))
				_enabledObservers.Remove(observer);
		}

		public T GetObserver<T>() where T : IEngineLifecycleObserver => TryGetObserver(out T observer) ? observer : default;

		private Boolean TryGetObserver<T>(out T observer) where T : IEngineLifecycleObserver =>
			(observer = _registeredObservers.TryGetValue(typeof(T), out var o) ? (T)o : default) is not null;
	}
}
