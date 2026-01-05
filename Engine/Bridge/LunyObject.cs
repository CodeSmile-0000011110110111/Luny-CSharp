using Luny.Engine.Events;
using Luny.Engine.Identity;
using Luny.Exceptions;
using System;
using System.Diagnostics.CodeAnalysis;
using SystemObject = System.Object;

namespace Luny.Engine.Bridge
{
	/// <summary>
	/// Engine-agnostic interface for engine objects/nodes.
	/// Provides unified access to common object properties and operations.
	/// Safeguards all access to avoid exceptions when the engine-native object may have been destroyed.
	/// </summary>
	public interface ILunyObject
	{
		/// <summary>
		/// Sent once when the object is "created". Guaranteed to be the object's first lifecycle event.
		/// Runs even if the object starts disabled.
		/// </summary>
		/// <remarks>
		/// Runs only when an object gets 'registered' with Luny, not when the engine-native object is created. Engine-native
		/// creation events (Unity: Awake / Godot: _init or ctor) will run before OnCreate.
		/// </remarks>
		/// <remarks>
		/// OnEnable will run right after OnCreate if the object starts enabled.
		/// If the object starts disabled, OnEnable will run once the object gets enabled.
		/// </remarks>
		public event Action OnCreate;
		/// <summary>
		/// Sent once when the object is "destroyed". Guaranteed to be the object's last lifecycle event.
		/// Runs even for disabled objects.
		/// </summary>
		/// <remarks>
		/// Runs when the object is marked for deletion in Luny. The engine's native object is set inactive, but not yet destroyed.
		/// </remarks>
		public event Action OnDestroy;
		/// <summary>
		/// Sent once just before the object runs its first OnUpdate (or OnFixedUpdate, see below).
		/// If the object starts disabled, OnReady is deferred until the object gets enabled.
		/// If the object gets or is enabled, OnEnable is guaranteed to run before OnReady. Both run in the same frame.
		/// </summary>
		/// <remarks>
		/// Depending on whether OnFixedStep happens to run in the frame the object becomes "ready", the event order is either:
		///		OnReady => OnUpdate (first) => OnLateUpdate (first)
		/// or:
		///		OnReady => OnFixedStep (first) => OnUpdate (first) => OnLateUpdate (first)
		/// </remarks>
		public event Action OnReady;
		/// <summary>
		/// Sent every time the object's enabled state changes to "enabled": visible, updating, receiving events, interacting with other objects.
		/// Also runs right after OnCreate if the object starts enabled.
		/// The engine-native object is already set enabled when this event runs.
		/// </summary>
		/// <remarks>
		/// OnDestroy event will run even when a disabled object gets destroyed.
		/// </remarks>
		public event Action OnEnable;
		/// <summary>
		/// Sent every time the object's enabled state changes to "disabled": hidden, not updating, not receiving events, not interacting with other objects.
		/// If the object is enabled and gets destroyed, OnDisable will run before OnDestroy.
		/// The engine-native object is already set disabled when this event runs.
		/// </summary>
		public event Action OnDisable;

