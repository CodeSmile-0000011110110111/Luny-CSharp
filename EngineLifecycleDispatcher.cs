using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Luny
{
    /// <summary>
    /// Singleton dispatcher that will later discover and manage lifecycle observers.
    /// </summary>
    public sealed class EngineLifecycleDispatcher : IEngineLifecycleDispatcher
    {
        private static EngineLifecycleDispatcher _instance;

        private readonly LifecycleObserverRegistry _registry;

        /// <summary>
        /// Gets the singleton instance, creating it on first access.
        /// </summary>
        public static EngineLifecycleDispatcher Instance => _instance ??= new EngineLifecycleDispatcher();

        // CHANGE Step 4: Implement dispatcher methods to delegate to registry's enabled observers.
        private EngineLifecycleDispatcher()
        {
            if (_instance != null)
                Throw.LifecycleDispatcherSingletonDuplicationException(nameof(EngineLifecycleDispatcher));

            // CHANGE Step 5: Remove timing/log once verified—kept per RFC requirement to log discovery time.
            _registry = new LifecycleObserverRegistry();
            OnStartup();
        }

        // CHANGE Step 5: Verify delegation; remove CHANGE tags after Step 10 verification.
        public void OnUpdate(Double deltaTime)
        {
            foreach (var observer in _registry.EnabledObservers)
            {
                try
                {
                    observer.OnUpdate(deltaTime);
                }
                catch (Exception e)
                {
                    /* keep dispatch resilient */
                    Log.Exception(e);
                }
            }
        }
        public void OnLateUpdate(Double deltaTime)
        {
	        foreach (var observer in _registry.EnabledObservers)
	        {
		        try
		        {
			        observer.OnLateUpdate(deltaTime);
		        }
		        catch (Exception e)
		        {
			        /* keep dispatch resilient */
			        Log.Exception(e);
		        }
	        }
        }

        public void OnFixedStep(Double fixedDeltaTime)
        {
            foreach (var observer in _registry.EnabledObservers)
            {
                try
                {
                    observer.OnFixedStep(fixedDeltaTime);
                }
                catch (Exception e)
                {
                    /* keep dispatch resilient */
                    Log.Exception(e);
                }
            }
        }

        public void OnShutdown()
        {
            foreach (var observer in _registry.EnabledObservers)
            {
                try
                {
                    observer.OnShutdown();
                }
                catch (Exception e)
                {
                    /* keep dispatch resilient */
                    Log.Exception(e);
                }
            }
        }

        private void OnStartup()
        {
            foreach (var observer in _registry.EnabledObservers)
            {
                try
                {
                    observer.OnStartup();
                }
                catch (Exception e)
                {
                    /* keep dispatch resilient */
                    Log.Exception(e);
                }
            }
        }

        // CHANGE Step 6: Add EnableObserver/DisableObserver/IsObserverEnabled wrappers here.
        public void EnableObserver<T>() where T : IEngineLifecycle => _registry.EnableObserver<T>();

        public void DisableObserver<T>() where T : IEngineLifecycle => _registry.DisableObserver<T>();

        public Boolean IsObserverEnabled<T>() where T : IEngineLifecycle => _registry.IsObserverEnabled<T>();

        // CHANGE Step 3: Nested registry with discovery + timing per RFC.
        private sealed class LifecycleObserverRegistry
        {
            private readonly Dictionary<Type, IEngineLifecycle> _registeredObservers = new();
            private readonly List<IEngineLifecycle> _enabledObservers = new();

            public IEnumerable<IEngineLifecycle> EnabledObservers => _enabledObservers;

            public LifecycleObserverRegistry() => DiscoverAndInstantiateObservers();

            private void DiscoverAndInstantiateObservers()
            {
                var sw = Stopwatch.StartNew();

                var observerTypes = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a =>
                    {
                        try { return a.GetTypes(); }
                        catch { return Array.Empty<Type>(); }
                    })
                    .Where(t => typeof(IEngineLifecycle).IsAssignableFrom(t) && !t.IsAbstract);

                // TODO: sort observers deterministically
                // TODO: configure observer enabled states

                foreach (var type in observerTypes)
                {
	                Log.Info($"Creating observer instance: {type.Name} (Assembly: {type.Assembly.GetName().Name})");
                    var observer = (IEngineLifecycle)Activator.CreateInstance(type);
                    _registeredObservers[type] = observer;
					_enabledObservers.Add(observer); // enabled by default
                }

                sw.Stop();

                var ms = (Int32)Math.Round(sw.Elapsed.TotalMilliseconds, MidpointRounding.AwayFromZero);
                Log.Info($"[Luny] Registered {_enabledObservers.Count} {nameof(IEngineLifecycle)} observers in {ms} ms.");
            }

            public void EnableObserver<T>() where T : IEngineLifecycle
            {
                if (_registeredObservers.TryGetValue(typeof(T), out var observer))
                {
                    if (!_enabledObservers.Contains(observer))
                        _enabledObservers.Add(observer);
                }
            }

            public void DisableObserver<T>() where T : IEngineLifecycle
            {
                if (_registeredObservers.TryGetValue(typeof(T), out var observer))
                    _enabledObservers.Remove(observer);
            }

            public Boolean IsObserverEnabled<T>() where T : IEngineLifecycle => _registeredObservers.TryGetValue(typeof(T), out var observer) &&
                                                                                _enabledObservers.Contains(observer);
        }
    }
}
