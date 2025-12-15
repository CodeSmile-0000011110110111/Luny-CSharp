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
        public static EngineLifecycleDispatcher Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new EngineLifecycleDispatcher();
                }
                return _instance;
            }
        }

        // CHANGE Step 4: Implement dispatcher methods to delegate to registry's enabled observers.
        private EngineLifecycleDispatcher()
        {
            // CHANGE Step 5: Remove timing/log once verified—kept per RFC requirement to log discovery time.
            _registry = new LifecycleObserverRegistry();
        }

        // CHANGE Step 5: Verify delegation; remove CHANGE tags after Step 10 verification.
        public void OnUpdate(double deltaTime)
        {
            foreach (var observer in _registry.GetEnabledObservers())
            {
                try { observer.OnUpdate(deltaTime); } catch { /* keep dispatch resilient */ }
            }
        }

        public void OnFixedStep(double fixedDeltaTime)
        {
            foreach (var observer in _registry.GetEnabledObservers())
            {
                try { observer.OnFixedStep(fixedDeltaTime); } catch { /* keep dispatch resilient */ }
            }
        }

        public void OnShutdown()
        {
            foreach (var observer in _registry.GetEnabledObservers())
            {
                try { observer.OnShutdown(); } catch { /* keep dispatch resilient */ }
            }
        }

        // CHANGE Step 5: Add ThrowDuplicateAdapterException helper method here.
        public static void ThrowDuplicateAdapterException(string adapterTypeName, string existingObjectName, long existingInstanceId, string duplicateObjectName, long duplicateInstanceId)
        {
            throw new InvalidOperationException(
                $"Duplicate {adapterTypeName} singleton detected! " +
                $"Existing: Name='{existingObjectName}' InstanceID={existingInstanceId}, " +
                $"Duplicate: Name='{duplicateObjectName}' InstanceID={duplicateInstanceId}");
        }

        public static void ThrowAdapterRemovedException(String godotLifecycleAdapterName)
        {
            throw new InvalidOperationException($"{godotLifecycleAdapterName} unexpectedly removed from SceneTree! It must remain in scene at all times.");
        }

        // CHANGE Step 6: Add EnableObserver/DisableObserver/IsObserverEnabled wrappers here.
        public void EnableObserver<T>() where T : IEngineLifecycle => _registry.EnableObserver<T>();

        public void DisableObserver<T>() where T : IEngineLifecycle => _registry.DisableObserver<T>();

        public bool IsObserverEnabled<T>() where T : IEngineLifecycle => _registry.IsObserverEnabled<T>();

        // CHANGE Step 3: Nested registry with discovery + timing per RFC.
        private sealed class LifecycleObserverRegistry
        {
            private readonly Dictionary<Type, IEngineLifecycle> _registeredObservers = new();
            private readonly List<IEngineLifecycle> _enabledObservers = new();

            public LifecycleObserverRegistry()
            {
                DiscoverAndInstantiateObservers();
            }

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

                foreach (var type in observerTypes)
                {
                    var instance = (IEngineLifecycle)Activator.CreateInstance(type);
                    _registeredObservers[type] = instance;
                    _enabledObservers.Add(instance); // enabled by default
                    try { instance.OnStartup(); } catch { /* swallow to keep startup resilient */ }
                }

                sw.Stop();

                // CHANGE Step 3: Log discovery timing (ms). Engine-agnostic minimal log via Console for now; adapters may map to engine logs.
                try
                {
                    System.Console.WriteLine($"[Luny] Lifecycle discovery completed in {sw.Elapsed.TotalMilliseconds:F2} ms. Observers: {_enabledObservers.Count}");
                }
                catch { /* ignore logging failures */ }
            }

            public IEnumerable<IEngineLifecycle> GetEnabledObservers() => _enabledObservers;

            public void EnableObserver<T>() where T : IEngineLifecycle
            {
                if (_registeredObservers.TryGetValue(typeof(T), out var observer))
                {
                    if (!_enabledObservers.Contains(observer))
                    {
                        _enabledObservers.Add(observer);
                    }
                }
            }

            public void DisableObserver<T>() where T : IEngineLifecycle
            {
                if (_registeredObservers.TryGetValue(typeof(T), out var observer))
                {
                    _enabledObservers.Remove(observer);
                }
            }

            public bool IsObserverEnabled<T>() where T : IEngineLifecycle
            {
                return _registeredObservers.TryGetValue(typeof(T), out var observer) && _enabledObservers.Contains(observer);
            }
        }

    }
}
