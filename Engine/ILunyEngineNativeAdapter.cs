using Luny.Exceptions;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Luny.Engine
{
	/// <summary>
	/// Marker interface for the native engine adapter.
	/// </summary>
	public interface ILunyEngineNativeAdapter
	{
		static Boolean IsApplicationQuitting;

		static ILunyEngineLifecycle CreateEngine(ref ILunyEngineNativeAdapter nativeAdapterSingleton, ILunyEngineNativeAdapter nativeAdapter)
		{
			nativeAdapterSingleton = ValidateAdapterSingletonInstance(nativeAdapterSingleton, nativeAdapter);
			return LunyEngine.CreateInstance(nativeAdapterSingleton);
		}

		[SuppressMessage("ReSharper", "HeuristicUnreachableCode")]
		static ILunyEngineNativeAdapter ValidateAdapterSingletonInstance(ILunyEngineNativeAdapter existingInstance,
			ILunyEngineNativeAdapter nativeAdapter)
		{
			if (existingInstance != null)
			{
				throw new LunyLifecycleException($"Duplicate {nameof(ILunyEngineNativeAdapter)} singleton detected! " +
				                                 $"Existing: {existingInstance}, Duplicate: {nativeAdapter}");
			}

			return nativeAdapter;
		}

		static void ThrowIfAdapterNull(ILunyEngineNativeAdapter adapter)
		{
			if (adapter == null)
				throw new LunyLifecycleException($"{nameof(ILunyEngineNativeAdapter)} is null");
		}

		static void ThrowIfLunyEngineNull(ILunyEngineLifecycle lunyEngineInternal)
		{
			if (lunyEngineInternal == null)
				throw new LunyLifecycleException($"{nameof(ILunyEngine)} is null");
		}

		static void ThrowIfPrematurelyRemoved(ILunyEngineNativeAdapter adapter, ILunyEngineLifecycle lunyEngineInternal)
		{
			if (!IsApplicationQuitting || adapter != null)
			{
				if (lunyEngineInternal != null)
					Shutdown(adapter, lunyEngineInternal);

				throw new LunyLifecycleException($"{adapter} unexpectedly removed from Scene! It must not be destroyed/removed manually.");
			}
		}

		static void Startup(ILunyEngineNativeAdapter nativeAdapter, ILunyEngineLifecycle lunyEngineInternal) =>
			lunyEngineInternal.OnEngineStartup(nativeAdapter);

		static void Shutdown(ILunyEngineNativeAdapter adapter, ILunyEngineLifecycle lunyEngineInternal)
		{
			LunyTraceLogger.LogInfoShuttingDown(adapter);
			lunyEngineInternal?.OnEngineShutdown(adapter);
		}

		static void ShutdownComplete(ILunyEngineNativeAdapter adapter) => LunyTraceLogger.LogInfoShutdownComplete(adapter);

		static void FixedStep(Double deltaTime, ILunyEngineNativeAdapter nativeAdapter, ILunyEngineLifecycle lunyEngine) =>
			lunyEngine.OnEngineFixedStep(deltaTime, nativeAdapter);

		static void Update(Double deltaTime, ILunyEngineNativeAdapter nativeAdapter, ILunyEngineLifecycle lunyEngine) =>
			lunyEngine.OnEngineUpdate(deltaTime, nativeAdapter);

		static void LateUpdate(Double deltaTime, ILunyEngineNativeAdapter nativeAdapter, ILunyEngineLifecycle lunyEngine) =>
			lunyEngine.OnEngineLateUpdate(deltaTime, nativeAdapter);

		static void EndLogging()
		{
			LunyLogger.Logger = null;
			GC.Collect(0, GCCollectionMode.Forced, true);
		}
	}
}
