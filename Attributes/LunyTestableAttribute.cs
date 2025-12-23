using System;

namespace Luny.Attributes
{
	/// <summary>
	/// Marks a class as testable, meaning it will only be instantiated in smoke test scenarios.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class LunyTestableAttribute : Attribute {}
}
