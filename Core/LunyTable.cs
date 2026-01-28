using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace Luny
{
	public sealed class VariableChangedArgs : EventArgs
	{
		public String Name { get; internal set; }
		public ILunyVariable Current { get; internal set; }
		public ILunyVariable Previous { get; internal set; }

		public override String ToString() => $"Variable '{Name}' changed: {Previous} -> {Current}";
	}

	/// <summary>
	/// Dictionary-based variable storage for LunyScript contexts.
	/// </summary>
	public interface ILunyTable : IEnumerable<KeyValuePair<String, LunyVariable>>
	{
		/// <summary>
		/// Sent when a variable changes.
		/// Caution: The event args instance becomes invalid after the call, it will be re-used by the next event.
		/// Copy the values if you want to keep them.
		/// </summary>
		event EventHandler<VariableChangedArgs> OnVariableChanged;
		LunyVariable this[String key] { get; set; }
		T Get<T>(String key);
		Boolean Has(String key);
		Boolean Remove(String key);
		void Clear();
	}

	/// <summary>
	/// Dictionary-based variable storage for LunyScript contexts.
	/// TODO: Consider restricting to number, bool and string values
	/// TODO: Replace with LuaTable when Lua integration is added.
	/// TODO: Optimize boxing/unboxing (consider variant type or generic storage).
	/// TODO: (maybe) create a "table registry" to keep the various tables instead of disposing them
	/// </summary>
	public sealed class LunyTable : ILunyTable
	{
		/// <summary>
		/// Fired when a variable is changed. Only invoked in debug builds.
		/// </summary>
		public event EventHandler<VariableChangedArgs> OnVariableChanged;

		private static readonly VariableChangedArgs CachedChangedEventArgs = new();

		// TODO: replace with LuaTable
		private readonly Dictionary<String, LunyVariable> _table = new();

		/// <summary>
		/// Gets or sets a variable by name.
		/// </summary>
		public LunyVariable this[String key]
		{
			get => _table.TryGetValue(key, out var value) ? value : LunyVariable.Create(key, 0);
			set
			{
				var oldValue = _table.TryGetValue(key, out var existing) ? existing : LunyVariable.Create(key, 0);
				var newValue = LunyVariable.Create(key, value.Value);
				_table[key] = newValue;
				NotifyVariableChanged(key, oldValue, newValue);
			}
		}

		/// <summary>
		/// Gets the number of variables.
		/// </summary>
		public Int32 Count => _table.Count;

		public IEnumerator<KeyValuePair<String, LunyVariable>> GetEnumerator() => _table.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		/// <summary>
		/// Gets a variable with type casting.
		/// </summary>
		public T Get<T>(String key)
		{
			if (!_table.TryGetValue(key, out var variable))
				return default;

			var value = variable.Value;
			if (value == null)
				return default;
			if (value is T tValue)
				return tValue;

			// Handle common conversions
			if (typeof(T) == typeof(Double))
				return (T)(Object)Convert.ToDouble(value, CultureInfo.InvariantCulture);
			if (typeof(T) == typeof(Single))
				return (T)(Object)Convert.ToSingle(value, CultureInfo.InvariantCulture);
			if (typeof(T) == typeof(Int32))
				return (T)(Object)Convert.ToInt32(value, CultureInfo.InvariantCulture);
			if (typeof(T) == typeof(Boolean))
				return (T)(Object)Convert.ToBoolean(value, CultureInfo.InvariantCulture);
			if (typeof(T) == typeof(String))
				return (T)(Object)value.ToString();
			if (typeof(T) == typeof(Number))
				return (T)(Object)new Number(Convert.ToDouble(value, CultureInfo.InvariantCulture));

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
		/// Clears all variables.
		/// </summary>
		public void Clear() => _table.Clear();

		public override String ToString()
		{
			if (_table.Count == 0)
				return "Variables: (empty)";

			var sb = new StringBuilder();
			sb.AppendLine($"Variables: ({_table.Count})");
			foreach (var kvp in _table)
				sb.AppendLine($"  {kvp.Key} = {kvp.Value}");

			return sb.ToString();
		}

		[Conditional("DEBUG")] [Conditional("LUNYSCRIPT_DEBUG")]
		private void NotifyVariableChanged(String key, ILunyVariable oldValue, ILunyVariable newValue)
		{
#if DEBUG || LUNYSCRIPT_DEBUG
			CachedChangedEventArgs.Name = key;
			CachedChangedEventArgs.Previous = oldValue;
			CachedChangedEventArgs.Current = newValue;
			OnVariableChanged?.Invoke(this, CachedChangedEventArgs);
#endif
		}
	}
}
