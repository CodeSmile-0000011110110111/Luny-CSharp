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

		void OnEngineStartup();
		void OnEnginePreUpdate() {}
		void OnEngineFixedStep(Double fixedDeltaTime);
		void OnEngineUpdate(Double deltaTime);
		void OnEngineLateUpdate(Double deltaTime);
		void OnEnginePostUpdate() {}
		void OnEngineShutdown();

		void OnSceneLoaded(ILunyScene loadedScene) {}
		void OnSceneUnloaded(ILunyScene unloadedScene) {}

		void OnObjectCreated(ILunyObject lunyObject) {}
		void OnObjectDestroyed(ILunyObject lunyObject) {}
	}
}
