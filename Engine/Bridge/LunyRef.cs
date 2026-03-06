using Luny.Engine.Bridge;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Luny
{
	public abstract class LunyRef<T> where T : class
	{
		private String _query;
		protected T _cachedObject;

		public T Value => _cachedObject ??= _query != null ? ResolveObject(_query) : null;

		protected LunyRef(String query)
		{
			if (String.IsNullOrEmpty(query))
				throw new ArgumentException("Query string cannot be null or empty.", nameof(query));

			_query = query;
		}

		protected LunyRef(Object obj)
		{
			if (obj == null)
				throw new ArgumentNullException("Object cannot be null or empty.", nameof(obj));

			_cachedObject = (T)obj;
		}

		protected abstract T ResolveObject([NotNull] String query);
	}

	public sealed class LunyObjectRef : LunyRef<LunyObject>
	{
		public static implicit operator LunyObjectRef(String name) => new(name);
		public static implicit operator LunyObjectRef(LunyObject obj) => new(obj);
		public static implicit operator LunyObject(LunyObjectRef objectRef) => objectRef.Value;

		public LunyObjectRef(String name)
			: base(name) {}

		public LunyObjectRef(ILunyObject obj)
			: base((LunyObject)obj) {}

		protected override LunyObject ResolveObject(String query) => (LunyObject)LunyEngine.Instance.Scene.FindObjectByName(query);
	}

	public sealed class LunyAssetRef : LunyRef<LunyAsset>
	{
		public static implicit operator LunyAssetRef(String name) => new(name);
		public static implicit operator LunyAssetRef(LunyAsset asset) => new(asset);

		public LunyAssetRef(String query)
			: base(query) {}

		public LunyAssetRef(ILunyAsset asset)
			: base((LunyAsset)asset) {}

		protected override LunyAsset ResolveObject(String query) => throw new NotImplementedException(nameof(ResolveObject));
	}
}
