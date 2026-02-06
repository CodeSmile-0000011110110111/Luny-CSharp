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
		/// Gets the time in seconds since the application started.
		/// Uses real time (not affected by Time.timeScale).
		/// </summary>
		Double ElapsedSeconds { get; }
	}

	internal interface ILunyTimeServiceInternal
	{
		void SetLunyFrameCount(Int64 frameCount);
		void IncrementLunyFrameCount();
	}

	public abstract class LunyTimeServiceBase : LunyEngineServiceBase, ILunyTimeServiceInternal
	{
		private Int64 _lunyFrameCount;
		public Int64 FrameCount => _lunyFrameCount;

		void ILunyTimeServiceInternal.SetLunyFrameCount(Int64 frameCount) => _lunyFrameCount = frameCount;
		public void IncrementLunyFrameCount() => _lunyFrameCount++;
	}
}
