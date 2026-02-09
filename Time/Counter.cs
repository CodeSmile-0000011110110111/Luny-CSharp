using System;

namespace Luny
{
	/// <summary>
	/// A stateful manager for discrete integer counts (e.g., frames or heartbeats).
	/// Supports auto-repeat and progress tracking.
	/// </summary>
	public sealed class Counter
	{
		public Int32 Current { get; private set; }
		public Int32 Target { get; set; }
		public Boolean IsRunning { get; set; }
		public Boolean AutoRepeat { get; set; }

		public event Action OnElapsed;

		/// <summary>
		/// Creates a counter with a count-towards target.
		/// </summary>
		public Counter(Int32 target) => Target = target;

		/// <summary>
		/// Starts or restarts the counter from zero. Unpauses Counter if paused.
		/// </summary>
		public void Start()
		{
			Stop();
			IsRunning = true;
		}

		/// <summary>
		/// Pauses the counter. Does not reset Current count.
		/// </summary>
		public void Pause() => IsRunning = false;

		/// <summary>
		/// Resumes the counter if paused. Does nothing if not paused.
		/// </summary>
		public void Resume() => IsRunning = true;

		/// <summary>
		/// Stops the counter and resets Current count to zero.
		/// </summary>
		public void Stop()
		{
			Current = 0;
			IsRunning = false;
		}

		/// <summary>
		/// Increments the counter by one, unless paused.
		/// Fires OnElapsed if target is reached.
		/// </summary>
		public void Increment()
		{
			if (!IsRunning)
				return;

			Current++;

			if (Current >= Target)
			{
				OnElapsed?.Invoke();

				if (AutoRepeat)
					Current = 0;
				else
					Pause();
			}
		}

		/// <summary>
		/// Returns the progress as a normalized value (0.0 to 1.0).
		/// </summary>
		public Double Progress => Target > 0 ? Math.Min(1.0, (Double)Current / Target) : 1.0;

		/// <summary>
		/// Returns the remaining count.
		/// </summary>
		public Int32 Remaining => Math.Max(0, Target - Current);

		public override String ToString() => $"{Current}/{Target}";
	}
}
