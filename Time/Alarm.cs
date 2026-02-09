using System;

namespace Luny
{
	/// <summary>
	/// A lightweight readonly struct representing a fixed point in the future.
	/// Used for high-performance polling in update loops.
	/// </summary>
	public readonly struct Alarm : IComparable<Alarm>, IComparable<Double>, IEquatable<Alarm>, IEquatable<Double>
	{
		public readonly Double Target;

		/// <summary>
		/// Creates an alarm set to fire after the specified time or ticks.
		/// </summary>
		public Alarm(Double target) => Target = target;

		/// <summary>
		/// Creates an alarm set to fire after the specified seconds.
		/// Note: Use From(now) to make it relative to a specific time.
		/// </summary>
		public static Alarm In(Double seconds) => new(seconds);

		/// <summary>
		/// Creates an alarm set to fire after the specified number of ticks.
		/// Note: Use From(now) to make it relative to a specific tick count.
		/// </summary>
		public static Alarm In(Int64 ticks) => new(ticks);

		/// <summary>
		/// Creates an alarm set to fire after the specified milliseconds.
		/// </summary>
		public static Alarm InMilliseconds(Double ms) => new(ms / 1000.0);

		/// <summary>
		/// Creates an alarm set to fire after the specified minutes.
		/// </summary>
		public static Alarm InMinutes(Double m) => new(m * 60.0);

		/// <summary>
		/// Creates an alarm set to fire after the specified hours.
		/// </summary>
		public static Alarm InHours(Double h) => new(h * 3600.0);

		/// <summary>
		/// Creates an alarm at a specific absolute time.
		/// </summary>
		public static Alarm At(Double absoluteTime) => new(absoluteTime);

		/// <summary>
		/// Creates an alarm at a specific total number of ticks.
		/// </summary>
		public static Alarm At(Int64 totalTicks) => new(totalTicks);

		/// <summary>
		/// Returns a new alarm relative to the provided time.
		/// Example: `Alarm.In(5).From(Time.ElapsedSeconds)`
		/// </summary>
		public Alarm From(Double now) => new(now + Target);

		/// <summary>
		/// Returns a new alarm relative to the provided tick.
		/// Example: `Alarm.In(5).From(Time.FrameCount)`
		/// </summary>
		public Alarm From(Int64 currentTick) => new(currentTick + Target);

		/// <summary>
		/// Returns true if the target time has been reached or passed.
		/// </summary>
		public Boolean IsElapsed(Double now) => now >= Target;

		/// <summary>
		/// Returns the remaining time in seconds.
		/// </summary>
		public Double RemainingSeconds(Double now) => Math.Max(0.0, Target - now);

		/// <summary>
		/// Returns the remaining time in milliseconds.
		/// </summary>
		public Double RemainingMilliseconds(Double now) => RemainingSeconds(now) * 1000.0;

		/// <summary>
		/// Returns the remaining time in minutes.
		/// </summary>
		public Double RemainingMinutes(Double now) => RemainingSeconds(now) / 60.0;

		public Int32 CompareTo(Alarm other) => Target.CompareTo(other.Target);
		public Int32 CompareTo(Double other) => Target.CompareTo(other);

		public Boolean Equals(Alarm other) => Target.Equals(other.Target);
		public Boolean Equals(Double other) => Target.Equals(other);

		public override Boolean Equals(Object obj) => obj switch
		{
			Alarm other => Equals(other),
			Double d => Equals(d),
			var _ => false,
		};

		public override Int32 GetHashCode() => Target.GetHashCode();

		public override String ToString() => Target.ToString();

		public static Boolean operator ==(Alarm left, Alarm right) => left.Equals(right);
		public static Boolean operator !=(Alarm left, Alarm right) => !left.Equals(right);
		public static Boolean operator <(Alarm left, Alarm right) => left.CompareTo(right) < 0;
		public static Boolean operator >(Alarm left, Alarm right) => left.CompareTo(right) > 0;
		public static Boolean operator <=(Alarm left, Alarm right) => left.CompareTo(right) <= 0;
		public static Boolean operator >=(Alarm left, Alarm right) => left.CompareTo(right) >= 0;

		public static Boolean operator ==(Double left, Alarm right) => left.Equals(right.Target);
		public static Boolean operator !=(Double left, Alarm right) => !left.Equals(right.Target);
		public static Boolean operator <(Double left, Alarm right) => left < right.Target;
		public static Boolean operator >(Double left, Alarm right) => left > right.Target;
		public static Boolean operator <=(Double left, Alarm right) => left <= right.Target;
		public static Boolean operator >=(Double left, Alarm right) => left >= right.Target;

		public static Boolean operator ==(Alarm left, Double right) => left.Target.Equals(right);
		public static Boolean operator !=(Alarm left, Double right) => !left.Target.Equals(right);
		public static Boolean operator <(Alarm left, Double right) => left.Target < right;
		public static Boolean operator >(Alarm left, Double right) => left.Target > right;
		public static Boolean operator <=(Alarm left, Double right) => left.Target <= right;
		public static Boolean operator >=(Alarm left, Double right) => left.Target >= right;
	}
}
