using System;

namespace Luny
{
	/// <summary>
	/// Engine-agnostic logger entry point. Delegates to an installable engine-specific logger.
	/// Default fallback logs to console until an engine replaces it.
	/// </summary>
	public static class LunyLog
	{
		private static ILunyLogger _logger = new ConsoleLogger();

		/// <summary>
		/// Installs an engine-specific logger. Pass <c>null</c> to revert to the default console logger.
		/// </summary>
		public static void SetLogger(ILunyLogger logger) => _logger = logger ?? new ConsoleLogger();

		public static void Info(String message) => _logger.Info(message);
		public static void Warn(String message) => _logger.Warn(message);
		public static void Error(String message) => _logger.Error(message);
		public static void Exception(Exception exception) => _logger.Exception(exception);

		private sealed class ConsoleLogger : ILunyLogger
		{
			public void Info(String message) => Console.WriteLine(message);
			public void Warn(String message) => Console.WriteLine(message);
			public void Error(String message) => Console.WriteLine(message);
			public void Exception(Exception exception) => Console.WriteLine(exception?.ToString());
		}
	}

	public interface ILunyLogger
	{
		void Info(String message);
		void Warn(String message);
		void Error(String message);
		void Exception(Exception exception);
	}
}
