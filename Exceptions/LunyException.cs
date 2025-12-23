using System;

namespace Luny.Exceptions
{
	/// <summary>
	/// Base class for all exceptions thrown by the Luny framework.
	/// </summary>
	public abstract class LunyException : Exception
	{
		protected LunyException(String message)
			: base(message) {}

		protected LunyException(String message, Exception innerException)
			: base(message, innerException) {}
	}
}
