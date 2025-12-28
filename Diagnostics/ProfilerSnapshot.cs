using System;
using System.Collections.Generic;

namespace Luny.Diagnostics
{
	public interface IProfilerSnapshot
	{
		IReadOnlyDictionary<EngineLifecycleEvents, IReadOnlyList<ObserverMetrics>> CategorizedMetrics { get; }
		DateTime Timestamp { get; }
		Int64 FrameCount { get; }
	}

	/// <summary>
	/// Immutable snapshot of profiler state at a specific point in time.
	/// Useful for querying performance metrics without blocking the profiler.
	/// </summary>
	public sealed class ProfilerSnapshot : IProfilerSnapshot
	{
		public IReadOnlyDictionary<EngineLifecycleEvents, IReadOnlyList<ObserverMetrics>> CategorizedMetrics { get; internal set; }
		public DateTime Timestamp { get; internal set; }
		public Int64 FrameCount { get; internal set; }

		public override String ToString() => $"ProfilerSnapshot @ {Timestamp:HH:mm:ss.fff}: {CategorizedMetrics[EngineLifecycleEvents.OnStartup]?.Count} observers";
	}
}
