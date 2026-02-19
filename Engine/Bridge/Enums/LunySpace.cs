namespace Luny.Engine.Bridge
{
	/// <summary>
	/// Specifies the coordinate space in which to operate.
	/// </summary>
	public enum LunySpace
	{
		/// <summary>
		/// Operates in world space (absolute, relative to scene origin).
		/// </summary>
		World,
		/// <summary>
		/// Operates in local space (relative to the object itself).
		/// </summary>
		Self,
	}
}
