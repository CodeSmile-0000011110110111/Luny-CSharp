using System;

namespace Luny.Engine.Bridge
{
	public struct LunyInputEvent
	{
		public String ActionName;
		public LunyInputActionType ActionType;
		public LunyVector2 Direction;
		public Single Axis;
		public Single Strength;
		public Boolean IsJustPressed;
		public Boolean IsPressed;
	}
}
