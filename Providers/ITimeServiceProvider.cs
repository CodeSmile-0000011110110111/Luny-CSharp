using Luny.Interfaces;
using System;

namespace Luny.Providers
{
	/// <summary>
	/// Provides engine-agnostic access to time and frame information.
	/// Implementations should use engine-native time sources (Time.frameCount, Time.realtimeSinceStartup, etc.)
	/// </summary>
	public interface ITimeServiceProvider : IEngineServiceProvider
	{
		/// <summary>
		/// Gets the total number of frames that have been rendered since the application started.
		/// </summary>
		Int64 FrameCount { get; }

		/// <summary>
		/// Gets the time in seconds since the application started.
		/// Uses real time (not affected by Time.timeScale).
		/// </summary>
		Double ElapsedSeconds { get; }
	}
}
