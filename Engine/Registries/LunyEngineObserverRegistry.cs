using System;
using System.Collections.Generic;

namespace Luny.Engine.Registries
{
	public interface ILunyObserverRegistry {}

	/// <summary>
	/// Registry that discovers, manages, and enables/disables engine observers.
	/// </summary>
	internal sealed class LunyEngineObserverRegistry : ILunyObserverRegistry
	{
		private readonly Dictionary<Type, ILunyEngineObserver> _registeredObservers = new();
		private readonly List<ILunyEngineObserver> _enabledObservers = new();

		public IEnumerable<ILunyEngineObserver> EnabledObservers => _enabledObservers;

		public LunyEngineObserverRegistry()
		{
			LunyTraceLogger.LogInfoInitializing(this);
			DiscoverAndInstantiateObservers();
			LunyTraceLogger.LogInfoInitialized(this);
		}

		~LunyEngineObserverRegistry() => LunyTraceLogger.LogInfoFinalized(this);

		private void DiscoverAndInstantiateObservers()
		{
			var sw = System.Diagnostics.Stopwatch.StartNew();

			var observerTypes = TypeDiscovery.FindAll<ILunyEngineObserver>();

			// TODO: sort observers deterministically
			// TODO: configure observer enabled states

			foreach (var type in observerTypes)
			{
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

		internal void Shutdown() => GC.SuppressFinalize(this);
	}
}
