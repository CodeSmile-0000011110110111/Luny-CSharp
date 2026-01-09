using Luny.Engine;
using Luny.Engine.Services;
using System;

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

		private Boolean HasService<TService>() where TService : LunyEngineServiceBase => _serviceRegistry.Has<TService>();
		private TService GetService<TService>() where TService : LunyEngineServiceBase => _serviceRegistry.Get<TService>();

		private Boolean TryGetService<TService>(out TService service) where TService : LunyEngineServiceBase =>
			_serviceRegistry.TryGet(out service);
	}
}
