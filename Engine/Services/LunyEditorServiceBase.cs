namespace Luny.Engine.Services
{
	/// <summary>
	/// Engine-agnostic editor service.
	/// CAUTION: Implementations must inherit from both IEditorService interface and EditorServiceBase class!
	/// </summary>
	public interface ILunyEditorService : ILunyEngineService
	{
		/// <summary>
		/// Pauses the in-editor player / play mode.
		/// Defaults to Debugger.Break() in engines that don't support player / play mode control by script.
		/// </summary>
		void PausePlayer();
	}

	internal interface ILunyEditorServiceInternal {}

	public abstract class LunyEditorServiceBase : LunyEngineServiceBase, ILunyEditorServiceInternal {}
}
