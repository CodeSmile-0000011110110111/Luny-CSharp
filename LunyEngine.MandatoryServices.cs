using Luny.Engine.Services;

namespace Luny
{
	public sealed partial class LunyEngine
	{
		public ILunyApplicationService Application { get; private set; }
		public ILunyDebugService Debug { get; private set; }
		public ILunyEditorService Editor { get; private set; }
		public ILunySceneService Scene { get; private set; }
		public ILunyTimeService Time { get; private set; }

		private void AssignMandatoryServices()
		{
			Application = (ILunyApplicationService)GetService<LunyApplicationServiceBase>();
			Debug = (ILunyDebugService)GetService<LunyDebugServiceBase>();
			Editor = (ILunyEditorService)GetService<LunyEditorServiceBase>();
			Scene = (ILunySceneService)GetService<LunySceneServiceBase>();
			Time = (ILunyTimeService)GetService<LunyTimeServiceBase>();
		}
	}
}
