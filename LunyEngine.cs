using Luny.Engine;
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
		ILunyApplicationService Application { get; }
		ILunyDebugService Debug { get; }
		ILunyEditorService Editor { get; }
		ILunySceneService Scene { get; }
		ILunyTimeService Time { get; }

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
		TService GetService<TService>() where TService : LunyEngineServiceBase;
		Boolean TryGetService<TService>(out TService service) where TService : LunyEngineServiceBase;
		Boolean HasService<TService>() where TService : LunyEngineServiceBase;
	}

	internal interface ILunyEngineInternal
	{
		static void ThrowOnSingletonDuplication(LunyEngine instance)
		{
			if (instance != null)
				throw new LunyLifecycleException($"Duplicate {nameof(LunyEngine)} singleton detected!");
		}
	}

	/// <summary>
	/// Singleton engine that discovers and manages services and lifecycle observers.
	/// </summary>
	public sealed partial class LunyEngine : ILunyEngine, ILunyEngineInternal
	{
		private static LunyEngine s_Instance;
		private static Boolean s_IsDisposed;

		private LunyServiceRegistry _serviceRegistry;
		private LunyObserverRegistry _observerRegistry;
		private LunyObjectRegistry _objectRegistry;
		private LunyObjectLifecycleManager _lifecycleManager;
		private LunyEngineProfiler _profiler;
		private ILunyTimeServiceInternal _timeInternal;

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
			LunyTraceLogger.LogInfoCreateSingletonInstance(typeof(LunyEngine));
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

		private LunyEngine() => ILunyEngineInternal.ThrowOnSingletonDuplication(s_Instance);

		private void Initialize()
		{
			LunyTraceLogger.LogInfoInitializing(this);

			LunyObjectID.Reset();

			_serviceRegistry = new LunyServiceRegistry();
			AssignMandatoryServices();
			_timeInternal = (ILunyTimeServiceInternal)Time;
			_timeInternal.SetLunyFrameCount(0); // frame "0" is anything pre-startup

			_profiler = new LunyEngineProfiler(Time);
			_observerRegistry = new LunyObserverRegistry();
			_objectRegistry = new LunyObjectRegistry();
			_lifecycleManager = new LunyObjectLifecycleManager(_objectRegistry);

			LunyTraceLogger.LogInfoInitialized(this);
		}

		public void EnableObserver<T>() where T : ILunyEngineObserver => _observerRegistry.EnableObserver<T>();
		public void DisableObserver<T>() where T : ILunyEngineObserver => _observerRegistry.DisableObserver<T>();
		public Boolean IsObserverEnabled<T>() where T : ILunyEngineObserver => _observerRegistry.IsObserverEnabled<T>();

		public Boolean HasService<TService>() where TService : LunyEngineServiceBase => _serviceRegistry.Has<TService>();
		public TService GetService<TService>() where TService : LunyEngineServiceBase => _serviceRegistry.Get<TService>();

		public Boolean TryGetService<TService>(out TService service) where TService : LunyEngineServiceBase =>
			_serviceRegistry.TryGet(out service);

		private void Startup() => _serviceRegistry.Startup();

		private void PreUpdate()
		{
			_serviceRegistry.PreUpdate();
			_lifecycleManager.PreUpdate();
		}

		private void PostUpdate()
		{
			// run "structural changes" here ..
			_serviceRegistry.PostUpdate();
			_lifecycleManager.PostUpdate(); // should run last to guarantee object cleanup

			_timeInternal.IncrementLunyFrameCount(); // bump FrameCount
		}

		private void Shutdown()
		{
			_lifecycleManager.Shutdown(_objectRegistry);
			_objectRegistry.Shutdown();
			_serviceRegistry.Shutdown();

			_serviceRegistry = null;
			_observerRegistry = null;
			_objectRegistry = null;
			_lifecycleManager = null;
			_profiler = null;
			_timeInternal = null;

			// ensure we won't get re-instantiated after this point
			s_IsDisposed = true;
			s_Instance = null;
		}

		~LunyEngine() => LunyTraceLogger.LogInfoFinalized(this);

		// TODO: HasObserver?
		public T GetObserver<T>() where T : ILunyEngineObserver => _observerRegistry.GetObserver<T>();
	}
}