		/// <summary>
		/// LunyScript-specific unique, immutable identifier. This ID is distinct from engine's native object ID!
		/// </summary>
		LunyObjectID LunyObjectID { get; }
		/// <summary>
		/// Engine-specific unique, immutable identifier, subject to engine's behaviour (ie may change between runs, or not).
		/// The ID is valid even after the engine-native object has been destroyed to aid debugging.
		/// </summary>
		LunyNativeObjectID NativeObjectID { get; }
		/// <summary>
		/// Gets the underlying engine-native object (GameObject, Node) as generic System.Object type.
		/// Use the Cast<T> method to avoid manually casting the reference.
		/// </summary>
		SystemObject NativeObject { get; }
		/// <summary>
		/// The name of the object in the scene hierarchy.
		/// </summary>
		String Name { get; }
		/// <summary>
		/// Whether the underlying engine object is valid/exists. Most commonly this means "not null" but in some engines like Godot,
		/// it also means the object is still in the scene hierarchy.
		/// </summary>
		Boolean IsValid { get; }
		/// <summary>
		/// Whether the engine object is processing and visible.
		/// Matches the "Active" state of Unity. Most events (update, input, collision, ..) will not run when the object is disabled.
		/// OnDestroy is the exception: It will run for a disabled object when it gets destroyed.
		/// </summary>
		/// <remarks>
		/// CAUTION: IsEnabled also toggles visibility. If the object's IsVisible is set to false,
		/// and then is disabled and enabled again, it will also be visible. If you wish the object to remain invisible,
		/// you will have to set IsVisible=false after re-enabling the object. This is a decent compromise supported by all engines.
		/// </remarks>
		Boolean IsEnabled { get; set; }
		/// <summary>
		/// Returns true only if BOTH the object itself AND all of its parents are enabled. Otherwise returns false.
		/// </summary>
		Boolean IsEnabledInHierarchy { get; }
		/// <summary>
		/// Whether the object is visible (gets rendered).
		/// CAUTION: This property does not imply that the object can be "seen"! IsVisible might be true while the object isn't visible
		/// on the screen, for example because it's outside the camera's viewport, obstructed by other objects closer to the camera,
		/// scaled infinitely small, completely transparent, or missing a resource (shader, texture, mesh) required to display it.
		/// </summary>
		Boolean IsVisible { get; set; }

		/// <summary>
		/// Gets the engine-native object as type T. Returns null for non-matching types.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		T As<T>() where T : class;

		/// <summary>
		/// Gets the engine-native object cast to T. Throws if the type cast is invalid.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		T Cast<T>() where T : class;

		/// <summary>
		/// Called when the framework decides to work with the object ("object awakes").
		/// This sends the OnCreate event and - if Enabled - the OnEnable event.
		/// </summary>
		/// <remarks>
		/// Must only be called once prior to using the LunyObject instance.
		/// LunyScript will automatically call this.
		/// </remarks>
		void ActivateOnceBeforeUse();

		/// <summary>
		/// Marks this object for destruction.
		/// Triggers OnDisable (if object is enabled) and OnDestroy events.
		/// The engine-native object is destroyed at the end of the current frame.
		/// </summary>
		void Destroy();
	}

	/// <summary>
	/// Engine-agnostic base class for wrapping engine objects/nodes.
	/// Provides unified access to common object properties and operations.
	/// </summary>
	public abstract class LunyObject : ILunyObject
	{
		public event Action OnCreate;
		public event Action OnDestroy;
		public event Action OnReady;
		public event Action OnEnable;
		public event Action OnDisable;

		private readonly LunyObjectID _lunyObjectID;
		private readonly LunyNativeObjectID _nativeObjectID;
		private SystemObject _nativeObject;
		private Boolean _isEnabled;
		private Boolean _isVisible;
		private Boolean _isDestroyed;
#if DEBUG
		private Boolean _isActivated;
#endif

		[NotNull] private static ILunyObjectLifecycleManagerInternal LifecycleManager => ((LunyEngine)LunyEngine.Instance).Lifecycle;

		public LunyObjectID LunyObjectID => _lunyObjectID;
		public LunyNativeObjectID NativeObjectID => _nativeObjectID;
		public SystemObject NativeObject => _nativeObject;

		public String Name
		{
			get => IsValid ? GetNativeObjectName() : $"<null:{_nativeObjectID}>";
			set
			{
				if (IsValid && !String.IsNullOrEmpty(value))
					SetNativeObjectName(value);
			}
		}

		public Boolean IsValid => !_isDestroyed && IsNativeObjectValid();

		public Boolean IsEnabled
		{
			get => _isEnabled && IsValid;
			set
			{
				if (_isEnabled != value && IsValid)
				{
					_isEnabled = value;
					SetEnabledState(_isEnabled);
				}
			}
		}

