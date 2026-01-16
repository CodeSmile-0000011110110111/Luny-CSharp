using Luny.Engine;
using Luny.Engine.Diagnostics;
using System;

namespace Luny
{
	public sealed partial class LunyEngine
	{
		private Boolean _didCallPreUpdate;

		/// <summary>
		/// CAUTION: Must only be called by engine-native lifecycle adapter!
		/// </summary>
		public void OnEngineStartup(ILunyEngineNativeAdapter nativeAdapter)
		{
			_timeInternal.IncrementLunyFrameCount(); // bump FrameCount
			LunyTraceLogger.LogInfoStartingUp(this);
			ILunyEngineLifecycle.ThrowIfNotCurrentAdapter(nativeAdapter, s_NativeAdapter);

			Startup();

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

			LunyTraceLogger.LogInfoStartupComplete(this);
		}

		/// <summary>
		/// CAUTION: Must only be called by engine-native lifecycle adapter!
		/// </summary>
		public void OnEngineShutdown(ILunyEngineNativeAdapter nativeAdapter)
		{
			LunyTraceLogger.LogInfoShuttingDown(this);
			ILunyEngineLifecycle.ThrowIfNotCurrentAdapter(nativeAdapter, s_NativeAdapter);

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

			Shutdown();

			LunyTraceLogger.LogInfoShutdownComplete(this);
		}

		/// <summary>
		/// CAUTION: Must only be called by engine-native lifecycle adapter!
		/// </summary>
		public void OnEngineFixedStep(Double fixedDeltaTime, ILunyEngineNativeAdapter nativeAdapter)
		{
			ILunyEngineLifecycle.ThrowIfNotCurrentAdapter(nativeAdapter, s_NativeAdapter);
			CheckAndCallPreUpdate();

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
		public void OnEngineUpdate(Double deltaTime, ILunyEngineNativeAdapter nativeAdapter)
		{
			ILunyEngineLifecycle.ThrowIfNotCurrentAdapter(nativeAdapter, s_NativeAdapter);
			CheckAndCallPreUpdate();

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
		public void OnEngineLateUpdate(Double deltaTime, ILunyEngineNativeAdapter nativeAdapter)
		{
			ILunyEngineLifecycle.ThrowIfNotCurrentAdapter(nativeAdapter, s_NativeAdapter);

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

			CallPostUpdate();
		}

		private void PreUpdateObservers()
		{
			foreach (var observer in _observerRegistry.EnabledObservers)
			{
				_profiler.BeginObserver(observer);
				try
				{
					observer.OnEnginePreUpdate();
				}
				catch (Exception e)
				{
					_profiler.RecordError(observer, LunyEngineLifecycleEvents.OnEnginePreUpdate, e);
					/* keep dispatch resilient */
					LunyLogger.LogException(e);
				}
				finally
				{
					_profiler.EndObserver(observer, LunyEngineLifecycleEvents.OnEnginePreUpdate);
				}
			}
		}

		private void PostUpdateObservers()
		{
			foreach (var observer in _observerRegistry.EnabledObservers)
			{
				_profiler.BeginObserver(observer);
				try
				{
					observer.OnEnginePostUpdate();
				}
				catch (Exception e)
				{
					_profiler.RecordError(observer, LunyEngineLifecycleEvents.OnEnginePostUpdate, e);
					/* keep dispatch resilient */
					LunyLogger.LogException(e);
				}
				finally
				{
					_profiler.EndObserver(observer, LunyEngineLifecycleEvents.OnEnginePostUpdate);
				}
			}
		}

		private void CheckAndCallPreUpdate()
		{
			if (!_didCallPreUpdate)
			{
				_didCallPreUpdate = true;

				PreUpdate(); // engine first
				PreUpdateObservers();
			}
		}

		private void CallPostUpdate()
		{
			PostUpdateObservers();
			PostUpdate(); // engine last
			_didCallPreUpdate = false;
		}
	}
}
