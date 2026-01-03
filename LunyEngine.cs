using Luny.Diagnostics;
using Luny.Exceptions;
using Luny.Registries;
using Luny.Services;
using System;

namespace Luny
{
	/// <summary>
	/// LunyEngine interface - receives callbacks from engine adapters and provides service access.
	/// </summary>
	public interface ILunyEngine
	{
		// Mandatory services
		IApplicationService Application { get; }
		IDebugService Debug { get; }
		IEditorService Editor { get; }
		ISceneService Scene { get; }
		ITimeService Time { get; }

		// Diagnostics
		IEngineProfiler Profiler { get; }

		// Lifecycle dispatch methods - receives callbacks from engine adapters
		void OnStartup();
		void OnFixedStep(Double fixedDeltaTime);
		void OnUpdate(Double deltaTime);
		void OnLateUpdate(Double deltaTime);
		void OnShutdown();

		// Observer management
		void EnableObserver<T>() where T : IEngineObserver;
		void DisableObserver<T>() where T : IEngineObserver;
		Boolean IsObserverEnabled<T>() where T : IEngineObserver;

		// Service access
		TService GetService<TService>() where TService : class, IEngineService;
		Boolean TryGetService<TService>(out TService service) where TService : class, IEngineService;
		Boolean HasService<TService>() where TService : class, IEngineService;
	}

	/// <summary>
	/// Singleton engine that discovers and manages services and lifecycle observers.
	/// </summary>
	public sealed partial class LunyEngine : ILunyEngine
	{
		private static LunyEngine s_Instance;
		private static Boolean s_IsDisposed;

		private EngineServiceRegistry<IEngineService> _serviceRegistry;
		private EngineObserverRegistry _observerRegistry;
		private EngineProfiler _profiler;
		private ITimeServiceInternal _timeInternal;

		/// <summary>
		/// Gets the engine profiler for performance monitoring.
		/// Profiling methods are no-ops in release builds unless LUNY_PROFILE is defined.
		/// </summary>
		public IEngineProfiler Profiler => _profiler;

		/// <summary>
		/// Gets the singleton instance, creating it on first access.
		/// </summary>
		public static ILunyEngine Instance => s_Instance;

		// We only pass the engine adapter instance to signal that this must only be called by the engine adapter
		internal static ILunyEngine CreateInstance(IEngineAdapter engineAdapter)
		{
			if (s_IsDisposed)
				throw new LunyLifecycleException($"{nameof(LunyEngine)} instance already disposed. It must not be created again.");
			if (s_Instance != null)
				throw new LunyLifecycleException($"{nameof(LunyEngine)} instance already exists.");
			if (engineAdapter == null)
				throw new ArgumentNullException(nameof(engineAdapter), "Engine adapter cannot be null.");

			// splitting ctor and init prevents stackoverflows for cases where LunyEngine.Instance is accessed within LunyEngine's ctor
			s_Instance = new LunyEngine();
			s_Instance.Initialize();
			return s_Instance;
		}

		internal static void ResetDisposedFlag_UnityEditorOnly() => s_IsDisposed = false;

		private LunyEngine()
		{
			if (s_Instance != null)
				LunyThrow.SingletonDuplicationException(nameof(LunyEngine));
		}

		private void Initialize()
		{
			LunyLogger.LogInfo("Initializing...", this);

			LunyID.Reset();

			_serviceRegistry = new EngineServiceRegistry<IEngineService>();
			AcquireMandatoryServices();
			_timeInternal.SetLunyFrameCount(1); // ensure we always start in frame "1"

			_observerRegistry = new EngineObserverRegistry(Scene);
			_profiler = new EngineProfiler(Time);

			LunyLogger.LogInfo("Initialization complete.", this);
		}

		/// <summary>
		/// CAUTION: Must only be called by engine-native lifecycle adapter!
		/// </summary>
		public void OnStartup()
		{
			LunyLogger.LogInfo($"{nameof(OnStartup)} running...", this);

			foreach (var observer in _observerRegistry.EnabledObservers)
			{
				_profiler.BeginObserver(observer);
				try
				{
					observer.OnStartup();
				}
				catch (Exception e)
				{
					_profiler.RecordError(observer, EngineLifecycleEvents.OnStartup, e);
					/* keep dispatch resilient */
					LunyLogger.LogException(e);
				}
				finally
				{
					_profiler.EndObserver(observer, EngineLifecycleEvents.OnStartup);
				}
			}

			LunyLogger.LogInfo($"{nameof(OnStartup)} complete.", this);
		}

