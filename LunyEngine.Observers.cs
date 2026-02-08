using Luny.Engine;
using Luny.Engine.Bridge;
using Luny.Engine.Diagnostics;
using System;

namespace Luny
{
	public sealed partial class LunyEngine
	{
		public void EnableObserver<T>() where T : ILunyEngineObserver => _observerRegistry.EnableObserver<T>();
		public void DisableObserver<T>() where T : ILunyEngineObserver => _observerRegistry.DisableObserver<T>();
		public Boolean IsObserverEnabled<T>() where T : ILunyEngineObserver => _observerRegistry.IsObserverEnabled<T>();
		public T GetObserver<T>() where T : ILunyEngineObserver => _observerRegistry.GetObserver<T>();

		private void InvokeObserversOnSceneUnloaded(ILunyScene loadedScene)
		{
			foreach (var observer in _observerRegistry.EnabledObservers)
			{
				_profiler.BeginObserver(observer);
				try
				{
					observer.OnSceneUnloaded(loadedScene);
				}
				catch (Exception e)
				{
					_profiler.RecordError(observer, LunyEngineLifecycleEvents.OnEngineSceneUnloaded, e);
					/* keep dispatch resilient */
					LunyLogger.LogException(e);
					throw;
				}
				finally
				{
					_profiler.EndObserver(observer, LunyEngineLifecycleEvents.OnEngineSceneUnloaded);
				}
			}
		}

		private void InvokeObserversOnSceneLoaded(ILunyScene loadedScene)
		{
			foreach (var observer in _observerRegistry.EnabledObservers)
			{
				_profiler.BeginObserver(observer);
				try
				{
					observer.OnSceneLoaded(loadedScene);
				}
				catch (Exception e)
				{
					_profiler.RecordError(observer, LunyEngineLifecycleEvents.OnEngineSceneLoaded, e);
					/* keep dispatch resilient */
					LunyLogger.LogException(e);
					throw;
				}
				finally
				{
					_profiler.EndObserver(observer, LunyEngineLifecycleEvents.OnEngineSceneLoaded);
				}
			}
		}

		private void InvokeObserversOnObjectRegistered(ILunyObject lunyObject)
		{
			foreach (var observer in _observerRegistry.EnabledObservers)
			{
				_profiler.BeginObserver(observer);
				try
				{
					observer.OnObjectRegistered(lunyObject);
				}
				catch (Exception e)
				{
					_profiler.RecordError(observer, LunyEngineLifecycleEvents.OnObjectCreated, e);
					LunyLogger.LogException(e);
				}
				finally
				{
					_profiler.EndObserver(observer, LunyEngineLifecycleEvents.OnObjectCreated);
				}
			}
		}

		private void InvokeObserversOnObjectUnregistered(ILunyObject lunyObject)
		{
			foreach (var observer in _observerRegistry.EnabledObservers)
			{
				_profiler.BeginObserver(observer);
				try
				{
					observer.OnObjectUnregistered(lunyObject);
				}
				catch (Exception e)
				{
					_profiler.RecordError(observer, LunyEngineLifecycleEvents.OnObjectDestroyed, e);
					LunyLogger.LogException(e);
				}
				finally
				{
					_profiler.EndObserver(observer, LunyEngineLifecycleEvents.OnObjectDestroyed);
				}
			}
		}
	}
}
