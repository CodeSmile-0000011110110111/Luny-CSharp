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

	public enum LunyPathType
	{
		Generic,
		Asset,
		Save,
	}

	/// <summary>
	/// Represents a path that can be converted between engine-native and engine-agnostic (Luny) formats.
	/// </summary>
	public class LunyPath : ILunyPath
	{
		private String _agnosticPath;
		private String _nativePath;
		/// <summary>
		/// The active path converter. Must be set by the engine adapter.
		/// </summary>
		public static ILunyPathConverter Converter { get; set; }

		public String NativePath => _nativePath ??= Converter?.ToNative(AgnosticPath, PathType) ?? AgnosticPath;
		public String AgnosticPath =>
			_agnosticPath ??= Converter?.ToLuny(NativePath, PathType).ToForwardSlashes() ?? NativePath.ToForwardSlashes();

		protected virtual LunyPathType PathType => LunyPathType.Generic;

		public static implicit operator String(LunyPath lunyPath) => lunyPath?.NativePath;

		public static LunyPath FromNative(String nativePath) => new(nativePath, true);
		public static LunyPath FromAgnostic(String agnosticPath) => new(agnosticPath, false);

		protected LunyPath(String path, Boolean isNative)
		{
			if (String.IsNullOrEmpty(path))
				throw new LunyBridgeException($"{GetType().Name} initialized with <null> or <empty> string");

			if (isNative)
				_nativePath = path;
			else
				_agnosticPath = path.ToForwardSlashes();
		}

		public override String ToString() => AgnosticPath;
	}

	/// <summary>
	/// Specialized path for engine save data.
	/// </summary>
	public sealed class LunySavePath : LunyPath
	{
		protected override LunyPathType PathType => LunyPathType.Save;

		public static implicit operator LunySavePath(String agnosticPath) => FromAgnostic(agnosticPath);

		public new static LunySavePath FromNative(String nativePath) => new(nativePath, true);
		public new static LunySavePath FromAgnostic(String agnosticPath) => new(agnosticPath, false);

		private LunySavePath(String path, Boolean isNative)
			: base(path, isNative) {}
	}
}
