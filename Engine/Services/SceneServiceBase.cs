using Luny.Engine.Bridge;
using System;
using System.Collections.Generic;

namespace Luny.Engine.Services
{
	/// <summary>
	/// Engine-agnostic scene information service.
	/// CAUTION: Implementations must inherit from both ISceneService interface and SceneServiceBase class!
	/// </summary>
	public interface ISceneService : ILunyEngineService
	{
		/// <summary>
		/// Gets the name of the currently active scene.
		/// </summary>
		String ActiveSceneName { get; }

		void ReloadScene();

		/// <summary>
		/// Gets all objects in the current scene.
		/// </summary>
		IReadOnlyList<ILunyObject> GetAllObjects();
		// TODO: provide IEnumerable for scene tree traversal from a given starting point (null = root)

		/// <summary>
		/// Finds an object by name in the current scene.
		/// </summary>
		ILunyObject FindObjectByName(String name);
	}

	public abstract class SceneServiceBase : LunyEngineServiceBase {}
}
