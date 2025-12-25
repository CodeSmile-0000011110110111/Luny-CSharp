using System;

namespace Luny
{
	/// <summary>
	/// Engine-native unique identifier value for engine objects.
	/// </summary>
	public readonly struct NativeID : IEquatable<NativeID>, IComparable<NativeID>
	{
		public readonly Int64 Value;

		public static implicit operator NativeID(Int64 v) => new(v);
		public static implicit operator NativeID(UInt64 v) => new(v);
		public NativeID(Int64 value) => Value = value;
		public NativeID(UInt64 value) => Value = (Int64)value;

		public Boolean Equals(NativeID other) => Value == other.Value;
		public override Boolean Equals(Object obj) => obj is NativeID other && Equals(other);
		public override Int32 GetHashCode() => Value.GetHashCode();
		public Int32 CompareTo(NativeID other) => Value.CompareTo(other.Value);
		public override String ToString() => $"{nameof(NativeID)}:{Value}";

		public static Boolean operator ==(NativeID left, NativeID right) => left.Equals(right);
		public static Boolean operator !=(NativeID left, NativeID right) => !left.Equals(right);
	}
}
