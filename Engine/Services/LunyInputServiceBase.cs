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
		/// Gets last known axis value for the named action.
		/// </summary>
		LunyVector2 GetAxisValue(String actionName);

		/// <summary>
		/// True only on the frame the button transitioned to pressed.
		/// </summary>
		Boolean GetButtonJustPressed(String actionName);

		/// <summary>
		/// True while button is held.
		/// </summary>
		Boolean GetButtonPressed(String actionName);

		/// <summary>
		/// Gets the analog trigger value (0.0–1.0) for the named button action.
		/// </summary>
		Single GetButtonValue(String actionName);
	}

	public abstract class LunyInputServiceBase : LunyEngineServiceBase, ILunyInputService
	{
		public event Action<LunyInputEvent> OnInputAction;

		private readonly Dictionary<String, LunyVector2> _axisValues = new();
		private readonly Dictionary<String, Boolean> _buttonPressed = new();
		private readonly Dictionary<String, Boolean> _buttonJustPressed = new();
		private readonly Dictionary<String, Single> _buttonValues = new();

		public LunyVector2 GetAxisValue(String actionName) => _axisValues.TryGetValue(actionName, out var v) ? v : default;

		public Boolean GetButtonPressed(String actionName) => _buttonPressed.TryGetValue(actionName, out var v) && v;

		public Boolean GetButtonJustPressed(String actionName) => _buttonJustPressed.TryGetValue(actionName, out var v) && v;

		public Single GetButtonValue(String actionName) => _buttonValues.TryGetValue(actionName, out var v) ? v : 0f;

		protected void RaiseDirectionalInput(String actionName, LunyVector2 value)
		{
			_axisValues[actionName] = value;
			OnInputAction?.Invoke(new LunyInputEvent
			{
				ActionName = actionName,
				ActionType = LunyInputActionType.Axis,
				AxisValue = value,
			});
		}

		protected void RaiseAxisInput(String actionName, float value)
		{
			throw new NotImplementedException(nameof(RaiseAxisInput));
			/*
			_axisValues[actionName] = value;
			OnInputAction?.Invoke(new LunyInputEvent
			{
				ActionName = actionName,
				ActionType = LunyInputActionType.Axis,
				AxisValue = value,
			});
		*/
		}


		protected void RaiseButtonInput(String actionName, Boolean pressed, Single analogValue = 1f)
		{
			var wasPressed = GetButtonPressed(actionName);
			_buttonPressed[actionName] = pressed;
			_buttonJustPressed[actionName] = pressed && !wasPressed;
			_buttonValues[actionName] = pressed ? analogValue : 0f;
			OnInputAction?.Invoke(new LunyInputEvent
			{
				ActionName = actionName,
				ActionType = LunyInputActionType.Button,
				IsPressed = pressed,
				IsJustPressed = pressed && !wasPressed,
				ButtonValue = pressed ? analogValue : 0f,
			});
		}

		/// <summary>
		/// Clears per-frame transition flags. Called at the start of each frame via OnServicePreUpdate.
		/// </summary>
		protected override void OnServicePostUpdate()
		{
			foreach (var kvp in _axisValues)
				_axisValues[kvp.Key] = LunyVector2.Zero;
			foreach (var kvp in _buttonPressed)
				_buttonPressed[kvp.Key] = false;
			foreach (var kvp in _buttonJustPressed)
				_buttonJustPressed[kvp.Key] = false;
			foreach (var kvp in _buttonValues)
				_buttonValues[kvp.Key] = 0.0f;
		}
	}
}
