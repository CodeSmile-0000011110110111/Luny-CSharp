using System;

namespace Luny.Tests
{
	/// <summary>
	/// Exception thrown when a test contract is violated during smoke testing.
	/// </summary>
	public sealed class LunyTestContractViolationException : Exception
	{
		public LunyTestContractViolationException(String message) : base(message)
		{
		}
	}
}
