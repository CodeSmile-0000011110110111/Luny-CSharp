namespace Luny.Interfaces.Providers
{
	public interface IEditorServiceProvider : IEngineServiceProvider
	{
		/// <summary>
		/// Pauses the in-editor player / play mode.
		/// Defaults to Debugger.Break() in engines that don't support player / play mode control by script.
		/// </summary>
		void PausePlayer();
	}
}
