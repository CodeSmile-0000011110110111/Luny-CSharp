using Luny.Engine;
using Luny.Engine.Diagnostics;
using Luny.Engine.Services;
using System;

namespace Luny
{
	public sealed partial class LunyEngine
	{
		private Boolean _didCallPreUpdateThisFrame;

		/// <summary>
		/// CAUTION: Must only be called by engine-native lifecycle adapter!
		/// </summary>
		public void EngineHeartbeat(ILunyEngineNativeAdapter nativeAdapter, Double fixedDeltaTime)
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
					_profiler.RecordError(observer, LunyEngineLifecycleEvents.OnEngineFixedStep, e);
					/* keep dispatch resilient */
					LunyLogger.LogException(e, this);
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
		public void EngineFrameUpdate(ILunyEngineNativeAdapter nativeAdapter, Double deltaTime)
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
					LunyLogger.LogException(e, this);
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
		public void EngineFrameLateUpdate(ILunyEngineNativeAdapter nativeAdapter)
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
					LunyLogger.LogException(e, this);
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
				_lifecycle.OnEnginePreUpdate();

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
						LunyLogger.LogException(e, this);
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
					LunyLogger.LogException(e, this);
				}
				finally
				{
					_profiler.EndObserver(observer, LunyEngineLifecycleEvents.OnEnginePostUpdate);
				}
			}

			// run "structural changes" here ..
			_serviceRegistry.OnEnginePostUpdate();
			_lifecycle.OnEnginePostUpdate(); // should run last to guarantee object cleanup

			_didCallPreUpdateThisFrame = false;
		}
	}
}
