using Luny.Engine;
using System;

namespace Luny
{
	public sealed partial class LunyEngine
	{
		public void EnableObserver<T>() where T : ILunyEngineObserver => _observerRegistry.EnableObserver<T>();
		public void DisableObserver<T>() where T : ILunyEngineObserver => _observerRegistry.DisableObserver<T>();
		public Boolean IsObserverEnabled<T>() where T : ILunyEngineObserver => _observerRegistry.IsObserverEnabled<T>();
		public T GetObserver<T>() where T : ILunyEngineObserver => _observerRegistry.GetObserver<T>();
	}
}
