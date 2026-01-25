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
		/// Gets objects with matching names in the current scene. Creates LunyObject instances.
		/// </summary>
		IReadOnlyList<ILunyObject> GetObjects(IReadOnlyList<string> objectNames);

		/// <summary>
		/// Finds a LunyObject by name in the current scene.
		/// </summary>
		//bool TryGetLunyObject(String name, out ILunyObject lunyObject);
		//bool TryGetNativeObject(String name, out object nativeObject);
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
