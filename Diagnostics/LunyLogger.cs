using Luny.Services;
using System;
using System.Collections.Generic;
using System.IO;

namespace Luny.Diagnostics
{
	public interface ILunyLogger
	{
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
	}

	/// <summary>
	/// Engine-agnostic logger entry point. Delegates to an installable engine-specific logger.
	/// Default fallback logs to console until an engine replaces it.
	/// Also provides internal logging system for Luny-specific logs (opt-in).
	/// </summary>
	public static class LunyLogger
	{
		private static readonly ILunyLogger _consoleLogger = new ConsoleLogger();
		private static ILunyLogger _logger = _consoleLogger;

		// Internal logging system (opt-in)
		private static List<LogEntry> _internalLog;

		/// <summary>
		/// Installs an engine-specific logger. Pass <c>null</c> to revert to the default console logger.
		/// </summary>
		public static ILunyLogger Logger
		{
			get => _logger;
			set
			{
				_logger = value ?? _consoleLogger;
				_logger?.LogInfo($"{nameof(LunyLogger)}.{nameof(Logger)} = {_logger}");
			}
		}

		/// <summary>
		/// Enables internal Luny logging system. When enabled, all log messages are recorded
		/// in an internal list with frame/time information. Useful for in-game console, file logging, etc.
		/// </summary>
		public static Boolean EnableInternalLogging { get; set; }

		public static void LogInfo(String message, Object context = null)
		{
			var time = LunyEngine.Instance?.Time;
			RecordInternalLog(LogLevel.Info, message, context, time);
			_logger.LogInfo(FormatWithContext(message, context, time));
		}

		public static void LogWarning(String message, Object context = null)
		{
			var time = LunyEngine.Instance?.Time;
			RecordInternalLog(LogLevel.Warning, message, context, time);
			_logger.LogWarning(FormatWithContext(message, context, time));
		}

		public static void LogError(String message, Object context = null)
		{
			var time = LunyEngine.Instance?.Time;
			RecordInternalLog(LogLevel.Error, message, context, time);
			_logger.LogError(FormatWithContext(message, context, time));
		}

		public static void LogException(Exception exception, Object context = null)
		{
			var time = LunyEngine.Instance?.Time;
			RecordInternalLog(LogLevel.Error, exception?.ToString() ?? "null exception", exception?.GetType(), time);

			// Preserve engine-native exception handling while still emitting a contextual header if provided
			if (context != null)
			{
				var header = FormatWithContext(exception?.Message, context, time);
				_logger.LogError(header);
			}
			_logger.LogException(exception);
		}

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

		private static void RecordInternalLog(LogLevel level, String message, Object context = null, ITimeService time = null)
		{
			if (!EnableInternalLogging)
				return;

			var frameCount = -1L;
			var elapsedSeconds = -1.0;
			if (time != null)
			{
				frameCount = time.FrameCount;
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

		private static String FormatWithContext(String message, Object context = null, ITimeService time = null)
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

		private sealed class ConsoleLogger : ILunyLogger
		{
			public void LogInfo(String message) => Console.WriteLine(message);
			public void LogWarning(String message) => Console.WriteLine($"[Warning] {message}");
			public void LogError(String message) => Console.WriteLine($"[ERROR] {message}");
			public void LogException(Exception exception) => LogError(exception.ToString());
		}
	}
}
