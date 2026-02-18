using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Luny
{
	public sealed class VariableChangedArgs : EventArgs
	{
		public String Name { get; internal set; }
		public Variable Current { get; internal set; }
		public Variable Previous { get; internal set; }

		public override String ToString() => $"Variable '{Name}' changed: {Previous} -> {Current}";
	}

	/// <summary>
	/// Dictionary-based variable storage..
	/// </summary>
	public interface ITable : IEnumerable<KeyValuePair<String, Variable>>
	{
		/// <summary>
		/// Sent when a variable changes.
		/// Caution: The event args instance becomes invalid after the call, it will be re-used by the next event.
		/// Copy the values if you want to keep them.
		/// </summary>
		event EventHandler<VariableChangedArgs> OnVariableChanged;
		Variable this[String key] { get; set; }
		T Get<T>(String key);
		Table.ScalarVarHandle GetHandle(String key);
		Table.ScalarVarHandle DefineConstant(String key, Variable value);
		Table.VarHandle<T> GetHandle<T>(String key);
		Boolean Has(String key);
		Boolean Remove(String key);
		void RemoveAll();
	}

	/// <summary>
	/// Dictionary-based variable storage.
	/// </summary>
	public sealed class Table : ITable
	{
		/// <summary>
		/// Fired when a variable is changed. Only invoked in debug builds.
		/// </summary>
		public event EventHandler<VariableChangedArgs> OnVariableChanged;

#if DEBUG || LUNY_DEBUG
		private static readonly VariableChangedArgs s_CachedChangedEventArgs = new();
#endif

		private readonly Dictionary<String, VarHandle> _table = new();

		/// <summary>
		/// Gets or sets a variable by name.
		/// </summary>
		public Variable this[String key]
		{
			get => _table.TryGetValue(key, out var handle) && handle is ScalarVarHandle scalar ? scalar.Value : null;
			set => GetHandle(key).Value = value;
		}

		/// <summary>
		/// Gets the number of variables.
		/// </summary>
		public Int32 Count => _table.Count;

		public IEnumerator<KeyValuePair<String, Variable>> GetEnumerator()
		{
			foreach (var kvp in _table)
			{
				if (kvp.Value is ScalarVarHandle scalar)
					yield return new KeyValuePair<String, Variable>(kvp.Key, scalar.Value);
			}
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		/// <summary>
		/// Gets a variable with type casting.
		/// </summary>
		public T Get<T>(String key)
		{
			if (!_table.TryGetValue(key, out var handle))
				return default;

			if (handle is ScalarVarHandle scalar)
				return scalar.Value.As<T>();

			if (handle is VarHandle<T> typed)
				return typed.Value;

			return default;
		}

		/// <summary>
		/// Checks if a variable exists.
		/// </summary>
		public Boolean Has(String key) => _table.ContainsKey(key);

		/// <summary>
		/// Removes a variable.
		/// </summary>
		public Boolean Remove(String key) => _table.Remove(key);

		/// <summary>
		/// Removes all variables.
		/// </summary>
		public void RemoveAll() => _table.Clear();

		/// <summary>
		/// Gets a handle to a variable.
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public ScalarVarHandle GetHandle(String key)
		{
			if (_table.TryGetValue(key, out var handle))
				return (ScalarVarHandle)handle;

			var scalar = new ScalarVarHandle(this, key);
			_table[key] = scalar;
			return scalar;
		}

		/// <summary>
		/// Gets or creates a typed handle to a variable.
		/// </summary>
		public VarHandle<T> GetHandle<T>(String key)
		{
			if (_table.TryGetValue(key, out var handle))
				return handle.As<T>();

			var typed = new VarHandle<T>(this, key);
			_table[key] = typed;
			return typed;
		}

		/// <summary>
		/// Defines a constant variable that cannot be modified after creation.
		/// </summary>
		/// <param name="key">The constant name.</param>
		/// <param name="value">The constant value.</param>
		/// <returns>The handle to the constant.</returns>
		public ScalarVarHandle DefineConstant(String key, Variable value)
		{
			if (_table.TryGetValue(key, out var existing))
				throw new InvalidOperationException($"Attempt to redefine constant: {existing}");

			var handle = new ScalarVarHandle(this, key, true);
			handle.SetInitialValue(value);
			_table[key] = handle;
			return handle;
		}

		/// <summary>
		/// Resets a variable's value to default.
		/// </summary>
		/// <param name="key"></param>
		public void ResetValue(String key)
		{
			if (_table.TryGetValue(key, out var handle))
				handle.Reset();
		}

		/// <summary>
		/// Resets values of all variables to their default value.
		/// </summary>
		public void ResetValues()
		{
			foreach (var handle in _table.Values)
				handle.Reset();
		}

		[ExcludeFromCodeCoverage]
		public override String ToString()
		{
			if (_table.Count == 0)
				return $"{nameof(Table)}: (empty)";

			var sb = new StringBuilder();
			sb.AppendLine($"{nameof(Table)} ({_table.Count}):");
			foreach (var kvp in _table)
				sb.AppendLine($"    [\"{kvp.Key}\"] = {kvp.Value}");

			return sb.ToString();
		}

		[Conditional("DEBUG")] [Conditional("LUNY_DEBUG")]
		internal void NotifyVariableChanged(String key, Variable currentValue, Variable previousValue)
		{
#if DEBUG || LUNY_DEBUG
			s_CachedChangedEventArgs.Name = key;
			s_CachedChangedEventArgs.Previous = previousValue;
			s_CachedChangedEventArgs.Current = currentValue;
			OnVariableChanged?.Invoke(this, s_CachedChangedEventArgs);
#endif
		}

		public abstract class VarHandle
		{
			protected readonly Table _owner;
			protected readonly String _name;
			protected readonly Boolean _isConstant;

			public String Name => _name;
			public Boolean IsConstant => _isConstant;

			protected VarHandle(Table owner, String name, Boolean isConstant)
			{
				_owner = owner;
				_name = name;
				_isConstant = isConstant;
			}

			public abstract void Reset();

			public VarHandle<T> As<T>()
			{
				if (this is VarHandle<T> typed)
					return typed;

				throw new InvalidCastException($"{nameof(VarHandle)} '{_name}' is {GetType().Name}, not {nameof(VarHandle)}<{typeof(T).Name}>");
			}

			public Boolean TryAs<T>(out VarHandle<T> result)
			{
				result = this as VarHandle<T>;
				return result != null;
			}
		}

		public sealed class ScalarVarHandle : VarHandle
		{
			private Variable _value;

			public Variable Value
			{
				get => _value;
				set
				{
					if (_isConstant)
						throw new InvalidOperationException($"Cannot modify constant '{_name}'");

					var previous = _value;
					_value = value;
					_owner.NotifyVariableChanged(_name, value, previous);
				}
			}

			internal ScalarVarHandle(Table owner, String name, Boolean isConstant = false)
				: base(owner, name, isConstant) {}

			internal void SetInitialValue(Variable value) => _value = value;

			public override void Reset() => _value = default;

			public override String ToString() => $"{_name}[{_value}] {(_isConstant ? "(const)" : "")}";
		}

		public sealed class VarHandle<T> : VarHandle
		{
			private T _value;

			public T Value
			{
				get => _value;
				set
				{
					if (_isConstant)
						throw new InvalidOperationException($"Cannot modify constant '{_name}'");

					_value = value;
				}
			}

			internal VarHandle(Table owner, String name, Boolean isConstant = false)
				: base(owner, name, isConstant) {}

			internal void SetInitialValue(T value) => _value = value;

			public override void Reset() => _value = default;

			public override String ToString() => $"{_name}[{_value}] {(_isConstant ? "(const)" : "")}";
		}
	}
}
