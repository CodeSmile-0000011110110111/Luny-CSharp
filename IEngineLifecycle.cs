using System;

namespace Luny
{
	/// <summary>
	/// Lifecycle observer interface - receives callbacks from dispatcher.
	/// </summary>
	public interface IEngineLifecycle
	{
		// Lifecycle observer interface - receives callbacks from dispatcher
		void OnStartup();
		void OnUpdate(Double deltaTime);
		void OnLateUpdate(Double deltaTime);
		void OnFixedStep(Double fixedDeltaTime);
		void OnShutdown();
	}
}
