using Luny.Interfaces.Providers;

namespace Luny
{
	internal sealed partial class LunyEngine
	{
		public IApplicationServiceProvider Application { get; private set; }
		public IDebugServiceProvider Debug { get; private set; }
		public IEditorServiceProvider Editor { get; private set; }
		public ISceneServiceProvider Scene { get; private set; }
		public ITimeServiceProvider Time { get; private set; }

		private void AcquireMandatoryServices()
		{
			Application = GetService<IApplicationServiceProvider>();
			Debug = GetService<IDebugServiceProvider>();
			Editor = GetService<IEditorServiceProvider>();
			Scene = GetService<ISceneServiceProvider>();
			Time = GetService<ITimeServiceProvider>();
		}
	}
}
