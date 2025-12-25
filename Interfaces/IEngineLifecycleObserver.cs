using System;

namespace Luny.Interfaces
{
	/// <summary>
	/// Lifecycle observer interface - receives callbacks from LunyEngine.
	/// </summary>
	public interface IEngineLifecycleObserver
	{
		public bool Enabled => true;

		// Lifecycle observer interface - receives callbacks from LunyEngine
		void OnStartup(ILunyEngine engine);
		void OnFixedStep(Double fixedDeltaTime);
		void OnUpdate(Double deltaTime);
		void OnLateUpdate(Double deltaTime);
		void OnShutdown();
	}
}
