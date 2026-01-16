using Luny.Engine;
using Luny.Engine.Bridge;
using Luny.Engine.Diagnostics;
using Luny.Engine.Identity;
using Luny.Engine.Services;
using Luny.Exceptions;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Luny
{
	/// <summary>
	/// LunyEngine interface - receives callbacks from engine adapters and provides service access.
	/// </summary>
	public interface ILunyEngine
	{
		// Mandatory services
		ILunyApplicationService Application { get; }
		ILunyDebugService Debug { get; }
		ILunyEditorService Editor { get; }
		ILunySceneService Scene { get; }
		ILunyTimeService Time { get; }

		ILunyObjectRegistry Objects { get; }
		ILunyEngineProfiler Profiler { get; }

		// Observer management
		void EnableObserver<T>() where T : ILunyEngineObserver;
		void DisableObserver<T>() where T : ILunyEngineObserver;
		Boolean IsObserverEnabled<T>() where T : ILunyEngineObserver;
		T GetObserver<T>() where T : ILunyEngineObserver;

		// Service access
		TService GetService<TService>() where TService : LunyEngineServiceBase;
		Boolean TryGetService<TService>(out TService service) where TService : LunyEngineServiceBase;
		Boolean HasService<TService>() where TService : LunyEngineServiceBase;
	}

	public interface ILunyEngineLifecycle
	{
		static void ThrowOnSingletonDuplication(LunyEngine instance)
		{
			if (instance != null)
				throw new LunyLifecycleException($"Duplicate {nameof(LunyEngine)} singleton detected!");
		}

		static void ThrowIfNotCurrentAdapter(ILunyEngineNativeAdapter actualAdapter, ILunyEngineNativeAdapter expectedAdapter)
		{
#if DEBUG || LUNY_DEBUG
			if (actualAdapter == null)
				throw new LunyLifecycleException($"Null adapter passed into {nameof(ILunyEngineLifecycle)} interface method!");
			if (actualAdapter != expectedAdapter)
				throw new LunyLifecycleException($"Wrong adapter {actualAdapter} passed into {nameof(ILunyEngineLifecycle)} interface method!");
#endif
		}

		// Lifecycle callbacks for engine adapter
		void OnEngineStartup(ILunyEngineNativeAdapter nativeAdapter);
		void OnEngineFixedStep(Double fixedDeltaTime, ILunyEngineNativeAdapter nativeAdapter);
		void OnEngineUpdate(Double deltaTime, ILunyEngineNativeAdapter nativeAdapter);
		void OnEngineLateUpdate(Double deltaTime, ILunyEngineNativeAdapter nativeAdapter);
		void OnEngineShutdown(ILunyEngineNativeAdapter nativeAdapter);
	}

	internal interface ILunyEngineInternal
	{
		ILunyObjectLifecycleManagerInternal Lifecycle { get; }
	}

	/// <summary>
	/// Singleton engine that discovers and manages services and lifecycle observers.
	/// </summary>
	public sealed partial class LunyEngine : ILunyEngine, ILunyEngineInternal, ILunyEngineLifecycle
	{
		private static LunyEngine s_Instance;
		private static ILunyEngineNativeAdapter s_NativeAdapter;
		private static Boolean s_IsDisposed;

		private LunyServiceRegistry _serviceRegistry;
		private LunyEngineObserverRegistry _observerRegistry;
		private LunyObjectRegistry _objectRegistry;
		private LunyObjectLifecycleManager _lifecycleManager;
		private LunyEngineProfiler _profiler;
		private ILunyTimeServiceInternal _timeInternal;

		public ILunyObjectRegistry Objects => _objectRegistry;

		ILunyObjectLifecycleManagerInternal ILunyEngineInternal.Lifecycle => _lifecycleManager;

		/// <summary>
		/// Gets the engine profiler for performance monitoring.
		/// Profiling methods are no-ops in release builds unless LUNY_PROFILE is defined.
		/// </summary>
		public ILunyEngineProfiler Profiler => _profiler;

		/// <summary>
		/// Gets the singleton instance, creating it on first access.
		/// </summary>
		public static ILunyEngine Instance => s_Instance;

		[SuppressMessage("ReSharper", "HeuristicUnreachableCode")]
		internal static ILunyEngineLifecycle CreateInstance(ILunyEngineNativeAdapter engineAdapter)
		{
			LunyTraceLogger.LogInfoCreateSingletonInstance(typeof(LunyEngine));
			if (s_IsDisposed)
				throw new LunyLifecycleException($"{nameof(LunyEngine)} instance already disposed. It must not be created again.");
			if (s_Instance != null)
				throw new LunyLifecycleException($"{nameof(LunyEngine)} instance already exists.");
			if (engineAdapter == null) // adapter instance is used to ensure only the creating adapter can run LunyEngine
				throw new ArgumentNullException(nameof(engineAdapter), $"{nameof(ILunyEngineNativeAdapter)} cannot be null");

			s_NativeAdapter = engineAdapter;

			// splitting ctor and Initialize prevents stackoverflows for cases where Instance is accessed from within ctor
			s_Instance = new LunyEngine();
			s_Instance.Initialize();
			return s_Instance;
		}

		internal static void ForceReset_UnityEditorAndUnitTestsOnly()
		{
			s_IsDisposed = false;
			s_NativeAdapter = null;
			s_Instance = null;
		}

		private LunyEngine() => ILunyEngineLifecycle.ThrowOnSingletonDuplication(s_Instance);

		private void Initialize()
		{
			try
			{
				LunyTraceLogger.LogInfoInitializing(this);

				LunyObjectID.Reset();

				_serviceRegistry = new LunyServiceRegistry();
				AssignMandatoryServices();
				_timeInternal = (ILunyTimeServiceInternal)Time;
				_timeInternal.SetLunyFrameCount(0); // frame "0" marks anything before OnEngineStartup()

				_profiler = new LunyEngineProfiler(Time);
				_observerRegistry = new LunyEngineObserverRegistry();
				_objectRegistry = new LunyObjectRegistry();
				_lifecycleManager = new LunyObjectLifecycleManager(_objectRegistry);

				LunyTraceLogger.LogInfoInitialized(this);
			}
			catch (Exception)
			{
				LunyLogger.LogError($"Error during {nameof(LunyEngine)} {nameof(Initialize)}!", this);
				throw;
			}
		}

		~LunyEngine() => LunyTraceLogger.LogInfoFinalized(this);

		private void Startup() // called from Heartbeat
		{
			try
			{
				var sceneService = (ILunySceneServiceInternal)Scene;
				sceneService.OnSceneLoaded += OnSceneLoaded;
				sceneService.OnSceneUnloaded += OnSceneUnloaded;

				_serviceRegistry.Startup();
			}
			catch (Exception)
			{
				LunyLogger.LogError($"Error during {nameof(LunyEngine)} {nameof(Startup)}!", this);
				throw;
			}
		}

		private void Shutdown() // called from Heartbeat
		{
			try
			{
				var sceneService = (ILunySceneServiceInternal)Scene;
				sceneService.OnSceneLoaded -= OnSceneLoaded;
				sceneService.OnSceneUnloaded -= OnSceneUnloaded;

				_lifecycleManager.Shutdown(_objectRegistry);
				_objectRegistry.Shutdown();
				_serviceRegistry.Shutdown();
			}
			catch (Exception e)
			{
				LunyLogger.LogError($"Error during {nameof(LunyEngine)} {nameof(Shutdown)}!", this);
				LunyLogger.LogException(e, this);
			}
			finally
			{
				_serviceRegistry = null;
				_observerRegistry = null;
				_objectRegistry = null;
				_lifecycleManager = null;
				_profiler = null;
				_timeInternal = null;

				// ensure we won't get re-instantiated after this point
				s_IsDisposed = true;
				s_NativeAdapter = null;
				s_Instance = null;
			}
		}

		private void PreUpdate() // called from Heartbeat
		{
			_serviceRegistry.PreUpdate();
			_lifecycleManager.PreUpdate();
		}

		private void PostUpdate() // called from Heartbeat
		{
			// run "structural changes" here ..
			_serviceRegistry.PostUpdate();
			_lifecycleManager.PostUpdate(); // should run last to guarantee object cleanup
		}

		private void OnSceneLoaded(ILunyScene loadedScene) // called by SceneService
		{
			LunyTraceLogger.LogInfoEventCallback(nameof(OnSceneLoaded), loadedScene?.ToString(), this);
			InvokeObserversOnSceneLoaded(loadedScene);
		}

		private void OnSceneUnloaded(ILunyScene unloadedScene) // called by SceneService
		{
			LunyTraceLogger.LogInfoEventCallback(nameof(OnSceneLoaded), unloadedScene?.ToString(), this);
			_lifecycleManager.DestroyNativeNullObjects();
			InvokeObserversOnSceneUnloaded(unloadedScene);
		}
	}
}
