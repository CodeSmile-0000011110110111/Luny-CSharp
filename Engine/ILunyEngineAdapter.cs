using Luny.Exceptions;
using System;

namespace Luny.Engine
{
	/// <summary>
	/// Marker interface for the native engine adapter.
	/// </summary>
	public interface ILunyEngineAdapter
	{
		static ILunyEngineAdapter ValidateAdapterSingletonInstance(ILunyEngineAdapter existingInstance, Object current)
		{
			if (existingInstance != null)
			{
				throw new LunyLifecycleException($"Duplicate {nameof(ILunyEngineAdapter)} singleton detected! " +
				                                 $"Existing: {existingInstance}, Duplicate: {current}");
			}

			if (current is not ILunyEngineAdapter adapter)
				throw new LunyLifecycleException($"New {nameof(ILunyEngineAdapter)} instance is null or incorrect type: {current}");

			return adapter;
		}

		static void AssertNotNull(ILunyEngineAdapter adapter)
		{
			if (adapter == null)
				throw new LunyLifecycleException($"{nameof(ILunyEngineAdapter)} is null");
		}

		static void AssertLunyEngineNotNull(ILunyEngine lunyEngine)
		{
			if (lunyEngine == null)
				throw new LunyLifecycleException($"{nameof(ILunyEngine)} is null");
		}

		static void AssertNotPrematurelyRemoved(ILunyEngineAdapter adapter, ILunyEngine lunyEngine)
		{
			if (adapter != null)
			{
				if (lunyEngine != null)
					ShutdownLunyEngine(adapter, lunyEngine);

				throw new LunyLifecycleException($"{adapter} unexpectedly removed from Scene! It must not be destroyed/removed manually.");
			}
		}

		static void ShutdownLunyEngine(ILunyEngineAdapter adapter, ILunyEngine lunyEngine)
		{
			LunyLogger.LogInfo("Shutting down...", adapter);
			lunyEngine?.OnShutdown();
		}

		static void ShutdownComplete(ILunyEngineAdapter adapter)
		{
			LunyLogger.LogInfo("Shutdown complete.", adapter);
			LunyLogger.Logger = null;
		}
	}
}
