namespace Luny
{
	/// <summary>
	/// Marker interface for engine-agnostic services: APIs such as Debug, Input, etc.
	/// Implementations are auto-discovered and registered at startup.
	/// </summary>
	public interface ILunyEngineService {}
}
