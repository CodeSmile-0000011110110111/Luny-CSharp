using System;

namespace Luny.Engine.Bridge
{
	/// <summary>
	/// Engine-native unique identifier value for engine objects.
	/// </summary>
	public readonly struct LunyNativeObjectId : IEquatable<LunyNativeObjectId>, IComparable<LunyNativeObjectId>
	{
		public readonly Int64 Value;

		public static implicit operator LunyNativeObjectId(Int64 v) => new(v);
		public static implicit operator LunyNativeObjectId(UInt64 v) => new(v);
		public LunyNativeObjectId(Int64 value) => Value = value;
		public LunyNativeObjectId(UInt64 value) => Value = (Int64)value;

		public Boolean Equals(LunyNativeObjectId other) => Value == other.Value;
		public override Boolean Equals(Object obj) => obj is LunyNativeObjectId other && Equals(other);
		public override Int32 GetHashCode() => Value.GetHashCode();
		public Int32 CompareTo(LunyNativeObjectId other) => Value.CompareTo(other.Value);
		public override String ToString() => $"NativeId:{Value}";

		public static Boolean operator ==(LunyNativeObjectId left, LunyNativeObjectId right) => left.Equals(right);
		public static Boolean operator !=(LunyNativeObjectId left, LunyNativeObjectId right) => !left.Equals(right);
	}
}
