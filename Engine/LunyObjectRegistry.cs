using Luny.Engine.Bridge;
using Luny.Engine.Identity;
using System;
using System.Collections.Generic;

namespace Luny.Engine
{
	public interface ILunyObjectRegistry {}

	internal interface ILunyObjectRegistryInternal : ILunyObjectRegistry
	{
		Int32 Count { get; }
		IEnumerable<ILunyObject> AllObjects { get; }
		void Register(ILunyObject lunyObject);
		Boolean Unregister(ILunyObject lunyObject);
		Boolean Unregister(LunyObjectID lunyObjectID);
		ILunyObject GetByLunyID(LunyObjectID lunyObjectID);
		ILunyObject GetByNativeID(LunyNativeObjectID lunyNativeObjectID);
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
		}

		/// <summary>
		/// Unregisters an object.
		/// </summary>
		public Boolean Unregister(ILunyObject lunyObject)
		{
			if (lunyObject == null)
				return false;

			return Unregister(lunyObject.LunyObjectID);
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

		/// <summary>
		/// Finds an object by its LunyID.
		/// </summary>
		public ILunyObject GetByLunyID(LunyObjectID lunyObjectID)
		{
			_objectsByLunyID.TryGetValue(lunyObjectID, out var lunyObject);
			return lunyObject;
		}

		/// <summary>
		/// Finds an object by its NativeID.
		/// </summary>
		public ILunyObject GetByNativeID(LunyNativeObjectID lunyNativeObjectID)
		{
			_objectsByNativeID.TryGetValue(lunyNativeObjectID, out var lunyObject);
			return lunyObject;
		}

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
