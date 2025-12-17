using System;

namespace Luny
{
	/// <summary>
	/// Engine-agnostic logger entry point. Delegates to an installable engine-specific logger.
	/// Default fallback logs to console until an engine replaces it.
	/// </summary>
	public static class LunyLogger
	{
		private static ILunyLogger _logger = new ConsoleLogger();

		/// <summary>
		/// Installs an engine-specific logger. Pass <c>null</c> to revert to the default console logger.
		/// </summary>
		public static void SetLogger(ILunyLogger logger) => _logger = logger ?? new ConsoleLogger();

		public static void LogInfo(String message, Object context = null) => _logger.LogInfo(FormatWithContext(message, context));
		public static void LogWarning(String message, Object context = null) => _logger.LogWarning(FormatWithContext(message, context));
		public static void LogError(String message, Object context = null) => _logger.LogError(FormatWithContext(message, context));

		public static void LogException(Exception exception, Object context = null)
		{
			// Preserve engine-native exception handling while still emitting a contextual header if provided
			if (context != null)
			{
				var header = FormatWithContext(exception?.Message, context);
				_logger.LogError(header);
			}
			_logger.LogException(exception);
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

	public interface ILunyLogger
	{
		void LogInfo(String message);
		void LogWarning(String message);
		void LogError(String message);
		void LogException(Exception exception);
	}
}
