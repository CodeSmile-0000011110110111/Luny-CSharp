using System;

namespace Luny.Engine.Services
{
	/// <summary>
	/// Engine-agnostic application control.
	/// CAUTION: Implementations must inherit from both IApplicationService interface and ApplicationServiceBase class!
	/// </summary>
	public interface IApplicationService : ILunyEngineService
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

	internal interface IApplicationServiceInternal {}

	public abstract class ApplicationServiceBase : IApplicationServiceInternal {}
}
