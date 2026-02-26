using System;

namespace Luny.Engine.Bridge.Physics
{
	public abstract class LunyCollision
	{
		public Object NativeObject { get; set; }

		public abstract String Tag { get; }
		public abstract String Name { get; }
		public abstract String LayerName { get; }
		public abstract Int32 Layer { get; }
		public abstract Boolean HasComponent(Type type);
	}
}
