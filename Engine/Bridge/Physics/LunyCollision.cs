using System;

namespace Luny.Engine.Bridge.Physics
{
	public abstract class LunyCollision
	{
		protected Object _nativeObject;

		public Object NativeObject => _nativeObject;

		public abstract LunyCollider Collider { get; }
		public abstract String Tag { get; }
		public abstract String Name { get; }
		public abstract String LayerName { get; }
		public abstract Int32 Layer { get; }

		protected LunyCollision() {}

		protected LunyCollision(Object collision) => _nativeObject = collision;

		public abstract Boolean HasComponent(Type type);
	}
}
