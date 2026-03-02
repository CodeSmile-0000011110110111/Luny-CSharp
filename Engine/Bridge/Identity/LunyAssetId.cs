using System;

namespace Luny.Engine.Bridge
{
	/// <summary>
	/// Represents a unique internal ID for an engine asset.
	/// Stable for the duration of a session.
	/// </summary>
	public readonly struct LunyAssetId : IEquatable<LunyAssetId>, IComparable<LunyAssetId>
	{
		private const Int32 StartId = 1;
		private static Int32 s_NextId = StartId;

		public readonly Int32 Value;
		public Boolean IsValid => Value >= StartId;
		internal static void Reset() => s_NextId = StartId;

		//private LunyAssetID() {} // requires C# 10
		private LunyAssetId(Int32 value) => Value = value;

		/// <summary>
		/// Generates a new unique ObjectID.
		/// </summary>
		public static LunyAssetId Generate() => new(s_NextId++);

		public Boolean Equals(LunyAssetId other) => Value == other.Value;
		public override Boolean Equals(Object obj) => obj is LunyAssetId other && Equals(other);
		public override Int32 GetHashCode() => Value.GetHashCode();
		public Int32 CompareTo(LunyAssetId other) => Value.CompareTo(other.Value);

		public static Boolean operator ==(LunyAssetId left, LunyAssetId right) => left.Equals(right);
		public static Boolean operator !=(LunyAssetId left, LunyAssetId right) => !left.Equals(right);

		public override String ToString() => $"AssetId:{Value}";

		public static implicit operator Int32(LunyAssetId id) => id.Value;
		public static implicit operator LunyAssetId(Int32 value) => new(value);
	}
}
