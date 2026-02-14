using System;

namespace Luny.Engine.Bridge
{
	/// <summary>
	/// Base interface for all engine-native assets.
	/// </summary>
	public interface ILunyAsset
	{
		/// <summary>
		/// The unique identifier for this asset.
		/// </summary>
		LunyAssetID AssetID { get; }

		/// <summary>
		/// The path used to load this asset.
		/// </summary>
		LunyAssetPath AssetPath { get; }

		/// <summary>
		/// The native asset reference.
		/// </summary>
		Object NativeAsset { get; }

		/// <summary>
		/// Cast the native asset to a given type. Returns null on type mismatch.
		/// </summary>
		T Cast<T>() where T : class;
	}

	public abstract class LunyAsset : ILunyAsset
	{
		public static Int32 s_NextAssetID;

		public LunyAssetID AssetID { get; }
		public LunyAssetPath AssetPath { get; }
		public Object NativeAsset { get; }

		protected LunyAsset(Object nativeAsset, LunyAssetPath assetPath)
		{
			AssetID = s_NextAssetID++;
			NativeAsset = nativeAsset ?? throw new ArgumentNullException(nameof(nativeAsset));
			AssetPath = assetPath ?? throw new ArgumentNullException(nameof(assetPath));
		}

		public T Cast<T>() where T : class => NativeAsset as T;

		public override String ToString() => $"[{GetType().Name}:{AssetID}@{AssetPath}] {NativeAsset}";
	}
}
