using Luny.Engine.Bridge;
using System;

namespace Luny.Engine
{
	/// <summary>
	/// Lifecycle observer interface - receives callbacks from LunyEngine.
	/// </summary>
	public interface ILunyEngineObserver
	{
		public Boolean Enabled => true;

		/// <summary>
		/// Runs once per lifetime, when the application launches. Processing of the first frame has not begun.
		/// </summary>
		void OnEngineStartup();
		/// <summary>
		/// Runs once per frame, before both frame update / heartbeat methods.
		/// </summary>
		void OnEngineFrameBegins() {}
		/// <summary>
		/// Runs on the engine's fixed stepping frequency. Most suitable for deterministic game logic and to modify the Physics simulation.
		/// </summary>
		/// <remarks>
		/// Engine internal physics simulation occurs right after a Heartbeat and before FrameUpdate.
		///
		/// Caution: Heartbeat frequency depends on engine time-stepping settings, and is not guaranteed to be in sync with FrameRate.
		/// The behaviour depends on each frame's delta time (frame rate) and the engine's fixed step (or physics) time setting.
		///		- Heartbeat may be called less often than FrameUpdate
		///		- Heartbeat may be called several times in a single frame (all before FrameUpdate)
		/// </remarks>
		void OnEngineHeartbeat();
		/// <summary>
		/// Runs once per frame. If the current frame runs a Heartbeat, the Heartbeat would have run right before.
		/// </summary>
		void OnEngineFrameUpdate();
		/// <summary>
		/// Runs once per frame, after all FrameUpdate ran.
		/// </summary>
		void OnEngineFrameLateUpdate() {}
		/// <summary>
		/// Runs once per frame, after all FrameUpdate and FrameLateUpdate ran.
		/// </summary>
		void OnEngineFrameEnds() {}
		/// <summary>
		/// Runs once per lifetime, when the application quits. Runs after all of the current frame's Heartbeat/Update methods ran.
		/// </summary>
		void OnEngineShutdown();

		/// <summary>
		/// Runs when a scene was loaded, before frame processing begins. This includes the first scene which the engine loads automatically.
		/// </summary>
		/// <param name="loadedScene"></param>
		void OnSceneLoaded(ILunyScene loadedScene) {}
		/// <summary>
		/// Runs when a scene was unloaded. All objects have been invalidated, if not destroyed.
		/// </summary>
		/// <param name="unloadedScene"></param>
		void OnSceneUnloaded(ILunyScene unloadedScene) {}

		/// <summary>
		/// Called when a LunyObject registered with LunyEngine, typically during creation.
		/// In future it may also be called for transferring native object ownership to LunyEngine.
		/// </summary>
		/// <param name="lunyObject"></param>
		void OnObjectRegistered(ILunyObject lunyObject) {}
		/// <summary>
		/// Called when a LunyObject has unregistered from LunyEngine, typically during destruction.
		/// In future it may also be called for releasing ownership of native objects from LunyEngine.
		/// </summary>
		/// <param name="lunyObject"></param>
		void OnObjectUnregistered(ILunyObject lunyObject) {}
	}
}
