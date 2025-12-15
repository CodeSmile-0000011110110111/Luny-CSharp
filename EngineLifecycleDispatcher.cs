namespace Luny
{
    /// <summary>
    /// Singleton dispatcher that will later discover and manage lifecycle observers.
    /// </summary>
    public sealed class EngineLifecycleDispatcher : IEngineLifecycleDispatcher
    {
        private static EngineLifecycleDispatcher _instance;

        /// <summary>
        /// Gets the singleton instance, creating it on first access.
        /// </summary>
        public static EngineLifecycleDispatcher Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new EngineLifecycleDispatcher();
                }
                return _instance;
            }
        }

        // CHANGE Step 3: Introduce LifecycleObserverRegistry and initialize it here.
        private EngineLifecycleDispatcher()
        {
            // CHANGE Step 3: _registry = new LifecycleObserverRegistry(); and call startup for discovered observers.
        }

        // CHANGE Step 4: Implement dispatcher methods to delegate to registry's enabled observers.
        public void OnUpdate(double deltaTime) { }

        public void OnFixedStep(double fixedDeltaTime) { }

        public void OnShutdown() { }

        // CHANGE Step 5: Add ThrowDuplicateAdapterException helper method here.
        // CHANGE Step 6: Add EnableObserver/DisableObserver/IsObserverEnabled wrappers here.
        // CHANGE Step 3/4: Add nested LifecycleObserverRegistry class and discovery logic per RFC.
    }
}
