using System;
using System.Runtime.CompilerServices;

namespace Luny.Engine.Bridge
{
	public struct LunyVector3 : IEquatable<LunyVector3>
	{
		private System.Numerics.Vector3 _value;

		public Single X
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)] get => _value.X;
			[MethodImpl(MethodImplOptions.AggressiveInlining)] set => _value.X = value;
		}

		public Single Y
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)] get => _value.Y;
			[MethodImpl(MethodImplOptions.AggressiveInlining)] set => _value.Y = value;
		}

		public Single Z
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)] get => _value.Z;
			[MethodImpl(MethodImplOptions.AggressiveInlining)] set => _value.Z = value;
		}

		public Single this[Int32 index]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => index switch
			{
				0 => X,
				1 => Y,
				2 => Z,
				var _ => throw new IndexOutOfRangeException($"Invalid {nameof(LunyVector3)} index: {index}"),
			};
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set
			{
				switch (index)
				{
					case 0: X = value; break;
					case 1: Y = value; break;
					case 2: Z = value; break;
					default: throw new IndexOutOfRangeException($"Invalid {nameof(LunyVector3)} index: {index}");
				}
			}
		}

		// Constructors
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public LunyVector3(Single x, Single y, Single z) => _value = new System.Numerics.Vector3(x, y, z);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public LunyVector3(Single x, Single y) => _value = new System.Numerics.Vector3(x, y, 0f);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal LunyVector3(System.Numerics.Vector3 value) => _value = value;

		// Static properties
		public static LunyVector3 Zero => new(System.Numerics.Vector3.Zero);
		public static LunyVector3 One => new(System.Numerics.Vector3.One);
		public static LunyVector3 Up => new(0f, 1f, 0f);
		public static LunyVector3 Down => new(0f, -1f, 0f);
		public static LunyVector3 Left => new(-1f, 0f, 0f);
		public static LunyVector3 Right => new(1f, 0f, 0f);
		public static LunyVector3 Forward => new(0f, 0f, 1f);
		public static LunyVector3 Back => new(0f, 0f, -1f);
		public static LunyVector3 PositiveInfinity => new(Single.PositiveInfinity, Single.PositiveInfinity, Single.PositiveInfinity);
		public static LunyVector3 NegativeInfinity => new(Single.NegativeInfinity, Single.NegativeInfinity, Single.NegativeInfinity);

		// Instance properties
		public Single Magnitude
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)] get => _value.Length();
		}

		public Single SqrMagnitude
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)] get => _value.LengthSquared();
		}

		public LunyVector3 Normalized
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				var len = _value.Length();
				return len > 1e-05f ? new(_value / len) : Zero;
			}
		}

		// Instance methods
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Normalize()
		{
			var len = _value.Length();
			if (len > 1e-05f)
				_value /= len;
			else
				_value = System.Numerics.Vector3.Zero;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Set(Single newX, Single newY, Single newZ) => _value = new System.Numerics.Vector3(newX, newY, newZ);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Scale(LunyVector3 scale) => _value *= scale._value;

		// Static methods
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Single Dot(LunyVector3 lhs, LunyVector3 rhs) => System.Numerics.Vector3.Dot(lhs._value, rhs._value);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static LunyVector3 Cross(LunyVector3 lhs, LunyVector3 rhs) => new(System.Numerics.Vector3.Cross(lhs._value, rhs._value));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Single Distance(LunyVector3 a, LunyVector3 b) => System.Numerics.Vector3.Distance(a._value, b._value);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Single SqrDistance(LunyVector3 a, LunyVector3 b) => System.Numerics.Vector3.DistanceSquared(a._value, b._value);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static LunyVector3 Lerp(LunyVector3 a, LunyVector3 b, Single t) =>
			new(System.Numerics.Vector3.Lerp(a._value, b._value, Math.Clamp(t, 0f, 1f)));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static LunyVector3 LerpUnclamped(LunyVector3 a, LunyVector3 b, Single t) =>
			new(System.Numerics.Vector3.Lerp(a._value, b._value, t));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static LunyVector3 Max(LunyVector3 lhs, LunyVector3 rhs) => new(System.Numerics.Vector3.Max(lhs._value, rhs._value));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static LunyVector3 Min(LunyVector3 lhs, LunyVector3 rhs) => new(System.Numerics.Vector3.Min(lhs._value, rhs._value));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static LunyVector3 Scale(LunyVector3 a, LunyVector3 b) => new(a._value * b._value);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static LunyVector3 Reflect(LunyVector3 inDirection, LunyVector3 inNormal) =>
			new(System.Numerics.Vector3.Reflect(inDirection._value, inNormal._value));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static LunyVector3 Normalize(LunyVector3 value) => value.Normalized;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static LunyVector3 ClampMagnitude(LunyVector3 vector, Single maxLength)
		{
			var sqrMag = vector._value.LengthSquared();
			if (sqrMag > maxLength * maxLength)
			{
				var mag = MathF.Sqrt(sqrMag);
				return new(vector._value / mag * maxLength);
			}
			return vector;
		}

		public static LunyVector3 MoveTowards(LunyVector3 current, LunyVector3 target, Single maxDistanceDelta)
		{
			var diff = target._value - current._value;
			var dist = diff.Length();
			if (dist <= maxDistanceDelta || dist < 1e-05f)
				return target;

			return new(current._value + diff / dist * maxDistanceDelta);
		}

		public static LunyVector3 Project(LunyVector3 vector, LunyVector3 onNormal)
		{
			var sqrMag = System.Numerics.Vector3.Dot(onNormal._value, onNormal._value);
			if (sqrMag < Single.Epsilon)
				return Zero;

			var dot = System.Numerics.Vector3.Dot(vector._value, onNormal._value);
			return new(onNormal._value * (dot / sqrMag));
		}

		public static LunyVector3 ProjectOnPlane(LunyVector3 vector, LunyVector3 planeNormal) =>
			new(vector._value - Project(vector, planeNormal)._value);

		public static Single Angle(LunyVector3 from, LunyVector3 to)
		{
			var denominator = MathF.Sqrt(from._value.LengthSquared() * to._value.LengthSquared());
			if (denominator < 1e-15f)
				return 0f;

			var dot = Math.Clamp(System.Numerics.Vector3.Dot(from._value, to._value) / denominator, -1f, 1f);
			return MathF.Acos(dot) * (180f / MathF.PI);
		}

		public static Single SignedAngle(LunyVector3 from, LunyVector3 to, LunyVector3 axis)
		{
			var unsignedAngle = Angle(from, to);
			var cross = Cross(from, to);
			var sign = MathF.Sign(Dot(axis, cross));
			return unsignedAngle * (sign < 0 ? -1f : 1f);
		}

		// Operators
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static LunyVector3 operator +(LunyVector3 a, LunyVector3 b) => new(a._value + b._value);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static LunyVector3 operator -(LunyVector3 a, LunyVector3 b) => new(a._value - b._value);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static LunyVector3 operator -(LunyVector3 a) => new(-a._value);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static LunyVector3 operator *(LunyVector3 a, Single d) => new(a._value * d);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static LunyVector3 operator *(Single d, LunyVector3 a) => new(a._value * d);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static LunyVector3 operator *(LunyVector3 a, LunyVector3 b) => new(a._value * b._value);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static LunyVector3 operator /(LunyVector3 a, Single d) => new(a._value / d);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static LunyVector3 operator /(LunyVector3 a, LunyVector3 b) => new(a._value / b._value);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Boolean operator ==(LunyVector3 lhs, LunyVector3 rhs) => (lhs._value - rhs._value).LengthSquared() < 9.99999944e-11f;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Boolean operator !=(LunyVector3 lhs, LunyVector3 rhs) => !(lhs == rhs);

		// Equality
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Boolean Equals(LunyVector3 other) => _value.Equals(other._value);

		public override Boolean Equals(Object obj) => obj is LunyVector3 other && Equals(other);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override Int32 GetHashCode() => _value.GetHashCode();

		public override String ToString() => $"({X:F1}, {Y:F1}, {Z:F1})";

		public String ToString(String format) => $"({X.ToString(format)}, {Y.ToString(format)}, {Z.ToString(format)})";

		// Internal accessor for engine bridge extensions
		internal System.Numerics.Vector3 InternalValue
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)] get => _value;
		}
	}
}