		/// <summary>
		/// CAUTION: Must only be called by engine-native lifecycle adapter!
		/// </summary>
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
					_profiler.RecordError(observer, EngineLifecycleEvents.OnFixedStep, e);
					/* keep dispatch resilient */
					LunyLogger.LogException(e);
				}
				finally
				{
					_profiler.EndObserver(observer, EngineLifecycleEvents.OnFixedStep);
				}
			}
		}

		/// <summary>
		/// CAUTION: Must only be called by engine-native lifecycle adapter!
		/// </summary>
		public void OnUpdate(Double deltaTime)
		{
			// TODO: send "OnPreUpdate"

			foreach (var observer in _observerRegistry.EnabledObservers)
			{
				_profiler.BeginObserver(observer);
				try
				{
					// TODO: check if enabled state changed to true, if so send OnEnable

					observer.OnUpdate(deltaTime);
				}
				catch (Exception e)
				{
					_profiler.RecordError(observer, EngineLifecycleEvents.OnUpdate, e);
					/* keep dispatch resilient */
					LunyLogger.LogException(e);
				}
				finally
				{
					_profiler.EndObserver(observer, EngineLifecycleEvents.OnUpdate);
				}
			}
		}

		/// <summary>
		/// CAUTION: Must only be called by engine-native lifecycle adapter!
		/// </summary>
		public void OnLateUpdate(Double deltaTime)
		{
			foreach (var observer in _observerRegistry.EnabledObservers)
			{
				_profiler.BeginObserver(observer);
				try
				{
					observer.OnLateUpdate(deltaTime);

					// TODO: check if enabled state changed to false, if so send OnDisable
				}
				catch (Exception e)
				{
					_profiler.RecordError(observer, EngineLifecycleEvents.OnLateUpdate, e);
					/* keep dispatch resilient */
					LunyLogger.LogException(e);
				}
				finally
				{
					_profiler.EndObserver(observer, EngineLifecycleEvents.OnLateUpdate);
				}
			}

			// TODO: run structural changes here, ie "OnPostUpdate"

			// next frame
			_timeInternal.SetLunyFrameCount(Time.FrameCount + 1);
		}

		/// <summary>
		/// CAUTION: Must only be called by engine-native lifecycle adapter!
		/// </summary>
		public void OnShutdown()
		{
			LunyLogger.LogInfo($"{nameof(OnShutdown)} running...", this);

			foreach (var observer in _observerRegistry.EnabledObservers)
			{
				_profiler.BeginObserver(observer);
				try
				{
					observer.OnShutdown();
				}
				catch (Exception e)
				{
					_profiler.RecordError(observer, EngineLifecycleEvents.OnShutdown, e);
					/* keep dispatch resilient */
					LunyLogger.LogException(e);
				}
				finally
				{
					_profiler.EndObserver(observer, EngineLifecycleEvents.OnShutdown);
				}
			}

			LunyLogger.LogInfo($"{nameof(OnShutdown)} complete.", this);
			Dispose();
		}

		public void EnableObserver<T>() where T : IEngineObserver => _observerRegistry.EnableObserver<T>();
		public void DisableObserver<T>() where T : IEngineObserver => _observerRegistry.DisableObserver<T>();
		public Boolean IsObserverEnabled<T>() where T : IEngineObserver => _observerRegistry.IsObserverEnabled<T>();

		public Boolean HasService<TService>() where TService : class, IEngineService => _serviceRegistry.Has<TService>();
		public TService GetService<TService>() where TService : class, IEngineService => _serviceRegistry.Get<TService>();

		public Boolean TryGetService<TService>(out TService service) where TService : class, IEngineService =>
			_serviceRegistry.TryGet(out service);

		private void Dispose()
		{
			if (s_IsDisposed)
				throw new LunyLifecycleException($"{nameof(LunyEngine)} already disposed!");

			_serviceRegistry = null;
			_observerRegistry = null;
			_profiler = null;
			_timeInternal = null;
			s_Instance = null;

			// ensure we won't get re-instantiated after this point
			s_IsDisposed = true;

			LunyLogger.LogInfo("Disposed.", this);
		}

		~LunyEngine() => LunyLogger.LogInfo($"finalized {GetHashCode()}", this);

		// TODO: HasObserver?
		public T GetObserver<T>() where T : IEngineObserver => _observerRegistry.GetObserver<T>();
	}
}
