using Luny.Proxies;
using System;
using System.Collections.Generic;

namespace Luny.Services
{
	/// <summary>
	/// Engine-agnostic scene information service.
	/// </summary>
	public interface ISceneService : IEngineService
	{
		/// <summary>
		/// Gets the name of the currently active scene.
		/// </summary>
		String CurrentSceneName { get; }

		/// <summary>
		/// Gets all objects in the current scene.
		/// </summary>
		IReadOnlyList<ILunyObject> GetAllObjects();
		// TODO: provide IEnumerable for scene tree traversal from a given starting point (null = entire tree)

		/// <summary>
		/// Finds an object by name in the current scene.
		/// </summary>
		ILunyObject FindObjectByName(String name);
	}
}
