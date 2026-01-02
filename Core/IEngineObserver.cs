using System;

namespace Luny
{
	/// <summary>
	/// Lifecycle observer interface - receives callbacks from LunyEngine.
	/// </summary>
	public interface IEngineObserver
	{
		public bool Enabled => true;

		// Lifecycle observer interface - receives callbacks from LunyEngine
		void OnStartup();
		void OnFixedStep(Double fixedDeltaTime);
		void OnUpdate(Double deltaTime);
		void OnLateUpdate(Double deltaTime);
		void OnShutdown();
	}
}
