using System;

namespace Luny.Services
{
	public interface IDebugService : IEngineService
	{
		void LogInfo(String message);
		void LogWarning(String message);
		void LogError(String message);
		void LogException(Exception exception);
	}
}
