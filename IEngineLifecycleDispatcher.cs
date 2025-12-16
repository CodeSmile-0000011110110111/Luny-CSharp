using System;

namespace Luny
{
    /// <summary>
    /// Dispatcher interface - receives callbacks from engine adapters.
    /// </summary>
    public interface IEngineLifecycleDispatcher
    {
        // Dispatcher interface - receives callbacks from engine adapters
        void OnUpdate(double deltaTime);
        void OnLateUpdate(Double delta);
        void OnFixedStep(double fixedDeltaTime);
        void OnShutdown();
    }
}
