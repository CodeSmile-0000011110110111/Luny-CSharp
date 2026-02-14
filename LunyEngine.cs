using Luny.Engine;
using Luny.Engine.Bridge;
using Luny.Engine.Diagnostics;
using Luny.Engine.Registries;
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
		ILunyAssetService Asset { get; }
		ILunyDebugService Debug { get; }
		ILunyEditorService Editor { get; }
		ILunyObjectService Object { get; }
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
		void EngineStartup(ILunyEngineNativeAdapter nativeAdapter);
		void EngineHeartbeat(ILunyEngineNativeAdapter nativeAdapter, Double fixedDeltaTime);
		void EngineFrameUpdate(ILunyEngineNativeAdapter nativeAdapter, Double deltaTime);
		void EngineFrameLateUpdate(ILunyEngineNativeAdapter nativeAdapter);
		void EngineShutdown(ILunyEngineNativeAdapter nativeAdapter);
	}

	internal interface ILunyEngineInternal
	{
		ILunyObjectLifecycleInternal ObjectLifecycle { get; }
	}

	/// <summary>
	/// Singleton engine that discovers and manages services and lifecycle observers.
	/// </summary>
	public sealed partial class LunyEngine : ILunyEngine, ILunyEngineInternal, ILunyEngineLifecycle
	{
		private static LunyEngine s_Instance;
		private static ILunyEngineNativeAdapter s_EngineAdapter;
		private static Boolean s_IsDisposed;

		private LunyServiceRegistry _serviceRegistry;
		private LunyEngineObserverRegistry _observerRegistry;
		private LunyObjectRegistry _objectRegistry;
		private LunyObjectLifecycle _objectLifecycle;
		private LunyEngineProfiler _profiler;
		private ILunyTimeServiceInternal _timeInternal;

		// API Services
		public ILunyApplicationService Application { get; private set; }
		public ILunyAssetService Asset { get; private set; }
		public ILunyDebugService Debug { get; private set; }
		public ILunyEditorService Editor { get; private set; }
		public ILunyObjectService Object { get; private set; }
		public ILunySceneService Scene { get; private set; }
		public ILunyTimeService Time { get; private set; }

		ILunyObjectRegistry ILunyEngine.Objects => _objectRegistry;
		ILunyObjectLifecycleInternal ILunyEngineInternal.ObjectLifecycle => _objectLifecycle;

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

			s_EngineAdapter = engineAdapter;

			// splitting ctor and Initialize prevents stackoverflows for cases where Instance is accessed from within ctor
			s_Instance = new LunyEngine();
			s_Instance.Initialize(engineAdapter.Engine);
			return s_Instance;
		}

		internal static void ForceReset_UnityEditorAndUnitTestsOnly()
		{
			s_IsDisposed = false;
			ILunyEngineNativeAdapter.IsApplicationQuitting = false;
		}

		private LunyEngine() => ILunyEngineLifecycle.ThrowOnSingletonDuplication(s_Instance);

		private void Initialize(NativeEngine engine)
		{
			try
			{
				LunyTraceLogger.LogInfoInitializing(this);

				LunyObjectID.Reset();
				LunyAssetID.Reset();

				_serviceRegistry = new LunyServiceRegistry(engine);
				AssignMandatoryServices();
				_timeInternal = (ILunyTimeServiceInternal)Time;
				_timeInternal.SetLunyFrameAndHeartbeatCount(0); // frame "0" marks anything before OnEngineStartup()

				_profiler = new LunyEngineProfiler(Time);
				_observerRegistry = new LunyEngineObserverRegistry();
				_objectRegistry = new LunyObjectRegistry();
				_objectLifecycle = new LunyObjectLifecycle();

				LunyTraceLogger.LogInfoInitialized(this);
			}
			catch (Exception)
			{
				LunyLogger.LogError($"Error during {nameof(LunyEngine)} {nameof(Initialize)}!", this);
				throw;
			}
		}

		public Boolean HasService<TService>() where TService : LunyEngineServiceBase => _serviceRegistry.Has<TService>();
		public TService GetService<TService>() where TService : LunyEngineServiceBase => _serviceRegistry.Get<TService>();

		public Boolean TryGetService<TService>(out TService service) where TService : LunyEngineServiceBase =>
			_serviceRegistry.TryGet(out service);

		public void EnableObserver<T>() where T : ILunyEngineObserver => _observerRegistry.EnableObserver<T>();
		public void DisableObserver<T>() where T : ILunyEngineObserver => _observerRegistry.DisableObserver<T>();
		public Boolean IsObserverEnabled<T>() where T : ILunyEngineObserver => _observerRegistry.IsObserverEnabled<T>();
		public T GetObserver<T>() where T : ILunyEngineObserver => _observerRegistry.GetObserver<T>();

		private void AssignMandatoryServices()
		{
			Application = GetService<LunyApplicationServiceBase>();
			Asset = GetService<LunyAssetServiceBase>();
			Debug = GetService<LunyDebugServiceBase>();
			Editor = GetService<LunyEditorServiceBase>();
			Object = GetService<LunyObjectServiceBase>();
			Scene = GetService<LunySceneServiceBase>();
			Time = GetService<LunyTimeServiceBase>();
		}

		~LunyEngine() => LunyTraceLogger.LogInfoFinalized(this);
	}
}
