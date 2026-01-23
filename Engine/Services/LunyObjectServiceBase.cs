using Luny.Engine.Bridge;
using Luny.Engine.Bridge.Enums;
using System;

namespace Luny.Engine.Services
{
	/// <summary>
	/// Provides engine-agnostic methods for creating objects and primitives.
	/// </summary>
	public interface ILunyObjectService : ILunyEngineService
	{
		ILunyObject CreateEmpty(String name);
		ILunyObject CreatePrimitive(String name, LunyPrimitiveType type);
	}

	public abstract class LunyObjectServiceBase : LunyEngineServiceBase {}
}
