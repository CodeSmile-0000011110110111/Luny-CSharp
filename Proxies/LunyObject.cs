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
		public ObjectID ID { get; private set; }

		/// <summary>
		/// The name of the object in the scene hierarchy.
		/// </summary>
		public abstract String Name { get; }

		/// <summary>
		/// Whether the underlying engine object is valid/exists.
		/// </summary>
		public abstract Boolean IsValid { get; }

		protected LunyObject()
		{
			ID = ObjectID.Generate();
		}

		/// <summary>
		/// Gets the underlying engine-native object.
		/// </summary>
		public abstract Object GetNativeObject();

		public override String ToString() => $"{Name} ({ID})";
	}
}
