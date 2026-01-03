using Luny.Diagnostics;
using Luny.Exceptions;
using System;

namespace Luny
{
	/// <summary>
	/// Marker interface for the native engine adapter.
	/// </summary>
	public interface IEngineAdapter
	{
		static IEngineAdapter ValidateAdapterSingletonInstance(IEngineAdapter existingInstance, Object current)
		{
			if (existingInstance != null)
			{
				throw new LunyLifecycleException($"Duplicate {nameof(IEngineAdapter)} singleton detected! " +
				                                 $"Existing: {existingInstance}, Duplicate: {current}");
			}

			if (current is not IEngineAdapter adapter)
				throw new LunyLifecycleException($"New {nameof(IEngineAdapter)} instance is null or incorrect type: {current}");

			return adapter;
		}

		static void AssertNotNull(IEngineAdapter adapter)
		{
			if (adapter == null)
				throw new LunyLifecycleException($"{nameof(IEngineAdapter)} is null");
		}

		static void AssertLunyEngineNotNull(ILunyEngine lunyEngine)
		{
			if (lunyEngine == null)
				throw new LunyLifecycleException($"{nameof(ILunyEngine)} is null");
		}

		static void AssertNotPrematurelyRemoved(IEngineAdapter adapter, ILunyEngine lunyEngine)
		{
			if (adapter != null)
			{
				if (lunyEngine != null)
					ShutdownLunyEngine(adapter, lunyEngine);

				throw new LunyLifecycleException($"{adapter} unexpectedly removed from Scene! It must not be destroyed/removed manually.");
			}
		}

		static void ShutdownLunyEngine(IEngineAdapter adapter, ILunyEngine lunyEngine)
		{
			LunyLogger.LogInfo("Shutting down...", adapter);
			lunyEngine?.OnShutdown();
		}

		static void ShutdownComplete(IEngineAdapter adapter)
		{
			LunyLogger.LogInfo("Shutdown complete.", adapter);
			LunyLogger.Logger = null;
		}
	}
}
