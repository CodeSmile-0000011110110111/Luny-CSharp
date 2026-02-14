using System;

namespace Luny.Engine.Bridge
{
	/// <summary>
	/// Represents a unique internal ID for an engine asset.
	/// Stable for the duration of a session.
	/// </summary>
	public readonly struct LunyAssetID : IEquatable<LunyAssetID>, IComparable<LunyAssetID>
	{
		private const Int32 StartID = 1;
		private static Int32 s_NextID = StartID;
		private readonly Int32 _value;

		public Boolean IsValid => _value >= StartID;
		internal static void Reset() => s_NextID = StartID;

		//private LunyAssetID() {} // requires C# 10
		private LunyAssetID(Int32 value) => _value = value;

		/// <summary>
		/// Generates a new unique ObjectID.
		/// </summary>
		public static LunyAssetID Generate() => new(s_NextID++);

		public Boolean Equals(LunyAssetID other) => _value == other._value;
		public override Boolean Equals(Object obj) => obj is LunyAssetID other && Equals(other);
		public override Int32 GetHashCode() => _value.GetHashCode();
		public Int32 CompareTo(LunyAssetID other) => _value.CompareTo(other._value);

		public static Boolean operator ==(LunyAssetID left, LunyAssetID right) => left.Equals(right);
		public static Boolean operator !=(LunyAssetID left, LunyAssetID right) => !left.Equals(right);

		public override String ToString() => _value.ToString();

		public static implicit operator Int32(LunyAssetID id) => id._value;
		public static implicit operator LunyAssetID(Int32 value) => new(value);
	}
}
