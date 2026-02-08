using System;

namespace Luny.Engine.Services
{
	/// <summary>
	/// Provides engine-agnostic access to time and frame information.
	/// </summary>
	/// <remarks>
	/// IMPORTANT: Implementations must inherit from both the ILuny***Service interface and its corresponding Luny***ServiceBase class!
	/// </remarks>
	public interface ILunyTimeService : ILunyEngineService
	{
		/// <summary>
		/// Gets the engine's frame count since the application started.
		/// </summary>
		/// <remarks>
		/// CAUTION: This value may differ between engines! Some engines start with frame 0, others in frame 1.
		/// Unity for instance launches in frame 0 where it runs Awake and OnEnable, but by Start it has incremented FrameCount to 1.
		/// While in Godot the entire first frame's FrameCount is 0.
		/// Prefer FrameCount since it is the same for all engines.
		/// </remarks>
		Int64 EngineFrameCount { get; }

		/// <summary>
		/// Gets the total number of frames LunyEngine has run since launch. This value is consistent across engines.
		/// </summary>
		/// <remarks>
		/// Initialization (startup, OnCreate, OnEnable) occurs in FrameCount 0. By OnReady (and before first OnHeartbeat/OnFrame) FrameCount will be 1.
		/// </remarks>
		Int64 FrameCount { get; }

		/// <summary>
		/// Gets the total number of heartbeats LunyEngine has run since launch. This value is consistent across engines.
		/// </summary>
		Int64 HeartbeatCount { get; }

		/// <summary>
		/// Gets the time in seconds since the application started.
		/// Uses real time (not affected by Time.timeScale).
		/// </summary>
		Double ElapsedSeconds { get; }

		/// <summary>
		/// Time since last frame update. Updates once before frame processing begins. Value changes with framerate.
		/// </summary>
		Double DeltaTime { get; }
		/// <summary>
		/// Time between two heartbeats. Value remains the same during runtime (constant).
		/// </summary>
		Double FixedDeltaTime { get; }

		/// <summary>
		/// Holds timing related values.
		/// </summary>
		LunyTime Time { get; }
	}

	internal interface ILunyTimeServiceInternal
	{
		void SetLunyFrameAndHeartbeatCount(Int64 frameCount);
		void SetFixedDeltaTime(Double fixedDeltaTime);
		void SetDeltaTime(Double deltaTime);
		void IncrementHeartbeatCount();
		void IncrementFrameCount();
	}

	public class LunyTime
	{
		public Int64 HeartbeatCount { get; internal set; }
		public Int64 FrameCount { get; internal set; }
		public Double DeltaTime { get; internal set; }
		public Double FixedDeltaTime { get; internal set; }
	}

	public abstract class LunyTimeServiceBase : LunyEngineServiceBase, ILunyTimeServiceInternal
	{
		private readonly LunyTime _time = new();
		public LunyTime Time => _time;

		public Int64 HeartbeatCount => _time.HeartbeatCount;
		public Int64 FrameCount => _time.FrameCount;
		public Double FixedDeltaTime => _time.FixedDeltaTime;
		public Double DeltaTime => _time.DeltaTime;

		void ILunyTimeServiceInternal.SetLunyFrameAndHeartbeatCount(Int64 frameCount)
		{
			_time.FrameCount = frameCount;
			_time.HeartbeatCount = frameCount;
		}

		void ILunyTimeServiceInternal.SetFixedDeltaTime(Double fixedDeltaTime) => _time.FixedDeltaTime = fixedDeltaTime;
		void ILunyTimeServiceInternal.SetDeltaTime(Double deltaTime) => _time.DeltaTime = deltaTime;
		void ILunyTimeServiceInternal.IncrementHeartbeatCount() => _time.HeartbeatCount++;
		void ILunyTimeServiceInternal.IncrementFrameCount() => _time.FrameCount++;
	}
}
