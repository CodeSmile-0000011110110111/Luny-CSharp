using System;

namespace Luny.Engine.Services
{
	/// <summary>
	/// Provides engine-agnostic access to time and frame information.
	/// Implementations should use engine-native time sources (Time.frameCount, Time.realtimeSinceStartup, etc.)
	/// CAUTION: Implementations must inherit from both ITimeService interface and TimeServiceBase class!
	/// </summary>
	public interface ITimeService : IEngineService
	{
		/// <summary>
		/// Gets the total number of frames that have been rendered since the application started.
		/// CAUTION: This value may differ between engines ie one engine may start in frame 0 while another may start in frame 1.
		/// Unity for instance launches in frame 0 where it runs Awake and OnEnable, but in Start it has incremented FrameCount to 1.
		/// While in Godot the entire first frame's FrameCount is 0.
		/// Only rely on relative differences ie compare current FrameCount with a previous FrameCount timestamp.
		/// Otherwise use the LunyFrameCount which is the same for all engines.
		/// </summary>
		Int64 EngineFrameCount { get; }

		/// <summary>
		/// Gets the total number of frames LunyEngine has updated since it started. This value is the same for all engines.
		/// LunyEngine will always initialize with a FrameCount of 1 - the first frame it normally participates in.
		/// </summary>
		Int64 FrameCount { get; }

		/// <summary>
		/// Gets the time in seconds since the application started.
		/// Uses real time (not affected by Time.timeScale).
		/// </summary>
		Double ElapsedSeconds { get; }
	}

	internal interface ITimeServiceInternal
	{
		void SetLunyFrameCount(Int64 frameCount);
	}

	public abstract class TimeServiceBase : ITimeServiceInternal
	{
		protected Int64 _lunyFrameCount;
		public Int64 FrameCount => _lunyFrameCount;
		void ITimeServiceInternal.SetLunyFrameCount(Int64 frameCount) => _lunyFrameCount = frameCount;
	}
}
