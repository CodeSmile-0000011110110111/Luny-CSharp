using System;
using System.Globalization;

namespace Luny
{
	/// <summary>
	/// A primitive type that wraps a double and provides implicit conversions from and to various types.
	/// </summary>
	[Serializable]
	public readonly struct Number : IConvertible, IFormattable, IComparable,
		IComparable<Number>, IEquatable<Number>,
		IComparable<Double>, IEquatable<Double>,
		IComparable<Int64>, IEquatable<Int64>,
		IComparable<UInt64>, IEquatable<UInt64>
	{
		private readonly Double _value;

		public static implicit operator Number(Double value) => new(value);
		public static implicit operator Number(Single value) => new(value);
		public static implicit operator Number(Int32 value) => new(value);
		public static implicit operator Number(Int64 value) => new(value);
		public static implicit operator Number(Int16 value) => new(value);
		public static implicit operator Number(Byte value) => new(value);
		public static implicit operator Number(UInt32 value) => new(value);
		public static implicit operator Number(UInt64 value) => new(value);
		public static implicit operator Number(UInt16 value) => new(value);
		public static implicit operator Number(SByte value) => new(value);
		public static implicit operator Number(Decimal value) => new((Double)value);
		public static implicit operator Number(Boolean value) => new(value ? 1.0 : 0.0);
		public static implicit operator Number(TimeSpan value) => new(value.Ticks);
		public static implicit operator Number(DateTime value) => new(value.ToBinary());

		public static implicit operator Number(String value)
		{
			if (String.IsNullOrEmpty(value))
				return new Number(0.0);

			try
			{
				return new Number(Convert.ToDouble(value, CultureInfo.InvariantCulture));
			}
			catch (Exception e)
			{
				LunyLogger.LogWarning($"{nameof(Number)}: failed to convert string '{value}' => {e.Message}");
				return new Number(0.0);
			}
		}

		public static implicit operator Double(Number number) => number._value;
		public static implicit operator Single(Number number) => (Single)number._value;
		public static implicit operator Int32(Number number) => (Int32)number._value;
		public static implicit operator Int64(Number number) => (Int64)number._value;
		public static implicit operator Int16(Number number) => (Int16)number._value;
		public static implicit operator Byte(Number number) => (Byte)number._value;
		public static implicit operator UInt32(Number number) => (UInt32)number._value;
		public static implicit operator UInt64(Number number) => (UInt64)number._value;
		public static implicit operator UInt16(Number number) => (UInt16)number._value;
		public static implicit operator SByte(Number number) => (SByte)number._value;
		public static implicit operator Decimal(Number number) => (Decimal)number._value;
		public static implicit operator TimeSpan(Number number) => TimeSpan.FromTicks((Int64)number._value);
		public static implicit operator DateTime(Number number) => DateTime.FromBinary((Int64)number._value);

		public static implicit operator Boolean(Number number) => Math.Abs(number._value) > Double.Epsilon;
		public static implicit operator String(Number number) => number._value.ToString(CultureInfo.InvariantCulture);

		public TypeCode GetTypeCode() => TypeCode.Double;
		public Boolean ToBoolean(IFormatProvider provider) => Convert.ToBoolean(_value);
		public Byte ToByte(IFormatProvider provider) => Convert.ToByte(_value);
		public Char ToChar(IFormatProvider provider) => (Char)(Int32)(_value >= (Double)Char.MaxValue ? Char.MaxValue : _value <= (Double)Char.MinValue ? Char.MinValue : _value);
		public TimeSpan ToTimeSpan(IFormatProvider provider) => TimeSpan.FromTicks(_value >= (Double)Int64.MaxValue ? Int64.MaxValue : _value <= (Double)Int64.MinValue ? Int64.MinValue : (Int64)_value);
		public DateTime ToDateTime(IFormatProvider provider) => DateTime.FromBinary(_value >= (Double)Int64.MaxValue ? Int64.MaxValue : _value <= (Double)Int64.MinValue ? Int64.MinValue : (Int64)_value);
		public Decimal ToDecimal(IFormatProvider provider) => Convert.ToDecimal(_value);
		public Double ToDouble(IFormatProvider provider) => _value;
		public Int16 ToInt16(IFormatProvider provider) => Convert.ToInt16(_value);
		public Int32 ToInt32(IFormatProvider provider) => Convert.ToInt32(_value);
		public Int64 ToInt64(IFormatProvider provider) => Convert.ToInt64(_value);
		public SByte ToSByte(IFormatProvider provider) => Convert.ToSByte(_value);
		public Single ToSingle(IFormatProvider provider) => Convert.ToSingle(_value);
		public String ToString(IFormatProvider provider) => _value.ToString(provider);
		public Object ToType(Type conversionType, IFormatProvider provider) => Convert.ChangeType(_value, conversionType, provider);
		public UInt16 ToUInt16(IFormatProvider provider) => Convert.ToUInt16(_value);
		public UInt32 ToUInt32(IFormatProvider provider) => Convert.ToUInt32(_value);
		public UInt64 ToUInt64(IFormatProvider provider) => Convert.ToUInt64(_value);

		public String ToString(String format, IFormatProvider formatProvider) => _value.ToString(format, formatProvider);
		public override String ToString() => _value.ToString(CultureInfo.InvariantCulture);

		public static Number operator +(Number a, Number b) => a._value + b._value;
		public static Number operator -(Number a, Number b) => a._value - b._value;
		public static Number operator *(Number a, Number b) => a._value * b._value;
		public static Number operator /(Number a, Number b) => a._value / b._value;
		public static Number operator %(Number a, Number b) => a._value % b._value;

		public static Number operator +(Number a, Boolean b) =>
			throw new InvalidOperationException($"Cannot perform arithmetic operations with {nameof(Boolean)}.");

		public static Number operator +(Boolean a, Number b) =>
			throw new InvalidOperationException($"Cannot perform arithmetic operations with {nameof(Boolean)}.");

		public static Number operator -(Number a, Boolean b) =>
			throw new InvalidOperationException($"Cannot perform arithmetic operations with {nameof(Boolean)}.");

		public static Number operator -(Boolean a, Number b) =>
			throw new InvalidOperationException($"Cannot perform arithmetic operations with {nameof(Boolean)}.");

		public static Number operator *(Number a, Boolean b) =>
			throw new InvalidOperationException($"Cannot perform arithmetic operations with {nameof(Boolean)}.");

		public static Number operator *(Boolean a, Number b) =>
			throw new InvalidOperationException($"Cannot perform arithmetic operations with {nameof(Boolean)}.");

		public static Number operator /(Number a, Boolean b) =>
			throw new InvalidOperationException($"Cannot perform arithmetic operations with {nameof(Boolean)}.");

		public static Number operator /(Boolean a, Number b) =>
			throw new InvalidOperationException($"Cannot perform arithmetic operations with {nameof(Boolean)}.");

		public static Number operator %(Number a, Boolean b) =>
			throw new InvalidOperationException($"Cannot perform arithmetic operations with {nameof(Boolean)}.");

		public static Number operator %(Boolean a, Number b) =>
			throw new InvalidOperationException($"Cannot perform arithmetic operations with {nameof(Boolean)}.");

		public static Number operator +(Number a, String b) =>
			throw new InvalidOperationException($"Cannot perform arithmetic operations with {nameof(String)}.");

		public static Number operator +(String a, Number b) =>
			throw new InvalidOperationException($"Cannot perform arithmetic operations with {nameof(String)}.");

		public static Number operator -(Number a, String b) =>
			throw new InvalidOperationException($"Cannot perform arithmetic operations with {nameof(String)}.");

		public static Number operator -(String a, Number b) =>
			throw new InvalidOperationException($"Cannot perform arithmetic operations with {nameof(String)}.");

		public static Number operator *(Number a, String b) =>
			throw new InvalidOperationException($"Cannot perform arithmetic operations with {nameof(String)}.");

		public static Number operator *(String a, Number b) =>
			throw new InvalidOperationException($"Cannot perform arithmetic operations with {nameof(String)}.");

		public static Number operator /(Number a, String b) =>
			throw new InvalidOperationException($"Cannot perform arithmetic operations with {nameof(String)}.");

		public static Number operator /(String a, Number b) =>
			throw new InvalidOperationException($"Cannot perform arithmetic operations with {nameof(String)}.");

		public static Number operator %(Number a, String b) =>
			throw new InvalidOperationException($"Cannot perform arithmetic operations with {nameof(String)}.");

		public static Number operator %(String a, Number b) =>
			throw new InvalidOperationException($"Cannot perform arithmetic operations with {nameof(String)}.");

		public static Number operator +(Number a) => a._value;
		public static Number operator -(Number a) => -a._value;
		public static Boolean operator ==(Number a, Number b) => a._value.Equals(b._value);
		public static Boolean operator !=(Number a, Number b) => !a._value.Equals(b._value);
		public static Boolean operator <(Number a, Number b) => a._value < b._value;
		public static Boolean operator >(Number a, Number b) => a._value > b._value;
		public static Boolean operator <=(Number a, Number b) => a._value <= b._value;
		public static Boolean operator >=(Number a, Number b) => a._value >= b._value;

		public Number(Double value) => _value = value;
		public Number(Boolean value) => _value = (Number)value;
		public Number(String value) => _value = (Number)value;

		public Int32 CompareTo(Object obj)
		{
			if (obj == null)
				return 1;
			if (obj is Number other)
				return _value.CompareTo(other._value);
			if (obj is Double d)
				return _value.CompareTo(d);
			if (obj is Int64 l)
				return _value.CompareTo(l);
			if (obj is UInt64 ul)
				return _value.CompareTo(ul);

			throw new ArgumentException($"{obj} ({obj.GetType().Name}) is not an arithmetic type");
		}

		public Int32 CompareTo(Number other) => _value.CompareTo(other._value);
		public Int32 CompareTo(Double other) => _value.CompareTo(other);
		public Int32 CompareTo(Int64 other) => _value.CompareTo(other);
		public Int32 CompareTo(UInt64 other) => _value.CompareTo(other);

		public override Boolean Equals(Object obj)
		{
			if (obj is Number other)
				return Equals(other);
			if (obj is Double d)
				return _value.Equals(d);
			if (obj is Single f)
				return _value.Equals((Double)f);
			if (obj is Int32 i)
				return Equals((Int64)i);
			if (obj is Int64 l)
				return Equals(l);
			if (obj is UInt32 ui)
				return Equals((UInt64)ui);
			if (obj is UInt64 ul)
				return Equals(ul);

			return false;
		}

		public Boolean Equals(Number other) => _value.Equals(other._value);
		public Boolean Equals(Double other) => _value.Equals(other);
		public Boolean Equals(Int64 other) => _value.Equals(other);
		public Boolean Equals(UInt64 other) => _value.Equals(other);

		public override Int32 GetHashCode() => _value.GetHashCode();
	}
}
