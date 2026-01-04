using Luny.Diagnostics;
using Luny.Proxies;
using Luny.Registries;
using System;
using System.Collections.Generic;

namespace Luny.Core
{
    /// <summary>
    /// Manages the lifecycle state transitions of LunyObjects, including deferred
    /// OnReady execution and structural changes (destruction).
    /// </summary>
    public sealed class LunyObjectLifecycleManager : ILunyObjectLifecycleManager
    {
        private ILunyObjectRegistry _registry;
        private Queue<ILunyObject> _pendingReady = new();
        private Queue<ILunyObject> _pendingDestroy = new();
        private Dictionary<LunyID, ILunyObject> _pendingReadyWaitingForEnable = new();

        public LunyObjectLifecycleManager(ILunyObjectRegistry registry)
        {
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        }

        /// <summary>
        /// Queues an object for its OnReady event.
        /// </summary>
        public void EnqueueReady(ILunyObject lunyObject)
        {
            if (lunyObject == null) return;
            
            if (lunyObject.IsEnabled)
            {
                _pendingReady.Enqueue(lunyObject);
            }
            else
            {
                _pendingReadyWaitingForEnable[lunyObject.LunyID] = lunyObject;
            }
        }

        /// <summary>
        /// Queues an object for deferred destruction.
        /// </summary>
        public void EnqueueDestroy(ILunyObject lunyObject)
        {
            if (lunyObject == null) return;
            
            _pendingDestroy.Enqueue(lunyObject);
            _registry.Unregister(lunyObject);
        }

        /// <summary>
        /// Processes the OnReady queue. Should be called at the start of Update/FixedUpdate.
        /// </summary>
        public void ProcessPendingReady()
        {
            while (_pendingReady.Count > 0)
            {
                var obj = _pendingReady.Dequeue();
                if (obj is LunyObject lunyObjectImpl && lunyObjectImpl.IsValid)
                {
                    lunyObjectImpl.InvokeOnReady();
                }
            }
        }

        /// <summary>
        /// Processes the destruction queue. Should be called at the end of the frame.
        /// </summary>
        public void ProcessPendingDestroy()
        {
            while (_pendingDestroy.Count > 0)
            {
                var obj = _pendingDestroy.Dequeue();
                if (obj is LunyObject lunyObjectImpl)
                {
                    if (lunyObjectImpl.IsValid)
                    {
                        LunyLogger.LogWarning($"Object {lunyObjectImpl.Name} was in destroy queue but is still valid. Forcing destruction.");
                    }
                    
                    lunyObjectImpl.DestroyNativeObject();
                }
            }
        }

        /// <summary>
        /// Notifies the manager that an object's enabled state has changed.
        /// Used to move objects from the waiting queue to the ready queue.
        /// </summary>
        public void OnObjectEnabled(ILunyObject lunyObject)
        {
            if (lunyObject != null && _pendingReadyWaitingForEnable.Remove(lunyObject.LunyID, out var obj))
            {
                _pendingReady.Enqueue(obj);
            }
        }

        public void Dispose()
        {
            _pendingReady?.Clear();
            _pendingDestroy?.Clear();
            _pendingReadyWaitingForEnable?.Clear();
            _pendingReady = null;
            _pendingDestroy = null;
            _pendingReadyWaitingForEnable = null;
            _registry = null;
        }
    }
}
