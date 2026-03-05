using System;

namespace Luny.Engine.Bridge
{
	/// <summary>
	/// Shared utility helpers for transform blocks.
	/// </summary>
	public static class LunyTime
	{
		/// <summary>Engine delta time cast to Single, cached once per call site.</summary>
		internal static Single DeltaTime => (Single)LunyEngine.Instance.Time.DeltaTime;
	}
}
