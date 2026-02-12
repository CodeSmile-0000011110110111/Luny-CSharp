using Luny.Engine;
using Luny.Engine.Bridge;
using Luny.Engine.Diagnostics;
using Luny.Engine.Services;
using System;

namespace Luny
{
	public sealed partial class LunyEngine
	{
		private Boolean _didCallPreUpdateThisFrame;

		void ILunyEngineLifecycle.EngineStartup(ILunyEngineNativeAdapter nativeAdapter)
		{
			ILunyEngineLifecycle.ThrowIfNotCurrentAdapter(nativeAdapter, s_EngineAdapter);
			LunyTraceLogger.LogInfoStartingUp(this);

			// Observers Startup
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
					//LunyLogger.LogException(e, this);
					throw;
				}
				finally
				{
					_profiler.EndObserver(observer, LunyEngineLifecycleEvents.OnEngineStartup);
				}
			}

			// Services Startup
			try
			{
				var sceneService = (ILunySceneServiceInternal)Scene;
				sceneService.OnSceneLoaded += OnSceneLoaded;
				sceneService.OnSceneUnloaded += OnSceneUnloaded;

				_serviceRegistry.Startup();
			}
			catch (Exception)
			{
				//LunyLogger.LogError($"Error during {nameof(LunyEngine)} {nameof(ILunyEngineLifecycle.EngineStartup)}!", this);
				throw;
			}

