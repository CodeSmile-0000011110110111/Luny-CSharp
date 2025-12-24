using Luny.Proxies;
using System;
using System.Collections.Generic;

namespace Luny.Interfaces.Providers
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

		/// <summary>
		/// Gets all objects in the current scene.
		/// </summary>
		IReadOnlyList<LunyObject> GetAllObjects();
		// TODO: provide IEnumerable for scene tree traversal from a given starting point (null = entire tree)

		/// <summary>
		/// Finds an object by name in the current scene.
		/// </summary>
		LunyObject FindObjectByName(String name);
	}
}
