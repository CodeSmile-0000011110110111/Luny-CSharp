using Luny.Engine.Bridge.Identity;
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
	}
}
