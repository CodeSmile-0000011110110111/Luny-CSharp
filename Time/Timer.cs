using System;

namespace Luny
{
	/// <summary>
	/// A stateful manager for a countdown or count-up.
	/// Supports pausing, time-scaling, and auto-repeat.
	/// </summary>
	public sealed class Timer
	{
		public event Action OnElapsed;
		public Double Current { get; private set; }
		public Double Duration { get; set; }
		public Double TimeScale { get; set; } = 1.0;
		public Boolean IsRunning { get; set; }
		public Boolean AutoRepeat { get; set; }

		/// <summary>
		/// Returns the progress as a normalized value (0.0 to 1.0).
		/// </summary>
		public Double Progress => Duration > 0.0 ? Math.Min(1.0, Current / Duration) : 1.0;

		/// <summary>
		/// Returns the remaining time in seconds.
		/// </summary>
		public Double RemainingSeconds => Math.Max(0.0, Duration - Current);

		/// <summary>
		/// Returns the remaining time in milliseconds.
		/// </summary>
		public Double RemainingMilliseconds => RemainingSeconds * 1000.0;

		/// <summary>
		/// Returns the remaining time in minutes.
		/// </summary>
		public Double RemainingMinutes => RemainingSeconds / 60.0;

		/// <summary>
		/// Creates a new timer with duration in seconds.
		/// </summary>
		public static Timer FromSeconds(Double seconds) => new(seconds);

		/// <summary>
		/// Creates a new timer with duration in milliseconds.
		/// </summary>
		public static Timer FromMilliseconds(Double ms) => new(ms / 1000.0);

		/// <summary>
		/// Creates a new timer with duration in minutes.
		/// </summary>
		public static Timer FromMinutes(Double minutes) => new(minutes * 60.0);

		/// <summary>
		/// Creates a new timer with duration in seconds.
		/// </summary>
		public Timer(Double duration) => Duration = duration;

		/// <summary>
		/// Starts or restarts the timer from zero.
		/// </summary>
		public void Start()
		{
			Stop();
			IsRunning = true;
		}

		/// <summary>
		/// Pauses the timer. Does not reset Current time.
		/// </summary>
		public void Pause() => IsRunning = false;

		/// <summary>
		/// Resumes the timer if paused. Does nothing if not paused.
		/// </summary>
		public void Resume() => IsRunning = true;

		/// <summary>
		/// Stops the timer and resets elapsed time to zero.
		/// </summary>
		public void Stop()
		{
			IsRunning = false;
			Current = 0.0;
		}

		/// <summary>
		/// Advances the timer by the specified delta time, unless paused.
		/// Fires OnElapsed if duration is reached.
		/// </summary>
		public void Tick(Double deltaTime)
		{
			if (!IsRunning)
				return;

			Current += deltaTime * TimeScale;

			// account for floating point accumulation errors
			const Double Epsilon = 1e-6;
			if (Current + Epsilon >= Duration)
			{
				OnElapsed?.Invoke();

				if (AutoRepeat)
					Current -= Duration;
				else
					IsRunning = false;
			}
		}

		public override String ToString() => ToString(@"mm\:ss");

		/// <summary>
		/// Returns the current time formatted as a string using TimeSpan format.
		/// </summary>
		public String ToString(String format)
		{
			var timeSpan = TimeSpan.FromSeconds(Current);
			return timeSpan.ToString(format);
		}
	}
}
