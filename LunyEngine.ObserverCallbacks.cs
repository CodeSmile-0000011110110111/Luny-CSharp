using Luny.Engine.Bridge;
using Luny.Engine.Diagnostics;
using System;

namespace Luny
{
	public sealed partial class LunyEngine
	{
		private void OnSceneLoaded(ILunyScene loadedScene) // called by SceneService
		{
			LunyTraceLogger.LogInfoEventCallback(nameof(OnSceneLoaded), loadedScene?.ToString(), this);
			InvokeObserversOnSceneLoaded(loadedScene);
		}

		private void OnSceneUnloaded(ILunyScene unloadedScene) // called by SceneService
		{
			LunyTraceLogger.LogInfoEventCallback(nameof(OnSceneLoaded), unloadedScene?.ToString(), this);
			_objectRegistry.OnSceneUnloaded(unloadedScene);
			InvokeObserversOnSceneUnloaded(unloadedScene);
		}

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
					//LunyLogger.LogException(e);
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
					//LunyLogger.LogException(e);
					throw;
				}
				finally
				{
					_profiler.EndObserver(observer, LunyEngineLifecycleEvents.OnEngineSceneLoaded);
				}
			}
		}

		// called by LunyObjectRegistry
		internal void ObjectRegistered(ILunyObject lunyObject) => InvokeObserversOnObjectRegistered(lunyObject);
		internal void ObjectUnregistered(ILunyObject lunyObject) => InvokeObserversOnObjectUnregistered(lunyObject);

		private void InvokeObserversOnObjectRegistered(ILunyObject lunyObject)
		{
			LunyLogger.LogInfo($"Registered: {lunyObject}", this);

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
					//LunyLogger.LogException(e);
					throw;
				}
				finally
				{
					_profiler.EndObserver(observer, LunyEngineLifecycleEvents.OnObjectCreated);
				}
			}
		}

		private void InvokeObserversOnObjectUnregistered(ILunyObject lunyObject)
		{
			LunyLogger.LogInfo($"Unregistered: {lunyObject}", this);

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
					//LunyLogger.LogException(e);
					throw;
				}
				finally
				{
					_profiler.EndObserver(observer, LunyEngineLifecycleEvents.OnObjectDestroyed);
				}
			}
		}
	}
}
