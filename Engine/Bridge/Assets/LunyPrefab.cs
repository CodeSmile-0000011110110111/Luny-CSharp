using System;

namespace Luny.Engine.Bridge
{
	/// <summary>
	/// Represents a loadable engine prefab.
	/// </summary>
	public interface ILunyPrefab : ILunyAsset {}

	public abstract class LunyPrefab : LunyAsset, ILunyPrefab
	{
		protected LunyPrefab(Object nativePrefabAsset, LunyAssetPath assetPath)
			: base(nativePrefabAsset, assetPath) {}

		public abstract T Instantiate<T>() where T : class;
	}
}
