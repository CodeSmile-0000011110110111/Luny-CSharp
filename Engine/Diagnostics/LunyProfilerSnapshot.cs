using System;
using System.Collections.Generic;

namespace Luny.Engine.Diagnostics
{
	/// <summary>
	/// Immutable snapshot of profiler state at a specific point in time.
	/// Useful for querying performance metrics without blocking the profiler.
	/// </summary>
	public interface ILunyProfilerSnapshot
	{
		IReadOnlyDictionary<LunyEngineLifecycleEvents, IReadOnlyList<LunyObserverMetrics>> CategorizedMetrics { get; }
		DateTime Timestamp { get; }
		Int64 FrameCount { get; }
	}

	/// <summary>
	/// Immutable snapshot of profiler state at a specific point in time.
	/// Useful for querying performance metrics without blocking the profiler.
	/// </summary>
	internal sealed class LunyProfilerSnapshot : ILunyProfilerSnapshot
	{
		public IReadOnlyDictionary<LunyEngineLifecycleEvents, IReadOnlyList<LunyObserverMetrics>> CategorizedMetrics { get; internal set; }
		public DateTime Timestamp { get; internal set; }
		public Int64 FrameCount { get; internal set; }

		public override String ToString() =>
			$"LunyProfilerSnapshot @ {Timestamp:HH:mm:ss.fff}: {CategorizedMetrics[LunyEngineLifecycleEvents.OnEngineStartup]?.Count} observers";
	}
}
