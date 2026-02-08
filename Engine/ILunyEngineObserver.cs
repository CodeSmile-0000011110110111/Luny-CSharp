using Luny.Engine.Bridge;
using Luny.Engine.Services;
using System;

namespace Luny.Engine
{
	/// <summary>
	/// Lifecycle observer interface - receives callbacks from LunyEngine.
	/// </summary>
	public interface ILunyEngineObserver
	{
		public Boolean Enabled => true;

		void OnEngineStartup();
		void OnEngineFrameBegins() {}
		void OnEngineHeartbeat();
		void OnEngineFrameUpdate();
		void OnEngineFrameLateUpdate() {}
		void OnEngineFrameEnds() {}
		void OnEngineShutdown();

		void OnSceneLoaded(ILunyScene loadedScene) {}
		void OnSceneUnloaded(ILunyScene unloadedScene) {}

		void OnObjectCreated(ILunyObject lunyObject) {}
		void OnObjectDestroyed(ILunyObject lunyObject) {}
	}
}
