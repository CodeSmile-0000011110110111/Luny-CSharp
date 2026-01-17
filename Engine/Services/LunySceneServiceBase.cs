using Luny.Engine.Bridge;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Luny.Engine.Services
{
	/// <summary>
	/// Engine-agnostic scene information service.
	/// </summary>
	/// <remarks>
	/// IMPORTANT: Implementations must inherit from both the ILuny***Service interface and its corresponding Luny***ServiceBase class!
	/// </remarks>
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

	internal interface ILunySceneServiceInternal
	{
		event Action<ILunyScene> OnSceneLoaded;
		event Action<ILunyScene> OnSceneUnloaded;
	}

	public abstract class LunySceneServiceBase : LunyEngineServiceBase, ILunySceneServiceInternal
	{
		public event Action<ILunyScene> OnSceneLoaded;
		public event Action<ILunyScene> OnSceneUnloaded;
		private ILunyScene _currentScene;
		[MaybeNull] public ILunyScene CurrentScene { get => _currentScene; protected set => _currentScene = value; }

		protected void InvokeOnSceneLoaded(ILunyScene scene) => OnSceneLoaded?.Invoke(scene);
		protected void InvokeOnSceneUnloaded(ILunyScene scene) => OnSceneUnloaded?.Invoke(scene);

		public override String ToString() => _currentScene != null ? _currentScene.ToString() : $"<null:{GetType().Name}>";
	}
}
