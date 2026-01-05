using Luny.Engine;
using Luny.Engine.Bridge;
using Luny.Engine.Diagnostics;
using Luny.Engine.Identity;
using Luny.Engine.Services;
using Luny.Exceptions;
using System;

namespace Luny
{
	/// <summary>
	/// LunyEngine interface - receives callbacks from engine adapters and provides service access.
	/// </summary>
	public interface ILunyEngine
	{
		// Registries & Managers
		ILunyObjectRegistry Objects { get; }

		// Mandatory services
		IApplicationService Application { get; }
		IDebugService Debug { get; }
		IEditorService Editor { get; }
		ISceneService Scene { get; }
		ITimeService Time { get; }

		// Diagnostics
		ILunyEngineProfiler Profiler { get; }

		// Lifecycle dispatch methods - receives callbacks from engine adapters
		void OnEngineStartup();
		void OnEngineFixedStep(Double fixedDeltaTime);
		void OnEngineUpdate(Double deltaTime);
		void OnEngineLateUpdate(Double deltaTime);
		void OnEngineShutdown();

		// Observer management
		void EnableObserver<T>() where T : ILunyEngineObserver;
		void DisableObserver<T>() where T : ILunyEngineObserver;
		Boolean IsObserverEnabled<T>() where T : ILunyEngineObserver;

		// Service access
		TService GetService<TService>() where TService : class, ILunyEngineService;
		Boolean TryGetService<TService>(out TService service) where TService : class, ILunyEngineService;
		Boolean HasService<TService>() where TService : class, ILunyEngineService;
	}

	internal interface ILunyEngineInternal
	{
		static void SingletonDuplicationException() => throw new LunyLifecycleException($"Duplicate {nameof(LunyEngine)} singleton detected!");
	}

	/// <summary>
	/// Singleton engine that discovers and manages services and lifecycle observers.
	/// </summary>
	public sealed partial class LunyEngine : ILunyEngine, ILunyEngineInternal
	{
		private static LunyEngine s_Instance;
		private static Boolean s_IsDisposed;

		private LunyServiceRegistry<ILunyEngineService> _serviceRegistry;
		private LunyObserverRegistry _observerRegistry;
		private LunyObjectRegistry _objectRegistry;
		private LunyObjectLifecycleManager _lifecycleManager;
		private LunyEngineProfiler _profiler;
		private ITimeServiceInternal _timeInternal;

		public ILunyObjectRegistry Objects => _objectRegistry;
		internal ILunyObjectLifecycleManagerInternal Lifecycle => _lifecycleManager;

		/// <summary>
		/// Gets the engine profiler for performance monitoring.
		/// Profiling methods are no-ops in release builds unless LUNY_PROFILE is defined.
		/// </summary>
		public ILunyEngineProfiler Profiler => _profiler;

		/// <summary>
		/// Gets the singleton instance, creating it on first access.
		/// </summary>
		public static ILunyEngine Instance => s_Instance;

		internal static ILunyEngine CreateInstance(ILunyEngineNativeAdapter engineAdapter)
		{
			if (s_IsDisposed)
				throw new LunyLifecycleException($"{nameof(LunyEngine)} instance already disposed. It must not be created again.");
			if (s_Instance != null)
				throw new LunyLifecycleException($"{nameof(LunyEngine)} instance already exists.");
			// We only pass the engine adapter instance to signal that this must only be called by the engine adapter
			if (engineAdapter == null)
				throw new ArgumentNullException(nameof(engineAdapter), "Engine adapter cannot be null.");

			// splitting ctor and Initialize prevents stackoverflows for cases where Instance is accessed from within ctor
			s_Instance = new LunyEngine();
			s_Instance.Initialize();
			return s_Instance;
		}

		internal static void ResetDisposedFlag_UnityEditorOnly() => s_IsDisposed = false;

		private static Boolean IsSmokeTestScene(ISceneService sceneService)
		{
			// FIXME: remove hardcoded strings, find a better way to determine test mode
			var sceneName = sceneService.CurrentSceneName;
			return sceneName.StartsWith("Luny") && sceneName.EndsWith("SmokeTest");
		}

		private LunyEngine()
		{
			if (s_Instance != null)
				ILunyEngineInternal.SingletonDuplicationException();
		}

		private void Initialize()
		{
			LunyLogger.LogInfo("Initializing...", this);

			LunyObjectID.Reset();

			_serviceRegistry = new LunyServiceRegistry<ILunyEngineService>();
			AcquireMandatoryServices();

			_objectRegistry = new LunyObjectRegistry();
			_lifecycleManager = new LunyObjectLifecycleManager(_objectRegistry);

			_timeInternal = (ITimeServiceInternal)Time;
			_timeInternal.SetLunyFrameCount(1); // ensure we always start in frame "1"

			var isSmokeTestingScene = IsSmokeTestScene(Scene);
			_observerRegistry = new LunyObserverRegistry(isSmokeTestingScene);
			_profiler = new LunyEngineProfiler(Time);

			LunyLogger.LogInfo("Initialization complete.", this);
		}

		/// <summary>
		/// CAUTION: Must only be called by engine-native lifecycle adapter!
		/// </summary>
		public void OnEngineStartup()
		{
			LunyLogger.LogInfo($"{nameof(OnEngineStartup)} running...", this);

			foreach (var observer in _observerRegistry.EnabledObservers)
			{
				_profiler.BeginObserver(observer);
				try
				{
					observer.OnEngineStartup();
				}
				catch (Exception e)
				{
					_profiler.RecordError(observer, LunyEngineLifecycleEvents.OnEngineStartup, e);
					/* keep dispatch resilient */
					LunyLogger.LogException(e);
				}
				finally
				{
					_profiler.EndObserver(observer, LunyEngineLifecycleEvents.OnEngineStartup);
				}
			}

			LunyLogger.LogInfo($"{nameof(OnEngineStartup)} complete.", this);
		}

