using System;

namespace Luny.Engine.Bridge.Identity
{
	/// <summary>
	/// Represents a unique internal ID for an engine asset.
	/// Stable for the duration of a session.
	/// </summary>
	public readonly struct LunyAssetID : IEquatable<LunyAssetID>, IComparable<LunyAssetID>
	{
		private readonly UInt64 _value;

		public LunyAssetID(UInt64 value) => _value = value;

		public Boolean Equals(LunyAssetID other) => _value == other._value;
		public override Boolean Equals(Object obj) => obj is LunyAssetID other && Equals(other);
		public override Int32 GetHashCode() => _value.GetHashCode();
		public Int32 CompareTo(LunyAssetID other) => _value.CompareTo(other._value);

		public static Boolean operator ==(LunyAssetID left, LunyAssetID right) => left.Equals(right);
		public static Boolean operator !=(LunyAssetID left, LunyAssetID right) => !left.Equals(right);

		public override String ToString() => _value.ToString();
		
		public static implicit operator UInt64(LunyAssetID id) => id._value;
		public static implicit operator LunyAssetID(UInt64 value) => new(value);
	}
}
