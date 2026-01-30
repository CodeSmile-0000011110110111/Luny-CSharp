using Luny.Engine.Bridge;
using Luny.Engine.Bridge.Identity;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Luny.Engine.Registries
{
	public interface ILunyObjectRegistry
	{
		Int32 Count { get; }
		IEnumerable<ILunyObject> AllObjects { get; }
		Boolean TryGetByLunyID(LunyObjectID lunyObjectID, out ILunyObject lunyObject);
		Boolean TryGetByNativeID(LunyNativeObjectID lunyNativeObjectID, out ILunyObject lunyObject);
		ILunyObject GetByName(String objectName);
		ILunyObject FindByName(String objectName);
	}

	internal interface ILunyObjectRegistryInternal : ILunyObjectRegistry
	{
		void Register(ILunyObject lunyObject);
		Boolean Unregister(ILunyObject lunyObject);
		Boolean Unregister(LunyObjectID lunyObjectID);
	}

	/// <summary>
	/// Registry for tracking all active LunyObject instances.
	/// Provides O(1) lookup by both LunyID and NativeID.
	/// </summary>
	internal sealed class LunyObjectRegistry : ILunyObjectRegistryInternal
	{
		private Dictionary<LunyObjectID, ILunyObject> _objectsByLunyID = new();
		private Dictionary<LunyNativeObjectID, ILunyObject> _objectsByNativeID = new();

		/// <summary>
		/// Gets the total number of registered objects.
		/// </summary>
		public Int32 Count => _objectsByLunyID.Count;

		/// <summary>
		/// Gets all registered objects.
		/// </summary>
		public IEnumerable<ILunyObject> AllObjects => _objectsByLunyID.Values;

		/// <summary>
		/// Registers a new object. Throws if already registered.
		/// </summary>
		public void Register(ILunyObject lunyObject)
		{
			if (lunyObject == null)
				throw new ArgumentNullException(nameof(lunyObject));

			var lunyID = lunyObject.LunyObjectID;
			var nativeID = lunyObject.NativeObjectID;

#if DEBUG
			if (_objectsByLunyID.ContainsKey(lunyID))
				throw new InvalidOperationException($"Object with LunyID {lunyID} already registered.");
#endif

			_objectsByLunyID[lunyID] = lunyObject;
			_objectsByNativeID[nativeID] = lunyObject;

			((LunyEngine)LunyEngine.Instance).OnObjectCreated(lunyObject);
		}

		/// <summary>
		/// Unregisters an object.
		/// </summary>
		public Boolean Unregister(ILunyObject lunyObject)
		{
			if (lunyObject == null)
				return false;

			var removed = Unregister(lunyObject.LunyObjectID);
			if (removed)
				((LunyEngine)LunyEngine.Instance).OnObjectDestroyed(lunyObject);
			return removed;
		}

		/// <summary>
		/// Unregisters an object by its LunyID.
		/// </summary>
		public Boolean Unregister(LunyObjectID lunyObjectID)
		{
			if (_objectsByLunyID.Remove(lunyObjectID, out var lunyObject))
			{
				_objectsByNativeID.Remove(lunyObject.NativeObjectID);
				return true;
			}
			return false;
		}

		public ILunyObject GetByName(String objectName) => _objectsByLunyID.Values.FirstOrDefault(obj => obj.Name == objectName);

		public ILunyObject FindByName(String objectName)
		{
			var existing = GetByName(objectName);
			if (existing != null)
				return existing;

			var sceneObject = LunyEngine.Instance.Scene.FindObjectByName(objectName);
			if (sceneObject != null)
			{
				// sceneObject might have been already cached by the bridge (e.g. UnityGameObject.ToLunyObject)
				// check if it's already in our registries by its LunyID or NativeID
				if (TryGetByNativeID(sceneObject.NativeObjectID, out var registeredObject))
					return registeredObject;

				Register(sceneObject);
				return sceneObject;
			}

			// TODO: proxy fallback or auto-create if needed?
			// The task said "with proxy fallback" for Object.Create
			return null;
		}

		/// <summary>
		/// Finds an object by its NativeID.
		/// </summary>
		public Boolean TryGetByNativeID(LunyNativeObjectID lunyNativeObjectID, out ILunyObject lunyObject) =>
			_objectsByNativeID.TryGetValue(lunyNativeObjectID, out lunyObject);

		/// <summary>
		/// Finds an object by its LunyID.
		/// </summary>
		public Boolean TryGetByLunyID(LunyObjectID lunyObjectID, out ILunyObject lunyObject) =>
			_objectsByLunyID.TryGetValue(lunyObjectID, out lunyObject);

		~LunyObjectRegistry() => LunyTraceLogger.LogInfoFinalized(this);

		internal void Shutdown()
		{
			_objectsByLunyID.Clear();
			_objectsByNativeID.Clear();
			_objectsByLunyID = null;
			_objectsByNativeID = null;
		}
	}
}
