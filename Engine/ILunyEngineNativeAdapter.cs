using Luny.Exceptions;
using System;

namespace Luny.Engine
{
	/// <summary>
	/// Marker interface for the native engine adapter.
	/// </summary>
	public interface ILunyEngineNativeAdapter
	{
		static bool IsApplicationQuitting;

		static ILunyEngineNativeAdapter ValidateAdapterSingletonInstance(ILunyEngineNativeAdapter existingInstance, Object current)
		{
			if (existingInstance != null)
			{
				throw new LunyLifecycleException($"Duplicate {nameof(ILunyEngineNativeAdapter)} singleton detected! " +
				                                 $"Existing: {existingInstance}, Duplicate: {current}");
			}

			if (current is not ILunyEngineNativeAdapter adapter)
				throw new LunyLifecycleException($"New {nameof(ILunyEngineNativeAdapter)} instance is null or incorrect type: {current}");

			return adapter;
		}

		static void ThrowIfAdapterNull(ILunyEngineNativeAdapter adapter)
		{
			if (adapter == null)
				throw new LunyLifecycleException($"{nameof(ILunyEngineNativeAdapter)} is null");
		}

		static void ThrowIfLunyEngineNull(ILunyEngine lunyEngine)
		{
			if (lunyEngine == null)
				throw new LunyLifecycleException($"{nameof(ILunyEngine)} is null");
		}

		static void ThrowIfPrematurelyRemoved(ILunyEngineNativeAdapter adapter, ILunyEngine lunyEngine)
		{
			if (!IsApplicationQuitting || adapter != null)
			{
				if (lunyEngine != null)
					Shutdown(adapter, lunyEngine);

				throw new LunyLifecycleException($"{adapter} unexpectedly removed from Scene! It must not be destroyed/removed manually.");
			}
		}

		static void Shutdown(ILunyEngineNativeAdapter adapter, ILunyEngine lunyEngine)
		{
			LunyTraceLogger.LogInfoShuttingDown(adapter);
			lunyEngine?.OnEngineShutdown();
		}

		static void ShutdownComplete(ILunyEngineNativeAdapter adapter) => LunyTraceLogger.LogInfoShutdownComplete(adapter);

		static void EndLogging()
		{
			LunyLogger.Logger = null;
			GC.Collect(0, GCCollectionMode.Forced, true);
		}
	}
}
