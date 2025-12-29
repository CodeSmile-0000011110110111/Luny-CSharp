using System;

namespace Luny.Proxies
{
	/// <summary>
	/// Engine-agnostic interface for engine objects/nodes.
	/// Provides unified access to common object properties and operations.
	/// </summary>
	public interface ILunyObject
	{
		LunyID LunyID { get; }
		NativeID NativeID { get; }
		String Name { get; }
		Boolean IsValid { get; }
		Boolean Enabled { get; set; }
		Object GetNativeObject();
		T As<T>() where T : class;
	}

	/// <summary>
	/// Engine-agnostic base class for wrapping engine objects/nodes.
	/// Provides unified access to common object properties and operations.
	/// </summary>
	public abstract class LunyObject : ILunyObject
	{
		/// <summary>
		/// LunyScript-specific unique identifier (distinct from engine's native ID).
		/// </summary>
		public LunyID LunyID { get; }

		/// <summary>
		/// Engine-specific unique identifier, subject to engine's behaviour (ie may change between runs, or not).
		/// </summary>
		public abstract NativeID NativeID { get; }

		/// <summary>
		/// The name of the object in the scene hierarchy.
		/// </summary>
		public abstract String Name { get; set; }

		/// <summary>
		/// Whether the underlying engine object is valid/exists.
		/// </summary>
		public abstract Boolean IsValid { get; }

		/// <summary>
		/// Whether the engine object is receiving lifecycle events and runs scripts.
		/// Matches the "Active", "Enabled", or "Paused" (inverted) state of an engine object.
		/// </summary>
		/// <remarks>
		/// For engines using the "Paused" concept: enabled == "not paused" / disabled == "paused".
		/// </remarks>
		public abstract Boolean Enabled { get; set; }

		protected LunyObject() => LunyID = LunyID.Generate();

		/// <summary>
		/// Gets the underlying engine-native object as generic System.Object type (cast as necessary).
		/// Note: Engine implementations provide a property for typed access to the engine-native object. Get<T> can also be used.
		/// </summary>
		public abstract Object GetNativeObject();

		/// <summary>
		/// Gets the underlying engine-native object cast to T. Returns null if type T is mismatched.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public T As<T>() where T : class => GetNativeObject() as T;

		public override String ToString() => $"{(Enabled ? "☑" : "☐")} {Name} (Luny:{LunyID}|Native:{NativeID})";
	}
}
