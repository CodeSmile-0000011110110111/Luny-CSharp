using System;

namespace Luny.Interfaces
{
	public interface ILunyLogger
	{
		void LogInfo(String message);
		void LogWarning(String message);
		void LogError(String message);
		void LogException(Exception exception);
	}
}
