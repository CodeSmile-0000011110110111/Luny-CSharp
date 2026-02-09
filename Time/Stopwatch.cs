using System;

namespace Luny
{
	/// <summary>
	/// A lightweight readonly struct that records a point in time to measure duration.
	/// </summary>
	public readonly struct Stopwatch
	{
		private readonly Double _startTime;

		/// <summary>
		/// Starts measuring time from the provided start time.
		/// </summary>
		private Stopwatch(Double startTime) => _startTime = startTime;

		/// <summary>
		/// Starts measuring time from the provided start time.
		/// </summary>
		public static Stopwatch Start(Double now) => new(now);

		/// <summary>
		/// Returns the elapsed time in milliseconds since the timestamp was started.
		/// </summary>
		public Double ElapsedMilliseconds(Double now) => ElapsedSeconds(now) * 1000.0;

		/// <summary>
		/// Returns the elapsed time in seconds since the timestamp was started.
		/// </summary>
		public Double ElapsedSeconds(Double now) => now - _startTime;

		/// <summary>
		/// Returns the elapsed time in minutes since the timestamp was started.
		/// </summary>
		public Double ElapsedMinutes(Double now) => ElapsedSeconds(now) / 60.0;

		/// <summary>
		/// Returns the elapsed time in hours since the timestamp was started.
		/// </summary>
		public Double ElapsedHours(Double now) => ElapsedSeconds(now) / 3600.0;

		public override String ToString() => _startTime.ToString();
	}
}
