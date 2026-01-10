using System;
using System.Diagnostics;

namespace Luny
{
	public static class LunyTraceLogger
	{
		[DebuggerHidden] [Conditional("DEBUG")] [Conditional("LUNY_DEBUG")]
		public static void LogInfoInitializing(Object source)
		{
#if DEBUG || LUNY_DEBUG
			LunyLogger.LogInfo("Initializing...", source);
#endif
		}

		[DebuggerHidden] [Conditional("DEBUG")] [Conditional("LUNY_DEBUG")]
		public static void LogInfoInitialized(Object source)
		{
#if DEBUG || LUNY_DEBUG
			LunyLogger.LogInfo("Initialization complete.", source);
#endif
		}

		[DebuggerHidden] [Conditional("DEBUG")] [Conditional("LUNY_DEBUG")]
		public static void LogInfoStartingUp(Object source)
		{
#if DEBUG || LUNY_DEBUG
			LunyLogger.LogInfo("Starting up...", source);
#endif
		}

		[DebuggerHidden] [Conditional("DEBUG")] [Conditional("LUNY_DEBUG")]
		public static void LogInfoStartupComplete(Object source)
		{
#if DEBUG || LUNY_DEBUG
			LunyLogger.LogInfo("Startup complete.", source);
#endif
		}

		[DebuggerHidden] [Conditional("DEBUG")] [Conditional("LUNY_DEBUG")]
		public static void LogInfoShuttingDown(Object source)
		{
#if DEBUG || LUNY_DEBUG
			LunyLogger.LogInfo("Shutting down...", source);
#endif
		}

		[DebuggerHidden] [Conditional("DEBUG")] [Conditional("LUNY_DEBUG")]
		public static void LogInfoShutdownComplete(Object source)
		{
#if DEBUG || LUNY_DEBUG
			LunyLogger.LogInfo("Shutdown complete.", source);
#endif
		}

		[DebuggerHidden] [Conditional("DEBUG")] [Conditional("LUNY_DEBUG")]
		public static void LogInfoDisposing(Object source)
		{
#if DEBUG || LUNY_DEBUG
			LunyLogger.LogInfo("Disposing...", source);
#endif
		}

		[DebuggerHidden] [Conditional("DEBUG")] [Conditional("LUNY_DEBUG")]
		public static void LogInfoDisposed(Object source)
		{
#if DEBUG || LUNY_DEBUG
			LunyLogger.LogInfo("Disposed.", source);
#endif
		}

		[DebuggerHidden] [Conditional("DEBUG")] [Conditional("LUNY_DEBUG")]
		public static void LogInfoDestroying(Object source)
		{
#if DEBUG || LUNY_DEBUG
			LunyLogger.LogInfo("Destroying...", source);
#endif
		}

		[DebuggerHidden] [Conditional("DEBUG")] [Conditional("LUNY_DEBUG")]
		public static void LogInfoDestroyed(Object source)
		{
#if DEBUG || LUNY_DEBUG
			LunyLogger.LogInfo("Destroyed. This is the end.", source);
#endif
		}

		[DebuggerHidden] [Conditional("DEBUG")] [Conditional("LUNY_DEBUG")]
		public static void LogInfoFinalized(Object source)
		{
#if DEBUG || LUNY_DEBUG
			LunyLogger.LogInfo($"finalized {source?.GetHashCode()}", source);
#endif
		}

		[DebuggerHidden] [Conditional("DEBUG")] [Conditional("LUNY_DEBUG")]
		public static void LogInfoCreateSingletonInstance(Type type)
		{
#if DEBUG || LUNY_DEBUG
			LunyLogger.LogInfo("Creating singleton instance...", type);
#endif
		}

		[DebuggerHidden] [Conditional("DEBUG")] [Conditional("LUNY_DEBUG")]
		public static void LogInfoEventCallback(String eventName, String message, Object source)
		{
#if DEBUG || LUNY_DEBUG
			LunyLogger.LogInfo($"{eventName}: {message}", source);
#endif
		}

		[DebuggerHidden] [Conditional("DEBUG")] [Conditional("LUNY_DEBUG")]
		public static void LogTrace(String message, Object source)
		{
#if DEBUG || LUNY_DEBUG
			LunyLogger.LogInfo(message, source);
#endif
		}
	}
}
