using System;

namespace Luny.Engine.Bridge.Identity
{
	/// <summary>
	/// Unique identifier for an engine object/node in a LunyScript context.
	/// Separate from engine's native GetInstanceID() for cross-engine compatibility.
	/// Sequential integers for deterministic ordering and debugging.
	/// </summary>
	public readonly struct LunyObjectID : IEquatable<LunyObjectID>, IComparable<LunyObjectID>
	{
		private const Int32 StartID = 1;
		private static Int32 s_NextID = StartID;
		internal static void Reset() => s_NextID = StartID;

		public readonly Int32 Value;

		private LunyObjectID(Int32 value) => Value = value;

		/// <summary>
		/// Generates a new unique ObjectID.
		/// </summary>
		public static LunyObjectID Generate() => new(s_NextID++);

		public Boolean Equals(LunyObjectID other) => Value == other.Value;
		public override Boolean Equals(Object obj) => obj is LunyObjectID other && Equals(other);
		public override Int32 GetHashCode() => Value;
		public Int32 CompareTo(LunyObjectID other) => Value.CompareTo(other.Value);
		public override String ToString() => $"{nameof(LunyObjectID)}:{Value}";

		public static Boolean operator ==(LunyObjectID left, LunyObjectID right) => left.Equals(right);
		public static Boolean operator !=(LunyObjectID left, LunyObjectID right) => !left.Equals(right);
	}
}
