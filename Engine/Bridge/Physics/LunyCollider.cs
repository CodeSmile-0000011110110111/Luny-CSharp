using System;

namespace Luny.Engine.Bridge
{
	public abstract class LunyCollider
	{
		public Object NativeCollider { get; private set; }
		public Boolean IsValid { get; private set; }

		public abstract String Tag { get; }
		public abstract String Name { get; }
		public abstract String LayerName { get; }
		public abstract Int32 Layer { get; }

		public void SetNativeCollider(Object nativeCollider)
		{
			IsValid = nativeCollider != null;
			NativeCollider = nativeCollider;
		}

		public abstract Boolean HasComponent(Type type);
	}
}
