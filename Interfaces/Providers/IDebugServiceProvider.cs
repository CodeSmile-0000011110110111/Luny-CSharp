using Luny.Diagnostics;

namespace Luny.Interfaces.Providers
{
	public interface IDebugServiceProvider : IEngineServiceProvider, ILunyLogger
	{
		void PausePlayer();
	}
}
