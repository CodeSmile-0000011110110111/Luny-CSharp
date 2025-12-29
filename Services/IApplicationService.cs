using System;

namespace Luny.Services
{
	/// <summary>
	/// Engine-agnostic application control.
	/// </summary>
	public interface IApplicationService : IEngineService
	{
		/// <summary>
		/// Quits the application with optional exit code.
		/// </summary>
		/// <param name="exitCode">Exit code (default: 0)</param>
		void Quit(Int32 exitCode = 0);

		/// <summary>
		/// True if running in the editor.
		/// </summary>
		Boolean IsEditor { get; }

		/// <summary>
		/// True if the application is playing (not paused in editor).
		/// </summary>
		Boolean IsPlaying { get; }
	}
}
