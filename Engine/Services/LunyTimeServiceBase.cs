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
		/// Gets the time in seconds since the application started. Uses real time (not affected by Time.timeScale).
		/// </summary>
		Double ElapsedSeconds { get; }

		/// <summary>
		/// Time since last frame update. Updates once after heartbeat. Value varies with framerate.
		/// </summary>
		/// <remarks>
		/// When read in a Heartbeat method, be aware that DeltaTime is last frame's deltaTime.
		/// </remarks>
		Double DeltaTime { get; }
		/// <summary>
		/// Time between two heartbeats. Value remains fixed (constant) at runtime.
		/// </summary>
		Double FixedDeltaTime { get; }
	}

	internal interface ILunyTimeServiceInternal
	{
		void SetLunyFrameAndHeartbeatCount(Int64 frameCount);
		void SetFixedDeltaTime(Double fixedDeltaTime);
		void SetDeltaTime(Double deltaTime);
		void IncrementFrameCounters();
	}

	public abstract class LunyTimeServiceBase : LunyEngineServiceBase, ILunyTimeService, ILunyTimeServiceInternal
	{
		public Int64 HeartbeatCount { get; private set; }
		public Int64 FrameCount { get; private set; }
		public Double FixedDeltaTime { get; private set; }
		public Double DeltaTime { get; private set; }
		public abstract Double ElapsedSeconds { get; }
		public abstract Int64 EngineFrameCount { get; }

		void ILunyTimeServiceInternal.SetLunyFrameAndHeartbeatCount(Int64 frameCount)
		{
			FrameCount = frameCount;
			HeartbeatCount = frameCount;
		}

		void ILunyTimeServiceInternal.SetFixedDeltaTime(Double fixedDeltaTime) => FixedDeltaTime = fixedDeltaTime;
		void ILunyTimeServiceInternal.SetDeltaTime(Double deltaTime) => DeltaTime = deltaTime;

		void ILunyTimeServiceInternal.IncrementFrameCounters()
		{
			HeartbeatCount++;
			FrameCount++;
		}
	}
}
