namespace Luny.Engine.Bridge
{
	/// <summary>
	/// LunyObject lifecycle events.
	/// </summary>
	public enum LunySceneEvent
	{
		/// <summary>
		/// Runs at the end of the frame in which object was requested to be destroyed.
		/// Object is disabled and hidden between request to destroy and actual destruction.
		/// Unity: OnDestroy | Godot: N/A
		/// </summary>
		OnSceneUnloaded = 0,
		/// <summary>
		/// Runs immediately after object was instantiated.
		/// Unity: Awake | Godot: _init
		/// </summary>
		OnSceneLoaded = 1,
	}
}
