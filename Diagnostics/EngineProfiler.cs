using Luny.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Luny.Diagnostics
{
	/// <summary>
	/// Concrete implementation of engine-level profiling.
	/// Tracks execution time for each lifecycle observer with configurable rolling average.
	/// Public methods use [Conditional] attributes - completely stripped in release builds unless LUNY_PROFILE defined.
	/// </summary>
	public sealed class EngineProfiler
	{
		private readonly Dictionary<Type, ObserverMetrics> _metrics = new();
		private readonly Dictionary<IEngineLifecycleObserver, Stopwatch> _activeObservers = new();
		private Int32 _rollingAverageWindow = 60;

		public Int32 RollingAverageWindow
		{
			get => _rollingAverageWindow;
			set => _rollingAverageWindow = Math.Max(1, value); // Clamp to minimum 1
		}

		[Conditional("DEBUG")] [Conditional("LUNY_DEBUG")] [Conditional("LUNY_PROFILE")]
		public void BeginObserver(IEngineLifecycleObserver observer)
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
		public void EndObserver(IEngineLifecycleObserver observer)
		{
#if DEBUG || LUNY_DEBUG || LUNY_PROFILE
			if (!_activeObservers.TryGetValue(observer, out var sw))
				return;

			sw.Stop();
			var elapsed = sw.Elapsed.TotalMilliseconds;

			var type = observer.GetType();
			if (!_metrics.TryGetValue(type, out var metrics))
			{
				metrics = new ObserverMetrics { ObserverName = type.Name };
				_metrics[type] = metrics;
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

			if (metrics.CallCount == 1)
			{
				metrics.MinMs = newSample;
				metrics.MaxMs = newSample;
			}
			else
			{
				metrics.MinMs = Math.Min(metrics.MinMs, newSample);
				metrics.MaxMs = Math.Max(metrics.MaxMs, newSample);
			}
		}

		[Conditional("DEBUG")] [Conditional("LUNY_DEBUG")] [Conditional("LUNY_PROFILE")]
		public void RecordError(IEngineLifecycleObserver observer, Exception ex)
		{
#if DEBUG || LUNY_DEBUG || LUNY_PROFILE
			var type = observer.GetType();
			if (_metrics.TryGetValue(type, out var metrics))
				metrics.ErrorCount++;
#endif
		}

		public ProfilerSnapshot TakeSnapshot()
		{
#if DEBUG || LUNY_DEBUG || LUNY_PROFILE
			return new ProfilerSnapshot
			{
				ObserverMetrics = _metrics.Values.ToList(),
				Timestamp = DateTime.UtcNow,
			};
#else
			return new ProfilerSnapshot
			{
				ObserverMetrics = Array.Empty<ObserverMetrics>(),
				Timestamp = DateTime.UtcNow
			};
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
}
