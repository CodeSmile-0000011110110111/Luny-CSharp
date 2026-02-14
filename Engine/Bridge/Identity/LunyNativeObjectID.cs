using System;

namespace Luny.Engine.Bridge
{
	/// <summary>
	/// Engine-native unique identifier value for engine objects.
	/// </summary>
	public readonly struct LunyNativeObjectID : IEquatable<LunyNativeObjectID>, IComparable<LunyNativeObjectID>
	{
		public readonly Int64 Value;

		public static implicit operator LunyNativeObjectID(Int64 v) => new(v);
		public static implicit operator LunyNativeObjectID(UInt64 v) => new(v);
		public LunyNativeObjectID(Int64 value) => Value = value;
		public LunyNativeObjectID(UInt64 value) => Value = (Int64)value;

		public Boolean Equals(LunyNativeObjectID other) => Value == other.Value;
		public override Boolean Equals(Object obj) => obj is LunyNativeObjectID other && Equals(other);
		public override Int32 GetHashCode() => Value.GetHashCode();
		public Int32 CompareTo(LunyNativeObjectID other) => Value.CompareTo(other.Value);
		public override String ToString() => $"{nameof(LunyNativeObjectID)}:{Value}";

		public static Boolean operator ==(LunyNativeObjectID left, LunyNativeObjectID right) => left.Equals(right);
		public static Boolean operator !=(LunyNativeObjectID left, LunyNativeObjectID right) => !left.Equals(right);
	}
}
