using Luny.Engine.Bridge;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Luny.Engine
{
	internal interface ILunyObjectLifecycleManagerInternal
	{
		void OnObjectCreated(ILunyObject lunyObject);
		void OnObjectDestroyed(ILunyObject lunyObject);
		void OnObjectEnabled(ILunyObject lunyObject);
		void OnObjectDisabled(ILunyObject lunyObject);
	}

	/// <summary>
	/// Manages the lifecycle state transitions of LunyObjects, including deferred
	/// OnReady execution and structural changes (destruction).
	/// </summary>
	internal sealed class LunyObjectLifecycleManager : ILunyObjectLifecycleManagerInternal
	{
		private ILunyObjectRegistry _objects;
		private Queue<ILunyObject> _pendingReady = new();
		private Queue<ILunyObject> _pendingDestroy = new();
		private Dictionary<LunyID, ILunyObject> _pendingReadyWaitingForEnable = new();

		public LunyObjectLifecycleManager(ILunyObjectRegistry objectRegistry) =>
			_objects = objectRegistry ?? throw new ArgumentNullException(nameof(objectRegistry));

		/// <summary>
		/// Queues an object for its OnReady event.
		/// </summary>
		public void OnObjectCreated(ILunyObject lunyObject)
		{
			if (lunyObject.IsEnabled)
				_pendingReady.Enqueue(lunyObject);
			else
				_pendingReadyWaitingForEnable[lunyObject.LunyID] = lunyObject;
		}

		/// <summary>
		/// Queues an object for deferred destruction.
		/// </summary>
		public void OnObjectDestroyed(ILunyObject lunyObject)
		{
			_pendingDestroy.Enqueue(lunyObject);
			_objects.Unregister(lunyObject);
		}

		/// <summary>
		/// Notifies the manager that an object's enabled state has changed.
		/// Used to move objects from the waiting queue to the ready queue.
		/// </summary>
		public void OnObjectEnabled(ILunyObject lunyObject)
		{
			if (_pendingReadyWaitingForEnable.Remove(lunyObject.LunyID, out var obj))
				_pendingReady.Enqueue(obj);
		}

		public void OnObjectDisabled(ILunyObject lunyObject) {}

		~LunyObjectLifecycleManager() => LunyLogger.LogInfo($"finalized {GetHashCode()}", this);

		/// <summary>
		/// Processes the OnReady queue. Should be called at the start of Update/FixedUpdate.
		/// </summary>
		public void ProcessPendingReady()
		{
			while (_pendingReady.Count > 0)
			{
				var obj = _pendingReady.Dequeue();
				if (obj is LunyObject lunyObjectImpl && lunyObjectImpl.IsValid)
					lunyObjectImpl.InvokeOnReady();
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
						LunyLogger.LogWarning($"Object {lunyObjectImpl.Name} was in destroy queue but is still valid. Forcing destruction.");

					lunyObjectImpl.DestroyNativeObject();
				}
			}
		}

		public void Shutdown(LunyObjectRegistry objectRegistry)
		{
			// ensure all objects run their OnDestroy, must use a copy of collection because it will be modified
			var allObjects = objectRegistry.AllObjects.ToArray();
			foreach (var lunyObject in allObjects)
				lunyObject.Destroy();

			ProcessPendingDestroy();

			_pendingReady.Clear();
			_pendingDestroy.Clear();
			_pendingReadyWaitingForEnable.Clear();
			_pendingReady = null;
			_pendingDestroy = null;
			_pendingReadyWaitingForEnable = null;
			_objects = null;
		}
	}
}
