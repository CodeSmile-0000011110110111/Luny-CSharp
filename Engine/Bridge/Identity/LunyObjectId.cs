using System;

namespace Luny.Engine.Bridge
{
	/// <summary>
	/// Unique identifier for an engine object/node in a LunyScript context.
	/// Separate from engine's native GetInstanceID() for cross-engine compatibility.
	/// Sequential integers for deterministic ordering and debugging.
	/// </summary>
	public readonly struct LunyObjectId : IEquatable<LunyObjectId>, IComparable<LunyObjectId>
	{
		private const Int32 StartId = 1;
		private static Int32 s_NextId = StartId;
		internal static void Reset() => s_NextId = StartId;

		public readonly Int32 Value;

		private LunyObjectId(Int32 value) => Value = value;

		/// <summary>
		/// Generates a new unique ObjectID.
		/// </summary>
		public static LunyObjectId Generate() => new(s_NextId++);

		public Boolean Equals(LunyObjectId other) => Value == other.Value;
		public override Boolean Equals(Object obj) => obj is LunyObjectId other && Equals(other);
		public override Int32 GetHashCode() => Value;
		public Int32 CompareTo(LunyObjectId other) => Value.CompareTo(other.Value);
		public override String ToString() => $"ObjectId:{Value}";

		public static Boolean operator ==(LunyObjectId left, LunyObjectId right) => left.Equals(right);
		public static Boolean operator !=(LunyObjectId left, LunyObjectId right) => !left.Equals(right);
	}
}
