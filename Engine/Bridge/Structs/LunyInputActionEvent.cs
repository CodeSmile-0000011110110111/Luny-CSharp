using System;

namespace Luny.Engine.Bridge
{
	public sealed class LunyInputActionEvent
	{
		public String ActionMapName;
		public String ActionName;
		public LunyInputActionPhase Phase;
		public Int32 EventFrame;
	}

	public enum LunyInputActionPhase
	{
		Disabled,
		Waiting,
		Started,
		Performed,
		Canceled,
		Performing,
	}
}
