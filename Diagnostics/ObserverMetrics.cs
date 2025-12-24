using System;

namespace Luny.Diagnostics
{
	/// <summary>
	/// Performance metrics for a single lifecycle observer.
	/// Tracks execution time statistics and error counts.
	/// </summary>
	public sealed class ObserverMetrics
	{
		public String ObserverName;
		public ProfilerCategory Category;
		public Int32 CallCount;
		public Double TotalMs;
		public Double AverageMs;
		public Double MinMs;
		public Double MaxMs;
		public Int32 ErrorCount;

		public override String ToString() =>
			$"{ObserverName} [{Category}]: {CallCount} calls, {AverageMs:F2}ms avg ({MinMs:F2}-{MaxMs:F2}ms), {ErrorCount} errors";
	}
}
