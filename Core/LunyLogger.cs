using Luny.Engine.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Luny
{
	public interface ILunyLogger
	{
		void LogInfo(Object obj);
		void LogWarning(Object obj);
		void LogError(Object obj);
		void LogInfo(String message);
		void LogWarning(String message);
		void LogError(String message);
		void LogException(Exception exception);
	}

	/// <summary>
	/// Represents a single log entry in the internal Luny logging system.
	/// Uses FrameCount and ElapsedSeconds instead of DateTime for precise game-time tracking.
	/// </summary>
	public struct LogEntry
	{
		/// <summary>
		/// Frame number when this log entry was created (from ITimeServiceProvider).
		/// -1 if time service not available.
		/// </summary>
		public Int64 FrameCount;

		/// <summary>
		/// Elapsed time in seconds since application start (from ITimeServiceProvider).
		/// -1.0 if time service not available.
		/// </summary>
		public Double ElapsedSeconds;

		/// <summary>
		/// Severity level of the log message.
		/// </summary>
		public LogLevel Level;

		/// <summary>
		/// The log message content.
		/// </summary>
		public String Message;

		/// <summary>
		/// Context type name that generated this log (e.g., "LunyScriptRunner", "LogMessageBlock").
		/// </summary>
		public String Context;

		public override String ToString() => $"[F{FrameCount:D8}] [{ElapsedSeconds:F3}s] [{Level}] [{Context}] {Message}";
	}

	/// <summary>
	/// Log message severity levels.
	/// </summary>
	public enum LogLevel
	{
		Info,
		Warning,
		Error,
		Exception,
	}

	/// <summary>
	/// Engine-agnostic logger entry point. Delegates to an installable engine-specific logger.
	/// Default fallback logs to console until an engine replaces it.
	/// Also provides internal logging system for Luny-specific logs (opt-in).
	/// </summary>
	public static class LunyLogger
	{
		private const String Null = "<null>";
		private static readonly ILunyLogger _consoleLogger = new ConsoleLogger();
		private static ILunyLogger _logger = _consoleLogger;

		// Internal logging system (opt-in)
		private static List<LogEntry> _internalLog;

		/// <summary>
		/// Installs an engine-specific logger. Pass <c>null</c> to revert to the default console logger.
		/// </summary>
		public static ILunyLogger Logger { get => _logger; set => _logger = value ?? _consoleLogger; }

		/// <summary>
		/// Enables internal Luny logging system. When enabled, all log messages are recorded
		/// in an internal list with frame/time information. Useful for in-game console, file logging, etc.
		/// </summary>
		public static Boolean EnableInternalLogging { get; set; }

		public static void LogInfo(Object obj, Object context = null) =>
			LogMessage(obj != null ? obj.ToString() : Null, LogLevel.Info, context);

		public static void LogWarning(Object obj, Object context = null) =>
			LogMessage(obj != null ? obj.ToString() : Null, LogLevel.Warning, context);

		public static void LogError(Object obj, Object context = null) =>
			LogMessage(obj != null ? obj.ToString() : Null, LogLevel.Error, context);

		public static void LogInfo(String message, Object context = null) => LogMessage(message, LogLevel.Info, context);
		public static void LogWarning(String message, Object context = null) => LogMessage(message, LogLevel.Warning, context);
		public static void LogError(String message, Object context = null) => LogMessage(message, LogLevel.Error, context);
		public static void LogException(Exception exception, Object context = null) => LogMessage(null, LogLevel.Exception, context, exception);

		/// <summary>
		/// Gets the internal log entries. Returns empty array if EnableInternalLogging is false or no logs recorded.
		/// </summary>
		public static IReadOnlyList<LogEntry> GetInternalLog() => _internalLog ?? (IReadOnlyList<LogEntry>)Array.Empty<LogEntry>();

		/// <summary>
		/// Clears all internal log entries.
		/// </summary>
		public static void ClearInternalLog() => _internalLog?.Clear();

		/// <summary>
		/// Writes the internal log to a file. Format: [FrameXXXXXXXX] [Time] [Level] [Context] Message
		/// </summary>
		public static void WriteInternalLogToFile(String path)
		{
			if (_internalLog == null || _internalLog.Count == 0)
				return;

			using var writer = new StreamWriter(path);
			foreach (var entry in _internalLog)
				writer.WriteLine(entry.ToString());
		}

		private static void LogMessage(String message, LogLevel logLevel, Object context = null, Exception exception = null)
		{
			var time = LunyEngine.Instance?.Time;
			message ??= exception?.ToString();
			RecordInternalLog(logLevel, message, context, time);
			var formattedMessage = FormatWithContext(message, context, time);
			switch (logLevel)
			{
				case LogLevel.Info:
					_logger.LogInfo(formattedMessage);
					break;
				case LogLevel.Warning:
					_logger.LogWarning(formattedMessage);
					break;
				case LogLevel.Error:
					_logger.LogError(formattedMessage);
					break;
				case LogLevel.Exception:
					_logger.LogException(exception);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, context?.ToString());
			}
		}

		private static String FormatWithContext(String message, Object context = null, ILunyTimeService time = null)
		{
			var prefix = context switch
			{
				Type t => $"[{t.Name}] ",
				String s => $"[{s}] ",
				var _ => context != null ? $"[{context.GetType().Name}] " : String.Empty,
			};

			var frameCount = time == null ? String.Empty : $"[{time.FrameCount}] ";

			return $"{frameCount}{prefix}{message}";
		}

		private static void RecordInternalLog(LogLevel level, String message, Object context = null, ILunyTimeService time = null)
		{
			if (!EnableInternalLogging)
				return;

			var frameCount = -1L;
			var elapsedSeconds = -1.0;
			if (time != null)
			{
				frameCount = time.EngineFrameCount;
				elapsedSeconds = time.ElapsedSeconds;
			}

			_internalLog ??= new List<LogEntry>();
			_internalLog.Add(new LogEntry
			{
				FrameCount = frameCount,
				ElapsedSeconds = elapsedSeconds,
				Level = level,
				Message = message,
				Context = context switch
				{
					Type t => t.Name,
					String s => s,
					null => null,
					var _ => context.GetType().Name,
				},
			});
		}

		private sealed class ConsoleLogger : ILunyLogger
		{
			public void LogInfo(Object obj) => LogInfo(obj != null ? obj.ToString() : Null);
			public void LogWarning(Object obj) => LogWarning(obj != null ? obj.ToString() : Null);
			public void LogError(Object obj) => LogError(obj != null ? obj.ToString() : Null);
			public void LogInfo(String message) => Console.WriteLine(message);
			public void LogWarning(String message) => Console.WriteLine($"[WARN] {message}");
			public void LogError(String message) => Console.WriteLine($"[ERROR] {message}");
			public void LogException(Exception exception) => Console.WriteLine($"[EXCEPTION] {exception}");
		}
	}

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
			LunyLogger.LogWarning($"LEAKED: ({source?.GetHashCode()}) or did not call Shutdown? => investigate!", source);
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
