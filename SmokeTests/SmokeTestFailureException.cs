using System;

namespace Luny.Tests
{
	/// <summary>
	/// Exception thrown when a test contract is violated during smoke testing.
	/// </summary>
	internal sealed class SmokeTestFailureException : Exception
	{
		public SmokeTestFailureException(String message)
			: base(message) {}
	}
}
