namespace Luny.Services
{
	/// <summary>
	/// Engine-agnostic editor service.
	/// CAUTION: Implementations must inherit from both IEditorService interface and EditorServiceBase class!
	/// </summary>
	public interface IEditorService : IEngineService
	{
		/// <summary>
		/// Pauses the in-editor player / play mode.
		/// Defaults to Debugger.Break() in engines that don't support player / play mode control by script.
		/// </summary>
		void PausePlayer();
	}

	internal interface IEditorServiceInternal
	{
	}

	public abstract class EditorServiceBase : IEditorServiceInternal
	{
	}
}
