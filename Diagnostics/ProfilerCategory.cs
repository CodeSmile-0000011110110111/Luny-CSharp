using System;

namespace Luny.Diagnostics
{
	/// <summary>
	/// Categorizes profiler metrics based on the engine lifecycle stage.
	/// </summary>
	[Flags]
	public enum ProfilerCategory
	{
		OnStartup = 1 << 0,
		OnFixedStep = 1 << 1,
		OnUpdate = 1 << 2,
		OnLateUpdate = 1 << 3,
		OnShutdown = 1 << 4,
	}
}
