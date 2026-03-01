using System;

namespace Luny
{
	public static class SystemDoubleExtensions
	{
		public static Boolean IsInteger(this Double value) => Math.Abs(value - Math.Round(value)) < Double.Epsilon;
	}
}
