using Luny.Proxies;
using System;
using System.Collections.Generic;

namespace Luny.Registries
{
    /// <summary>
    /// Registry for tracking all active LunyObject instances.
    /// Provides O(1) lookup by both LunyID and NativeID.
    /// </summary>
    public sealed class LunyObjectRegistry : ILunyObjectRegistry
    {
        private Dictionary<LunyID, ILunyObject> _objectsByLunyID = new();
        private Dictionary<NativeID, ILunyObject> _objectsByNativeID = new();

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
            if (lunyObject == null) throw new ArgumentNullException(nameof(lunyObject));

            var lunyID = lunyObject.LunyID;
            var nativeID = lunyObject.NativeID;

            if (_objectsByLunyID.ContainsKey(lunyID))
                throw new InvalidOperationException($"Object with LunyID {lunyID} already registered.");

            _objectsByLunyID[lunyID] = lunyObject;
            _objectsByNativeID[nativeID] = lunyObject;
        }

        /// <summary>
        /// Unregisters an object.
        /// </summary>
        public Boolean Unregister(ILunyObject lunyObject)
        {
            if (lunyObject == null) return false;
            return Unregister(lunyObject.LunyID);
        }

        /// <summary>
        /// Unregisters an object by its LunyID.
        /// </summary>
        public Boolean Unregister(LunyID lunyID)
        {
            if (_objectsByLunyID.Remove(lunyID, out var lunyObject))
            {
                _objectsByNativeID.Remove(lunyObject.NativeID);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Finds an object by its LunyID.
        /// </summary>
        public ILunyObject GetByLunyID(LunyID lunyID)
        {
            _objectsByLunyID.TryGetValue(lunyID, out var lunyObject);
            return lunyObject;
        }

        /// <summary>
        /// Finds an object by its NativeID.
        /// </summary>
        public ILunyObject GetByNativeID(NativeID nativeID)
        {
            _objectsByNativeID.TryGetValue(nativeID, out var lunyObject);
            return lunyObject;
        }

        /// <summary>
        /// Clears and disposes the registry.
        /// </summary>
        public void Dispose()
        {
            _objectsByLunyID?.Clear();
            _objectsByNativeID?.Clear();
            _objectsByLunyID = null;
            _objectsByNativeID = null;
        }
    }
}
