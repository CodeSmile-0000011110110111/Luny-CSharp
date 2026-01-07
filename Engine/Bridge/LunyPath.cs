using CodeSmile;
using Luny.Exceptions;
using System;

namespace Luny.Engine.Bridge
{
	/// <summary>
	/// Interface for engine-agnostic path representations.
	/// </summary>
	public interface ILunyPath
	{
		String AgnosticPath { get; }
		String NativePath { get; }
	}

	public abstract class LunyPath : ILunyPath
	{
		private String _agnosticPath;
		public String NativePath { get; }
		public String AgnosticPath => _agnosticPath ??= ToEngineAgnosticPath(NativePath).ToForwardSlashes();

		public static implicit operator String(LunyPath lunyPath) => lunyPath.NativePath;

		private LunyPath() {}

		protected LunyPath(String nativePath)
		{
			if (String.IsNullOrEmpty(nativePath))
				throw new LunyBridgeException($"{this}: {nameof(LunyPath)} initialized with <null> or <empty> string");

			NativePath = nativePath;
			_agnosticPath = null;
		}

		/// <summary>
		/// Converts the native path to a path relative to the project without any engine artifacts.
		/// </summary>
		/// <param name="nativePath"></param>
		/// <returns></returns>
		protected abstract String ToEngineAgnosticPath(String nativePath);

		public override String ToString() => NativePath;
	}
}
