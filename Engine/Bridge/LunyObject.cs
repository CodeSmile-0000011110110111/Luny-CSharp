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
	/// </summary>
	public interface ILunyObject
	{
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
		/// <summary>
		/// LunyScript-specific unique identifier. This ID is distinct from engine's native object ID!
		/// </summary>
		LunyObjectID LunyObjectID { get; }
		/// <summary>
		/// Engine-specific unique identifier, subject to engine's behaviour (ie may change between runs, or not).
		/// Note: The ID must not change during the object's lifetime.
		/// The ID is accessible even if the object has been destroyed to aid debugging.
		/// </summary>
		LunyNativeObjectID NativeObjectID { get; }
		/// <summary>
		/// Gets the underlying engine-native object as generic System.Object type (cast as necessary).
		/// </summary>
		SystemObject NativeObject { get; }
		/// <summary>
		/// The name of the object in the scene hierarchy.
		/// </summary>
		String Name { get; }
		/// <summary>
		/// Whether the underlying engine object is valid/exists.
		/// </summary>
		Boolean IsValid { get; }
		/// <summary>
		/// Whether the engine object is receiving lifecycle events, runs scripts, and is visible.
		/// Matches the "Active", "Enabled", or "Paused" (inverted) state of an engine object.
		/// </summary>
		/// <remarks>
		/// For engines using the "Paused" concept: enabled == "not paused" / disabled == "paused".
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
		/// Gets the underlying engine-native object cast to T.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		T Cast<T>() where T : class;

		/// <summary>
		/// Called when the framework decides to work with the object ("object awakes").
		/// This sends the OnCreate event and - if Enabled - the OnEnable event.
		/// Must only be called once prior to using the LunyObject instance.
		/// </summary>
		void ActivateOnceBeforeUse();

		/// <summary>
		/// Destroys this object, triggering OnDisable/OnDestroy events and performing/queuing native object destruction.
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

		public T Cast<T>() where T : class => _nativeObject as T;

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
