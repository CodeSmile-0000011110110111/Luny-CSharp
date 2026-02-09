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
		Table.VarHandle GetHandle(String key);
		Table.VarHandle DefineConstant(String key, Variable value);
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
			get => _table.TryGetValue(key, out var handle) ? handle.Value : null;
			set => GetHandle(key).Value = value;
		}

		/// <summary>
		/// Gets the number of variables.
		/// </summary>
		public Int32 Count => _table.Count;

		public IEnumerator<KeyValuePair<String, Variable>> GetEnumerator()
		{
			foreach (var kvp in _table)
				yield return new KeyValuePair<String, Variable>(kvp.Key, kvp.Value.Value);
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		/// <summary>
		/// Gets a variable with type casting.
		/// </summary>
		public T Get<T>(String key)
		{
			if (!_table.TryGetValue(key, out var handle))
				return default;

			return handle.Value.As<T>();
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
		public VarHandle GetHandle(String key)
		{
			if (!_table.TryGetValue(key, out var handle))
			{
				handle = new VarHandle(this, key);
				_table[key] = handle;
			}

			return handle;
		}

		/// <summary>
		/// Defines a constant variable that cannot be modified after creation.
		/// </summary>
		/// <param name="key">The constant name.</param>
		/// <param name="value">The constant value.</param>
		/// <returns>The handle to the constant.</returns>
		public VarHandle DefineConstant(String key, Variable value)
		{
			if (_table.TryGetValue(key, out var existing))
				throw new InvalidOperationException($"Attempt to redefine constant: {existing}");

			var handle = new VarHandle(this, key, true);
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
				handle.Value = default;
		}

		/// <summary>
		/// Resets values of all variables to their default value.
		/// </summary>
		public void ResetValues()
		{
			foreach (var handle in _table.Values)
				handle.Value = default;
		}

		[ExcludeFromCodeCoverage]
		public override String ToString()
		{
			if (_table.Count == 0)
				return $"{nameof(Table)}: (empty)";

			var sb = new StringBuilder();
			sb.AppendLine($"{nameof(Table)} ({_table.Count}):");
			foreach (var kvp in _table)
				sb.AppendLine($"    [\"{kvp.Key}\"] = {kvp.Value.Value}");

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

		public sealed class VarHandle
		{
			private readonly Table _owner;
			private readonly String _name;
			private readonly Boolean _isConstant;
			private Variable _value;

			public String Name => _name;
			public Boolean IsConstant => _isConstant;

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

			internal VarHandle(Table owner, String name, Boolean isConstant = false)
			{
				_owner = owner;
				_name = name;
				_isConstant = isConstant;
			}

			internal void SetInitialValue(Variable value) => _value = value;

			public override String ToString() => $"{_name}[{_value}] {(_isConstant ? "(const)" : "")}";
		}
	}
}
