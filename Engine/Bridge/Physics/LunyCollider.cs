using System;

namespace Luny.Engine.Bridge.Physics
{
	public abstract class LunyCollider
	{
		protected Object _nativeObject;
		public Object NativeObject => _nativeObject;

		public abstract String Tag { get; }
		public abstract String Name { get; }
		public abstract String LayerName { get; }
		public abstract Int32 Layer { get; }

		protected LunyCollider() {}
		protected LunyCollider(Object collider) => _nativeObject = collider;

		public abstract Boolean HasComponent(Type type);
	}
}
