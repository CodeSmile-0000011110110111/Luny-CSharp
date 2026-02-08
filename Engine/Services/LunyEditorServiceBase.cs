namespace Luny.Engine.Services
{
	/// <summary>
	/// Engine-agnostic editor service.
	/// </summary>
	/// <remarks>
	/// IMPORTANT: Implementations must inherit from both the ILuny***Service interface and its corresponding Luny***ServiceBase class!
	/// </remarks>
	public interface ILunyEditorService : ILunyEngineService
	{
		/// <summary>
		/// Pauses the in-editor player / play mode.
		/// Defaults to Debugger.Break() in engines that don't support player / play mode control by script.
		/// </summary>
		void PausePlayer();
	}

	internal interface ILunyEditorServiceInternal {}

	public abstract class LunyEditorServiceBase : LunyEngineServiceBase, ILunyEditorService, ILunyEditorServiceInternal
	{
		public abstract void PausePlayer();
	}
}
