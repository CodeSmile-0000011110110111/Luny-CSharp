using System;

namespace Luny.Tests
{
	/// <summary>
	/// Engine-agnostic assertion helper for smoke tests.
	/// Throws LunyTestContractViolationException on assertion failure.
	/// </summary>
	internal static class LunyAssert
	{
		/// <summary>
		/// Asserts that a condition is true.
		/// </summary>
		public static void That(Boolean condition, String message)
		{
			if (!condition)
				throw new SmokeTestFailureException(message);
		}

		/// <summary>
		/// Asserts that a condition is true.
		/// </summary>
		public static void IsTrue(Boolean condition, String message) => That(condition, message);

		/// <summary>
		/// Asserts that a condition is false.
		/// </summary>
		public static void IsFalse(Boolean condition, String message) => That(!condition, message);

		/// <summary>
		/// Asserts that a value is null.
		/// </summary>
		public static void IsNull(Object value, String message) => That(value == null, message);

		/// <summary>
		/// Asserts that a value is not null.
		/// </summary>
		public static void IsNotNull(Object value, String message) => That(value != null, message);

		/// <summary>
		/// Asserts that an integer value is zero.
		/// </summary>
		public static void IsZero(Int32 value, String message) => That(value == 0, message);

		/// <summary>
		/// Asserts that two values are equal.
		/// </summary>
		public static void AreEqual<T>(T expected, T actual, String message) => That(Equals(expected, actual), message);

		/// <summary>
		/// Asserts that two values are not equal.
		/// </summary>
		public static void AreNotEqual<T>(T expected, T actual, String message) => That(!Equals(expected, actual), message);

		/// <summary>
		/// Asserts that a value is greater than a threshold.
		/// </summary>
		public static void IsGreaterThan(Int32 value, Int32 threshold, String message) => That(value > threshold, message);

		/// <summary>
		/// Asserts that a value is greater than or equal to a threshold.
		/// </summary>
		public static void IsGreaterThanOrEqualTo(Int32 value, Int32 threshold, String message) => That(value >= threshold, message);

		/// <summary>
		/// Asserts that a value is less than a threshold.
		/// </summary>
		public static void IsLessThan(Int32 value, Int32 threshold, String message) => That(value < threshold, message);

		/// <summary>
		/// Asserts that a value is less than or equal to a threshold.
		/// </summary>
		public static void IsLessThanOrEqualTo(Int32 value, Int32 threshold, String message) => That(value <= threshold, message);
	}
}
