using System;

namespace Luny.Engine.Bridge
{
	public static class VectorUtil
	{
		/// <summary> Returns a copy of <paramref name="v"/> with the X component set to zero. </summary>
		public static LunyVector3 LockAxisX(LunyVector3 v) => new(0d, v.Y, v.Z);

		/// <summary> Returns a copy of <paramref name="v"/> with the Y component set to zero. </summary>
		public static LunyVector3 LockAxisY(LunyVector3 v) => new(v.X, 0d, v.Z);

		/// <summary> Returns a copy of <paramref name="v"/> with the Z component set to zero. </summary>
		public static LunyVector3 LockAxisZ(LunyVector3 v) => new(v.X, v.Y, 0d);

		/// <summary>
		/// Computes the axis-masked direction from <paramref name="from"/> toward <paramref name="to"/>
		/// and returns false when the result is effectively zero (i.e. no movement should occur).
		/// </summary>
		/// <param name="from">Current world position.</param>
		/// <param name="to">Target world position.</param>
		/// <param name="axisLock">Per-axis multiplier (0 locks that axis, 1 keeps it).</param>
		/// <param name="maskedDirection">The axis-locked direction vector.</param>
		/// <returns>True if the masked direction has meaningful length; false if it is near-zero.</returns>
		internal static Boolean TryGetMaskedDirection(LunyVector3 from, LunyVector3 to, LunyVector3 axisLock,
			out LunyVector3 maskedDirection)
		{
			maskedDirection = (to - from) * axisLock;
			return maskedDirection.SqrMagnitude >= Single.Epsilon;
		}
	}
}
