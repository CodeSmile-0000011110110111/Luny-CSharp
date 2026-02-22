using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Luny.Engine.Bridge
{
	public struct LunyVector2 : IEquatable<LunyVector2>
	{
		private Vector2 _value;

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

		public Single this[Int32 index]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => index switch
			{
				0 => X,
				1 => Y,
				var _ => throw new IndexOutOfRangeException($"Invalid {nameof(LunyVector2)} index: {index}"),
			};
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set
			{
				switch (index)
				{
					case 0:
						X = value;
						break;
					case 1:
						Y = value;
						break;
					default:
						throw new IndexOutOfRangeException($"Invalid {nameof(LunyVector2)} index: {index}");
				}
			}
		}

		// Constructors
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public LunyVector2(Single x, Single y) => _value = new Vector2(x, y);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public LunyVector2(Double x, Double y) => _value = new Vector2((Single)x, (Single)y);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal LunyVector2(Vector2 value) => _value = value;

		// Static properties
		public static LunyVector2 Zero => new(Vector2.Zero);
		public static LunyVector2 One => new(Vector2.One);
		public static LunyVector2 Up => new(0f, 1f);
		public static LunyVector2 Down => new(0f, -1f);
		public static LunyVector2 Left => new(-1f, 0f);
		public static LunyVector2 Right => new(1f, 0f);
		public static LunyVector2 PositiveInfinity => new(Single.PositiveInfinity, Single.PositiveInfinity);
		public static LunyVector2 NegativeInfinity => new(Single.NegativeInfinity, Single.NegativeInfinity);

		// Instance properties
		public Single Magnitude
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)] get => _value.Length();
		}

		public Single SqrMagnitude
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)] get => _value.LengthSquared();
		}

		public LunyVector2 Normalized
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				var len = _value.Length();
				return len > 1e-05f ? new LunyVector2(_value / len) : Zero;
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
				_value = Vector2.Zero;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Set(Single newX, Single newY) => _value = new Vector2(newX, newY);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Scale(LunyVector2 scale) => _value *= scale._value;

		// Static methods
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Single Dot(LunyVector2 lhs, LunyVector2 rhs) => Vector2.Dot(lhs._value, rhs._value);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Single Distance(LunyVector2 a, LunyVector2 b) => Vector2.Distance(a._value, b._value);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Single SqrDistance(LunyVector2 a, LunyVector2 b) => Vector2.DistanceSquared(a._value, b._value);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static LunyVector2 Lerp(LunyVector2 a, LunyVector2 b, Single t) => new(Vector2.Lerp(a._value, b._value, Math.Clamp(t, 0f, 1f)));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static LunyVector2 Lerp(LunyVector2 a, LunyVector2 b, Double t) => Lerp(a, b, (Single)t);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static LunyVector2 LerpUnclamped(LunyVector2 a, LunyVector2 b, Single t) => new(Vector2.Lerp(a._value, b._value, t));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static LunyVector2 LerpUnclamped(LunyVector2 a, LunyVector2 b, Double t) => LerpUnclamped(a, b, (Single)t);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static LunyVector2 Max(LunyVector2 lhs, LunyVector2 rhs) => new(Vector2.Max(lhs._value, rhs._value));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static LunyVector2 Min(LunyVector2 lhs, LunyVector2 rhs) => new(Vector2.Min(lhs._value, rhs._value));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static LunyVector2 Scale(LunyVector2 a, LunyVector2 b) => new(a._value * b._value);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static LunyVector2 Reflect(LunyVector2 inDirection, LunyVector2 inNormal) =>
			new(Vector2.Reflect(inDirection._value, inNormal._value));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static LunyVector2 Normalize(LunyVector2 value) => value.Normalized;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static LunyVector2 ClampMagnitude(LunyVector2 vector, Single maxLength)
		{
			var sqrMag = vector._value.LengthSquared();
			if (sqrMag > maxLength * maxLength)
			{
				var mag = MathF.Sqrt(sqrMag);
				return new LunyVector2(vector._value / mag * maxLength);
			}
			return vector;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static LunyVector2 ClampMagnitude(LunyVector2 vector, Double maxLength) => ClampMagnitude(vector, (Single)maxLength);

		public static LunyVector2 MoveTowards(LunyVector2 current, LunyVector2 target, Single maxDistanceDelta)
		{
			var diff = target._value - current._value;
			var dist = diff.Length();
			if (dist <= maxDistanceDelta || dist < 1e-05f)
				return target;

			return new LunyVector2(current._value + diff / dist * maxDistanceDelta);
		}

		public static LunyVector2 MoveTowards(LunyVector2 current, LunyVector2 target, Double maxDistanceDelta) =>
			MoveTowards(current, target, (Single)maxDistanceDelta);

		public static LunyVector2 Perpendicular(LunyVector2 inDirection) => new(-inDirection.Y, inDirection.X);

		public static Single Angle(LunyVector2 from, LunyVector2 to)
		{
			var denominator = MathF.Sqrt(from._value.LengthSquared() * to._value.LengthSquared());
			if (denominator < 1e-15f)
				return 0f;

			var dot = Math.Clamp(Vector2.Dot(from._value, to._value) / denominator, -1f, 1f);
			return MathF.Acos(dot) * (180f / MathF.PI);
		}

		public static Single SignedAngle(LunyVector2 from, LunyVector2 to)
		{
			var unsignedAngle = Angle(from, to);
			var sign = MathF.Sign(from.X * to.Y - from.Y * to.X);
			return unsignedAngle * (sign < 0 ? -1f : 1f);
		}

		// Implicit conversion LunyVector2 <-> LunyVector3
		public static implicit operator LunyVector3(LunyVector2 v) => new(v.X, v.Y, 0f);
		public static explicit operator LunyVector2(LunyVector3 v) => new(v.X, v.Y);

		// Operators
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static LunyVector2 operator +(LunyVector2 a, LunyVector2 b) => new(a._value + b._value);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static LunyVector2 operator -(LunyVector2 a, LunyVector2 b) => new(a._value - b._value);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static LunyVector2 operator -(LunyVector2 a) => new(-a._value);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static LunyVector2 operator *(LunyVector2 a, Single d) => new(a._value * d);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static LunyVector2 operator *(LunyVector2 a, Double d) => new(a._value * (Single)d);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static LunyVector2 operator *(Single d, LunyVector2 a) => new(a._value * d);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static LunyVector2 operator *(Double d, LunyVector2 a) => new(a._value * (Single)d);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static LunyVector2 operator *(LunyVector2 a, LunyVector2 b) => new(a._value * b._value);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static LunyVector2 operator /(LunyVector2 a, Single d) => new(a._value / d);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static LunyVector2 operator /(LunyVector2 a, Double d) => new(a._value / (Single)d);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static LunyVector2 operator /(LunyVector2 a, LunyVector2 b) => new(a._value / b._value);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Boolean operator ==(LunyVector2 lhs, LunyVector2 rhs) => (lhs._value - rhs._value).LengthSquared() < 9.99999944e-11f;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Boolean operator !=(LunyVector2 lhs, LunyVector2 rhs) => !(lhs == rhs);

		// Equality
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Boolean Equals(LunyVector2 other) => _value.Equals(other._value);

		public override Boolean Equals(Object obj) => obj is LunyVector2 other && Equals(other);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override Int32 GetHashCode() => _value.GetHashCode();

		public override String ToString() => $"({X:0.###}, {Y:0.###})";

		public String ToString(String format) => $"({X.ToString(format)}, {Y.ToString(format)})";

		// Internal accessor for engine bridge extensions
		internal Vector2 InternalValue
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)] get => _value;
		}
	}
}
