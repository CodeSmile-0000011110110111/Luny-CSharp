using System;

namespace Luny
{
	/// <summary>
	/// Unique identifier for an engine object/node in a LunyScript context.
	/// Separate from engine's native GetInstanceID() for cross-engine compatibility.
	/// Sequential integers for deterministic ordering and debugging.
	/// </summary>
	public readonly struct ObjectID : IEquatable<ObjectID>, IComparable<ObjectID>
	{
		private static Int32 _nextID = 1;

		public readonly Int32 Value;

		private ObjectID(Int32 value) => Value = value;

		/// <summary>
		/// Generates a new unique ObjectID.
		/// </summary>
		public static ObjectID Generate() => new ObjectID(_nextID++);

		public Boolean Equals(ObjectID other) => Value == other.Value;
		public override Boolean Equals(Object obj) => obj is ObjectID other && Equals(other);
		public override Int32 GetHashCode() => Value;
		public Int32 CompareTo(ObjectID other) => Value.CompareTo(other.Value);
		public override String ToString() => $"ObjectID:{Value}";

		public static Boolean operator ==(ObjectID left, ObjectID right) => left.Equals(right);
		public static Boolean operator !=(ObjectID left, ObjectID right) => !left.Equals(right);
	}
}
