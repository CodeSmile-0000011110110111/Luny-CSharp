using System;
using System.Globalization;

namespace Luny
{
	public interface ILunyVariable : IEquatable<ILunyVariable>
	{
		String Name { get; }
		Object Value { get; }

		static LunyVariable Create(Object value) => LunyVariable.Create(value);
		static LunyVariable Create(String name, Object value) => LunyVariable.Create(name, value);

		Boolean AsBoolean();
		Number AsNumber();
		String AsString();
	}

	public readonly struct LunyVariable : ILunyVariable, IEquatable<LunyVariable>, IEquatable<Boolean>, IEquatable<Double>, IEquatable<String>
	{
		internal const String DefaultName = "(N/A)";

		private readonly String _name;

		public String Name
		{
			get
			{
#if DEBUG || LUNYSCRIPT_DEBUG
				return _name ?? DefaultName;
#else
				return DefaultName;
#endif
			}
		}

		public Object Value { get; }

		private LunyVariable(Object value)
		{
			Value = value;
			_name = null;
		}

		private LunyVariable(String name, Object value)
		{
			Value = value;
			_name = name;
		}

		public static LunyVariable Create(Object value) => new(value);
		public static LunyVariable Create(String name, Object value) => new(name, value);

		public Boolean AsBoolean() => Value is Boolean b ? b : false;

		public Number AsNumber() => Value is Number n ? n :
			Value is Double d ? d :
			Value is Single f ? (Number)f :
			Value is Int32 i ? (Number)i : default;

		public String AsString() => Value is String s ? s : default;

		public static implicit operator LunyVariable(Int32 v) => new(v);
		public static implicit operator LunyVariable(Single v) => new(v);
		public static implicit operator LunyVariable(Double v) => new(v);
		public static implicit operator LunyVariable(Boolean v) => new(v);
		public static implicit operator LunyVariable(String v) => new(v);
		public static implicit operator LunyVariable(Number v) => new((Double)v);

		public override String ToString()
		{
			if (Value == null)
				return "<null>";

			var type = Value.GetType();
			if (type == typeof(Double) || type == typeof(Single) || type == typeof(Int32))
				return $"{Convert.ToDouble(Value, CultureInfo.InvariantCulture)} (Number)";
			if (type == typeof(Boolean))
				return $"{Value} (Boolean)";
			if (type == typeof(String))
				return $"{Value} (String)";

			return Value.ToString();
		}

		public Boolean Equals(Boolean b) => AsBoolean() == b;
		public Boolean Equals(Double d) => AsNumber() == d;
		public Boolean Equals(String s) => AsString() == s;
		public Boolean Equals(LunyVariable other) => Equals(Value, other.Value);
		public Boolean Equals(ILunyVariable other) => other != null && Equals(Value, other.Value);

		public override Boolean Equals(Object obj)
		{
			if (obj is LunyVariable other)
				return Equals(other);
			if (obj is ILunyVariable iv)
				return Equals(iv);
			if (obj is Boolean b)
				return Equals(b);
			if (obj is Double d)
				return Equals(d);
			if (obj is String s)
				return Equals(s);

			return false;
		}

		public override Int32 GetHashCode()
		{
			unchecked
			{
				return Value != null ? Value.GetHashCode() : 0;
			}
		}

		public static Boolean operator ==(LunyVariable left, LunyVariable right) => left.Equals(right);
		public static Boolean operator !=(LunyVariable left, LunyVariable right) => !left.Equals(right);

		public static Boolean operator ==(LunyVariable left, Boolean right) => left.Equals(right);
		public static Boolean operator !=(LunyVariable left, Boolean right) => !left.Equals(right);
		public static Boolean operator ==(Boolean left, LunyVariable right) => right.Equals(left);
		public static Boolean operator !=(Boolean left, LunyVariable right) => !right.Equals(left);

		public static Boolean operator ==(LunyVariable left, Double right) => left.Equals(right);
		public static Boolean operator !=(LunyVariable left, Double right) => !left.Equals(right);
		public static Boolean operator ==(Double left, LunyVariable right) => right.Equals(left);
		public static Boolean operator !=(Double left, LunyVariable right) => !right.Equals(left);

		public static Boolean operator ==(LunyVariable left, String right) => left.Equals(right);
		public static Boolean operator !=(LunyVariable left, String right) => !left.Equals(right);
		public static Boolean operator ==(String left, LunyVariable right) => right.Equals(left);
		public static Boolean operator !=(String left, LunyVariable right) => !right.Equals(left);
	}
}
