using Luny.Exceptions;
using System;
using SystemObject = System.Object;

namespace Luny.Proxies
{
	/// <summary>
	/// Engine-agnostic interface for engine objects/nodes.
	/// Provides unified access to common object properties and operations.
	/// </summary>
	public interface ILunyObject
	{
		LunyID LunyID { get; }
		NativeID NativeID { get; }
		String Name { get; }
		Boolean IsValid { get; }
		Boolean IsEnabled { get; set; }
		void Destroy();
		SystemObject NativeObject { get; }
		T As<T>() where T : class;
	}

	/// <summary>
	/// Engine-agnostic base class for wrapping engine objects/nodes.
	/// Provides unified access to common object properties and operations.
	/// </summary>
	public abstract class LunyObject : ILunyObject
	{
		private readonly SystemObject _nativeObject;

		/// <summary>
		/// LunyScript-specific unique identifier. This ID is distinct from engine's native object ID!
		/// </summary>
		public LunyID LunyID { get; }

		/// <summary>
		/// Engine-specific unique identifier, subject to engine's behaviour (ie may change between runs, or not).
		/// </summary>
		public abstract NativeID NativeID { get; }

		/// <summary>
		/// The name of the object in the scene hierarchy.
		/// </summary>
		public abstract String Name { get; set; }

		/// <summary>
		/// Whether the underlying engine object is valid/exists.
		/// </summary>
		public abstract Boolean IsValid { get; }

		/// <summary>
		/// Whether the engine object is receiving lifecycle events and runs scripts.
		/// Matches the "Active", "Enabled", or "Paused" (inverted) state of an engine object.
		/// </summary>
		/// <remarks>
		/// For engines using the "Paused" concept: enabled == "not paused" / disabled == "paused".
		/// </remarks>
		public abstract Boolean IsEnabled { get; set; }

		/// <summary>
		/// Sent once when the object is "created". First event the object invokes.
		/// Technically this happens when an object gets 'registered' with Luny and after the engine object's "create" event ran.
		/// </summary>
		public event Action OnCreate;
		/// <summary>
		/// Sent once when the object is "destroyed". Last event the object invokes. Runs regardless of object's "enabled" state.
		/// Technically this occurs when the object is marked for deletion in Luny, but the engine's object still exists in disabled state.
		/// </summary>
		public event Action OnDestroy;
		/// <summary>
		/// Sent once just before the object first runs OnFixedStep/OnUpdate.
		/// </summary>
		/// <remarks>
		/// Depending on whether OnFixedStep happens to run in the frame the object becomes "ready", the event order is either:
		///		OnCreate => OnEnable => OnReady => OnUpdate (first) => OnLateUpdate (first)
		/// or:
		///		OnCreate => OnEnable => OnReady => OnFixedStep (first) => OnUpdate (first) => OnLateUpdate (first)
		/// </remarks>
		public event Action OnReady;
		/// <summary>
		/// Sent every time the object's enabled state changes to "enabled": visible, updating, interacting with other objects.
		/// Runs right after OnCreate if the object is created in enabled state.
		/// The object is already enabled when this event runs.
		/// </summary>
		public event Action OnEnable;
		/// <summary>
		/// Sent every time the object's enabled state changes to "disabled": hidden, not updating, not interacting with other objects.
		/// If the object is enabled and gets destroyed, OnDisable runs right before OnDestroy.
		/// The object is already disabled when this event runs.
		/// </summary>
		public event Action OnDisable;

		private LunyObject() { }

		/// <summary>
		/// Instantiates a LunyObject instance.
		/// </summary>
		protected LunyObject(SystemObject nativeObject)
		{
			if (nativeObject == null)
				throw new LunyLifecycleException($"{this}: missing native object!");

			_nativeObject = nativeObject;
			LunyID = LunyID.Generate();

			LunyEngine.Instance.Objects.Register(this);
		}

		/// <summary>
		/// Destroys this object, triggering OnDisable/OnDestroy events and performing/queuing native object destruction.
		/// </summary>
		public abstract void Destroy();

		/// <summary>
		/// Gets the underlying engine-native object as generic System.Object type (cast as necessary).
		/// </summary>
		public SystemObject NativeObject => _nativeObject;

		/// <summary>
		/// Gets the underlying engine-native object cast to T.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public T As<T>() where T : class => _nativeObject as T;

		internal void InvokeOnDestroy()
		{
			OnDestroy?.Invoke();
			LunyEngine.Instance.Lifecycle.EnqueueDestroy(this);
		}

		internal void InvokeOnReady() => OnReady?.Invoke();

		internal void InvokeOnEnable()
		{
			OnEnable?.Invoke();
			LunyEngine.Instance.Lifecycle.OnObjectEnabled(this);
		}

		internal void InvokeOnDisable() => OnDisable?.Invoke();

		/// <summary>
		/// Called when the framework decides to work with the object ("object awakes").
		/// This sends the OnCreate event and - if Enabled - the OnEnable event.
		/// </summary>
		public void Activate()
		{
			OnCreate?.Invoke();
			if (IsEnabled)
				InvokeOnEnable();
		}

		/// <summary>
		/// Destroys the underlying native engine object.
		/// Should only be called internally by lifecycle managers when the destroy operation is being queued.
		/// </summary>
		/// <exception cref="InvalidOperationException">Throws if Destroy() was not called before.</exception>
		public abstract void DestroyNativeObject();

		public override String ToString() => $"{(IsEnabled ? "☑" : "☐")} {Name} ({LunyID}, {NativeID})";
	}
}
