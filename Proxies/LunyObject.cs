using System;

namespace Luny.Proxies
{
	/// <summary>
	/// Engine-agnostic base class for wrapping engine objects/nodes.
	/// Provides unified access to common object properties and operations.
	/// </summary>
	public abstract class LunyObject
	{
		/// <summary>
		/// LunyScript-specific unique identifier (distinct from engine's native ID).
		/// </summary>
		public LunyID LunyID { get; }

		/// <summary>
		/// Engine-specific unique identifier, subject to engine's behaviour (ie may change between runs, or not).
		/// </summary>
		public abstract Int64 NativeId { get; }

		/// <summary>
		/// The name of the object in the scene hierarchy.
		/// </summary>
		public abstract String Name { get; }

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
		/// Note: Engine implementations should provide a property for typed access to the engine-native object.
		/// </summary>
		public abstract Object GetNativeObject();

		public override String ToString() => $"{(Enabled ? "☑" : "☐")} {Name} (Luny:{LunyID}|Native:{NativeId})";
	}
}
