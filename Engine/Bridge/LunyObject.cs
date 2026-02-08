using Luny.Engine.Bridge.Identity;
using Luny.Engine.Registries;
using Luny.Exceptions;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
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
		/// Called when the framework decides to work with the object ("object awakes").
		/// This sends the OnCreate event and - if Enabled - the OnEnable event.
		/// </summary>
		/// <remarks>
		/// Must only be called once prior to using the LunyObject instance.
		/// LunyScript will automatically call this.
		/// </remarks>
		void Initialize();

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
		T Cast<T>();

		/// <summary>
		/// Marks this object for destruction.
		/// Triggers OnDisable (if object is enabled) and OnDestroy events.
		/// The engine-native object is destroyed at the end of the current frame.
		/// </summary>
		void Destroy();
	}

	/// <summary>
	/// Proxy for engine-native objects/nodes/actors/...
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
		private ObjectState _state;

		[NotNull] private static ILunyObjectLifecycleInternal Lifecycle => ((ILunyEngineInternal)LunyEngine.Instance).Lifecycle;

		public LunyObjectID LunyObjectID => _lunyObjectID;
		public LunyNativeObjectID NativeObjectID => _nativeObjectID;
		public SystemObject NativeObject => _nativeObject;

#if DEBUG || LUNY_DEBUG
		private String DebugNativeObjectName { get; set; }
#else
		private String DebugNativeObjectName { get => String.Empty; set {} }
