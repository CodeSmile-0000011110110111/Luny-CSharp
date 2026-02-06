using Luny.Engine;
using Luny.Engine.Diagnostics;
using System;

namespace Luny
{
	public sealed partial class LunyEngine
	{
		private Boolean _didCallPreUpdateThisFrame;

		/// <summary>
		/// CAUTION: Must only be called by engine-native lifecycle adapter!
		/// </summary>
		public void OnEngineHeartbeat(Double fixedDeltaTime, ILunyEngineNativeAdapter nativeAdapter)
		{
			ILunyEngineLifecycle.ThrowIfNotCurrentAdapter(nativeAdapter, s_EngineAdapter);
			RunEnginePreUpdateOnce();

			foreach (var observer in _observerRegistry.EnabledObservers)
			{
				_profiler.BeginObserver(observer);
				try
				{
					observer.OnEngineHeartbeat(fixedDeltaTime);
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
		public void OnEngineFrameUpdate(Double deltaTime, ILunyEngineNativeAdapter nativeAdapter)
		{
			ILunyEngineLifecycle.ThrowIfNotCurrentAdapter(nativeAdapter, s_EngineAdapter);
			RunEnginePreUpdateOnce();

			foreach (var observer in _observerRegistry.EnabledObservers)
			{
				_profiler.BeginObserver(observer);
				try
				{
					// TODO: check if enabled state changed to true, if so send OnEnable

					observer.OnEngineFrameUpdate(deltaTime);
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
		public void OnEngineFrameLateUpdate(Double deltaTime, ILunyEngineNativeAdapter nativeAdapter)
		{
			ILunyEngineLifecycle.ThrowIfNotCurrentAdapter(nativeAdapter, s_EngineAdapter);

			foreach (var observer in _observerRegistry.EnabledObservers)
			{
				_profiler.BeginObserver(observer);
				try
				{
					observer.OnEngineFrameLateUpdate(deltaTime);

					// TODO: check if enabled state changed to false, if so send OnDisable
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

		private void RunEnginePreUpdateOnce()
		{
			if (!_didCallPreUpdateThisFrame)
			{
				_didCallPreUpdateThisFrame = true;

				// engine first
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