		/// <summary>
		/// CAUTION: Must only be called by engine-native lifecycle adapter!
		/// </summary>
		public void OnEngineFixedStep(Double fixedDeltaTime)
		{
			_lifecycleManager.ProcessPendingReady();

			foreach (var observer in _observerRegistry.EnabledObservers)
			{
				_profiler.BeginObserver(observer);
				try
				{
					observer.OnEngineFixedStep(fixedDeltaTime);
				}
				catch (Exception e)
				{
					_profiler.RecordError(observer, LunyEngineLifecycleEvents.OnEngineFixedStep, e);
					/* keep dispatch resilient */
					LunyLogger.LogException(e);
				}
				finally
				{
					_profiler.EndObserver(observer, LunyEngineLifecycleEvents.OnEngineFixedStep);
				}
			}
		}

		/// <summary>
		/// CAUTION: Must only be called by engine-native lifecycle adapter!
		/// </summary>
		public void OnEngineUpdate(Double deltaTime)
		{
			// TODO: send "OnPreUpdate"
			_lifecycleManager.ProcessPendingReady();

			foreach (var observer in _observerRegistry.EnabledObservers)
			{
				_profiler.BeginObserver(observer);
				try
				{
					// TODO: check if enabled state changed to true, if so send OnEnable

					observer.OnEngineUpdate(deltaTime);
				}
				catch (Exception e)
				{
					_profiler.RecordError(observer, LunyEngineLifecycleEvents.OnEngineUpdate, e);
					/* keep dispatch resilient */
					LunyLogger.LogException(e);
				}
				finally
				{
					_profiler.EndObserver(observer, LunyEngineLifecycleEvents.OnEngineUpdate);
				}
			}
		}

		/// <summary>
		/// CAUTION: Must only be called by engine-native lifecycle adapter!
		/// </summary>
		public void OnEngineLateUpdate(Double deltaTime)
		{
			foreach (var observer in _observerRegistry.EnabledObservers)
			{
				_profiler.BeginObserver(observer);
				try
				{
					observer.OnEngineLateUpdate(deltaTime);

					// TODO: check if enabled state changed to false, if so send OnDisable
				}
				catch (Exception e)
				{
					_profiler.RecordError(observer, LunyEngineLifecycleEvents.OnEngineLateUpdate, e);
					/* keep dispatch resilient */
					LunyLogger.LogException(e);
				}
				finally
				{
					_profiler.EndObserver(observer, LunyEngineLifecycleEvents.OnEngineLateUpdate);
				}
			}

			// TODO: run structural changes here, ie "OnPostUpdate"
			_lifecycleManager.ProcessPendingDestroy();

			// next frame
			_timeInternal.SetLunyFrameCount(Time.FrameCount + 1);
		}

		/// <summary>
		/// CAUTION: Must only be called by engine-native lifecycle adapter!
		/// </summary>
		public void OnEngineShutdown()
		{
			LunyLogger.LogInfo($"{nameof(OnEngineShutdown)} running...", this);

			foreach (var observer in _observerRegistry.EnabledObservers)
			{
				_profiler.BeginObserver(observer);
				try
				{
					observer.OnEngineShutdown();
				}
				catch (Exception e)
				{
					_profiler.RecordError(observer, LunyEngineLifecycleEvents.OnEngineShutdown, e);
					/* keep dispatch resilient */
					LunyLogger.LogException(e);
				}
				finally
				{
					_profiler.EndObserver(observer, LunyEngineLifecycleEvents.OnEngineShutdown);
				}
			}

			_lifecycleManager.Shutdown(_objectRegistry);
			_objectRegistry.Shutdown();

			LunyLogger.LogInfo($"{nameof(OnEngineShutdown)} complete.", this);
			Dispose();
		}

		public void EnableObserver<T>() where T : ILunyEngineObserver => _observerRegistry.EnableObserver<T>();
		public void DisableObserver<T>() where T : ILunyEngineObserver => _observerRegistry.DisableObserver<T>();
		public Boolean IsObserverEnabled<T>() where T : ILunyEngineObserver => _observerRegistry.IsObserverEnabled<T>();

		public Boolean HasService<TService>() where TService : class, ILunyEngineService => _serviceRegistry.Has<TService>();
		public TService GetService<TService>() where TService : class, ILunyEngineService => _serviceRegistry.Get<TService>();

		public Boolean TryGetService<TService>(out TService service) where TService : class, ILunyEngineService =>
			_serviceRegistry.TryGet(out service);

		private void Dispose()
		{
			if (s_IsDisposed)
				throw new LunyLifecycleException($"{nameof(LunyEngine)} already disposed!");

			_serviceRegistry = null;
			_observerRegistry = null;
			_objectRegistry = null;
			_lifecycleManager = null;
			_profiler = null;
			_timeInternal = null;
			s_Instance = null;

			// ensure we won't get re-instantiated after this point
			s_IsDisposed = true;

			LunyLogger.LogInfo("Disposed.", this);
		}

		~LunyEngine() => LunyLogger.LogInfo($"finalized {GetHashCode()}", this);

		// TODO: HasObserver?
		public T GetObserver<T>() where T : ILunyEngineObserver => _observerRegistry.GetObserver<T>();
	}
}
