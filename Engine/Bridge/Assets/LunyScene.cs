using Luny.Exceptions;
using System;
using SystemObject = System.Object;

namespace Luny.Engine.Bridge
{
	/// <summary>
	/// Engine-agnostic scene interface.
	/// </summary>
	public interface ILunyScene
	{
		SystemObject NativeScene { get; }
		String Name { get; }
		ILunyPath Path { get; }

		/// <summary>
		/// Gets the engine-native scene cast to T. Throws if the type cast is invalid.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		T Cast<T>();
	}

	/// <summary>
	/// Proxy for engine-native scene instances.
	/// </summary>
	public abstract class LunyScene : ILunyScene
	{
		private SystemObject _nativeScene;
		private ILunyPath _path;

		public SystemObject NativeScene => _nativeScene;
		public abstract String Name { get; }
		public ILunyPath Path => _path;

		private LunyScene() {}

		protected LunyScene(SystemObject nativeScene, ILunyPath scenePath)
		{
			if (nativeScene == null)
				throw new LunyBridgeException($"{this}: {nameof(LunyScene)} is <null>");
			if (scenePath == null)
				throw new LunyBridgeException($"{this}: scene path is <null>");

			_nativeScene = nativeScene;
			_path = scenePath;
		}

		public T Cast<T>() => (T)_nativeScene;

		public override String ToString() => $"{Name} ({Path})";
	}
}
