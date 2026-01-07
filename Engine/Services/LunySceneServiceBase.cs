using Luny.Engine.Bridge;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Luny.Engine.Services
{
	/// <summary>
	/// Engine-agnostic scene information service.
	/// CAUTION: Implementations must inherit from both ISceneService interface and SceneServiceBase class!
	/// </summary>
	public interface ILunySceneService : ILunyEngineService
	{
		[MaybeNull] ILunyScene CurrentScene { get; }

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

	internal interface ILunySceneServiceInternal {}

	public abstract class LunySceneServiceBase : LunyEngineServiceBase, ILunySceneServiceInternal
	{
		private ILunyScene _currentScene;
		[MaybeNull] public ILunyScene CurrentScene { get => _currentScene; protected set => _currentScene = value; }

		public override String ToString() => _currentScene != null ? _currentScene.ToString() : $"<null:{GetType().Name}>";
	}
}