		public Boolean IsEnabledInHierarchy => _isEnabled && IsValid && GetNativeObjectEnabledInHierarchy();
		public Boolean IsVisible
		{
			get => _isVisible && IsValid;
			set
			{
				if (_isVisible != value && IsValid)
				{
					_isVisible = value;
					SetVisibleState(_isVisible);
				}
			}
		}

		private LunyObject() {} // Hidden ctor

		/// <summary>
		/// Instantiates a LunyObject instance.
		/// </summary>
		protected LunyObject(SystemObject nativeObject, Int64 nativeObjectID, Boolean isNativeObjectEnabled, Boolean isNativeObjectVisible)
		{
			if (nativeObject == null)
				throw new LunyLifecycleException($"{this}: {nameof(LunyObject)} initialized with a <null> reference");

			_isEnabled = isNativeObjectEnabled;
			_isVisible = isNativeObjectVisible;
			_nativeObject = nativeObject;
			_nativeObjectID = nativeObjectID;
			_lunyObjectID = LunyObjectID.Generate();

			LunyEngine.Instance.Objects.Register(this);
		}

		public void ActivateOnceBeforeUse()
		{
#if DEBUG
			if (_isActivated)
				throw new LunyLifecycleException($"{this} has already been activated!");

			_isActivated = true;
#endif

			LifecycleManager.OnObjectCreated(this);
			OnCreate?.Invoke();

			SetVisibleState(_isVisible);
			if (_isEnabled)
				SetEnabledState(_isEnabled); // will trigger OnEnable
		}

		public T As<T>() where T : class => _nativeObject as T;
		public T Cast<T>() where T : class => (T)_nativeObject;

		public void Destroy()
		{
			// LunyLogger.LogInfo($"{nameof(UnityObject)}.{nameof(Destroy)}() => {this}", this);
			if (!IsValid)
				return;

			IsEnabled = false; // may trigger OnDisable
			OnDestroy?.Invoke();
			LifecycleManager.OnObjectDestroyed(this);

			// Mark as destroyed (native destruction happens at the end of the frame)
			_isDestroyed = true;
		}

		private void SetVisibleState(Boolean visible)
		{
			if (visible)
				SetNativeObjectVisible();
			else
				SetNativeObjectInvisible();
		}

		private void SetEnabledState(Boolean enabled)
		{
			if (enabled)
			{
				SetNativeObjectEnabled();
				LifecycleManager.OnObjectEnabled(this);
				OnEnable?.Invoke();
			}
			else
			{
				SetNativeObjectDisabled();
				LifecycleManager.OnObjectDisabled(this);
				OnDisable?.Invoke();
			}
		}

		// LunyObjectLifecycleManager calls this
		internal void InvokeOnReady() => OnReady?.Invoke();

		// Should only be called internally by LunyObjectLifecycleManager from pending destroy queue processing
		internal void DestroyNativeObjectInternal()
		{
			if (!_isDestroyed)
				throw new LunyLifecycleException($"{this}: {nameof(DestroyNativeObjectInternal)}() called without {nameof(Destroy)}()!");

			DestroyNativeObject();
			_nativeObject = null;
		}

		protected abstract void DestroyNativeObject();
		protected abstract Boolean IsNativeObjectValid();
		protected abstract String GetNativeObjectName();
		protected abstract void SetNativeObjectName(String name);
		protected abstract Boolean GetNativeObjectEnabledInHierarchy();
		protected abstract Boolean GetNativeObjectEnabled();
		protected abstract void SetNativeObjectEnabled();
		protected abstract void SetNativeObjectDisabled();
		protected abstract void SetNativeObjectVisible();
		protected abstract void SetNativeObjectInvisible();

		public override String ToString() => $"{(IsEnabled ? "☑" : "☐")} {Name} ({LunyObjectID}, {NativeObjectID})";
	}
}
