using Luny.Providers;
using Luny.Diagnostics;
using Luny.Exceptions;
using Luny.Interfaces;
using Luny.Proxies;
using Luny.Registries;
using System;

namespace Luny
{
    /// <summary>
    /// Singleton engine that discovers and manages services and lifecycle observers.
    /// </summary>
    public sealed class LunyEngine : ILunyEngine
    {
        private static LunyEngine _instance;

        private EngineServiceRegistry<IEngineServiceProvider> _serviceRegistry;
        private EngineLifecycleObserverRegistry _observerRegistry;
        private EngineProfiler _profiler;

        /// <summary>
        /// Gets the engine profiler for performance monitoring.
        /// Profiling methods are no-ops in release builds unless LUNY_PROFILE is defined.
        /// </summary>
        public EngineProfiler Profiler => _profiler;

        /// <summary>
        /// Gets the singleton instance, creating it on first access.
        /// </summary>
        public static LunyEngine Instance => _instance ??= new LunyEngine();

        private LunyEngine()
        {
            if (_instance != null)
                LunyThrow.SingletonDuplicationException(nameof(LunyEngine));

            _serviceRegistry = new EngineServiceRegistry<IEngineServiceProvider>();
            var sceneProvider = _serviceRegistry.Get<ISceneServiceProvider>();
            _observerRegistry = new EngineLifecycleObserverRegistry(sceneProvider);
            _profiler = new EngineProfiler();
        }

        public void OnStartup()
        {
            foreach (var observer in _observerRegistry.EnabledObservers)
            {
                _profiler.BeginObserver(observer);
                try
                {
                    observer.OnStartup();
                }
                catch (Exception e)
                {
                    _profiler.RecordError(observer, e);
                    /* keep dispatch resilient */
                    LunyLogger.LogException(e);
                }
                finally
                {
                    _profiler.EndObserver(observer);
                }
            }
        }

        public void OnUpdate(Double deltaTime)
        {
            foreach (var observer in _observerRegistry.EnabledObservers)
            {
                _profiler.BeginObserver(observer);
                try
                {
                    observer.OnUpdate(deltaTime);
                }
                catch (Exception e)
                {
                    _profiler.RecordError(observer, e);
                    /* keep dispatch resilient */
                    LunyLogger.LogException(e);
                }
                finally
                {
                    _profiler.EndObserver(observer);
                }
            }
        }

        public void OnLateUpdate(Double deltaTime)
        {
            foreach (var observer in _observerRegistry.EnabledObservers)
            {
                _profiler.BeginObserver(observer);
                try
                {
                    observer.OnLateUpdate(deltaTime);
                }
                catch (Exception e)
                {
                    _profiler.RecordError(observer, e);
                    /* keep dispatch resilient */
                    LunyLogger.LogException(e);
                }
                finally
                {
                    _profiler.EndObserver(observer);
                }
            }

            // TODO: run structural changes here, ie "OnCommitUpdate"
        }

        public void OnFixedStep(Double fixedDeltaTime)
        {
            foreach (var observer in _observerRegistry.EnabledObservers)
            {
                _profiler.BeginObserver(observer);
                try
                {
                    observer.OnFixedStep(fixedDeltaTime);
                }
                catch (Exception e)
                {
                    _profiler.RecordError(observer, e);
                    /* keep dispatch resilient */
                    LunyLogger.LogException(e);
                }
                finally
                {
                    _profiler.EndObserver(observer);
                }
            }
        }

        public void OnShutdown()
        {
            foreach (var observer in _observerRegistry.EnabledObservers)
            {
                _profiler.BeginObserver(observer);
                try
                {
                    observer.OnShutdown();
                }
                catch (Exception e)
                {
                    _profiler.RecordError(observer, e);
                    /* keep dispatch resilient */
                    LunyLogger.LogException(e);
                }
                finally
                {
                    _profiler.EndObserver(observer);
                }
            }

            // invalidate references
            _serviceRegistry = null;
            _observerRegistry = null;
            _profiler = null;
            _instance = null;
        }

        public Boolean IsObserverEnabled<T>() where T : IEngineLifecycleObserver => _observerRegistry.IsObserverEnabled<T>();
        public void EnableObserver<T>() where T : IEngineLifecycleObserver => _observerRegistry.EnableObserver<T>();

        public void DisableObserver<T>() where T : IEngineLifecycleObserver => _observerRegistry.DisableObserver<T>();
        public TService GetService<TService>() where TService : class, IEngineServiceProvider => _serviceRegistry.Get<TService>();

        public Boolean HasService<TService>() where TService : class, IEngineServiceProvider => _serviceRegistry.Has<TService>();

        public T GetObserver<T>() where T : IEngineLifecycleObserver => _observerRegistry.GetObserver<T>();
    }
}
