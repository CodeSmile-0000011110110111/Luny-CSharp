using System;
using System.Runtime.CompilerServices;

namespace Luny.Engine.Bridge
{
	public struct LunyQuaternion : IEquatable<LunyQuaternion>
	{
		private System.Numerics.Quaternion _value;

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

		public Single W
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)] get => _value.W;
			[MethodImpl(MethodImplOptions.AggressiveInlining)] set => _value.W = value;
		}

		public Single this[Int32 index]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => index switch
			{
				0 => X,
				1 => Y,
				2 => Z,
				3 => W,
				var _ => throw new IndexOutOfRangeException($"Invalid {nameof(LunyQuaternion)} index: {index}"),
			};
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set
			{
				switch (index)
				{
					case 0: X = value; break;
					case 1: Y = value; break;
					case 2: Z = value; break;
					case 3: W = value; break;
					default: throw new IndexOutOfRangeException($"Invalid {nameof(LunyQuaternion)} index: {index}");
				}
			}
		}

		// Constructors
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public LunyQuaternion(Single x, Single y, Single z, Single w) => _value = new System.Numerics.Quaternion(x, y, z, w);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal LunyQuaternion(System.Numerics.Quaternion value) => _value = value;

		// Static properties
		public static LunyQuaternion Identity => new(System.Numerics.Quaternion.Identity);

		// Instance properties
		public LunyVector3 EulerAngles
		{
			get
			{
				var q = _value;
				// Roll (X)
				var sinrCosp = 2f * (q.W * q.X + q.Y * q.Z);
				var cosrCosp = 1f - 2f * (q.X * q.X + q.Y * q.Y);
				var roll = MathF.Atan2(sinrCosp, cosrCosp);

				// Pitch (Y)
				var sinp = 2f * (q.W * q.Y - q.Z * q.X);
				Single pitch;
				if (MathF.Abs(sinp) >= 1f)
					pitch = LunyMath.CopySign(MathF.PI / 2f, sinp);
				else
					pitch = MathF.Asin(sinp);

				// Yaw (Z)
				var sinyCosp = 2f * (q.W * q.Z + q.X * q.Y);
				var cosyCosp = 1f - 2f * (q.Y * q.Y + q.Z * q.Z);
				var yaw = MathF.Atan2(sinyCosp, cosyCosp);

				const Single rad2Deg = 180f / MathF.PI;
				return new LunyVector3(roll * rad2Deg, pitch * rad2Deg, yaw * rad2Deg);
			}
			set
			{
				const Single deg2Rad = MathF.PI / 180f;
				var halfX = value.X * deg2Rad * 0.5f;
				var halfY = value.Y * deg2Rad * 0.5f;
				var halfZ = value.Z * deg2Rad * 0.5f;

				var sinX = MathF.Sin(halfX);
				var cosX = MathF.Cos(halfX);
				var sinY = MathF.Sin(halfY);
				var cosY = MathF.Cos(halfY);
				var sinZ = MathF.Sin(halfZ);
				var cosZ = MathF.Cos(halfZ);

				_value = new System.Numerics.Quaternion(
					cosY * sinX * cosZ + sinY * cosX * sinZ,
					sinY * cosX * cosZ - cosY * sinX * sinZ,
					cosY * cosX * sinZ - sinY * sinX * cosZ,
					cosY * cosX * cosZ + sinY * sinX * sinZ
				);
			}
		}

		public LunyQuaternion Normalized
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => new(System.Numerics.Quaternion.Normalize(_value));
		}

		// Instance methods
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Normalize() => _value = System.Numerics.Quaternion.Normalize(_value);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Set(Single newX, Single newY, Single newZ, Single newW) => _value = new System.Numerics.Quaternion(newX, newY, newZ, newW);

		// Static methods
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Single Dot(LunyQuaternion a, LunyQuaternion b) => System.Numerics.Quaternion.Dot(a._value, b._value);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static LunyQuaternion Slerp(LunyQuaternion a, LunyQuaternion b, Single t) =>
			new(System.Numerics.Quaternion.Slerp(a._value, b._value, Math.Clamp(t, 0f, 1f)));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static LunyQuaternion SlerpUnclamped(LunyQuaternion a, LunyQuaternion b, Single t) =>
			new(System.Numerics.Quaternion.Slerp(a._value, b._value, t));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static LunyQuaternion Lerp(LunyQuaternion a, LunyQuaternion b, Single t) =>
			new(System.Numerics.Quaternion.Lerp(a._value, b._value, Math.Clamp(t, 0f, 1f)));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static LunyQuaternion LerpUnclamped(LunyQuaternion a, LunyQuaternion b, Single t) =>
			new(System.Numerics.Quaternion.Lerp(a._value, b._value, t));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static LunyQuaternion Inverse(LunyQuaternion rotation) => new(System.Numerics.Quaternion.Inverse(rotation._value));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static LunyQuaternion Conjugate(LunyQuaternion value) => new(System.Numerics.Quaternion.Conjugate(value._value));

		public static Single Angle(LunyQuaternion a, LunyQuaternion b)
		{
			var dot = System.Numerics.Quaternion.Dot(a._value, b._value);
			return MathF.Acos(Math.Clamp(MathF.Abs(dot), 0f, 1f)) * 2f * (180f / MathF.PI);
		}

		public static LunyQuaternion Euler(Single x, Single y, Single z)
		{
			var result = new LunyQuaternion(0, 0, 0, 1);
			result.EulerAngles = new LunyVector3(x, y, z);
			return result;
		}

		public static LunyQuaternion Euler(LunyVector3 euler)
		{
			var result = new LunyQuaternion(0, 0, 0, 1);
			result.EulerAngles = euler;
			return result;
		}

		public static LunyQuaternion AngleAxis(Single angle, LunyVector3 axis) =>
			new(System.Numerics.Quaternion.CreateFromAxisAngle(
				new System.Numerics.Vector3(axis.X, axis.Y, axis.Z), angle * (MathF.PI / 180f)));

		public static LunyQuaternion LookRotation(LunyVector3 forward, LunyVector3 upwards)
		{
			var f = forward.Normalized;
			var u = upwards.Normalized;
			var r = LunyVector3.Cross(u, f).Normalized;
			u = LunyVector3.Cross(f, r);

			var m00 = r.X; var m01 = r.Y; var m02 = r.Z;
			var m10 = u.X; var m11 = u.Y; var m12 = u.Z;
			var m20 = f.X; var m21 = f.Y; var m22 = f.Z;

			var trace = m00 + m11 + m22;
			Single x, y, z, w;

			if (trace > 0f)
			{
				var s = MathF.Sqrt(trace + 1f) * 2f;
				w = 0.25f * s;
				x = (m12 - m21) / s;
				y = (m20 - m02) / s;
				z = (m01 - m10) / s;
			}
			else if (m00 > m11 && m00 > m22)
			{
				var s = MathF.Sqrt(1f + m00 - m11 - m22) * 2f;
				w = (m12 - m21) / s;
				x = 0.25f * s;
				y = (m01 + m10) / s;
				z = (m02 + m20) / s;
			}
			else if (m11 > m22)
			{
				var s = MathF.Sqrt(1f + m11 - m00 - m22) * 2f;
				w = (m20 - m02) / s;
				x = (m01 + m10) / s;
				y = 0.25f * s;
				z = (m12 + m21) / s;
			}
			else
			{
				var s = MathF.Sqrt(1f + m22 - m00 - m11) * 2f;
				w = (m01 - m10) / s;
				x = (m02 + m20) / s;
				y = (m12 + m21) / s;
				z = 0.25f * s;
			}

			return new LunyQuaternion(x, y, z, w).Normalized;
		}

		public static LunyQuaternion LookRotation(LunyVector3 forward) => LookRotation(forward, LunyVector3.Up);

		public static LunyQuaternion RotateTowards(LunyQuaternion from, LunyQuaternion to, Single maxDegreesDelta)
		{
			var angle = Angle(from, to);
			if (angle < Single.Epsilon)
				return to;

			return Slerp(from, to, Math.Min(1f, maxDegreesDelta / angle));
		}

		public static LunyVector3 operator *(LunyQuaternion rotation, LunyVector3 point)
		{
			var q = rotation._value;
			var u = new System.Numerics.Vector3(q.X, q.Y, q.Z);
			var s = q.W;
			var p = new System.Numerics.Vector3(point.X, point.Y, point.Z);
			var result = 2f * System.Numerics.Vector3.Dot(u, p) * u
				+ (s * s - System.Numerics.Vector3.Dot(u, u)) * p
				+ 2f * s * System.Numerics.Vector3.Cross(u, p);
			return new LunyVector3(result.X, result.Y, result.Z);
		}

		// Operators
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static LunyQuaternion operator *(LunyQuaternion lhs, LunyQuaternion rhs) =>
			new(System.Numerics.Quaternion.Multiply(lhs._value, rhs._value));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Boolean operator ==(LunyQuaternion lhs, LunyQuaternion rhs) =>
			MathF.Abs(Dot(lhs, rhs)) > 1f - 1e-06f;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Boolean operator !=(LunyQuaternion lhs, LunyQuaternion rhs) => !(lhs == rhs);

		// Equality
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Boolean Equals(LunyQuaternion other) => _value.Equals(other._value);

		public override Boolean Equals(Object obj) => obj is LunyQuaternion other && Equals(other);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override Int32 GetHashCode() => _value.GetHashCode();

		public override String ToString() => $"({X:F1}, {Y:F1}, {Z:F1}, {W:F1})";

		public String ToString(String format) => $"({X.ToString(format)}, {Y.ToString(format)}, {Z.ToString(format)}, {W.ToString(format)})";

		// Internal accessor for engine bridge extensions
		internal System.Numerics.Quaternion InternalValue
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)] get => _value;
		}
	}
}
