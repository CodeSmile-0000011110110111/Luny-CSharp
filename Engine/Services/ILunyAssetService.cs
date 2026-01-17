using System;

namespace Luny.Engine.Services
{
	/// <summary>
	/// Engine-agnostic Asset management service.
	/// </summary>
	/// <remarks>
	/// IMPORTANT: Implementations must inherit from both the ILuny***Service interface and its corresponding Luny***ServiceBase class!
	/// </remarks>
	public interface ILunyAssetService : ILunyEngineService
	{

	}
}
