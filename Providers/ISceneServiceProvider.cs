using System;

namespace Luny.Providers
{
	/// <summary>
	/// Engine-agnostic scene information provider.
	/// </summary>
	public interface ISceneServiceProvider : IEngineServiceProvider
	{
		/// <summary>
		/// Gets the name of the currently active scene.
		/// </summary>
		String CurrentSceneName { get; }
	}
}
