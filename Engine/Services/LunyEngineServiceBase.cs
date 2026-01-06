namespace Luny.Engine.Services
{
	/// <summary>
	/// Marker interface for engine-agnostic services: APIs such as Debug, Input, etc.
	/// Implementations are auto-discovered and registered at startup.
	/// </summary>
	public interface ILunyEngineService {}

	public abstract class LunyEngineServiceBase
	{
		internal void OnEngineStartup()
		{
			LunyTraceLogger.LogInfoStartingUp(this);
			OnServiceStartup();
			LunyTraceLogger.LogInfoStartupComplete(this);
		}

		internal void OnEngineShutdown()
		{
			LunyTraceLogger.LogInfoShuttingDown(this);
			OnServiceShutdown();
			LunyTraceLogger.LogInfoShutdownComplete(this);
		}

		protected abstract void OnServiceStartup();
		protected abstract void OnServiceShutdown();
	}
}
