using System;

namespace Luny.Engine
{
	/// <summary>
	/// Marker interface for engine-agnostic services: APIs such as Debug, Input, etc.
	/// Implementations are auto-discovered and registered at startup.
	/// </summary>
	public interface ILunyEngineService {}

	public abstract class LunyEngineServiceBase
	{
		internal void Initialize()
		{
			LunyTraceLogger.LogInfoInitializing(this);
			OnServiceInitialize();
			LunyTraceLogger.LogInfoInitialized(this);
		}
		internal void Startup()
		{
			LunyTraceLogger.LogInfoStartingUp(this);
			OnServiceStartup();
			LunyTraceLogger.LogInfoStartupComplete(this);
		}


		internal void Shutdown()
		{
			LunyTraceLogger.LogInfoShuttingDown(this);
			OnServiceShutdown();
			LunyTraceLogger.LogInfoShutdownComplete(this);
		}

		internal void PreUpdate()
		{
			// OnServicePreUpdate();
		}
		internal void PostUpdate()
		{
			// OnServicePostUpdate();
		}

		protected abstract void OnServiceInitialize();
		protected abstract void OnServiceStartup();
		protected abstract void OnServiceShutdown();
		// protected abstract void OnServicePreUpdate();
		// protected abstract void OnServicePostUpdate();
	}
}
