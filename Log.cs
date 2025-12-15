using System;

namespace Luny
{
	public static class Log
	{
		public static void Error(string message)
		{
			// TODO: hook into engine logging
			Console.WriteLine(message);
		}
		public static void Exception(Exception e)
		{
			// TODO: hook into engine logging
			Console.WriteLine(e.ToString());
		}

		public static void Info(String message)
		{
			// TODO: logging
			Console.WriteLine(message);
		}
	}
}
