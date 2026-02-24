using Luny.Engine.Bridge;
using System;
using System.Collections.Generic;

namespace Luny.Engine.Services
{
	/// <summary>
	/// Engine-agnostic input service providing event-driven and polling access to input actions.
	/// </summary>
	/// <remarks>
	/// IMPORTANT: Implementations must inherit from both the ILunyInputService interface and its corresponding LunyInputServiceBase class!
	/// </remarks>
	public interface ILunyInputService : ILunyEngineService
	{
		/// <summary>
		/// Fired when any enabled action changes value.
		/// </summary>
		event Action<LunyInputEvent> OnInputAction;

		/// <summary>
		/// Gets last known axis vector for the named action.
		/// </summary>
		LunyVector2 GetDirection(String actionName);

		/// <summary>
		/// Gets last known axis vector as rotation (quaternion). Assumes positive Y is up vector.
		/// </summary>
		LunyQuaternion GetRotation(String actionName);

		/// <summary>
		/// Gets last known axis vector as rotation (quaternion) with custom up vector.
		/// </summary>
		LunyQuaternion GetRotation(String actionName, LunyVector3 worldUp);

		/// <summary>
		/// Gets the analog trigger value (0.0–1.0) for the named button action.
		/// </summary>
		Single GetAxis(String actionName);

		/// <summary>
		/// True only on the frame the button transitioned to pressed.
		/// </summary>
		Boolean GetButtonJustPressed(String actionName);

		/// <summary>
		/// True while button is held.
		/// </summary>
		Boolean GetButtonPressed(String actionName);

		/// <summary>
		/// Returns a button's pressed strength. Used by triggers configured to act like buttons.
		/// </summary>
		Single GetButtonStrength(String actionName);

		/// <summary>
		/// Activate a given control scheme eg "Gamepad" or "Keyboard&Mouse" to prevent other device input from coming through.
		/// </summary>
		/// <param name="schemeName"></param>
		void SetControlScheme(String schemeName);
	}

	public abstract class LunyInputServiceBase : LunyEngineServiceBase, ILunyInputService
	{
		public event Action<LunyInputEvent> OnInputAction;

		private readonly Dictionary<String, LunyVector2> _directionVectors = new();
		private readonly Dictionary<String, Single> _axisValues = new();
		private readonly Dictionary<String, Single> _buttonStrengthValues = new();
		private readonly Dictionary<String, Boolean> _buttonPressed = new();
		private readonly Dictionary<String, Boolean> _buttonJustPressed = new();

		public LunyVector2 GetDirection(String actionName) => _directionVectors.TryGetValue(actionName, out var v) ? v : default;

		public LunyQuaternion GetRotation(String actionName)
		{
			if (_directionVectors.TryGetValue(actionName, out var v) && v != LunyVector2.Zero)
				return LunyQuaternion.Euler(v.X, 0f, v.Y);

			return LunyQuaternion.Identity;
		}

		public LunyQuaternion GetRotation(String actionName, LunyVector3 worldUp)
		{
			if (_directionVectors.TryGetValue(actionName, out var v) && v != LunyVector2.Zero)
				return LunyQuaternion.LookRotation(new LunyVector3(v.X, 0f, v.Y), worldUp);

			return LunyQuaternion.Identity;
		}

		public Single GetAxis(String actionName) => _axisValues.TryGetValue(actionName, out var v) ? v : 0f;
		public Single GetButtonStrength(String actionName) => _buttonStrengthValues.TryGetValue(actionName, out var v) ? v : 0f;
		public Boolean GetButtonPressed(String actionName) => _buttonPressed.TryGetValue(actionName, out var v) && v;
		public Boolean GetButtonJustPressed(String actionName) => _buttonJustPressed.TryGetValue(actionName, out var v) && v;

		public abstract void SetControlScheme(String schemeName);

		protected void SetDirectionalInput(String actionName, LunyVector2 value)
		{
			_directionVectors[actionName] = value;
			var evt = new LunyInputEvent { ActionName = actionName, ActionType = LunyInputActionType.Directional, Direction = value };
			OnInputAction?.Invoke(evt);
		}

		protected void SetAxisInput(String actionName, Single value)
		{
			_axisValues[actionName] = value;
			var evt = new LunyInputEvent { ActionName = actionName, ActionType = LunyInputActionType.Axis, Axis = value };
			OnInputAction?.Invoke(evt);
		}

		protected void SetButtonInput(String actionName, Boolean pressed, Single strength = 1f)
		{
			var wasPressed = GetButtonPressed(actionName);
			var justPressed = pressed && !wasPressed;
			var strengthValue = pressed ? strength : 0f;

			var evt = new LunyInputEvent
			{
				ActionName = actionName,
				ActionType = LunyInputActionType.Button,
				IsPressed = _buttonPressed[actionName] = pressed,
				IsJustPressed = _buttonJustPressed[actionName] = justPressed,
				Strength = _buttonStrengthValues[actionName] = strengthValue,
			};
			OnInputAction?.Invoke(evt);
		}

		/// <summary>
		/// Clears per-frame transition flags. Called at the start of each frame via OnServicePreUpdate.
		/// </summary>
		protected override void OnServicePostUpdate() => _buttonJustPressed.Clear();

		/// <summary>
		/// Simulates axis input for testing. In real Unity, this comes from InputSystem callbacks.
		/// </summary>
		internal void SimulateDirectionalInput(String actionName, LunyVector2 value) => SetDirectionalInput(actionName, value);

		/// <summary>
		/// Simulates axis input for testing. In real Unity, this comes from InputSystem callbacks.
		/// </summary>
		internal void SimulateAxisInput(String actionName, Single value) => SetAxisInput(actionName, value);

		/// <summary>
		/// Simulates button press for testing. In real Unity, this comes from InputSystem callbacks.
		/// </summary>
		internal void SimulateButtonInput(String actionName, Boolean pressed, Single analogValue = 1f) =>
			SetButtonInput(actionName, pressed, analogValue);
	}
}
