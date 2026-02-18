using System;
using System.Collections.Generic;

namespace Luny
{
	public readonly struct Variable<T> : IVariable, IEquatable<Variable<T>>
	{
		private readonly T _value;

		public T Value => _value;

		public Variable(T value) => _value = value;

		// IVariable (cold path — boxing acceptable)
		public Variable.ValueType Type => Variable.ValueType.Struct;
		public Boolean AsBoolean() => _value != null;
		public Double AsDouble() => 0.0;
		public String AsString() => _value?.ToString() ?? "null";

		public Boolean Equals(Variable<T> other) =>
			EqualityComparer<T>.Default.Equals(_value, other._value);

		public override Boolean Equals(Object obj) =>
			obj is Variable<T> other && Equals(other);

		public override Int32 GetHashCode() =>
			_value != null ? EqualityComparer<T>.Default.GetHashCode(_value) : 0;

		public static Boolean operator ==(Variable<T> left, Variable<T> right) => left.Equals(right);
		public static Boolean operator !=(Variable<T> left, Variable<T> right) => !left.Equals(right);

		public static implicit operator Variable<T>(T value) => new(value);
		public static implicit operator T(Variable<T> v) => v._value;

		public override String ToString() => _value?.ToString() ?? "null";
	}
}
