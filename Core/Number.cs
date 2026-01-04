using System;
using System.Globalization;

namespace Luny
{
	/// <summary>
	/// A primitive type that wraps a double and provides implicit conversions from and to various types.
	/// </summary>
	[Serializable]
	public readonly struct Number : IComparable, IConvertible, IFormattable, IComparable<Number>, IEquatable<Number>, IComparable<Double>,
		IEquatable<Double>
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

		public static implicit operator Number(String value) =>
			new(String.IsNullOrEmpty(value) ? 0.0 : Convert.ToDouble(value, CultureInfo.InvariantCulture));

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

		public static implicit operator Boolean(Number number) => Math.Abs(number._value) > Double.Epsilon;
		public static implicit operator String(Number number) => number._value.ToString(CultureInfo.InvariantCulture);

		public static Number operator +(Number a, Number b) => a._value + b._value;
		public static Number operator -(Number a, Number b) => a._value - b._value;
		public static Number operator *(Number a, Number b) => a._value * b._value;
		public static Number operator /(Number a, Number b) => a._value / b._value;
		public static Number operator %(Number a, Number b) => a._value % b._value;

		public static Number operator +(Number a) => a._value;
		public static Number operator -(Number a) => -a._value;

		public static Boolean operator ==(Number a, Number b) => a._value.Equals(b._value);
		public static Boolean operator !=(Number a, Number b) => !a._value.Equals(b._value);
		public static Boolean operator <(Number a, Number b) => a._value < b._value;
		public static Boolean operator >(Number a, Number b) => a._value > b._value;
		public static Boolean operator <=(Number a, Number b) => a._value <= b._value;
		public static Boolean operator >=(Number a, Number b) => a._value >= b._value;

		public Number(Double value) => _value = value;

		public Int32 CompareTo(Object obj)
		{
			if (obj == null)
				return 1;
			if (obj is Number other)
				return _value.CompareTo(other._value);
			if (obj is Double d)
				return _value.CompareTo(d);

			throw new ArgumentException("Object must be of type Number or Double");
		}

		public Int32 CompareTo(Double other) => _value.CompareTo(other);

		public Int32 CompareTo(Number other) => _value.CompareTo(other._value);

		public TypeCode GetTypeCode() => TypeCode.Double;
		public Boolean ToBoolean(IFormatProvider provider) => Convert.ToBoolean(_value);
		public Byte ToByte(IFormatProvider provider) => Convert.ToByte(_value);
		public Char ToChar(IFormatProvider provider) => Convert.ToChar(_value);
		public DateTime ToDateTime(IFormatProvider provider) => Convert.ToDateTime(_value);
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
		public Boolean Equals(Double other) => _value.Equals(other);

		public Boolean Equals(Number other) => _value.Equals(other._value);

		public String ToString(String format, IFormatProvider formatProvider) => _value.ToString(format, formatProvider);

		public override Boolean Equals(Object obj)
		{
			if (obj is Number other)
				return Equals(other);
			if (obj is Double d)
				return _value.Equals(d);

			return false;
		}

		public override Int32 GetHashCode() => _value.GetHashCode();

		public override String ToString() => _value.ToString(CultureInfo.InvariantCulture);
	}
}