			LunyTraceLogger.LogInfoStartupComplete(this);
		}

		void ILunyEngineLifecycle.EngineShutdown(ILunyEngineNativeAdapter nativeAdapter)
		{
			LunyTraceLogger.LogInfoShuttingDown(this);
			ILunyEngineLifecycle.ThrowIfNotCurrentAdapter(nativeAdapter, s_EngineAdapter);

			// Observers Shutdown
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
					//LunyLogger.LogException(e, this);
					throw;
				}
				finally
				{
					_profiler.EndObserver(observer, LunyEngineLifecycleEvents.OnEngineShutdown);
				}
			}

			// Services & Engine Shutdown
			try
			{
				var sceneService = (ILunySceneServiceInternal)Scene;
				sceneService.OnSceneLoaded -= OnSceneLoaded;
				sceneService.OnSceneUnloaded -= OnSceneUnloaded;

				_profiler.Shutdown();
				_objectLifecycle.Shutdown(_objectRegistry);
				_objectRegistry.Shutdown();
				_serviceRegistry.Shutdown();
				_observerRegistry.Shutdown();
			}
			catch (Exception)
			{
				LunyLogger.LogError($"Error during {nameof(LunyEngine)} {nameof(ILunyEngineLifecycle.EngineShutdown)}!", this);
				throw;
			}
			finally
			{
				_serviceRegistry = null;
				_observerRegistry = null;
				_objectRegistry = null;
				_objectLifecycle = null;
				_profiler = null;
				_timeInternal = null;
				LunyPath.Converter = null;

				// ensure we won't get re-instantiated after this point
				s_IsDisposed = true;
				s_EngineAdapter = null;
				s_Instance = null;
				GC.SuppressFinalize(this);

				LunyTraceLogger.LogInfoShutdownComplete(this);
			}
		}

		void ILunyEngineLifecycle.EngineHeartbeat(ILunyEngineNativeAdapter nativeAdapter, Double fixedDeltaTime)
		{
			ILunyEngineLifecycle.ThrowIfNotCurrentAdapter(nativeAdapter, s_EngineAdapter);

			_timeInternal.SetFixedDeltaTime(fixedDeltaTime);
			RunEnginePreUpdateOncePerFrame();

			foreach (var observer in _observerRegistry.EnabledObservers)
			{
				_profiler.BeginObserver(observer);
				try
				{
					observer.OnEngineHeartbeat();
				}
				catch (Exception e)
				{
					_profiler.RecordError(observer, LunyEngineLifecycleEvents.OnEngineHeartbeat, e);
					/* keep dispatch resilient */
					//LunyLogger.LogException(e, this);
					throw;
				}
				finally
				{
					_profiler.EndObserver(observer, LunyEngineLifecycleEvents.OnEngineHeartbeat);
				}
			}
		}

		void ILunyEngineLifecycle.EngineFrameUpdate(ILunyEngineNativeAdapter nativeAdapter, Double deltaTime)
		{
			ILunyEngineLifecycle.ThrowIfNotCurrentAdapter(nativeAdapter, s_EngineAdapter);

			_timeInternal.SetDeltaTime(deltaTime);
			RunEnginePreUpdateOncePerFrame();

			foreach (var observer in _observerRegistry.EnabledObservers)
			{
				_profiler.BeginObserver(observer);
				try
				{
					observer.OnEngineFrameUpdate();
				}
				catch (Exception e)
				{
					_profiler.RecordError(observer, LunyEngineLifecycleEvents.OnEngineUpdate, e);
					/* keep dispatch resilient */
					//LunyLogger.LogException(e, this);
					throw;
				}
				finally
				{
					_profiler.EndObserver(observer, LunyEngineLifecycleEvents.OnEngineUpdate);
				}
			}
		}

		void ILunyEngineLifecycle.EngineFrameLateUpdate(ILunyEngineNativeAdapter nativeAdapter)
		{
			ILunyEngineLifecycle.ThrowIfNotCurrentAdapter(nativeAdapter, s_EngineAdapter);

			foreach (var observer in _observerRegistry.EnabledObservers)
			{
				_profiler.BeginObserver(observer);
				try
				{
					observer.OnEngineFrameLateUpdate();
				}
				catch (Exception e)
				{
					_profiler.RecordError(observer, LunyEngineLifecycleEvents.OnEngineLateUpdate, e);
					/* keep dispatch resilient */
					//LunyLogger.LogException(e, this);
					throw;
				}
				finally
				{
					_profiler.EndObserver(observer, LunyEngineLifecycleEvents.OnEngineLateUpdate);
				}
			}

			RunEnginePostUpdate();
		}

		private void RunEnginePreUpdateOncePerFrame()
		{
			if (!_didCallPreUpdateThisFrame)
			{
				_didCallPreUpdateThisFrame = true;

				// engine services first
				_timeInternal.IncrementFrameCounters();
				_serviceRegistry.OnEnginePreUpdate();
				_objectLifecycle.OnEnginePreUpdate();

				foreach (var observer in _observerRegistry.EnabledObservers)
				{
					_profiler.BeginObserver(observer);
					try
					{
						observer.OnEngineFrameBegins();
					}
					catch (Exception e)
					{
						_profiler.RecordError(observer, LunyEngineLifecycleEvents.OnEnginePreUpdate, e);
						/* keep dispatch resilient */
						//LunyLogger.LogException(e, this);
						throw;
					}
					finally
					{
						_profiler.EndObserver(observer, LunyEngineLifecycleEvents.OnEnginePreUpdate);
					}
				}
			}
		}

		private void RunEnginePostUpdate()
		{
			foreach (var observer in _observerRegistry.EnabledObservers)
			{
				_profiler.BeginObserver(observer);
				try
				{
					observer.OnEngineFrameEnds();
				}
				catch (Exception e)
				{
					_profiler.RecordError(observer, LunyEngineLifecycleEvents.OnEnginePostUpdate, e);
					/* keep dispatch resilient */
					//LunyLogger.LogException(e, this);
					throw;
				}
				finally
				{
					_profiler.EndObserver(observer, LunyEngineLifecycleEvents.OnEnginePostUpdate);
				}
			}

			// run "structural changes" here ..
			_serviceRegistry.OnEnginePostUpdate();
			_objectLifecycle.OnEnginePostUpdate(); // should run last to guarantee object cleanup

			_didCallPreUpdateThisFrame = false;
		}
	}
}
