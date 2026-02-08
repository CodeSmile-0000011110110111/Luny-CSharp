using System;
using System.Runtime.CompilerServices;

namespace Luny.Engine.Services
{
	/// <summary>
	/// Engine-agnostic debug and logging service.
	/// </summary>
	/// <remarks>
	/// IMPORTANT: Implementations must inherit from both the ILuny***Service interface and its corresponding Luny***ServiceBase class!
	/// </remarks>
	public interface ILunyDebugService : ILunyEngineService
	{
		static String GetMethodName([CallerMemberName] String name = "") => name;

		void LogInfo(String message);
		void LogWarning(String message);
		void LogError(String message);
		void LogException(Exception exception);
	}

	internal interface ILunyDebugServiceInternal {}

	public abstract class LunyDebugServiceBase : LunyEngineServiceBase, ILunyDebugService, ILunyDebugServiceInternal
	{
		public abstract void LogInfo(String message);
		public abstract void LogWarning(String message);
		public abstract void LogError(String message);
		public abstract void LogException(Exception exception);
	}
}
