using System;
using Luny.Providers;

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
			OnStartup();
		}

		public void OnUpdate(Double deltaTime)
		{
			foreach (var observer in _observerRegistry.EnabledObservers)
			{
				try
				{
					observer.OnUpdate(deltaTime);
				}
				catch (Exception e)
				{
					/* keep dispatch resilient */
					LunyLogger.LogException(e);
				}
			}
		}

		public void OnLateUpdate(Double deltaTime)
		{
			foreach (var observer in _observerRegistry.EnabledObservers)
			{
				try
				{
					observer.OnLateUpdate(deltaTime);
				}
				catch (Exception e)
				{
					/* keep dispatch resilient */
					LunyLogger.LogException(e);
				}
			}
		}

		public void OnFixedStep(Double fixedDeltaTime)
		{
			foreach (var observer in _observerRegistry.EnabledObservers)
			{
				try
				{
					observer.OnFixedStep(fixedDeltaTime);
				}
				catch (Exception e)
				{
					/* keep dispatch resilient */
					LunyLogger.LogException(e);
				}
			}
		}

		public void OnShutdown()
		{
			foreach (var observer in _observerRegistry.EnabledObservers)
			{
				try
				{
					observer.OnShutdown();
				}
				catch (Exception e)
				{
					/* keep dispatch resilient */
					LunyLogger.LogException(e);
				}
			}

			// invalidate references
			_serviceRegistry = null;
			_observerRegistry = null;
			_instance = null;
		}

		private void OnStartup()
		{
			foreach (var observer in _observerRegistry.EnabledObservers)
			{
				try
				{
					observer.OnStartup();
				}
				catch (Exception e)
				{
					/* keep dispatch resilient */
					LunyLogger.LogException(e);
				}
			}
		}

		public void EnableObserver<T>() where T : IEngineLifecycleObserver => _observerRegistry.EnableObserver<T>();

		public void DisableObserver<T>() where T : IEngineLifecycleObserver => _observerRegistry.DisableObserver<T>();

		public Boolean IsObserverEnabled<T>() where T : IEngineLifecycleObserver => _observerRegistry.IsObserverEnabled<T>();

		public TService GetService<TService>() where TService : class, IEngineServiceProvider => _serviceRegistry.Get<TService>();

		public Boolean HasService<TService>() where TService : class, IEngineServiceProvider => _serviceRegistry.Has<TService>();
	}
}
