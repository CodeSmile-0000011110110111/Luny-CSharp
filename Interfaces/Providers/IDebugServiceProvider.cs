using System;

namespace Luny.Interfaces.Providers
{
	public interface IDebugServiceProvider : IEngineServiceProvider
	{
		void LogInfo(String message);
		void LogWarning(String message);
		void LogError(String message);
		void LogException(Exception exception);
	}
}
