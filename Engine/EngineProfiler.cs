using Luny.Engine.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Luny.Engine
{
	/// <summary>
	/// Concrete implementation of engine-level profiling.
	/// Tracks execution time for each lifecycle observer with configurable rolling average.
	/// Public methods use [Conditional] attributes - completely stripped in release builds unless LUNY_PROFILE defined.
	/// </summary>
	public interface IEngineProfiler
	{
		Int32 RollingAverageWindow { get; set; }

		IProfilerSnapshot TakeSnapshot();
	}

	/// <summary>
	/// Concrete implementation of engine-level profiling.
	/// Tracks execution time for each lifecycle observer with configurable rolling average.
	/// Public methods use [Conditional] attributes - completely stripped in release builds unless LUNY_PROFILE defined.
	/// </summary>
	internal sealed class EngineProfiler : IEngineProfiler
	{
		private readonly Dictionary<Type, Dictionary<EngineLifecycleEvents, ObserverMetrics>> _metrics = new();
		private readonly Dictionary<IEngineObserver, Stopwatch> _activeObservers = new();
		private Int32 _rollingAverageWindow = 30;
		private ITimeService _timeService;

		public Int32 RollingAverageWindow
		{
			get => _rollingAverageWindow;
			set => _rollingAverageWindow = Math.Max(1, value); // Clamp to minimum 1
		}

		public EngineProfiler(ITimeService timeService) => _timeService = timeService;

		public IProfilerSnapshot TakeSnapshot()
		{
#if DEBUG || LUNY_DEBUG || LUNY_PROFILE
			var categorized = new Dictionary<EngineLifecycleEvents, IReadOnlyList<ObserverMetrics>>();
			var allMetrics = _metrics.Values.SelectMany(d => d.Values).ToList();

			foreach (var category in (EngineLifecycleEvents[])Enum.GetValues(typeof(EngineLifecycleEvents)))
			{
				categorized[category] = allMetrics
					.Where(m => m.Category == category)
					.ToList();
			}

			return new ProfilerSnapshot
			{
				CategorizedMetrics = categorized,
				Timestamp = DateTime.UtcNow,
				FrameCount = _timeService.EngineFrameCount,
			};
#else
			return new ProfilerSnapshot
			{
				CategorizedMetrics = new Dictionary<ProfilerCategory, IReadOnlyList<ObserverMetrics>>(),
				Timestamp = DateTime.UtcNow
			};
#endif
		}

		~EngineProfiler() => LunyLogger.LogInfo($"finalized {GetHashCode()}", this);

		[Conditional("DEBUG")] [Conditional("LUNY_DEBUG")] [Conditional("LUNY_PROFILE")]
		internal void BeginObserver(IEngineObserver observer)
		{
#if DEBUG || LUNY_DEBUG || LUNY_PROFILE
			if (!_activeObservers.TryGetValue(observer, out var sw))
			{
				sw = new Stopwatch();
				_activeObservers[observer] = sw;
			}
			sw.Restart();
#endif
		}

		[Conditional("DEBUG")] [Conditional("LUNY_DEBUG")] [Conditional("LUNY_PROFILE")]
		internal void EndObserver(IEngineObserver observer, EngineLifecycleEvents category)
		{
#if DEBUG || LUNY_DEBUG || LUNY_PROFILE
			if (!_activeObservers.TryGetValue(observer, out var sw))
				return;

			sw.Stop();
			var elapsed = sw.Elapsed.TotalMilliseconds;

			var type = observer.GetType();
			if (!_metrics.TryGetValue(type, out var categoryDict))
			{
				categoryDict = new Dictionary<EngineLifecycleEvents, ObserverMetrics>();
				_metrics[type] = categoryDict;
			}

			if (!categoryDict.TryGetValue(category, out var metrics))
			{
				metrics = new ObserverMetrics { ObserverName = type.Name, Category = category };
				categoryDict[category] = metrics;
			}

			UpdateMetrics(metrics, elapsed);
#endif
		}

		private void UpdateMetrics(ObserverMetrics metrics, Double newSample)
		{
			metrics.CallCount++;
			metrics.TotalMs += newSample;

			// Rolling average: disabled if window <= 1
			if (_rollingAverageWindow <= 1)
				metrics.AverageMs = newSample; // No averaging, just use current sample
			else
			{
				// Simple rolling average calculation
				var window = Math.Min(_rollingAverageWindow, metrics.CallCount);
				metrics.AverageMs = (metrics.AverageMs * (window - 1) + newSample) / window;
			}

			if (metrics.CallCount % RollingAverageWindow == 0 || metrics.CallCount == 1)
				metrics.MinMs = metrics.MaxMs = newSample;
			else
			{
				metrics.MinMs = Math.Min(metrics.MinMs, newSample);
				metrics.MaxMs = Math.Max(metrics.MaxMs, newSample);
			}
		}

		[Conditional("DEBUG")] [Conditional("LUNY_DEBUG")] [Conditional("LUNY_PROFILE")]
		internal void RecordError(IEngineObserver observer, EngineLifecycleEvents category, Exception ex)
		{
#if DEBUG || LUNY_DEBUG || LUNY_PROFILE
			var type = observer.GetType();
			if (_metrics.TryGetValue(type, out var categoryDict) && categoryDict.TryGetValue(category, out var metrics))
				metrics.ErrorCount++;
#endif
		}

		[Conditional("DEBUG")] [Conditional("LUNY_DEBUG")] [Conditional("LUNY_PROFILE")]
		public void Reset()
		{
#if DEBUG || LUNY_DEBUG || LUNY_PROFILE
			_metrics.Clear();
			_activeObservers.Clear();
#endif
		}
	}

	/// <summary>
	/// Performance metrics for a single lifecycle observer.
	/// Tracks execution time statistics and error counts.
	/// </summary>
	public sealed class ObserverMetrics
	{
		public String ObserverName;
		public EngineLifecycleEvents Category;
		public Int32 CallCount;
		public Double TotalMs;
		public Double AverageMs;
		public Double MinMs;
		public Double MaxMs;
		public Int32 ErrorCount;

		public override String ToString() =>
			$"{ObserverName} [{Category}]: {CallCount} calls, {AverageMs:F2}ms avg ({MinMs:F2}-{MaxMs:F2}ms), {ErrorCount} errors";
	}

	/// <summary>
	/// Immutable snapshot of profiler state at a specific point in time.
	/// Useful for querying performance metrics without blocking the profiler.
	/// </summary>
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
	internal sealed class ProfilerSnapshot : IProfilerSnapshot
	{
		public IReadOnlyDictionary<EngineLifecycleEvents, IReadOnlyList<ObserverMetrics>> CategorizedMetrics { get; internal set; }
		public DateTime Timestamp { get; internal set; }
		public Int64 FrameCount { get; internal set; }

		public override String ToString() =>
			$"ProfilerSnapshot @ {Timestamp:HH:mm:ss.fff}: {CategorizedMetrics[EngineLifecycleEvents.OnStartup]?.Count} observers";
	}
}
