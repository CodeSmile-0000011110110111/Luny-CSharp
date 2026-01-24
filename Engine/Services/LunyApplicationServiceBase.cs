using System;

namespace Luny.Engine.Services
{
	/// <summary>
	/// Engine-agnostic application control.
	/// </summary>
	/// <remarks>
	/// IMPORTANT: Implementations must inherit from both the ILuny***Service interface and its corresponding Luny***ServiceBase class!
	/// </remarks>
	public interface ILunyApplicationService : ILunyEngineService
	{
		/// <summary>
		/// True if running in the editor.
		/// </summary>
		Boolean IsEditor { get; }
		/// <summary>
		/// True if the application is playing (not paused in editor).
		/// </summary>
		Boolean IsPlaying { get; }

		/// <summary>
		/// Quits the application with optional exit code.
		/// </summary>
		/// <param name="exitCode">Exit code (default: 0)</param>
		void Quit(Int32 exitCode = 0);
	}

	internal interface ILunyApplicationServiceInternal {}

	public abstract class LunyApplicationServiceBase : LunyEngineServiceBase,
		ILunyApplicationServiceInternal {}
}
