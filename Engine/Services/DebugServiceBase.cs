using System;

namespace Luny.Engine.Services
{
	/// <summary>
	/// Engine-agnostic debug and logging service.
	/// CAUTION: Implementations must inherit from both IDebugService interface and DebugServiceBase class!
	/// </summary>
	public interface IDebugService : IEngineService
	{
		void LogInfo(String message);
		void LogWarning(String message);
		void LogError(String message);
		void LogException(Exception exception);
	}

	internal interface IDebugServiceInternal {}

	public abstract class DebugServiceBase : IDebugServiceInternal {}
}
