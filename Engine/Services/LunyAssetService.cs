using Luny.Engine.Bridge;
using System;
using System.Collections.Generic;
using System.IO;

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

	public abstract class LunyAssetServiceBase : LunyEngineServiceBase, ILunyAssetService, ILunyAssetServiceInternal
	{
		private readonly Dictionary<LunyAssetID, ILunyAsset> _cache = new();
		private readonly Dictionary<String, LunyAssetID> _pathToId = new();

		public T Load<T>(LunyAssetPath path) where T : class, ILunyAsset
		{
			var agnosticPath = path.AgnosticPath;
			if (TryGetCached(agnosticPath, out T cachedAsset))
			{
				LunyLogger.LogInfo($"Skip load, asset already cached: {cachedAsset}", this);
				return cachedAsset;
			}

			// Try to load via tiered lookup
			var loadedAsset = TryLoadWithTieredLookup<T>(path);
			if (loadedAsset == null)
			{
				loadedAsset = GetPlaceholder<T>(path);
				LunyLogger.LogWarning($"Asset not found: '{agnosticPath}' (type: {typeof(T).Name}) => using placeholder: {loadedAsset}", this);
			}

			AddToCache(loadedAsset, agnosticPath);
			return loadedAsset;
		}

		public void Unload(LunyAssetID id)
		{
			if (_cache.Remove(id, out var asset))
			{
				_pathToId.Remove(asset.AssetPath.AgnosticPath);
				UnloadAsset(asset);
			}
		}

		private void AddToCache<T>(T loadedAsset, String agnosticPath) where T : class, ILunyAsset
		{
			var assetId = LunyAssetID.Generate();
			_cache[assetId] = loadedAsset;
			_pathToId[agnosticPath] = assetId;
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
				LunyLogger.LogError($"No extension mapping found for asset type {typeof(T).Name}", this);
				return null;
			}

			var agnosticPath = path.AgnosticPath;
			var typeFolderName = GetFolderNameFromType(typeof(T));

			foreach (var ext in extensions)
			{
				var isExtValid = !String.IsNullOrWhiteSpace(ext);

				// Tier 1: direct load {Path}.{ext}
				var directPath = isExtValid ? Path.ChangeExtension(agnosticPath, ext) : agnosticPath;
				var assetPath = LunyAssetPath.FromAgnostic(directPath);
				var asset = LoadAsset<T>(assetPath);
				if (asset != null)
					return asset;

				// Tier 2: Luny/{Type}/{Path}.{ext}
				var lunyPath = $"Luny/{typeFolderName}/{agnosticPath}";
				if (isExtValid)
					lunyPath = Path.ChangeExtension(lunyPath, ext);

				assetPath = LunyAssetPath.FromAgnostic(lunyPath);
				asset = LoadAsset<T>(assetPath);
				if (asset != null)
					return asset;
			}

			return null;
		}

		private String GetFolderNameFromType(Type type)
		{
			var name = type.Name;
			if (name.StartsWith("ILuny"))
				return name.Substring(5);

			if (name.StartsWith("Luny"))
				return name.Substring(4);

			throw new ArgumentException($"Unhandled asset type: {type.Name}");
		}

		/// <summary>
		/// Engine-native implementation of tiered asset lookup.
		/// </summary>
		protected abstract T LoadAsset<T>(LunyAssetPath path) where T : class, ILunyAsset;

		/// <summary>
		/// Engine-native implementation of asset unloading.
		/// </summary>
		protected abstract void UnloadAsset(ILunyAsset asset);

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
