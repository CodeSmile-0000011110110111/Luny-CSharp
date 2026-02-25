using System;

namespace Luny.Engine.Bridge.Physics
{
	public sealed class LunyCollider
	{
		public Object NativeObject { get; set; }

		public String Tag => throw new NotImplementedException();
		public String Name => throw new NotImplementedException();
		public String LayerName => throw new NotImplementedException();
		public Int32 LayerIndex => throw new NotImplementedException();
		public Boolean HasComponent(Type type) => throw new NotImplementedException();
	}
}
