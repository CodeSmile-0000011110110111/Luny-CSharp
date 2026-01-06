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
		void OnEngineFixedStep(Double fixedDeltaTime);
		void OnEngineUpdate(Double deltaTime);
		void OnEngineLateUpdate(Double deltaTime);
		void OnEngineShutdown();
	}
}
