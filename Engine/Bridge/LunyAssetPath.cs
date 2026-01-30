using System;

namespace Luny.Engine.Bridge
{
	/// <summary>
	/// Specialized path for engine assets.
	/// </summary>
	public sealed class LunyAssetPath : LunyPath
	{
		protected override LunyPathType PathType => LunyPathType.Asset;

		private LunyAssetPath(String path, Boolean isNative)
			: base(path, isNative) {}

		public static new LunyAssetPath FromNative(String nativePath) => new(nativePath, true);
		public static new LunyAssetPath FromAgnostic(String agnosticPath) => new(agnosticPath, false);

		public static implicit operator LunyAssetPath(String agnosticPath) => FromAgnostic(agnosticPath);
	}
}
