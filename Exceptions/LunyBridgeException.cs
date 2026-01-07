using System;

namespace Luny.Exceptions
{
	/// <summary>
	/// Exception thrown when a bridging exception occurs in the Luny framework,
	/// such as initializing a proxy object with a null reference.
	/// </summary>
	public sealed class LunyBridgeException : LunyException
	{
		public LunyBridgeException(String message)
			: base(message) {}
	}
}
