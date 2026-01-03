using System;

namespace Luny
{
	/// <summary>
	/// Unique identifier for an engine object/node in a LunyScript context.
	/// Separate from engine's native GetInstanceID() for cross-engine compatibility.
	/// Sequential integers for deterministic ordering and debugging.
	/// </summary>
	public readonly struct LunyID : IEquatable<LunyID>, IComparable<LunyID>
	{
		private const Int32 StartID = 1;
		private static Int32 s_NextID = StartID;
		internal static void Reset() => s_NextID = StartID;

		public readonly Int32 Value;

		private LunyID(Int32 value) => Value = value;

		/// <summary>
		/// Generates a new unique ObjectID.
		/// </summary>
		public static LunyID Generate() => new(s_NextID++);

		public Boolean Equals(LunyID other) => Value == other.Value;
		public override Boolean Equals(Object obj) => obj is LunyID other && Equals(other);
		public override Int32 GetHashCode() => Value;
		public Int32 CompareTo(LunyID other) => Value.CompareTo(other.Value);
		public override String ToString() => $"{nameof(LunyID)}:{Value}";

		public static Boolean operator ==(LunyID left, LunyID right) => left.Equals(right);
		public static Boolean operator !=(LunyID left, LunyID right) => !left.Equals(right);
	}
}
