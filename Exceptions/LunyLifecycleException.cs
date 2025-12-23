using System;

namespace Luny.Exceptions
{
	/// <summary>
	/// Exception thrown when a lifecycle-related error occurs in the Luny framework,
	/// such as singleton duplication or unexpected adapter removal.
	/// </summary>
	public sealed class LunyLifecycleException : LunyException
	{
		public LunyLifecycleException(String message)
			: base(message) {}
	}
}
