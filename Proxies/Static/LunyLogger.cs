using Luny.Diagnostics;
using Luny.Interfaces;
using Luny.Providers;
using System;
using System.Collections.Generic;
using System.IO;

namespace Luny.Proxies
{
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
		public static ILunyLogger Logger { get => _logger; set => _logger = value ?? _consoleLogger; }

		/// <summary>
		/// Enables internal Luny logging system. When enabled, all log messages are recorded
		/// in an internal list with frame/time information. Useful for in-game console, file logging, etc.
		/// </summary>
		public static Boolean EnableInternalLogging { get; set; }

		public static void LogInfo(String message, Object context = null)
		{
			RecordInternalLog(LogLevel.Info, message, context);
			_logger.LogInfo(FormatWithContext(message, context));
		}

		public static void LogWarning(String message, Object context = null)
		{
			RecordInternalLog(LogLevel.Warning, message, context);
			_logger.LogWarning(FormatWithContext(message, context));
		}

		public static void LogError(String message, Object context = null)
		{
			RecordInternalLog(LogLevel.Error, message, context);
			_logger.LogError(FormatWithContext(message, context));
		}

		public static void LogException(Exception exception, Object context = null)
		{
			RecordInternalLog(LogLevel.Error, exception?.ToString() ?? "null exception", exception?.GetType());

			// Preserve engine-native exception handling while still emitting a contextual header if provided
			if (context != null)
			{
				var header = FormatWithContext(exception?.Message, context);
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

		private static void RecordInternalLog(LogLevel level, String message, Object context)
		{
			if (!EnableInternalLogging)
				return;

			_internalLog ??= new List<LogEntry>();

			var timeService = LunyEngine.Instance?.GetService<ITimeServiceProvider>();

			_internalLog.Add(new LogEntry
			{
				FrameCount = timeService?.FrameCount ?? -1,
				ElapsedSeconds = timeService?.ElapsedSeconds ?? -1.0,
				Level = level,
				Message = message,
				Context = context switch
				{
					Type t => t.Name,
					String s => s,
					null => "null",
					var _ => context.GetType().Name,
				},
			});
		}

		private static String FormatWithContext(String message, Object context)
		{
			if (context == null)
				return message;

			var prefix = context switch
			{
				Type t => t.Name,
				String s => s,
				var _ => context.GetType().Name,
			};

			return prefix == null ? message : $"[{prefix}] {message}";
		}

		private sealed class ConsoleLogger : ILunyLogger
		{
			public void LogInfo(String message) => Console.WriteLine(message);
			public void LogWarning(String message) => Console.WriteLine(message);
			public void LogError(String message) => Console.WriteLine(message);
			public void LogException(Exception exception) => Console.WriteLine(exception?.ToString());
		}
	}
}
