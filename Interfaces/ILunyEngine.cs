using System;

namespace Luny.Interfaces
{
	/// <summary>
	/// LunyEngine interface - receives callbacks from engine adapters and provides service access.
	/// </summary>
	public interface ILunyEngine
	{
		// Lifecycle dispatch methods - receives callbacks from engine adapters
		void OnStartup();
		void OnFixedStep(Double fixedDeltaTime);
		void OnUpdate(Double deltaTime);
		void OnLateUpdate(Double deltaTime);
		void OnShutdown();

		// Observer management
		void EnableObserver<T>() where T : IEngineLifecycleObserver;
		void DisableObserver<T>() where T : IEngineLifecycleObserver;
		Boolean IsObserverEnabled<T>() where T : IEngineLifecycleObserver;

		// Service access
		TService GetService<TService>() where TService : class, IEngineServiceProvider;
		Boolean HasService<TService>() where TService : class, IEngineServiceProvider;
	}
}
