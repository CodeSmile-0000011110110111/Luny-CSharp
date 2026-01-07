using System;

namespace Luny.Engine.Services
{
	/// <summary>
	/// Engine-agnostic debug and logging service.
	/// CAUTION: Implementations must inherit from both IDebugService interface and DebugServiceBase class!
	/// </summary>
	public interface ILunyDebugService : ILunyEngineService
	{
		void LogInfo(String message);
		void LogWarning(String message);
		void LogError(String message);
		void LogException(Exception exception);
	}

	internal interface ILunyDebugServiceInternal {}

	public abstract class LunyDebugServiceBase : LunyEngineServiceBase, ILunyDebugServiceInternal {}
}
