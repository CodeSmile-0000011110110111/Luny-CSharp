using Luny.Engine.Bridge.Identity;
using Luny.Engine.Registries;
using Luny.Engine.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Luny.Engine.Bridge
{
	internal interface ILunyObjectLifecycleInternal
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
	internal sealed class LunyObjectLifecycle : ILunyObjectLifecycleInternal
	{
		private ILunyObjectRegistryInternal _lunyObjects;
		private Queue<ILunyObject> _pendingReady = new();
		private Queue<ILunyObject> _pendingDestroy = new();
		private Dictionary<LunyObjectID, ILunyObject> _pendingReadyWaitingForEnable = new();

		public LunyObjectLifecycle(ILunyObjectRegistryInternal objectRegistry) =>
			_lunyObjects = objectRegistry ?? throw new ArgumentNullException(nameof(objectRegistry));

		/// <summary>
		/// Queues an object for its OnReady event.
		/// </summary>
		public void OnObjectCreated(ILunyObject lunyObject)
		{
			if (lunyObject.IsEnabledInHierarchy)
				_pendingReady.Enqueue(lunyObject);
			else
				_pendingReadyWaitingForEnable[lunyObject.LunyObjectID] = lunyObject;
		}

		/// <summary>
		/// Queues an object for deferred destruction.
		/// </summary>
		public void OnObjectDestroyed(ILunyObject lunyObject)
		{
			_pendingDestroy.Enqueue(lunyObject);
			_lunyObjects.Unregister(lunyObject);
		}

		/// <summary>
		/// Notifies the manager that an object's enabled state has changed.
		/// Used to move objects from the waiting queue to the ready queue.
		/// </summary>
		public void OnObjectEnabled(ILunyObject lunyObject)
		{
			if (_pendingReadyWaitingForEnable.Remove(lunyObject.LunyObjectID, out var obj))
				_pendingReady.Enqueue(obj);
		}

		public void OnObjectDisabled(ILunyObject lunyObject) {}

		~LunyObjectLifecycle() => LunyTraceLogger.LogInfoFinalized(this);

		public void OnEnginePreUpdate() => ProcessPendingReady();
		public void OnEnginePostUpdate() => ProcessPendingDestroy();

		internal void DestroyNativeNullObjects()
		{
			var allObjects = _lunyObjects.AllObjects.ToList(); // need to operate on a copy
			foreach (var lunyObject in allObjects)
			{
				if (!lunyObject.IsValid)
				{
					LunyLogger.LogWarning($"Object {lunyObject} is no longer valid, destroying.", this);
					lunyObject.Destroy();
				}
			}
		}

		private void ProcessPendingReady()
		{
			while (_pendingReady.Count > 0)
			{
				var obj = _pendingReady.Dequeue();
				if (obj is LunyObject lunyObjectImpl && lunyObjectImpl.IsValid)
					lunyObjectImpl.InvokeOnReady();
			}
		}

		private void ProcessPendingDestroy()
		{
			if (_pendingDestroy.Count > 0)
				LunyTraceLogger.LogTrace($"Destroying {_pendingDestroy.Count} pending objects ...", this);

			while (_pendingDestroy.Count > 0)
			{
				var obj = _pendingDestroy.Dequeue();
				if (obj is LunyObject lunyObjectImpl)
					lunyObjectImpl.DestroyNativeObjectInternal();
			}
		}

		public void Shutdown(LunyObjectRegistry objectRegistry)
		{
			// ensure all objects run their OnDestroy, must use a copy of collection because it will be modified
			var allObjects = objectRegistry.AllObjects.ToArray();
			foreach (var lunyObject in allObjects)
				lunyObject.Destroy();

			OnEnginePostUpdate();

			_pendingReady.Clear();
			_pendingDestroy.Clear();
			_pendingReadyWaitingForEnable.Clear();
			_pendingReady = null;
			_pendingDestroy = null;
			_pendingReadyWaitingForEnable = null;
			_lunyObjects = null;
		}
	}
}
