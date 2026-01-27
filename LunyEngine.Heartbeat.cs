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
		public void OnEngineFixedStep(Double fixedDeltaTime, ILunyEngineNativeAdapter nativeAdapter)
		{
			ILunyEngineLifecycle.ThrowIfNotCurrentAdapter(nativeAdapter, s_EngineAdapter);
			RunEnginePreUpdateOnce();

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
		public void OnEngineUpdate(Double deltaTime, ILunyEngineNativeAdapter nativeAdapter)
		{
			ILunyEngineLifecycle.ThrowIfNotCurrentAdapter(nativeAdapter, s_EngineAdapter);
			RunEnginePreUpdateOnce();

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
		public void OnEngineLateUpdate(Double deltaTime, ILunyEngineNativeAdapter nativeAdapter)
		{
			ILunyEngineLifecycle.ThrowIfNotCurrentAdapter(nativeAdapter, s_EngineAdapter);

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
				_lifecycleManager.OnEnginePreUpdate();

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
					observer.OnEnginePostUpdate();
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
			_lifecycleManager.OnEnginePostUpdate(); // should run last to guarantee object cleanup

			_didCallPreUpdateThisFrame = false;
		}
	}
}
