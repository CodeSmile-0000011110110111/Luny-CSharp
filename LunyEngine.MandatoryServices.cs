using Luny.Engine.Services;

namespace Luny
{
	public sealed partial class LunyEngine
	{
		public IApplicationService Application { get; private set; }
		public IDebugService Debug { get; private set; }
		public IEditorService Editor { get; private set; }
		public ISceneService Scene { get; private set; }
		public ITimeService Time { get; private set; }

		private void AcquireMandatoryServices()
		{
			Application = GetService<IApplicationService>();
			Debug = GetService<IDebugService>();
			Editor = GetService<IEditorService>();
			Scene = GetService<ISceneService>();
			Time = GetService<ITimeService>();
		}
	}
}
