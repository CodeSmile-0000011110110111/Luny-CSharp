using System;

namespace Luny.Engine.Bridge
{
	public abstract class LunyCollision
	{
		public Object NativeCollision { get; private set; }
		public Boolean IsValid { get; private set; }

		public abstract String Tag { get; }
		public abstract String Name { get; }
		public abstract String LayerName { get; }
		public abstract Int32 Layer { get; }
		public abstract LunyCollider Collider { get; }

		public virtual void SetNativeCollision(Object nativeCollision)
		{
			IsValid = nativeCollision != null;
			NativeCollision = nativeCollision;
		}

		public abstract Boolean HasComponent(Type type);
	}
}
