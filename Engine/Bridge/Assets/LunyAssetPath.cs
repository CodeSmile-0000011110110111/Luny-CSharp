using System;

namespace Luny.Engine.Bridge
{
	/// <summary>
	/// Specialized path for engine assets.
	/// </summary>
	public sealed class LunyAssetPath : LunyPath
	{
		protected override LunyPathType PathType => LunyPathType.Asset;

		public static implicit operator LunyAssetPath(String agnosticPath) => FromAgnostic(agnosticPath);

		public new static LunyAssetPath FromNative(String nativePath) => new(nativePath, true);
		public new static LunyAssetPath FromAgnostic(String agnosticPath) => new(agnosticPath, false);

		private LunyAssetPath(String path, Boolean isNative)
			: base(path, isNative) {}
	}
}
