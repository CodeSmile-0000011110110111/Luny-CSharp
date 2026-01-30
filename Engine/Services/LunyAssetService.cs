using Luny.Engine.Bridge;
using Luny.Engine.Bridge.Identity;
using System;
using System.Collections.Generic;

namespace Luny.Engine.Services
{
	/// <summary>
	/// Engine-agnostic Asset management service.
	/// </summary>
	/// <remarks>
	/// IMPORTANT: Implementations must inherit from both the ILuny***Service interface and its corresponding Luny***ServiceBase class!
	/// </remarks>
	public interface ILunyAssetService : ILunyEngineService
	{
		/// <summary>
		/// Loads an asset of type T by its Luny path.
		/// </summary>
		/// <param name="path">The agnostic path to the asset.</param>
		/// <typeparam name="T">The bridge interface type of the asset (e.g., ILunyPrefab).</typeparam>
		/// <returns>The loaded asset bridge, or a "Missing Asset" placeholder if loading fails.</returns>
		T Load<T>(LunyAssetPath path) where T : class, ILunyAsset;

		/// <summary>
		/// Unloads an asset by its internal ID.
		/// </summary>
		/// <param name="id">The internal asset ID.</param>
		void Unload(LunyAssetID id);
	}

	internal interface ILunyAssetServiceInternal {}

	public abstract class LunyAssetServiceBase : LunyEngineServiceBase, ILunyAssetServiceInternal, ILunyAssetService
	{
		private readonly Dictionary<LunyAssetID, ILunyAsset> _cache = new();
		private readonly Dictionary<String, LunyAssetID> _pathToId = new();

		public T Load<T>(LunyAssetPath path) where T : class, ILunyAsset
		{
			var agnosticPath = path.AgnosticPath;
			if (TryGetCached(agnosticPath, out T load))
				return load;

			// Try to load via tiered lookup
			var loadedAsset = TryLoadWithTieredLookup<T>(path);
			if (loadedAsset == null)
			{
				LunyLogger.LogWarning($"Asset not found: '{agnosticPath}' (type: {typeof(T).Name}) => using placeholder");
				loadedAsset = GetPlaceholder<T>(path);
			}

			AddToCache(loadedAsset, agnosticPath);

			return loadedAsset;
		}

		private void AddToCache<T>(T loadedAsset, String agnosticPath) where T : class, ILunyAsset
		{
			var assetId = new LunyAssetID();
			_cache[assetId] = loadedAsset;
			_pathToId[agnosticPath] = assetId;
		}

		public void Unload(LunyAssetID id)
		{
			if (_cache.Remove(id, out var asset))
			{
				_pathToId.Remove(asset.AssetPath.AgnosticPath);
				UnloadNative(asset);
			}
		}

		private Boolean TryGetCached<T>(String agnosticPath, out T load) where T : class, ILunyAsset
		{
			if (_pathToId.TryGetValue(agnosticPath, out var id))
			{
				if (_cache.TryGetValue(id, out var cachedAsset) && cachedAsset is T asset)
				{
					load = asset;
					return true;
				}
			}

			load = null;
			return false;
		}

		private T TryLoadWithTieredLookup<T>(LunyAssetPath path) where T : class, ILunyAsset
		{
			var mappings = GetExtensionMapping();
			if (!mappings.TryGetValue(typeof(T), out var extensions))
			{
				LunyLogger.LogError($"No extension mapping found for asset type {typeof(T).Name}");
				return null;
			}

			var agnosticPath = path.AgnosticPath;
			var typeFolderName = GetTypeFolderName(typeof(T));

			foreach (var ext in extensions)
			{
				var extension = ext.StartsWith(".") ? ext : "." + ext;

				// Tier 1: Luny/{Type}/{Path}.{ext}
				var lunyPath = $"Luny/{typeFolderName}/{agnosticPath}{extension}";
				var asset = LoadNative<T>(LunyAssetPath.FromAgnostic(lunyPath));
				if (asset != null)
					return asset;

				// Tier 2: {Path}.{ext}
				var directPath = $"{agnosticPath}{extension}";
				asset = LoadNative<T>(LunyAssetPath.FromAgnostic(directPath));
				if (asset != null)
					return asset;
			}

			return null;
		}

		private String GetTypeFolderName(Type type)
		{
			var name = type.Name;
			if (name.StartsWith("ILuny"))
				name = name.Substring(5);
			else if (name.StartsWith("Luny"))
				name = name.Substring(4);

			return name;
		}

		/// <summary>
		/// Engine-native implementation of tiered asset lookup.
		/// </summary>
		protected abstract T LoadNative<T>(LunyAssetPath path) where T : class, ILunyAsset;

		/// <summary>
		/// Engine-native implementation of asset unloading.
		/// </summary>
		protected abstract void UnloadNative(ILunyAsset asset);

		/// <summary>
		/// Provides the extension mapping for different asset types.
		/// </summary>
		protected abstract IReadOnlyDictionary<Type, String[]> GetExtensionMapping();

		/// <summary>
		/// Returns a "Missing Asset" placeholder of the requested type.
		/// </summary>
		protected abstract T GetPlaceholder<T>(LunyAssetPath path) where T : class, ILunyAsset;
	}
}
