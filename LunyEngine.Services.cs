using Luny.Engine;
using Luny.Engine.Services;
using System;

namespace Luny
{
	public sealed partial class LunyEngine
	{
		public ILunyApplicationService Application { get; private set; }
		public ILunyAssetService Asset { get; private set; }
		public ILunyDebugService Debug { get; private set; }
		public ILunyEditorService Editor { get; private set; }
		public ILunyObjectService Object { get; private set; }
		public ILunySceneService Scene { get; private set; }
		public ILunyTimeService Time { get; private set; }

		public Boolean HasService<TService>() where TService : LunyEngineServiceBase => _serviceRegistry.Has<TService>();
		public TService GetService<TService>() where TService : LunyEngineServiceBase => _serviceRegistry.Get<TService>();

		public Boolean TryGetService<TService>(out TService service) where TService : LunyEngineServiceBase =>
			_serviceRegistry.TryGet(out service);

		private void AssignMandatoryServices()
		{
			Application = (ILunyApplicationService)GetService<LunyApplicationServiceBase>();
			Asset = (ILunyAssetService)GetService<LunyAssetServiceBase>();
			Debug = (ILunyDebugService)GetService<LunyDebugServiceBase>();
			Editor = (ILunyEditorService)GetService<LunyEditorServiceBase>();
			Object = (ILunyObjectService)GetService<LunyObjectServiceBase>();
			Scene = (ILunySceneService)GetService<LunySceneServiceBase>();
			Time = (ILunyTimeService)GetService<LunyTimeServiceBase>();
		}
	}
}
