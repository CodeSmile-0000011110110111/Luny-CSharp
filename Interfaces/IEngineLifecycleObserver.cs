using System;

namespace Luny.Interfaces
{
	/// <summary>
	/// Lifecycle observer interface - receives callbacks from LunyEngine.
	/// </summary>
	public interface IEngineLifecycleObserver
	{
		// Lifecycle observer interface - receives callbacks from LunyEngine
		void OnStartup();
		void OnFixedStep(Double fixedDeltaTime);
		void OnUpdate(Double deltaTime);
		void OnLateUpdate(Double deltaTime);
		void OnShutdown();
	}
}