#endif

		public String Name
		{
			get => IsValid ? GetNativeObjectName() : $"<null:{DebugNativeObjectName}({_nativeObjectID})>";
			set
			{
				if (IsValid)
				{
					SetNativeObjectName(value);
#if DEBUG || LUNY_DEBUG
					DebugNativeObjectName = GetNativeObjectName();
#endif
				}
			}
		}

		public Boolean IsValid => !_state.IsDestroyed && IsNativeObjectValid();

		public Boolean IsEnabled
		{
			get => _state.IsEnabled && IsValid;
			set
			{
				if (_state.IsEnabled != value && IsValid)
					SetEnabledState(value);
			}
		}

		public Boolean IsEnabledInHierarchy => _state.IsEnabled && IsValid && GetNativeObjectEnabledInHierarchy();
		public Boolean IsVisible
		{
			get => _state.IsVisible && IsValid;
			set
			{
				if (_state.IsVisible != value && IsValid)
					SetVisibleState(value);
			}
		}

		private LunyObject() {} // Hidden ctor

		/// <summary>
		/// Instantiates a LunyObject instance.
		/// </summary>
		protected LunyObject(SystemObject nativeObject, Int64 nativeObjectID, Boolean isNativeObjectEnabled, Boolean isNativeObjectVisible)
		{
			if (nativeObject == null)
				throw new LunyBridgeException($"{this}: {nameof(LunyObject)} created with a <null> reference");

			_state.IsEnabled = isNativeObjectEnabled;
			_state.IsVisible = isNativeObjectVisible;
			_nativeObject = nativeObject;
			_nativeObjectID = nativeObjectID;
			_lunyObjectID = LunyObjectID.Generate();

			((ILunyObjectRegistryInternal)LunyEngine.Instance.Objects).Register(this);
		}

		protected static Boolean TryGetCached(Int64 nativeId, out ILunyObject lunyObject) =>
			LunyEngine.Instance.Objects.TryGetByNativeID(nativeId, out lunyObject);

		public T As<T>() where T : class => _nativeObject as T;
		public T Cast<T>() => (T)_nativeObject;

		public void Initialize()
		{
			ThrowIfInitializedAgain();
			_state.IsInitialized = true;

			DebugNativeObjectName = GetNativeObjectName();

			Lifecycle.OnObjectCreated(this);
			OnCreate?.Invoke();

			SetVisibleState(_state.IsVisible);
			if (_state.IsEnabled)
				SetEnabledState(_state.IsEnabled); // will trigger OnEnable
		}

		public void Destroy()
		{
			if (_state.IsDestroyed)
				return;

			IsEnabled = false; // may trigger OnDisable
			OnDestroy?.Invoke();
			UnregisterAllEvents();

			// Mark as destroyed (native destruction happens at the end of the frame)
			_state.IsDestroyed = true;

			Lifecycle.OnObjectDestroyed(this);
			((ILunyObjectRegistryInternal)LunyEngine.Instance.Objects).Unregister(this);
		}

		private void UnregisterAllEvents()
		{
			OnCreate = null;
			OnDestroy = null;
			OnEnable = null;
			OnDisable = null;
			OnReady = null;
		}

		~LunyObject() => LunyTraceLogger.LogInfoFinalized(this);

		private void SetVisibleState(Boolean visible)
		{
			_state.IsVisible = visible;

			if (visible)
				SetNativeObjectVisible();
			else
				SetNativeObjectInvisible();
		}

		private void SetEnabledState(Boolean enabled)
		{
			_state.IsEnabled = enabled;

			if (enabled)
			{
				SetNativeObjectEnabled();
				Lifecycle.OnObjectEnabled(this);
				OnEnable?.Invoke();
			}
			else
			{
				SetNativeObjectDisabled();
				Lifecycle.OnObjectDisabled(this);
				OnDisable?.Invoke();
			}
		}

		// LunyObjectLifecycleManager calls this
		internal void InvokeOnReady()
		{
			ThrowIfAlreadyReady();

			_state.IsReady = true;
			OnReady?.Invoke();
		}

		// Should only be called internally by LunyObjectLifecycleManager from pending destroy queue processing
		internal void DestroyNativeObjectInternal()
		{
			if (!_state.IsDestroyed)
				throw new LunyLifecycleException($"{this}: {nameof(DestroyNativeObjectInternal)}() called without prior {nameof(Destroy)}()");

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

		[Conditional("DEBUG")] [Conditional("LUNY_DEBUG")]
		private void ThrowIfInitializedAgain()
		{
#if DEBUG || LUNY_DEBUG
			if (_state.IsInitialized)
				throw new LunyLifecycleException($"{this} has already been initialized!");
#endif
		}

		[Conditional("DEBUG")] [Conditional("LUNY_DEBUG")]
		private void ThrowIfAlreadyReady()
		{
#if DEBUG || LUNY_DEBUG
			if (_state.IsReady)
				throw new LunyLifecycleException($"{this} is already Ready!");
#endif
		}

		private struct ObjectState
		{
			[Flags]
			private enum StateFlags
			{
				Destroyed = 1 << 0,
				Initialized = 1 << 1,
				Enabled = 1 << 2,
				Visible = 1 << 3,
				Ready = 1 << 4,
			}

			private StateFlags _flags;

			public Boolean IsInitialized
			{
				get => (_flags & StateFlags.Initialized) != 0;
				set => SetFlag(StateFlags.Initialized, value);
			}

			public Boolean IsEnabled
			{
				get => (_flags & StateFlags.Enabled) != 0;
				set => SetFlag(StateFlags.Enabled, value);
			}

			public Boolean IsVisible
			{
				get => (_flags & StateFlags.Visible) != 0;
				set => SetFlag(StateFlags.Visible, value);
			}

			public Boolean IsDestroyed
			{
				get => (_flags & StateFlags.Destroyed) != 0;
				set => SetFlag(StateFlags.Destroyed, value);
			}

			public Boolean IsReady
			{
				get => (_flags & StateFlags.Ready) != 0;
				set => SetFlag(StateFlags.Ready, value);
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private void SetFlag(StateFlags flag, Boolean value)
			{
				if (value)
					_flags |= flag;
				else
					_flags &= ~flag;
			}

			public override String ToString()
			{
				var sb = new StringBuilder("(");
				var first = true;

				if (IsDestroyed)
				{
					sb.Append(nameof(StateFlags.Destroyed));
					first = false;
				}
				if (IsInitialized)
				{
					AppendSeparatorIfNeeded();
					sb.Append(nameof(StateFlags.Initialized));
					first = false;
				}
				if (IsEnabled)
				{
					AppendSeparatorIfNeeded();
					sb.Append(nameof(StateFlags.Enabled));
					first = false;
				}
				if (IsVisible)
				{
					AppendSeparatorIfNeeded();
					sb.Append(nameof(StateFlags.Visible));
				}
				if (IsReady)
				{
					AppendSeparatorIfNeeded();
					sb.Append(nameof(StateFlags.Ready));
				}

				sb.Append(")");
				return sb.ToString();

				void AppendSeparatorIfNeeded()
				{
					if (!first)
						sb.Append("|");
				}
			}
		}
	}
}
